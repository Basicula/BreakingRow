using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour {
  [SerializeField] private FieldConfiguration m_field_configuration;
  [SerializeField] private bool m_is_auto_play;

  [SerializeReference] private GameObject m_game_element_prefab;
  [SerializeReference] private GameInfo m_game_info;
  [SerializeReference] private GameObject m_abilities;
  [SerializeReference] private GameObject m_input_handler;
  [SerializeReference] private GameObject m_background_grid;
  [SerializeReference] private GameObject m_holes_contour_overlay;
  [SerializeReference] private GameObject m_holes_fill_overlay;
  [SerializeReference] private GameObject m_holes_image;
  [SerializeReference] private GameObject m_field_image;

  private Rect m_max_active_zone_rect;
  private Vector2 m_max_active_zone_anchor_min;
  private Vector2 m_max_active_zone_anchor_max;
  private Vector2 m_max_active_zone_center;

  private FieldData m_field_data;
  private IFieldElementsSpawner m_elements_spawner;
  private IFieldElementsMover m_elements_mover;
  private IFieldResolver m_field_resolver;
  private ElementStyleProvider m_element_style_provider;

  private GameElement[,] m_field;
  private float m_grid_step;
  private float m_half_grid_step;
  private float m_element_size;
  private float m_element_offset;
  private float m_inner_grid_stroke_width;
  private float m_outer_grid_stroke_width;
  private Vector2 m_field_center;
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
    var active_zone = m_input_handler.GetComponent<RectTransform>();
    m_max_active_zone_rect = active_zone.rect;
    m_max_active_zone_anchor_min = active_zone.anchorMin;
    m_max_active_zone_anchor_max = active_zone.anchorMax;
    m_max_active_zone_center = active_zone.localPosition;

    if (!m_field_configuration.Load())
      m_field_configuration.InitCellsConfiguration();

    _Init();
  }

  void Update() {
    if (!_IsAvailable())
      return;

    bool scenario_result = m_field_configuration.fill_strategy switch {
      FieldConfiguration.FillStrategy.MoveThenSpawn => _MoveThenSpawnElements(),
      FieldConfiguration.FillStrategy.SpawnThenMove => _SpawnThenMoveElements(),
      _ => throw new System.NotImplementedException(),
    };
    if (scenario_result)
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
    if (m_field_data.HasEmptyCells()) {
      var created_elements = m_elements_spawner.SpawnElements();
      foreach (var element_position in created_elements)
        _InitElement(element_position.Item1, element_position.Item2);
      return true;
    }
    return false;
  }

  private bool _SpawnThenMoveElements() {
    if (m_field_data.HasEmptyCells()) {
      var element_move_changes = m_elements_mover.Move().moved;
      foreach (var element_move in element_move_changes) {
        var first = element_move.Item1;
        var second = element_move.Item2[^1];
        m_field[second.Item1, second.Item2].transform.position = m_field[first.Item1, first.Item2].transform.position;
        m_field[first.Item1, first.Item2].MoveTo(_GetElementPosition(second.Item1, second.Item2));
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
        var target_element_position = _GetElementPosition(row_id, column_id);
        m_field[element_position.Item1, element_position.Item2].gameObject.transform.position = target_element_position;
        m_field[element_position.Item1, element_position.Item2].MoveTo(_GetElementPosition(element_position.Item1, element_position.Item2));
      }
      return true;
    }
    return false;
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

  private void _Init() {
    m_field_data = new FieldData(m_field_configuration);
    m_elements_spawner = new SimpleCommonElementsSpawner(m_field_data);
    m_elements_spawner.InitElements();
    m_elements_mover = new ClassicMover(m_field_data);
    _InitFieldResolver();

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
    m_game_info.moves_count = m_field_resolver.GetAllMoves().Count;
    m_field_center = new Vector2(m_grid_step * m_field_configuration.width / 2 - m_max_active_zone_center.x, m_grid_step * m_field_configuration.height / 2 + m_max_active_zone_center.y);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        Vector2 position = _GetElementPosition(row_id, column_id);
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
    var element_position = _GetElementPosition(m_mouse_down_position);
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
    if (delta.magnitude / m_grid_step < 0.5)
      return;
    delta.Normalize();
    var element_position = _GetElementPosition(m_mouse_down_position + delta * m_grid_step);
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
    var main_element_position = _GetElementPosition(Camera.main.ScreenToWorldPoint(i_event_position));
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
    var main_element_position = _GetElementPosition(Camera.main.ScreenToWorldPoint(i_applied_position));
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

  private void _InitBackgroundGrid() {
    var svg = new SVG();
    var rect_size = new Vector2(m_grid_step, m_grid_step);
    var rect_color = "rgba(20, 20, 20, 0.5)";
    var rect_stroke_props = new SVGStrokeProps("#000000", m_inner_grid_stroke_width);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        svg.Add(new SVGRect(new Vector2(column_id * m_grid_step, row_id * m_grid_step), rect_size, rect_color, rect_stroke_props));

    m_background_grid.transform.localPosition = m_input_handler.transform.localPosition;
    var sprite_renderer = m_background_grid.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = SVG.BuildSprite(svg, m_grid_step);
  }

  private List<List<(int, int)>> _GetHoles() {
    var holes = m_field_data.GetHoles();
    var outer_hole = new List<(int, int)>();
    for (int row_id = -1; row_id <= m_field_configuration.height; ++row_id)
      for (int column_id = -1; column_id <= m_field_configuration.width; ++column_id) {
        if (row_id >= 0 && row_id < m_field_configuration.height &&
          column_id >= 0 && column_id < m_field_configuration.width)
          continue;
        outer_hole.Add((row_id, column_id));
      }
    for (int hole_id = 0; hole_id < holes.Count; ++hole_id) {
      bool is_outer = false;
      foreach (var (row_id, column_id) in holes[hole_id]) {
        if (row_id == 0 || row_id == m_field_configuration.height - 1 ||
          column_id == 0 || column_id == m_field_configuration.width - 1) {
          is_outer = true;
          break;
        }
      }
      if (is_outer) {
        outer_hole.AddRange(holes[hole_id].ToArray());
        holes.RemoveAt(hole_id);
        --hole_id;
      }
    }
    holes.Add(outer_hole);
    return holes;
  }

  private List<List<(int, int)>> _GetHolePaths(List<(int, int)> i_hole) {
    var hole_edges = new List<((int, int), (int, int))>();
    foreach (var (row_id, column_id) in i_hole) {
      hole_edges.Add(((row_id, column_id), (row_id, column_id + 1)));
      hole_edges.Add(((row_id, column_id + 1), (row_id + 1, column_id + 1)));
      hole_edges.Add(((row_id + 1, column_id + 1), (row_id + 1, column_id)));
      hole_edges.Add(((row_id + 1, column_id), (row_id, column_id)));
    }
    for (int i = 0; i < hole_edges.Count; ++i)
      for (int j = i + 1; j < hole_edges.Count; ++j)
        if (hole_edges[i].Item1 == hole_edges[j].Item2 && hole_edges[i].Item2 == hole_edges[j].Item1) {
          hole_edges.RemoveAt(j);
          hole_edges.RemoveAt(i);
          --i;
          break;
        }
    var paths = new List<List<(int, int)>>();
    int edge_id = hole_edges.Count;
    while (hole_edges.Count > 0) {
      if (edge_id >= hole_edges.Count) {
        paths.Add(new List<(int, int)>());
        paths[^1].Add(hole_edges[0].Item1);
        paths[^1].Add(hole_edges[0].Item2);
        hole_edges.RemoveAt(0);
        edge_id = 0;
        continue;
      }
      if (paths[^1][^1] == hole_edges[edge_id].Item1) {
        paths[^1].Add(hole_edges[edge_id].Item2);
        hole_edges.RemoveAt(edge_id);
        edge_id = 0;
      } else
        ++edge_id;
    }
    return paths;
  }

  private void _InitHoleOverlays() {
    var holes = _GetHoles();
    var stroke_svg = new SVG();
    var fill_svg = new SVG();
    var fill_color = "rgba(20, 20, 20, 0.5)";
    var offset_value = m_outer_grid_stroke_width / 2;
    var hole_stroke = new SVGStrokeProps("#000000", m_outer_grid_stroke_width);
    var no_stroke = new SVGStrokeProps("none", 0);

    foreach (var hole in holes) {
      var paths = _GetHolePaths(hole);
      var fill_path = new SVGPath {
        fill_color = fill_color,
        stroke_props = no_stroke
      };
      var stroke_path = new SVGPath {
        fill_color = "rgba(20, 20, 20, 0.25)",
        stroke_props = hole_stroke
      };
      foreach (var path_points in paths) {
        // Remove last point as it's same as first one
        path_points.RemoveAt(path_points.Count - 1);
        if (path_points[0].Item1 == -1 && path_points[0].Item2 == -1) {
          // Extend outer hole to cover all unused outer space
          path_points.Clear();
          var max_row_count = Mathf.RoundToInt(Screen.height / m_grid_step) + 1;
          var max_column_count = Mathf.RoundToInt(Screen.width / m_grid_step) + 1;
          var additional_rows = Mathf.Max(0, max_row_count - m_field_configuration.height) / 2 + 1;
          var additional_columns = Mathf.Max(0, max_column_count - m_field_configuration.width) / 2 + 1;
          path_points.Add((-additional_rows, -additional_columns));
          path_points.Add((-additional_rows, m_field_configuration.width + additional_columns));
          path_points.Add((m_field_configuration.height + additional_rows, m_field_configuration.width + additional_columns));
          path_points.Add((m_field_configuration.height + additional_rows, -additional_columns));
        }

        var offset_path_points = new List<Vector2>(path_points.Count);
        foreach (var (row_id, column_id) in path_points)
          offset_path_points.Add(new Vector2(column_id * m_grid_step, -row_id * m_grid_step));

        fill_path.MoveTo(offset_path_points[0]);
        for (int point_id = 1; point_id < offset_path_points.Count; ++point_id)
          fill_path.LineTo(offset_path_points[point_id]);
        fill_path.Close();

        var prev_offset_direction = new Vector2(0, 0);
        for (int point_id = 0; point_id <= path_points.Count; ++point_id) {
          var curr_point_id = point_id == path_points.Count ? 0 : point_id;
          var prev_point_id = point_id == 0 ? path_points.Count - 1 : point_id - 1;
          var edge_direction = new Vector2(path_points[curr_point_id].Item2, path_points[curr_point_id].Item1) -
            new Vector2(path_points[prev_point_id].Item2, path_points[prev_point_id].Item1);
          var offset_direction = new Vector2(-edge_direction.y, -edge_direction.x);
          offset_direction *= offset_value;
          if (prev_offset_direction == new Vector2(0, 0)) {
            prev_offset_direction = offset_direction;
            continue;
          }
          if (Vector2.Dot(prev_offset_direction, offset_direction) == 0.0f) {
            offset_path_points[curr_point_id] = offset_path_points[curr_point_id] + offset_direction;
            offset_path_points[point_id - 1] = offset_path_points[point_id - 1] + offset_direction;
          } else
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

    m_holes_fill_overlay.transform.localPosition = m_input_handler.transform.localPosition;
    var holes_fill_mask = m_holes_fill_overlay.GetComponent<SpriteMask>();
    holes_fill_mask.sprite = SVG.BuildSprite(fill_svg, m_grid_step);

    var background_image_size = m_holes_image.GetComponent<SpriteRenderer>().sprite.rect.size;
    var x_scale = (Screen.width + 2 * m_outer_grid_stroke_width) / background_image_size.x;
    var y_scale = (Screen.height + 2 * m_outer_grid_stroke_width) / background_image_size.x;
    m_holes_image.transform.localScale = new Vector3(x_scale, y_scale, 1);
    m_field_image.transform.localScale = new Vector3(x_scale, y_scale, 1);

    m_holes_contour_overlay.transform.localPosition = m_input_handler.transform.localPosition;
    var sprite_renderer = m_holes_contour_overlay.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = SVG.BuildSprite(stroke_svg, m_grid_step);
  }

  private void _InitInputHandler() {
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
    input_handler_component.on_input_down = _HandlePointerDown;
    input_handler_component.on_input_up = _HandlePointerUp;
    input_handler_component.on_ability_move = _HandleAbilityMove;
    input_handler_component.on_ability_apply = _HandleAbility;
  }

  private void _InitCameraViewport() {
    Camera.main.orthographicSize = Screen.height / 2.0f + m_outer_grid_stroke_width;
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

  private Vector2 _GetElementPosition(int row_id, int column_id) {
    return new Vector2(
      m_grid_step * column_id - m_field_center.x + m_half_grid_step,
      m_field_center.y - m_half_grid_step - m_grid_step * row_id
    );
  }

  private (int, int) _GetElementPosition(Vector2 position) {
    var row_id = Mathf.FloorToInt((m_field_center.y - position.y) / m_grid_step);
    var column_id = Mathf.FloorToInt((m_field_center.x + position.x) / m_grid_step);
    return (row_id, column_id);
  }

  private void _InitElement(int row_id, int column_id, bool i_with_animation = true) {
    var element = m_field_data[row_id, column_id];
    m_field[row_id, column_id].Create(m_element_style_provider.Get(element), i_with_animation);
    m_field[row_id, column_id].transform.GetChild(2).GetComponent<SpriteRenderer>().size = new Vector2(m_element_size, m_element_size);
    m_field[row_id, column_id].transform.GetChild(3).GetComponent<SpriteRenderer>().size = new Vector2(m_grid_step, m_grid_step);
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
