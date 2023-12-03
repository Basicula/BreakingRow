using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour {
  public struct MaxActiveZone {
    public readonly Rect rect;
    public readonly Vector2 anchor_min;
    public readonly Vector2 anchor_max;
    public readonly Vector3 position;

    public MaxActiveZone(RectTransform i_transform) {
      rect = new Rect(i_transform.rect);
      anchor_min = new Vector2(i_transform.anchorMin.x, i_transform.anchorMin.y);
      anchor_max = new Vector2(i_transform.anchorMax.x, i_transform.anchorMax.y);
      position = new Vector3(i_transform.localPosition.x, i_transform.localPosition.y, i_transform.localPosition.z);
    }
  }

  [SerializeField] private FieldConfiguration m_field_configuration;
  [SerializeField] private bool m_is_auto_play;

  [SerializeReference] private GameObject m_game_element_prefab;
  [SerializeReference] private GameInfo m_game_info;
  [SerializeReference] private GameObject m_abilities;
  [SerializeReference] private GameObject m_input_handler;
  [SerializeReference] private GameFieldBackgroundGrid m_background_grid;
  [SerializeReference] private GameFieldHoles m_holes;

  private MaxActiveZone m_max_active_zone;

  private FieldData m_field_data;
  private IFieldElementsSpawner m_elements_spawner;
  private IFieldElementsMover m_elements_mover;
  private IFieldResolver m_field_resolver;
  private ElementStyleProvider m_element_style_provider;

  private GameElement[,] m_field;
  private FieldGridConfiguration m_grid_configuration;
  private readonly List<(int, int)> m_selected_elements;
  private readonly List<(int, int)> m_highlighted_elements;
  private (int, int) m_highlighted_focus_position;
  private Vector2 m_mouse_down_position;
  private ((int, int), (int, int))? m_reverse_move;

  public GameField() {
    m_selected_elements = new List<(int, int)>();
    m_highlighted_elements = new List<(int, int)>();
  }

  void Start() {
    m_max_active_zone = new MaxActiveZone(m_input_handler.GetComponent<RectTransform>());

    if (!m_field_configuration.Load())
      m_field_configuration.InitCellsConfiguration();

    _Init();
  }

  void Update() {
    if (!_IsAvailable())
      return;

    if (_FillField())
      return;

    if (_ProcessElementGroups())
      // If moved and then we successfully procced groups then we don't want to remember how to undo it
      m_reverse_move = null;

    if (m_reverse_move.HasValue) {
      _MakeMove(m_reverse_move.Value.Item1, m_reverse_move.Value.Item2);
      m_reverse_move = null;
    }

    m_game_info.moves_count = m_field_resolver.GetAllMoves().Count;

    if (m_is_auto_play)
      _AutoMove();
    _Save();
  }

  private bool _ProcessElementGroups() {
    var changes = m_field_resolver.Process();
    foreach (var (row_id, column_id) in changes.destroyed)
      m_field[row_id, column_id].Destroy();
    foreach (var (value, positions) in changes.combined) {
      foreach (var element in positions)
        m_field[element.Item1, element.Item2].Destroy();
      m_game_info.UpdateScore(value, positions.Count, true);
    }
    foreach (var position in changes.created)
      _InitElement(position.Item1, position.Item2, true);
    foreach (var position in changes.updated)
      _InitElement(position.Item1, position.Item2, true);
    return changes.combined.Count != 0;
  }

  private bool _FillField() {
    return m_field_configuration.fill_strategy switch {
      FieldConfiguration.FillStrategy.MoveThenSpawn => _MoveThenSpawnElements(),
      FieldConfiguration.FillStrategy.SpawnThenMove => _SpawnThenMoveElements(),
      _ => throw new System.NotImplementedException(),
    };
  }

  private bool _MoveThenSpawnElements() {
    var element_move_changes = m_elements_mover.Move().moved;
    if (element_move_changes.Count > 0) {
      foreach (var element_move in element_move_changes) {
        var first = element_move.Item1;
        var second = element_move.Item2[^1];
        var target_position = m_field[second.Item1, second.Item2].transform.position;
        m_field[second.Item1, second.Item2].transform.position = m_field[first.Item1, first.Item2].transform.position;
        m_field[first.Item1, first.Item2].MoveTo(target_position);
        (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
          (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
      }
      return true;
    }
    var created_elements = m_elements_spawner.SpawnElements();
    if (created_elements.Count > 0) {
      foreach (var element_position in created_elements)
        _InitElement(element_position.Item1, element_position.Item2);
      return true;
    }
    return false;
  }

  private bool _SpawnThenMoveElements() {
    var element_move_changes = m_elements_mover.Move().moved;
    foreach (var element_move in element_move_changes) {
      var first = element_move.Item1;
      var second = element_move.Item2[^1];
      m_field[second.Item1, second.Item2].transform.position = m_field[first.Item1, first.Item2].transform.position;
      m_field[first.Item1, first.Item2].MoveTo(m_grid_configuration.GetElementPosition(second));
      (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
        (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
    }
    var created_elements = m_elements_spawner.SpawnElements();
    int main_line = -1;
    var offset = 0;
    var direction = m_field_data.GetMoveDirection();
    foreach (var element_position in created_elements) {
      if (direction.Item2 == 0 && element_position.Item2 != main_line) {
        main_line = element_position.Item2;
        offset = 0;
      } else if (direction.Item1 == 0 && element_position.Item1 != main_line) {
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
      var target_element_position = m_grid_configuration.GetElementPosition(row_id, column_id);
      m_field[element_position.Item1, element_position.Item2].gameObject.transform.position = target_element_position;
      m_field[element_position.Item1, element_position.Item2].MoveTo(m_grid_configuration.GetElementPosition(element_position));
    }
    return element_move_changes.Count > 0 || created_elements.Count > 0;
  }

  public void Restart() {
    Reset();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        _InitElement(row_id, column_id);
  }

  public void Reset() {
    m_elements_spawner.Reset();
    m_field_data.Reset();
    m_elements_spawner.InitElements();
    m_game_info.Reset();
    for (int ability_id = 0; ability_id < m_abilities.transform.childCount; ++ability_id) {
      var ability_game_object = m_abilities.transform.GetChild(ability_id).gameObject;
      ability_game_object.GetComponent<AbilityBase>().Reset();
    }
  }

  public void Init(FieldConfiguration i_field_configuration) {
    var old_field_configuration = m_field_configuration.Clone();
    m_field_configuration.Update(i_field_configuration);

    bool is_init_needed = m_field_configuration.width != old_field_configuration.width ||
      m_field_configuration.height != old_field_configuration.height ||
      m_field_configuration.active_elements_count != old_field_configuration.active_elements_count;
    var old_cells = old_field_configuration.GetCellsConfiguration();
    var new_cells = m_field_configuration.GetCellsConfiguration();
    for (int row_id = 0; row_id < i_field_configuration.height && !is_init_needed; ++row_id)
      for (int column_id = 0; column_id < i_field_configuration.width && !is_init_needed; ++column_id)
        is_init_needed = new_cells[row_id, column_id] != old_cells[row_id, column_id];

    if (is_init_needed) {
      Reset();
      for (int row_id = 0; row_id < old_field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < old_field_configuration.width; ++column_id)
          Destroy(m_field[row_id, column_id].gameObject);
      _Init();
    } else if (m_field_configuration.mode != old_field_configuration.mode)
      _InitFieldResolver();
    else if (m_field_configuration.move_type != old_field_configuration.move_type)
      _InitFieldMover();
  }

  private void _InitFieldResolver() {
    switch (m_field_configuration.mode) {
      case FieldConfiguration.Mode.Classic:
        m_field_resolver = new ClassicFieldResolver(m_field_data);
        break;
      case FieldConfiguration.Mode.Accumulated:
        m_field_resolver = new AccumulativeFieldResolver(m_field_data);
        break;
      default:
        throw new System.NotImplementedException();
    }
  }

  private void _InitFieldMover() {
    switch (m_field_configuration.move_type) {
      case FieldConfiguration.MoveType.Immobile:
        m_elements_mover = new ImmobileMover(m_field_data);
        break;
      case FieldConfiguration.MoveType.Fall:
        m_elements_mover = new StraightFallMover(m_field_data);
        break;
      case FieldConfiguration.MoveType.FallAndSlide:
      default:
        throw new System.NotImplementedException();
    }
  }

  private void _Init() {
    m_field_data = new FieldData(m_field_configuration);
    m_elements_spawner = new SimpleCommonElementsSpawner(m_field_data);
    m_elements_spawner.InitElements();
    _InitFieldMover();
    _InitFieldResolver();

    m_grid_configuration = new FieldGridConfiguration(m_field_configuration, m_max_active_zone);

    m_background_grid.Init(m_field_configuration, m_grid_configuration);
    m_holes.Init(m_field_data, m_grid_configuration);
    _InitInputHandler();
    _InitCameraViewport();

    m_element_style_provider = new ElementStyleProvider(m_grid_configuration.element_size);
    m_field = new GameElement[m_field_configuration.height, m_field_configuration.width];
    m_game_info.moves_count = m_field_resolver.GetAllMoves().Count;
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        Vector2 position = m_grid_configuration.GetElementPosition(row_id, column_id);
        m_field[row_id, column_id] = Instantiate(m_game_element_prefab, position, Quaternion.identity).GetComponent<GameElement>();
        m_field[row_id, column_id].transform.parent = transform;
        _InitElement(row_id, column_id);
      }
  }

  private Vector2 _PointerEventPositionToWorldPosition(Vector2 i_event_position) {
    var world_mouse_event_position = Camera.main.ScreenToWorldPoint(i_event_position);
    return (Vector2)world_mouse_event_position;
  }

  private void _HandlePointerDown(Vector2 i_event_position) {
    if (!_IsAvailable())
      return;
    m_mouse_down_position = _PointerEventPositionToWorldPosition(i_event_position);
    var element_position = m_grid_configuration.GetElementPosition(m_mouse_down_position);
    if (!_IsValidCell(element_position)) {
      Debug.LogError($"Bad element position on pointer down: {element_position} with event position {i_event_position}");
      return;
    }
    _SelectElement(element_position);
    _ProcessSelectedElements();
  }

  private void _HandlePointerUp(Vector2 i_event_position) {
    if (m_selected_elements.Count != 1)
      return;
    var mouse_up_position = _PointerEventPositionToWorldPosition(i_event_position);
    var delta = mouse_up_position - m_mouse_down_position;
    if (delta.magnitude / m_grid_configuration.grid_step < 0.5)
      return;
    delta.Normalize();
    var element_position = m_grid_configuration.GetElementPosition(m_mouse_down_position + delta * m_grid_configuration.grid_step);
    if (!_IsValidCell(element_position)) {
      Debug.LogError($"Bad element position on pointer up: {element_position} with event position {i_event_position}");
      return;
    }
    _SelectElement(element_position);
    _ProcessSelectedElements();
  }

  private void _HandleAbilityMove(string i_ability_name, Vector2 i_event_position) {
    if (!_IsAvailable())
      return;
    var main_element_position = m_grid_configuration.GetElementPosition(Camera.main.ScreenToWorldPoint(i_event_position));
    var offset_from_previous_main = Mathf.Abs(main_element_position.Item1 - m_highlighted_focus_position.Item1) +
      Mathf.Abs(main_element_position.Item2 - m_highlighted_focus_position.Item2);
    if (offset_from_previous_main == 0)
      return;
    _ClearHighlighting();
    if (!_IsValidCell(main_element_position))
      return;
    switch (i_ability_name) {
      case "RemoveElement":
        _HighlightElement(main_element_position);
        break;
      case "Bomb":
        for (int row_id = main_element_position.Item1 - 1; row_id <= main_element_position.Item1 + 1; ++row_id)
          for (int column_id = main_element_position.Item2 - 1; column_id <= main_element_position.Item2 + 1; ++column_id)
            if (_IsValidCell(row_id, column_id))
              _HighlightElement((row_id, column_id));
        break;
      case "RemoveElementsByValue":
        if (_IsValidCell(main_element_position)) {
          var target_element = m_field_data[main_element_position];
          for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
            for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
              if (m_field_data[row_id, column_id] == target_element)
                _HighlightElement((row_id, column_id));
        }
        break;
      default:
        break;
    }
    m_highlighted_focus_position = main_element_position;
  }

  private void _HandleAbility(string i_ability_name, AbilityBase i_ability, Vector2 i_applied_position) {
    if (!_IsAvailable())
      return;
    var main_element_position = m_grid_configuration.GetElementPosition(Camera.main.ScreenToWorldPoint(i_applied_position));
    _ClearHighlighting();
    if (!_IsValidCell(main_element_position))
      return;
    m_game_info.SpentScore(i_ability.price);
    i_ability.NextPrice();
    switch (i_ability_name) {
      case "RemoveElement":
        var removed_element_info = m_field_data.RemoveZone(main_element_position.Item1, main_element_position.Item2,
          main_element_position.Item1, main_element_position.Item2);
        m_field[main_element_position.Item1, main_element_position.Item2].Destroy();
        foreach (var element_info in removed_element_info)
          m_game_info.UpdateScore(element_info.Key, element_info.Value, false);
        break;
      case "Bomb":
        var zone_to_remove = ((main_element_position.Item1 - 1, main_element_position.Item2 - 1),
          (main_element_position.Item1 + 1, main_element_position.Item2 + 1));
        var removed_zone_info = m_field_data.RemoveZone(zone_to_remove.Item1.Item1, zone_to_remove.Item1.Item2,
          zone_to_remove.Item2.Item1, zone_to_remove.Item2.Item2);
        for (int row_id = zone_to_remove.Item1.Item1; row_id <= zone_to_remove.Item2.Item1; ++row_id)
          for (int column_id = zone_to_remove.Item1.Item2; column_id <= zone_to_remove.Item2.Item2; ++column_id)
            if (_IsValidCell(row_id, column_id))
              m_field[row_id, column_id].Destroy();
        foreach (var element_info in removed_zone_info)
          m_game_info.UpdateScore(element_info.Key, element_info.Value, false);
        break;
      case "RemoveElementsByValue":
        var value = m_field_data[main_element_position].value;
        var removed_elements = m_field_data.RemoveSameAs(m_field_data[main_element_position]);
        foreach (var (row_id, column_id) in removed_elements)
          m_field[row_id, column_id].Destroy();
        m_game_info.UpdateScore(value, removed_elements.Count, false);
        break;
      default:
        return;
    }
  }

  public void HandleStaticAbility(string i_name, StaticAbility i_ability) {
    if (!_IsAvailable())
      return;
    m_game_info.SpentScore(i_ability.price);
    i_ability.NextPrice();
    switch (i_name) {
      case "Shuffle":
        m_field_data.Shuffle();
        for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
          for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
            m_field[row_id, column_id].Destroy();
            _InitElement(row_id, column_id);
          }
        break;
      case "UpgradeGenerator":
        var removed_elements = m_elements_spawner.Upgrade();
        foreach (var (row_id, column_id) in removed_elements)
          m_field[row_id, column_id].Destroy();
        break;
      case "Search":
        var moves = m_field_resolver.GetAllMoves();
        var move = moves[UnityEngine.Random.Range(0, moves.Count)];
        _ClearHighlighting();
        _HighlightElement(move.first);
        _HighlightElement(move.second);
        Invoke(nameof(_ClearHighlighting), 1);
        break;
      default:
        break;
    }
  }
  private void _SelectElement((int, int) i_position) {
    m_selected_elements.Add(i_position);
    m_field[i_position.Item1, i_position.Item2].UpdateSelection(true);
  }

  private void _HighlightElement((int, int) i_position) {
    m_highlighted_elements.Add(i_position);
    m_field[i_position.Item1, i_position.Item2].UpdateHighlighting(true);
  }

  private void _ClearHighlighting() {
    m_highlighted_focus_position = (-1, -1);
    foreach (var element_position in m_highlighted_elements)
      m_field[element_position.Item1, element_position.Item2].UpdateHighlighting(false);
    m_highlighted_elements.Clear();
  }

  private void _ProcessSelectedElements() {
    if (m_selected_elements.Count != 2)
      return;

    var first = m_selected_elements[0];
    var second = m_selected_elements[1];
    var distance = Mathf.Abs(first.Item1 - second.Item1) + Mathf.Abs(first.Item2 - second.Item2);
    if (distance > 1) {
      var position = m_selected_elements[0];
      m_field[position.Item1, position.Item2].UpdateSelection(false);
      m_selected_elements.RemoveAt(0);
    } else {
      foreach (var position in m_selected_elements)
        m_field[position.Item1, position.Item2].UpdateSelection(false);
      m_selected_elements.Clear();
      if (distance == 1)
        _MakeMove(first, second);
    }
  }

  private void _InitInputHandler() {
    // Fit input handler size to actual grid size
    var input_handler_rect_transform = m_input_handler.GetComponent<RectTransform>();
    var anchors = m_grid_configuration.GetAnchors();
    input_handler_rect_transform.anchorMin = anchors.Item1;
    input_handler_rect_transform.anchorMax = anchors.Item2;

    // Init input handler callbacks
    var input_handler_component = m_input_handler.GetComponent<GameFieldInputHandler>();
    input_handler_component.on_input_down = _HandlePointerDown;
    input_handler_component.on_input_up = _HandlePointerUp;
    input_handler_component.on_ability_move = _HandleAbilityMove;
    input_handler_component.on_ability_apply = _HandleAbility;
  }

  private void _InitCameraViewport() {
    Camera.main.orthographicSize = Screen.height / 2.0f + m_grid_configuration.outer_grid_stroke_width;
    Camera.main.aspect = (float)Screen.width / Screen.height;
  }

  private void _AutoMove() {
    var moves = m_field_resolver.GetAllMoves();
    if (moves.Count > 0) {
      moves.Sort((IFieldResolver.MoveDetails first, IFieldResolver.MoveDetails second) => second.strike - first.strike);
      var max_strike_value = moves[0].strike;
      int max_strike_value_count = 0;
      foreach (var move_details in moves) {
        if (move_details.strike < max_strike_value)
          break;
        ++max_strike_value_count;
      }
      var move_index = UnityEngine.Random.Range(0, max_strike_value_count);
      var move = moves[move_index];
      _MakeMove(move.first, move.second);
    }
  }

  private void _MakeMove((int, int) first, (int, int) second) {
    if (!m_field_data.IsMoveAvailable(first, second))
      return;
    m_field_data.SwapCells(first.Item1, first.Item2, second.Item1, second.Item2);
    (m_field[first.Item1, first.Item2], m_field[second.Item1, second.Item2]) =
      (m_field[second.Item1, second.Item2], m_field[first.Item1, first.Item2]);
    m_field[first.Item1, first.Item2].MoveTo(m_field[second.Item1, second.Item2].transform.position);
    m_field[second.Item1, second.Item2].MoveTo(m_field[first.Item1, first.Item2].transform.position);
    if (!m_reverse_move.HasValue)
      m_reverse_move = (first, second);
  }

  private void _InitElement(int row_id, int column_id, bool i_with_animation = true) {
    var element = m_field_data[row_id, column_id];
    m_field[row_id, column_id].Create(m_element_style_provider.Get(element), i_with_animation);
    m_field[row_id, column_id].transform.GetChild(2).GetComponent<SpriteRenderer>().size = new Vector2(m_grid_configuration.element_size, m_grid_configuration.element_size);
    m_field[row_id, column_id].transform.GetChild(3).GetComponent<SpriteRenderer>().size = new Vector2(m_grid_configuration.grid_step, m_grid_configuration.grid_step);
  }

  private void OnDestroy() {
    _Save();
  }

  private void _Save() {
    m_field_configuration.Save();
    m_field_data.Save();
    m_elements_spawner.Save();
  }

  private bool _IsAvailable() {
    for (int i = 0; i < m_field_configuration.height; ++i)
      for (int j = 0; j < m_field_configuration.width; ++j)
        if (!m_field[i, j].IsAvailable())
          return false;
    return true;
  }

  private bool _IsValidCell(int i_row_id, int i_column_id) {
    return i_row_id >= 0 && i_row_id < m_field_configuration.height && i_column_id >= 0 && i_column_id < m_field_configuration.width;
  }

  private bool _IsValidCell((int, int) i_position) {
    return _IsValidCell(i_position.Item1, i_position.Item2);
  }

  public FieldConfiguration field_configuration {
    get => m_field_configuration;
  }
}
