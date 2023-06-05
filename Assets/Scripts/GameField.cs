using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
  public int width;
  public int height;
  public GameObject game_element_prefab;
  public bool is_auto_play;
  public GameInfo game_info;
  public GameObject abilities;

  private GameElement[,] m_field;
  private GameFieldData m_field_data;
  private ElementStyleProvider m_element_style_provider;
  private float m_grid_step;
  private float m_half_grid_step;
  private float m_element_offset;
  private float m_element_size;
  private Vector2 m_field_center;
  private List<(int, int)> m_to_create;
  private List<(int, int)> m_selected_elements;
  private List<(int, int)> m_highlighted_elements;
  private Vector2 m_mouse_down_position;
  private ((int, int), (int, int))? m_reverse_move;

  [SerializeReference] private GameObject m_input_handler;

  public GameField()
  {
    m_to_create = new List<(int, int)>();
    m_selected_elements = new List<(int, int)>();
    m_highlighted_elements = new List<(int, int)>();
  }

  void Start()
  {
    Camera.main.orthographicSize = Screen.height / 2;
    Camera.main.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
    float screen_width = Screen.width;
    float screen_height = Screen.height;
    var min_dimmension = Mathf.Min(screen_width, screen_height);
    m_grid_step = Mathf.Min(screen_width / width, screen_height / height);
    m_half_grid_step = m_grid_step / 2;
    m_element_offset = m_grid_step * 0.1f;
    m_element_size = m_grid_step - 2 * m_element_offset;
    m_element_style_provider = new ElementStyleProvider(m_element_size);
    m_field = new GameElement[height, width];
    m_field_data = new GameFieldData(width, height);
    game_info.moves_count = m_field_data.GetAllMoves().Count;
    m_field_center = new Vector2(min_dimmension / 2, min_dimmension / 2);
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
      {
        Vector2 position = this._GetElementPosition(row_id, column_id);
        m_field[row_id, column_id] = Instantiate(game_element_prefab, position, Quaternion.identity).GetComponent<GameElement>();
        m_field[row_id, column_id].transform.parent = transform;
        this._InitElement(row_id, column_id);
      }
    this._InitInputHandler();
  }

  void Update()
  {
    if (!this._IsAvailable())
      return;
    if (m_reverse_move.HasValue)
    {
      this._MakeMove(m_reverse_move.Value.Item1, m_reverse_move.Value.Item2);
      m_reverse_move = null;
    }
    game_info.moves_count = m_field_data.GetAllMoves().Count;
    if (m_to_create.Count != 0)
    {
      foreach (var (row_id, column_id) in m_to_create)
        this._InitElement(row_id, column_id);
      m_to_create.Clear();
      return;
    }
    var element_move_changes = m_field_data.ElementsMoveChanges();
    if (element_move_changes.Count > 0)
    {
      m_field_data.MoveElements();
      foreach (var element_move in element_move_changes)
      {
        var first = element_move.Item1;
        var second = element_move.Item2;
        (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
          (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
        var target_position = m_field[first.Item1, first.Item2].transform.position;
        m_field[first.Item1, first.Item2].transform.position = m_field[second.Item1, second.Item2].transform.position;
        m_field[second.Item1, second.Item2].MoveTo(target_position);
      }
      return;
    }
    if (m_field_data.HasEmptyCells())
    {
      var created_elements = m_field_data.SpawnNewValues();
      foreach (var element_position in created_elements)
        this._InitElement(element_position.Item1, element_position.Item2);
      return;
    }
    if (m_field_data.HasGroups())
    {
      var groups_details = m_field_data.AccumulateGroups();
      foreach (var group_details in groups_details)
      {
        foreach (var element in group_details.group)
        {
          m_field[element.Item1, element.Item2].Destroy();
          if (m_field_data.At(element.Item1, element.Item2) != -1)
            m_to_create.Add(element);
        }
        game_info.UpdateScore(group_details.value, group_details.group.Count);
      }
      return;
    }
    this._AutoMove();
  }

  public void Restart()
  {
    m_field_data.Reset();
    game_info.Reset();
    for (int ability_id = 0; ability_id < abilities.transform.childCount; ++ability_id)
    {
      var ability_game_object = abilities.transform.GetChild(ability_id).gameObject;
      ability_game_object.GetComponent<Ability>().Reset();
    }
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        this._InitElement(row_id, column_id);
  }

  private Vector2 _GetMouseEventPosition()
  {
    var world_mouse_event_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    return new Vector2(world_mouse_event_position.x, world_mouse_event_position.y);
  }

  private void _HandlePointerDown()
  {
    if (!this._IsAvailable())
      return;
    m_mouse_down_position = this._GetMouseEventPosition();
    this._SelectElement(this._GetElementPosition(m_mouse_down_position));
    this._ProcessSelectedElements();
  }

  private void _HandlePointerUp()
  {
    if (m_selected_elements.Count != 1)
      return;
    var mouse_up_position = this._GetMouseEventPosition();
    var delta = mouse_up_position - m_mouse_down_position;
    if (delta.magnitude / m_grid_step < 0.5)
      return;
    delta.Normalize();
    this._SelectElement(this._GetElementPosition(delta * m_grid_step + m_mouse_down_position));
    this._ProcessSelectedElements();
  }

  private void _HandleAbilityMove(string i_ability_name, Vector2 i_event_position)
  {
    var elements_to_highlight = new List<(int, int)>();
    var main_element_position = this._GetElementPosition(Camera.main.ScreenToWorldPoint(i_event_position));
    switch (i_ability_name)
    {
      case "RemoveElement":
        elements_to_highlight.Add(main_element_position);
        break;
      default:
        break;
    }
    foreach (var element_position in m_highlighted_elements)
      if (!elements_to_highlight.Contains(element_position))
        m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(false);
    if (main_element_position.Item1 >= height || main_element_position.Item1 < 0 ||
      main_element_position.Item2 >= width || main_element_position.Item2 < 0)
      m_highlighted_elements.Clear();
    else
    {
      foreach (var element_position in elements_to_highlight)
        if (!m_highlighted_elements.Contains(element_position))
          m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(true);
      m_highlighted_elements = elements_to_highlight;
    }
  }

  private void _HandleAbility(string i_ability_name, Ability i_ability, Vector2 i_applied_position)
  {
    var main_element_position = this._GetElementPosition(Camera.main.ScreenToWorldPoint(i_applied_position));
    if (main_element_position.Item1 >= height || main_element_position.Item1 < 0 ||
      main_element_position.Item2 >= width || main_element_position.Item2 < 0)
      return;
    game_info.SpentScore(i_ability.price);
    i_ability.NextPrice();
    switch (i_ability_name)
    {
      case "RemoveElement":
        var removed_zone_info = m_field_data.RemoveZone(main_element_position.Item1, main_element_position.Item2,
          main_element_position.Item1, main_element_position.Item2);
        m_field[main_element_position.Item1, main_element_position.Item2].Destroy();
        foreach (var element_info in removed_zone_info)
          game_info.UpdateScore(element_info.Key, element_info.Value);
        break;
      default:
        return;
    }
  }

  private void _SelectElement((int, int) i_position)
  {
    m_selected_elements.Add(i_position);
    m_field[i_position.Item1, i_position.Item2].UpdateSelection(true);
  }

  private void _ProcessSelectedElements()
  {
    if (m_selected_elements.Count != 2)
      return;

    var first = m_selected_elements[0];
    var second = m_selected_elements[1];
    var distance = Mathf.Abs(first.Item1 - second.Item1) + Mathf.Abs(first.Item2 - second.Item2);
    if (distance > 1)
    {
      var position = m_selected_elements[0];
      m_field[position.Item1, position.Item2].UpdateSelection(false);
      m_selected_elements.RemoveAt(0);
    }
    else
    {
      foreach (var position in m_selected_elements)
        m_field[position.Item1, position.Item2].UpdateSelection(false);
      m_selected_elements.Clear();
      if (distance == 1)
        this._MakeMove(first, second);
    }
  }

  private void _InitInputHandler()
  {
    var input_handler_rect_transform = m_input_handler.GetComponent<RectTransform>();
    var height_fraction = (Screen.height - m_grid_step * height) / Screen.height / 2;
    input_handler_rect_transform.anchorMin = new Vector2(0, height_fraction);
    input_handler_rect_transform.anchorMax = new Vector2(1, 1 - height_fraction);
    var input_handler_component = m_input_handler.GetComponent<GameFieldInputHandler>();
    input_handler_component.on_input_down = this._HandlePointerDown;
    input_handler_component.on_input_up = this._HandlePointerUp;
    input_handler_component.on_ability_move = this._HandleAbilityMove;
    input_handler_component.on_ability_apply = this._HandleAbility;
  }

  private void _AutoMove()
  {
    if (!is_auto_play)
      return;
    var moves = m_field_data.GetAllMoves();
    if (moves.Count > 0)
    {
      moves.Sort((GameFieldData.MoveDetails first, GameFieldData.MoveDetails second) => second.strike - first.strike);
      var max_strike_value = moves[0].strike;
      int max_strike_value_count = 0;
      foreach (var move_details in moves)
      {
        if (move_details.strike < max_strike_value)
          break;
        ++max_strike_value_count;
      }
      var move_index = Random.Range(0, max_strike_value_count);
      var move = moves[move_index];
      this._MakeMove(move.first, move.second);
    }
  }

  private void _MakeMove((int, int) first, (int, int) second)
  {
    m_field_data.SwapCells(first.Item1, first.Item2, second.Item1, second.Item2);
    (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
      (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
    m_field[first.Item1, first.Item2].MoveTo(m_field[second.Item1, second.Item2].transform.position);
    m_field[second.Item1, second.Item2].MoveTo(m_field[first.Item1, first.Item2].transform.position);
    if (!m_field_data.HasGroups() && !m_reverse_move.HasValue)
      m_reverse_move = (first, second);
  }

  private Vector2 _GetElementPosition(int row_id, int column_id)
  {
    return new Vector2(
      m_grid_step * column_id - m_field_center.x + m_half_grid_step,
      m_field_center.y - m_half_grid_step - m_grid_step * row_id
    );
  }

  private (int, int) _GetElementPosition(Vector2 position)
  {
    var row_id = Mathf.FloorToInt((m_field_center.y - position.y) / m_grid_step);
    var column_id = Mathf.FloorToInt((m_field_center.x + position.x) / m_grid_step);
    return (row_id, column_id);
  }

  private void _InitElement(int row_id, int column_id)
  {
    int value = m_field_data.At(row_id, column_id);
    if (value >= 0)
      m_field[row_id, column_id].Create(m_element_style_provider.Get(value), value);
    else
      m_field[row_id, column_id].Destroy();
  }

  private void OnDestroy()
  {
    m_field_data.Save();
  }

  private bool _IsAvailable()
  {
    for (int i = 0; i < height; ++i)
      for (int j = 0; j < width; ++j)
        if (!m_field[i, j].IsAvailable())
          return false;
    return true;
  }
}
