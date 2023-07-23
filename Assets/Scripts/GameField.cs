using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class GameField : MonoBehaviour
{
  [SerializeField] private FieldConfiguration m_field_configuration;
  [SerializeField] private bool m_is_auto_play;

  [SerializeReference] private GameObject m_game_element_prefab;
  [SerializeReference] private GameInfo m_game_info;
  [SerializeReference] private GameObject m_abilities;

  private Rect m_max_active_zone_rect;
  private Vector2 m_max_active_zone_anchor_min;
  private Vector2 m_max_active_zone_anchor_max;
  private Vector2 m_max_active_zone_center;
  private GameObject m_input_handler;

  private GameElement[,] m_field;
  private FieldData m_field_data;
  private ElementStyleProvider m_element_style_provider;
  private float m_grid_step;
  private float m_half_grid_step;
  private float m_element_size;
  private float m_element_offset;
  private float m_inner_grid_stroke_width;
  private float m_outer_grid_stroke_width;
  private Vector2 m_field_center;
  private List<(int, int)> m_to_create;
  private List<(int, int)> m_selected_elements;
  private List<(int, int)> m_highlighted_elements;
  private Vector2 m_mouse_down_position;
  private ((int, int), (int, int))? m_reverse_move;

  public GameField()
  {
    m_to_create = new List<(int, int)>();
    m_selected_elements = new List<(int, int)>();
    m_highlighted_elements = new List<(int, int)>();
  }

  void Start()
  {
    m_input_handler = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
    var active_zone = m_input_handler.GetComponent<RectTransform>();
    m_max_active_zone_rect = active_zone.rect;
    m_max_active_zone_anchor_min = active_zone.anchorMin;
    m_max_active_zone_anchor_max = active_zone.anchorMax;
    m_max_active_zone_center = active_zone.localPosition;
    _Init();
  }

  void Update()
  {
    if (!_IsAvailable())
      return;
    if (m_reverse_move.HasValue)
    {
      _MakeMove(m_reverse_move.Value.Item1, m_reverse_move.Value.Item2);
      m_reverse_move = null;
    }
    m_game_info.moves_count = m_field_data.GetAllMoves().Count;
    if (m_to_create.Count != 0)
    {
      foreach (var (row_id, column_id) in m_to_create)
        _InitElement(row_id, column_id);
      m_to_create.Clear();
      return;
    }

    bool scenario_result = false;
    switch (m_field_configuration.spawn_move_scenario)
    {
      case FieldConfiguration.SpawnMoveScenario.MoveThenSpawn:
        scenario_result = _MoveThenSpawnElements();
        break;
      case FieldConfiguration.SpawnMoveScenario.SpawnThenMove:
        scenario_result = _SpawnThenMoveElements();
        break;
      default:
        throw new System.NotImplementedException();
    }
    if (scenario_result)
      return;

    if (_ProcessElementGroups())
      return;
    if (m_is_auto_play)
      _AutoMove();
    m_field_data.Save();
  }

  private bool _ProcessElementGroups()
  {
    if (!m_field_data.HasGroups())
      return false;

    var groups_details = m_field_data.ProcessGroups();
    foreach (var group_details in groups_details)
    {
      foreach (var element in group_details.group)
      {
        m_field[element.Item1, element.Item2].Destroy();
        if (m_field_data.At(element.Item1, element.Item2) != -1)
          m_to_create.Add(element);
      }
      m_game_info.UpdateScore(group_details.value, group_details.group.Count);
    }
    return true;
  }

  private bool _MoveThenSpawnElements()
  {
    var element_move_changes = m_field_data.ElementsMoveChanges();
    if (element_move_changes.Count > 0)
    {
      m_field_data.MoveElements();
      foreach (var element_move in element_move_changes)
      {
        var first = element_move.Item1;
        var second = element_move.Item2;
        var target_position = m_field[second.Item1, second.Item2].transform.position;
        m_field[second.Item1, second.Item2].transform.position = m_field[first.Item1, first.Item2].transform.position;
        m_field[first.Item1, first.Item2].MoveTo(target_position);
        (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
          (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
      }
      return true;
    }
    if (m_field_data.HasEmptyCells())
    {
      var created_elements = m_field_data.SpawnNewValues();
      foreach (var element_position in created_elements)
        this._InitElement(element_position.Item1, element_position.Item2);
      return true;
    }
    return false;
  }

  private bool _SpawnThenMoveElements()
  {
    if (m_field_data.HasEmptyCells())
    {
      var element_move_changes = m_field_data.ElementsMoveChanges();
      m_field_data.MoveElements();
      foreach (var element_move in element_move_changes)
      {
        var first = element_move.Item1;
        var second = element_move.Item2;
        m_field[second.Item1, second.Item2].transform.position = m_field[first.Item1, first.Item2].transform.position;
        m_field[first.Item1, first.Item2].MoveTo(_GetElementPosition(second.Item1, second.Item2));
        (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
          (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
      }
      var created_elements = m_field_data.SpawnNewValues();
      int main_line = -1;
      var offset = 0;
      var direction = m_field_data.GetMoveDirection();
      created_elements.Sort((first, second) =>
        direction.Item2 == 0 ?
          first.Item2 == second.Item2 ? -direction.Item1 * (second.Item1 - first.Item1) : second.Item2 - first.Item2 :
          first.Item1 == second.Item1 ? -direction.Item2 * (second.Item2 - first.Item2) : second.Item1 - first.Item1
      );
      foreach (var element_position in created_elements)
      {
        if (direction.Item2 == 0 && element_position.Item2 != main_line)
        {
          main_line = element_position.Item2;
          offset = 0;
        }
        else if (direction.Item1 == 0 && element_position.Item1 != main_line)
        {
          main_line = element_position.Item1;
          offset = 0;
        }
        --offset;
        _InitElement(element_position.Item1, element_position.Item2, false);
        var row_id = direction.Item1 == 0 ? 0 : direction.Item1 < 0 ? offset : m_field_configuration.height - 1 - offset;
        var column_id = direction.Item2 == 0 ? 0 : direction.Item2 < 0 ? offset : m_field_configuration.width - 1 - offset;
        if (row_id == 0)
          row_id = element_position.Item1;
        if (column_id == 0)
          column_id = element_position.Item2;
        var target_element_position = _GetElementPosition(row_id, column_id);
        m_field[element_position.Item1, element_position.Item2].gameObject.transform.position = target_element_position;
        m_field[element_position.Item1, element_position.Item2].MoveTo(_GetElementPosition(element_position.Item1, element_position.Item2));
      }
      return true;
    }
    return false;
  }

  public void Restart()
  {
    Reset();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        this._InitElement(row_id, column_id);
  }

  public void Reset()
  {
    m_field_data.Reset();
    m_game_info.Reset();
    for (int ability_id = 0; ability_id < m_abilities.transform.childCount; ++ability_id)
    {
      var ability_game_object = m_abilities.transform.GetChild(ability_id).gameObject;
      ability_game_object.GetComponent<AbilityBase>().Reset();
    }
  }

  public void Init(FieldConfiguration i_field_configuration)
  {
    bool is_init_needed = m_field_configuration.width != i_field_configuration.width ||
      m_field_configuration.height != i_field_configuration.height ||
      m_field_configuration.active_elements_count != i_field_configuration.active_elements_count;

    var old_cells = m_field_configuration.GetCells();
    var new_cells = i_field_configuration.GetCells();
    for (int row_id = 0; row_id < i_field_configuration.height; ++row_id)
    {
      if (is_init_needed)
        break;
      for (int column_id = 0; column_id < i_field_configuration.width; ++column_id)
      {
        if (is_init_needed)
          break;
        is_init_needed = new_cells[row_id, column_id] != old_cells[row_id, column_id];
      }
    }

    if (is_init_needed)
    {
      Reset();
      Destroy(transform.GetChild(2).gameObject);
      Destroy(transform.GetChild(3).gameObject);
      for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
          Destroy(m_field[row_id, column_id].gameObject);
    }

    m_field_configuration = i_field_configuration;
    m_field_data.field_configuration = i_field_configuration;

    if (is_init_needed)
      _Init();
  }

  private void _Init()
  {
    m_field_data = new FieldData(m_field_configuration, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    m_field_configuration = m_field_data.field_configuration;

    m_grid_step = Mathf.Min(m_max_active_zone_rect.width / m_field_configuration.width, m_max_active_zone_rect.height / m_field_configuration.height);
    m_half_grid_step = m_grid_step / 2;
    m_element_offset = m_grid_step * 0.05f;
    m_element_size = m_grid_step - 2 * m_element_offset;
    m_inner_grid_stroke_width = m_element_offset / 4;
    m_outer_grid_stroke_width = m_element_offset;

    _InitBackgroundGrid();
    _InitHoleOverlays();
    _InitInputHandler();
    _InitCameraViewport();

    m_element_style_provider = new ElementStyleProvider(m_element_size);
    m_field = new GameElement[m_field_configuration.height, m_field_configuration.width];
    m_game_info.moves_count = m_field_data.GetAllMoves().Count;
    m_field_center = new Vector2(m_grid_step * m_field_configuration.width / 2 - m_max_active_zone_center.x, m_grid_step * m_field_configuration.height / 2 + m_max_active_zone_center.y);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        Vector2 position = this._GetElementPosition(row_id, column_id);
        m_field[row_id, column_id] = Instantiate(m_game_element_prefab, position, Quaternion.identity).GetComponent<GameElement>();
        m_field[row_id, column_id].transform.parent = transform;
        this._InitElement(row_id, column_id);
      }
  }

  private Vector2 _PointerEventPositionToWorldPosition(Vector2 i_event_position)
  {
    var world_mouse_event_position = Camera.main.ScreenToWorldPoint(i_event_position);
    return new Vector2(world_mouse_event_position.x, world_mouse_event_position.y);
  }

  private void _HandlePointerDown(Vector2 i_event_position)
  {
    if (!this._IsAvailable())
      return;
    m_mouse_down_position = this._PointerEventPositionToWorldPosition(i_event_position);
    var element_position = this._GetElementPosition(m_mouse_down_position);
    if (!this._IsValidCell(element_position))
    {
      Debug.LogError($"Bad element position on pointer down: {element_position} with event position {i_event_position}");
      return;
    }
    this._SelectElement(element_position);
    this._ProcessSelectedElements();
  }

  private void _HandlePointerUp(Vector2 i_event_position)
  {
    if (m_selected_elements.Count != 1)
      return;
    var mouse_up_position = this._PointerEventPositionToWorldPosition(i_event_position);
    var delta = mouse_up_position - m_mouse_down_position;
    if (delta.magnitude / m_grid_step < 0.5)
      return;
    delta.Normalize();
    var element_position = this._GetElementPosition(m_mouse_down_position + delta * m_grid_step);
    if (!this._IsValidCell(element_position))
    {
      Debug.LogError($"Bad element position on pointer up: {element_position} with event position {i_event_position}");
      return;
    }
    this._SelectElement(element_position);
    this._ProcessSelectedElements();
  }

  private void _HandleAbilityMove(string i_ability_name, Vector2 i_event_position)
  {
    if (!this._IsAvailable())
      return;
    var elements_to_highlight = new List<(int, int)>();
    var main_element_position = this._GetElementPosition(Camera.main.ScreenToWorldPoint(i_event_position));
    if (!this._IsValidCell(main_element_position))
    {
      this._ClearHighlighting();
      return;
    }
    switch (i_ability_name)
    {
      case "RemoveElement":
        elements_to_highlight.Add(main_element_position);
        break;
      case "Bomb":
        for (int row_id = main_element_position.Item1 - 1; row_id <= main_element_position.Item1 + 1; ++row_id)
          for (int column_id = main_element_position.Item2 - 1; column_id <= main_element_position.Item2 + 1; ++column_id)
            if (this._IsValidCell(row_id, column_id))
              elements_to_highlight.Add((row_id, column_id));
        break;
      case "RemoveElementsByValue":
        if (this._IsValidCell(main_element_position))
        {
          var target_value = m_field_data.At(main_element_position.Item1, main_element_position.Item2);
          for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
            for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
              if (m_field_data.At(row_id, column_id) == target_value)
                elements_to_highlight.Add((row_id, column_id));
        }
        break;
      default:
        break;
    }
    foreach (var element_position in m_highlighted_elements)
      if (!elements_to_highlight.Contains(element_position))
        m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(false);
    foreach (var element_position in elements_to_highlight)
      if (!m_highlighted_elements.Contains(element_position))
        m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(true);
    m_highlighted_elements = elements_to_highlight;
  }

  private void _HandleAbility(string i_ability_name, AbilityBase i_ability, Vector2 i_applied_position)
  {
    if (!this._IsAvailable())
      return;
    var main_element_position = this._GetElementPosition(Camera.main.ScreenToWorldPoint(i_applied_position));
    if (!this._IsValidCell(main_element_position))
      return;
    m_game_info.SpentScore(i_ability.price);
    i_ability.NextPrice();
    switch (i_ability_name)
    {
      case "RemoveElement":
        var removed_element_info = m_field_data.RemoveZone(main_element_position.Item1, main_element_position.Item2,
          main_element_position.Item1, main_element_position.Item2);
        m_field[main_element_position.Item1, main_element_position.Item2].Destroy();
        foreach (var element_info in removed_element_info)
          m_game_info.UpdateScore(element_info.Key, element_info.Value);
        break;
      case "Bomb":
        var zone_to_remove = ((main_element_position.Item1 - 1, main_element_position.Item2 - 1),
          (main_element_position.Item1 + 1, main_element_position.Item2 + 1));
        var removed_zone_info = m_field_data.RemoveZone(zone_to_remove.Item1.Item1, zone_to_remove.Item1.Item2,
          zone_to_remove.Item2.Item1, zone_to_remove.Item2.Item2);
        for (int row_id = zone_to_remove.Item1.Item1; row_id <= zone_to_remove.Item2.Item1; ++row_id)
          for (int column_id = zone_to_remove.Item1.Item2; column_id <= zone_to_remove.Item2.Item2; ++column_id)
            if (this._IsValidCell(row_id, column_id))
              m_field[row_id, column_id].Destroy();
        foreach (var element_info in removed_zone_info)
          m_game_info.UpdateScore(element_info.Key, element_info.Value);
        break;
      case "RemoveElementsByValue":
        var value = m_field_data.At(main_element_position.Item1, main_element_position.Item2);
        for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
          for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
            if (m_field_data.At(row_id, column_id) == value)
              m_field[row_id, column_id].Destroy();
        var removed_count = m_field_data.RemoveValue(value);
        m_game_info.UpdateScore(value, removed_count);
        break;
      default:
        return;
    }
    this._ClearHighlighting();
  }

  public void HandleStaticAbility(string i_name, StaticAbility i_ability)
  {
    if (!this._IsAvailable())
      return;
    m_game_info.SpentScore(i_ability.price);
    i_ability.NextPrice();
    switch (i_name)
    {
      case "Shuffle":
        m_field_data.Shuffle();
        for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
          for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
          {
            m_field[row_id, column_id].Destroy();
            m_to_create.Add((row_id, column_id));
          }
        break;
      case "UpgradeGenerator":
        var small_value = m_field_data.values_interval[0];
        m_field_data.IncreaseValuesInterval();
        for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
          for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
            if (m_field_data.At(row_id, column_id) == small_value)
              m_field[row_id, column_id].Destroy();
        var removed_count = m_field_data.RemoveValue(small_value);
        m_game_info.UpdateScore(small_value, removed_count);
        break;
      case "Search":
        var moves = m_field_data.GetAllMoves();
        var move = moves[UnityEngine.Random.Range(0, moves.Count)];
        this._ClearHighlighting();
        this._HighlightElement(move.first);
        this._HighlightElement(move.second);
        Invoke("_ClearHighlighting", 1);
        break;
      default:
        break;
    }
  }
  private void _SelectElement((int, int) i_position)
  {
    m_selected_elements.Add(i_position);
    m_field[i_position.Item1, i_position.Item2].UpdateSelection(true);
  }

  private void _HighlightElement((int, int) i_position)
  {
    m_highlighted_elements.Add(i_position);
    m_field[i_position.Item1, i_position.Item2].UpdateHighlighting(true);
  }

  private void _ClearHighlighting()
  {
    foreach (var element_position in m_highlighted_elements)
      m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(false);
    m_highlighted_elements.Clear();
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

  private void _InitBackgroundGrid()
  {
    SVG svg = new SVG();
    var rect_size = new Vector2(m_grid_step, m_grid_step);
    var rect_color = "rgba(20, 20, 20, 0.5)";
    var rect_stroke_props = new SVGStrokeProps("#000000", m_inner_grid_stroke_width);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        svg.Add(new SVGRect(new Vector2(column_id * m_grid_step, row_id * m_grid_step), rect_size, rect_color, rect_stroke_props));

    using System.IO.StringReader textReader = new System.IO.StringReader(svg.GetXML());
    var sceneInfo = SVGParser.ImportSVG(textReader);
    var geometries = VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
    {
      StepDistance = m_grid_step,
      SamplingStepSize = 1,
      MaxCordDeviation = 0.0f,
      MaxTanAngleDeviation = 0.0f
    });
    var sprite = VectorUtils.BuildSprite(geometries, 1, VectorUtils.Alignment.Center, Vector2.zero, 128, false);
    GameObject background = new GameObject();
    background.transform.parent = gameObject.transform;
    background.transform.localPosition = m_input_handler.transform.localPosition;
    var sprite_renderer = background.AddComponent<SpriteRenderer>();
    sprite_renderer.sprite = sprite;
    sprite_renderer.sortingOrder = -1;
  }

  private List<List<(int, int)>> _GetHolePaths(List<(int, int)> i_hole)
  {
    var hole_edges = new List<((int, int), (int, int))>();
    foreach (var (row_id, column_id) in i_hole)
    {
      hole_edges.Add(((row_id, column_id), (row_id, column_id + 1)));
      hole_edges.Add(((row_id, column_id + 1), (row_id + 1, column_id + 1)));
      hole_edges.Add(((row_id + 1, column_id + 1), (row_id + 1, column_id)));
      hole_edges.Add(((row_id + 1, column_id), (row_id, column_id)));
    }
    for (int i = 0; i < hole_edges.Count; ++i)
      for (int j = i + 1; j < hole_edges.Count; ++j)
        if (hole_edges[i].Item1 == hole_edges[j].Item2 && hole_edges[i].Item2 == hole_edges[j].Item1)
        {
          hole_edges.RemoveAt(j);
          hole_edges.RemoveAt(i);
          --i;
          break;
        }
    var paths = new List<List<(int, int)>>();
    int edge_id = hole_edges.Count;
    while (hole_edges.Count > 0)
    {
      if (edge_id >= hole_edges.Count)
      {
        paths.Add(new List<(int, int)>());
        paths[paths.Count - 1].Add(hole_edges[0].Item1);
        paths[paths.Count - 1].Add(hole_edges[0].Item2);
        hole_edges.RemoveAt(0);
        edge_id = 0;
        continue;
      }
      if (paths[paths.Count - 1][paths[paths.Count - 1].Count - 1] == hole_edges[edge_id].Item1)
      {
        paths[paths.Count - 1].Add(hole_edges[edge_id].Item2);
        hole_edges.RemoveAt(edge_id);
        edge_id = 0;
      }
      else
        ++edge_id;
    }
    return paths;
  }

  private void _InitHoleOverlays()
  {
    var holes = m_field_data.GetHoles();
    var outer_hole = new FieldData.GroupDetails();
    outer_hole.group = new List<(int, int)>();
    for (int row_id = 0; row_id <= m_field_configuration.height + 1; ++row_id)
      for (int column_id = 0; column_id <= m_field_configuration.width + 1; ++column_id)
      {
        if (row_id > 0 && row_id <= m_field_configuration.height &&
          column_id > 0 && column_id <= m_field_configuration.width)
          continue;
        outer_hole.group.Add((row_id, column_id));
      }
    for (int hole_id = 0; hole_id < holes.Count; ++hole_id)
    {
      bool is_outer = false;
      for (int element_id = 0; element_id < holes[hole_id].group.Count; ++element_id)
      {
        var (row_id, column_id) = holes[hole_id].group[element_id];
        holes[hole_id].group[element_id] = (row_id + 1, column_id + 1);
        is_outer = is_outer || (row_id == 0 || row_id == m_field_configuration.height - 1 ||
          column_id == 0 || column_id == m_field_configuration.width - 1);
      }
      if (is_outer)
      {
        for (int element_id = 0; element_id < holes[hole_id].group.Count; ++element_id)
          outer_hole.group.Add(holes[hole_id].group[element_id]);
        holes.RemoveAt(hole_id);
        --hole_id;
      }
    }
    holes.Add(outer_hole);

    SVG stroke_svg = new SVG();
    SVG fill_svg = new SVG();
    var fill_color = "rgba(20, 20, 20, 0.5)";
    var offset_value = m_outer_grid_stroke_width / 2;
    var hole_stroke = new SVGStrokeProps("#000000", m_outer_grid_stroke_width);
    var no_stroke = new SVGStrokeProps("none", 0);

    foreach (var hole in holes)
    {
      var paths = _GetHolePaths(hole.group);
      var fill_path = new SVGPath();
      fill_path.fill_color = fill_color;
      fill_path.stroke_props = no_stroke;
      var stroke_path = new SVGPath();
      stroke_path.fill_color = "rgba(20, 20, 20, 0.25)";
      stroke_path.stroke_props = hole_stroke;
      foreach (var path_points in paths)
      {
        // Remove last point as it's same as first one
        path_points.RemoveAt(path_points.Count - 1);
        if (path_points[0].Item1 == 0 && path_points[0].Item2 == 0)
        {
          path_points.Clear();
          var max_row_count = Mathf.RoundToInt(Screen.height / m_grid_step) + 1;
          var max_column_count = Mathf.RoundToInt(Screen.width / m_grid_step) + 1;
          var additional_rows = Mathf.Max(0, max_row_count - m_field_configuration.height) / 2 + 1;
          var additional_columns = Mathf.Max(0, max_column_count - m_field_configuration.width) / 2 + 1;
          path_points.Add((-additional_rows, -additional_columns));
          path_points.Add((-additional_rows, m_field_configuration.width + 2 + additional_columns));
          path_points.Add((m_field_configuration.height + 2 + additional_rows, m_field_configuration.width + 2 + additional_columns));
          path_points.Add((m_field_configuration.height + 2 + additional_rows, -additional_columns));
        }

        var offset_path_points = new List<Vector2>(path_points.Count);
        foreach (var (row_id, column_id) in path_points)
          offset_path_points.Add(new Vector2(column_id * m_grid_step, (m_field_configuration.height - row_id + 2) * m_grid_step));

        fill_path.MoveTo(offset_path_points[0]);
        for (int point_id = 1; point_id < offset_path_points.Count; ++point_id)
          fill_path.LineTo(offset_path_points[point_id]);
        fill_path.Close();

        var prev_offset_direction = new Vector2(0, 0);
        for (int point_id = 0; point_id <= path_points.Count; ++point_id)
        {
          var curr_point_id = point_id == path_points.Count ? 0 : point_id;
          var prev_point_id = point_id == 0 ? path_points.Count - 1 : point_id - 1;
          var edge_direction = new Vector2(path_points[curr_point_id].Item2, path_points[curr_point_id].Item1) -
            new Vector2(path_points[prev_point_id].Item2, path_points[prev_point_id].Item1);
          var offset_direction = new Vector2(-edge_direction.y, -edge_direction.x);
          offset_direction *= offset_value;
          if (prev_offset_direction == new Vector2(0, 0))
          {
            prev_offset_direction = offset_direction;
            continue;
          }
          if (Vector2.Dot(prev_offset_direction, offset_direction) == 0.0f)
          {
            offset_path_points[curr_point_id] = offset_path_points[curr_point_id] + offset_direction;
            offset_path_points[point_id - 1] = offset_path_points[point_id - 1] + offset_direction;
          }
          else
            offset_path_points[curr_point_id] = offset_path_points[curr_point_id] + offset_direction;
          prev_offset_direction = offset_direction;
        }

        stroke_path.MoveTo(offset_path_points[0]);
        for (int point_id = 1; point_id < offset_path_points.Count; ++point_id)
          stroke_path.LineTo(offset_path_points[point_id]);
        stroke_path.Close();
      }
      fill_svg.Add(fill_path);
      stroke_svg.Add(stroke_path);
    }
    string svg_text = fill_svg.GetXML();
    var textReader = new System.IO.StringReader(svg_text);
    var sceneInfo = SVGParser.ImportSVG(textReader);
    var geometries = VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
    {
      StepDistance = m_grid_step,
      SamplingStepSize = 1,
      MaxCordDeviation = 0.0f,
      MaxTanAngleDeviation = 0.0f
    });
    var sprite = VectorUtils.BuildSprite(geometries, 1, VectorUtils.Alignment.Center, Vector2.zero, 128, false);

    GameObject holes_fill = transform.GetChild(1).gameObject;
    holes_fill.transform.localPosition = m_input_handler.transform.localPosition;
    var holes_fill_mask = holes_fill.GetComponent<SpriteMask>();
    holes_fill_mask.sprite = sprite;
    var holes_background_image = transform.GetChild(1).GetChild(0).gameObject;
    var background_image_size = holes_background_image.GetComponent<SpriteRenderer>().sprite.rect.size;
    var x_scale = (Screen.width + 2 * m_outer_grid_stroke_width) / background_image_size.x;
    var y_scale = (Screen.height + 2 * m_outer_grid_stroke_width) / background_image_size.x;
    holes_background_image.transform.localScale = new Vector3(x_scale, y_scale, 1);
    var field_background_image = transform.GetChild(1).GetChild(1).gameObject;
    field_background_image.transform.localScale = new Vector3(x_scale, y_scale, 1);

    svg_text = stroke_svg.GetXML();
    textReader = new System.IO.StringReader(svg_text);
    sceneInfo = SVGParser.ImportSVG(textReader);
    geometries = VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
    {
      StepDistance = m_grid_step,
      SamplingStepSize = 1,
      MaxCordDeviation = 0.0f,
      MaxTanAngleDeviation = 0.0f
    });
    var stroke_sprite = VectorUtils.BuildSprite(geometries, 1, VectorUtils.Alignment.Center, Vector2.zero, 128, false);
    GameObject holes_stroke = new GameObject();
    holes_stroke.transform.SetParent(transform);
    holes_stroke.transform.localPosition = m_input_handler.transform.localPosition;
    var sprite_renderer = holes_stroke.AddComponent<SpriteRenderer>();
    sprite_renderer.sprite = stroke_sprite;
    sprite_renderer.sortingOrder = 1;
  }

  private void _InitInputHandler()
  {
    // Fit input handler size to actual grid size
    var input_handler_rect_transform = m_input_handler.GetComponent<RectTransform>();
    var rect = input_handler_rect_transform.rect;
    var anchor_delta = m_max_active_zone_anchor_max - m_max_active_zone_anchor_min;
    var height_fraction = anchor_delta.y * (m_max_active_zone_rect.height - m_grid_step * m_field_configuration.height) / m_max_active_zone_rect.height / 2;
    var width_fraction = anchor_delta.x * (m_max_active_zone_rect.width - m_grid_step * m_field_configuration.width) / m_max_active_zone_rect.width / 2;
    input_handler_rect_transform.anchorMin = m_max_active_zone_anchor_min + new Vector2(width_fraction, height_fraction);
    input_handler_rect_transform.anchorMax = m_max_active_zone_anchor_max - new Vector2(width_fraction, height_fraction);

    // Init input handler callbacks
    var input_handler_component = m_input_handler.GetComponent<GameFieldInputHandler>();
    input_handler_component.on_input_down = this._HandlePointerDown;
    input_handler_component.on_input_up = this._HandlePointerUp;
    input_handler_component.on_ability_move = this._HandleAbilityMove;
    input_handler_component.on_ability_apply = this._HandleAbility;
  }

  private void _InitCameraViewport()
  {
    var active_zone = m_input_handler.GetComponent<RectTransform>();
    Camera.main.orthographicSize = Screen.height / 2.0f + m_outer_grid_stroke_width;
    Camera.main.aspect = (float)Screen.width / Screen.height;
    Camera.main.transform.position = new Vector3(
      m_input_handler.transform.localPosition.x,
      m_input_handler.transform.localPosition.y,
      Camera.main.transform.position.z);
  }

  private void _AutoMove()
  {
    var moves = m_field_data.GetAllMoves();
    if (moves.Count > 0)
    {
      moves.Sort((FieldData.MoveDetails first, FieldData.MoveDetails second) => second.strike - first.strike);
      var max_strike_value = moves[0].strike;
      int max_strike_value_count = 0;
      foreach (var move_details in moves)
      {
        if (move_details.strike < max_strike_value)
          break;
        ++max_strike_value_count;
      }
      var move_index = UnityEngine.Random.Range(0, max_strike_value_count);
      var move = moves[move_index];
      _MakeMove(move.first, move.second);
    }
  }

  private void _MakeMove((int, int) first, (int, int) second)
  {
    if (!m_field_data.IsMoveAvailable(first, second))
      return;
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

  private void _InitElement(int row_id, int column_id, bool i_with_animation = true)
  {
    int value = m_field_data.At(row_id, column_id);
    if (value >= 0)
      m_field[row_id, column_id].Create(m_element_style_provider.Get(value), i_with_animation);
    else
      m_field[row_id, column_id].Destroy();
    m_field[row_id, column_id].transform.GetChild(2).GetComponent<SpriteRenderer>().size = new Vector2(m_element_size, m_element_size);
    m_field[row_id, column_id].transform.GetChild(3).GetComponent<SpriteRenderer>().size = new Vector2(m_grid_step, m_grid_step);
  }

  private void OnDestroy()
  {
    m_field_data.Save();
  }

  private bool _IsAvailable()
  {
    for (int i = 0; i < m_field_configuration.height; ++i)
      for (int j = 0; j < m_field_configuration.width; ++j)
        if (!m_field[i, j].IsAvailable())
          return false;
    return true;
  }

  private bool _IsValidCell(int i_row_id, int i_column_id)
  {
    return i_row_id >= 0 && i_row_id < m_field_configuration.height && i_column_id >= 0 && i_column_id < m_field_configuration.width;
  }

  private bool _IsValidCell((int, int) i_position)
  {
    return _IsValidCell(i_position.Item1, i_position.Item2);
  }

  public FieldConfiguration field_configuration
  {
    get => m_field_configuration;
  }
}
