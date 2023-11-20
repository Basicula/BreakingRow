using System;
using System.Collections.Generic;

public class FieldData {
  private readonly FieldConfiguration m_field_configuration;
  private FieldElement[,] m_field;

  private List<(int, int)> m_should_stay_empty;

  private readonly string m_save_file_path;

  public FieldData(FieldConfiguration i_field_configuration) {
    m_field_configuration = i_field_configuration;
    m_save_file_path = Utilities.GetSavePath("FieldData");
    _Init();
    _Load();
  }

  public FieldElement this[int i_row_id, int i_column_id] {
    get => m_field[i_row_id, i_column_id];
    set => m_field[i_row_id, i_column_id] = value;
  }

  public FieldElement this[(int, int) i_cell_id] {
    get => m_field[i_cell_id.Item1, i_cell_id.Item2];
    set => m_field[i_cell_id.Item1, i_cell_id.Item2] = value;
  }

  public FieldConfiguration configuration { get => m_field_configuration; }

  public int RemoveValue(int value) {
    int count = 0;
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        if (m_field[row_id, column_id].value == value) {
          ++count;
          m_field[row_id, column_id] = FieldElementsFactory.empty_element;
        }
    return count;
  }

  public Dictionary<int, int> RemoveZone(int row1, int column1, int row2, int column2) {
    row1 = Math.Min(Math.Max(row1, 0), m_field_configuration.height - 1);
    row2 = Math.Min(Math.Max(row2, 0), m_field_configuration.height - 1);
    column1 = Math.Min(Math.Max(column1, 0), m_field_configuration.width - 1);
    column2 = Math.Min(Math.Max(column2, 0), m_field_configuration.width - 1);
    var removed_values = new Dictionary<int, int>();
    for (int row_id = row1; row_id <= row2; ++row_id)
      for (int column_id = column1; column_id <= column2; ++column_id) {
        int value = m_field[row_id, column_id].value;
        if (removed_values.ContainsKey(value))
          ++removed_values[value];
        else
          removed_values[value] = 1;
        m_field[row_id, column_id] = FieldElementsFactory.empty_element;
      }
    return removed_values;
  }

  public List<List<(int, int)>> GetHoles() {
    var hole_groups = new List<List<(int, int)>>();
    var visited = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(visited, false);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        if (m_field[row_id, column_id] != FieldElementsFactory.hole_element || visited[row_id, column_id])
          continue;
        var group = new List<(int, int)>();
        var to_check = new Queue<(int, int)>();
        to_check.Enqueue((row_id, column_id));
        while (to_check.Count > 0) {
          var (row, column) = to_check.Dequeue();
          if (row < 0 || column < 0 || row >= m_field_configuration.height || column >= m_field_configuration.width)
            continue;
          if (m_field[row, column] != FieldElementsFactory.hole_element || visited[row, column])
            continue;
          visited[row, column] = true;
          to_check.Enqueue((row - 1, column));
          to_check.Enqueue((row + 1, column));
          to_check.Enqueue((row, column - 1));
          to_check.Enqueue((row, column + 1));
          group.Add((row, column));
        }
        hole_groups.Add(group);
      }
    return hole_groups;
  }

  public bool HasEmptyCells() {
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        if (m_field[row_id, column_id] == FieldElementsFactory.empty_element && !m_should_stay_empty.Contains((row_id, column_id)))
          return true;
    return false;
  }

  public bool IsMoveAvailable((int, int) i_from, (int, int) i_to) {
    FieldElement from = m_field[i_from.Item1, i_from.Item2];
    FieldElement to = m_field[i_to.Item1, i_to.Item2];
    return from.interactable && to.interactable;
  }

  public List<((int, int), (int, int))> MoveElements() {
    var values = m_field.Clone() as FieldElement[,];
    var changes = new List<((int, int), (int, int))>();

    m_should_stay_empty.Clear();
    var empty_element = (-1, -1);
    var it = new FieldDataIterator(m_field_configuration.move_direction, m_field_configuration.height, m_field_configuration.width);
    while (!it.Finished()) {
      if (!it.IsValid()) {
        it.Validate();
        empty_element = (-1, -1);
      }
      var curr_element = it.current;
      if (values[curr_element.Item1, curr_element.Item2] == FieldElementsFactory.empty_element) {
        if (empty_element == (-1, -1))
          empty_element = curr_element;
      } else if (values[curr_element.Item1, curr_element.Item2].movable && empty_element != (-1, -1)) {
        (values[curr_element.Item1, curr_element.Item2], values[empty_element.Item1, empty_element.Item2]) =
          (values[empty_element.Item1, empty_element.Item2], values[curr_element.Item1, curr_element.Item2]);
        changes.Add((curr_element, empty_element));
        it.current = empty_element;
        empty_element = (-1, -1);
      } else if (!values[curr_element.Item1, curr_element.Item2].movable && values[curr_element.Item1, curr_element.Item2] != FieldElementsFactory.hole_element) {
        if (empty_element != (-1, -1)) {
          var stay_empty_element = empty_element;
          while (stay_empty_element != curr_element) {
            m_should_stay_empty.Add(stay_empty_element);
            stay_empty_element.Item1 += it.direction.Item1;
            stay_empty_element.Item2 += it.direction.Item2;
          }
        }
        empty_element = (-1, -1);
      }
      it.Increment(false);
    }
    foreach (var change in changes)
      SwapCells(change.Item1.Item1, change.Item1.Item2, change.Item2.Item1, change.Item2.Item2);
    return changes;
  }

  public (int, int) GetMoveDirection() {
    switch (m_field_configuration.move_direction) {
      case FieldConfiguration.MoveDirection.TopToBottom:
        return (-1, 0);
      case FieldConfiguration.MoveDirection.RightToLeft:
        return (0, 1);
      case FieldConfiguration.MoveDirection.BottomToTop:
        return (1, 0);
      case FieldConfiguration.MoveDirection.LeftToRight:
        return (0, -1);
      default:
        throw new NotImplementedException();
    }
  }

  public void SwapCells(int row1, int column1, int row2, int column2) {
    if (!_IsValidElementPosition(row1, column1) || !_IsValidElementPosition(row2, column2))
      return;

    (m_field[row1, column1], m_field[row2, column2]) =
      (m_field[row2, column2], m_field[row1, column1]);
  }

  private bool _IsValidElementPosition(int i_row, int i_column) {
    return i_row >= 0 && i_column >= 0 &&
      i_row < m_field_configuration.height && i_column < m_field_configuration.width;
  }

  public struct MoveDetails {
    public (int, int) first;
    public (int, int) second;
    public int strike;
    public MoveDetails((int, int) first, (int, int) second, int strike) {
      this.first = first;
      this.second = second;
      this.strike = strike;
    }
  }

  public List<MoveDetails> GetAllMoves() {
    var moves_data = new List<MoveDetails>();

    bool is_move_exists(MoveDetails new_move) {
      foreach (var move_data in moves_data) {
        if (move_data.first.Item1 == new_move.first.Item1 && move_data.first.Item2 == new_move.first.Item2 &&
          move_data.second.Item1 == new_move.second.Item1 && move_data.second.Item2 == new_move.second.Item2 ||
          move_data.first.Item1 == new_move.second.Item1 && move_data.first.Item2 == new_move.second.Item2 &&
          move_data.second.Item1 == new_move.first.Item1 && move_data.second.Item2 == new_move.first.Item2)
          return true;
      }
      return false;
    }

    var neighbors = new (int, int)[4] { (0, 1), (0, -1), (1, 0), (-1, 0) };
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        foreach (var neighbor in neighbors) {
          var neighbor_row_id = row_id + neighbor.Item1;
          var neighbor_column_id = column_id + neighbor.Item2;
          if (neighbor_row_id < 0 || neighbor_row_id >= m_field_configuration.height ||
              neighbor_column_id < 0 || neighbor_column_id >= m_field_configuration.width)
            continue;
          var new_move = new MoveDetails((row_id, column_id), (neighbor_row_id, neighbor_column_id), 0);
          if (!IsMoveAvailable(new_move.first, new_move.second))
            continue;
          SwapCells(row_id, column_id, neighbor_row_id, neighbor_column_id);
          var first_cross_group = _CrossGroupAt(row_id, column_id);
          var second_cross_group = _CrossGroupAt(neighbor_row_id, neighbor_column_id);
          if (first_cross_group.Count > 0 || second_cross_group.Count > 0) {
            new_move.strike = Math.Max(first_cross_group.Count, second_cross_group.Count);
            if (!is_move_exists(new_move))
              moves_data.Add(new_move);
          }
          SwapCells(row_id, column_id, neighbor_row_id, neighbor_column_id);
        }
    return moves_data;
  }

  public void Shuffle() {
    do {
      for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
          var other_row_id = UnityEngine.Random.Range(0, m_field_configuration.height);
          var other_column_id = UnityEngine.Random.Range(0, m_field_configuration.width);
          SwapCells(row_id, column_id, other_row_id, other_column_id);
        }
    } while (GetAllMoves().Count == 0);
  }

  private static void _InitArray<T>(T[,] array, T default_value) {
    for (int i = 0; i < array.GetLength(0); ++i)
      for (int j = 0; j < array.GetLength(1); ++j)
        array[i, j] = default_value;
  }

  private List<(int, int)> _CrossGroupAt(int row_id, int column_id) {
    var target_element = m_field[row_id, column_id];

    (int, int) get_coordinates(int row_id, int column_id, int range_index, bool check_row) {
      if (check_row)
        return (row_id, range_index);
      return (range_index, column_id);
    }
    (int, int) check_line(int row_id, int column_id, bool check_row) {
      var r = check_row ? column_id + 1 : row_id + 1;
      var l = check_row ? column_id - 1 : row_id - 1;
      var max_r = check_row ? m_field_configuration.width : m_field_configuration.height;
      while (true) {
        var right_coordinates = get_coordinates(row_id, column_id, r, check_row);
        if (r < max_r && this[right_coordinates] == target_element) {
          ++r;
          continue;
        }
        var left_coordinates = get_coordinates(row_id, column_id, l, check_row);
        if (l >= 0 && this[left_coordinates] == target_element) {
          --l;
          continue;
        }
        break;
      }
      return (r, l);
    }

    bool[,] taken = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(taken, false);
    var to_check = new Queue<(int, int, bool)>();
    to_check.Enqueue((row_id, column_id, true));
    to_check.Enqueue((row_id, column_id, false));
    var group = new List<(int, int)>();
    while (to_check.Count > 0) {
      var current_element = to_check.Dequeue();
      if (!m_field[current_element.Item1, current_element.Item2].combinable)
        continue;
      (int line_r, int line_l) = check_line(current_element.Item1, current_element.Item2, current_element.Item3);
      if (line_r - line_l > 3) {
        for (int i = line_l + 1; i < line_r; ++i) {
          var coordinates = get_coordinates(current_element.Item1, current_element.Item2, i, current_element.Item3);
          if (taken[coordinates.Item1, coordinates.Item2])
            continue;
          group.Add(coordinates);
          taken[coordinates.Item1, coordinates.Item2] = true;
          to_check.Enqueue((coordinates.Item1, coordinates.Item2, !current_element.Item3));
        }
      }
    }
    return group;
  }

  private List<List<(int, int)>> _GetCrossGroups() {
    var taken = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(taken, false);
    var groups = new List<List<(int, int)>>();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id) {
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        if (taken[row_id, column_id])
          continue;
        var group = _CrossGroupAt(row_id, column_id);
        if (group.Count == 0)
          continue;
        foreach (var element in group)
          taken[element.Item1, element.Item2] = true;
        groups.Add(group);
      }
    }
    return groups;
  }

  public struct GroupDetails {
    public List<(int, int)> group;
    public int value;

    public GroupDetails(List<(int, int)> group, int value) {
      this.group = group;
      this.value = value;
    }
  }

  private List<GroupDetails> _RemoveGroups(int count = -1) {
    var groups = _GetCrossGroups();
    if (count == -1)
      count = groups.Count;
    count = Math.Min(count, groups.Count);
    var group_details = new List<GroupDetails>();
    if (count == 0)
      return group_details;
    for (int group_id = 0; group_id < count; ++group_id) {
      var group = groups[group_id];
      group_details.Add(new GroupDetails(group, m_field[group[0].Item1, group[0].Item2].value));
      foreach (var element in group)
        m_field[element.Item1, element.Item2] = FieldElementsFactory.empty_element;
    }
    return group_details;
  }

  private List<GroupDetails> _AccumulateGroups() {
    var groups = _GetCrossGroups();
    var group_details = new List<GroupDetails>();
    for (int group_id = 0; group_id < groups.Count; ++group_id) {
      var group = groups[group_id];
      var value = m_field[group[0].Item1, group[0].Item2].value;
      group_details.Add(new GroupDetails(group, value));
      var accumulated_value = (int)Math.Pow(2, value) * group.Count;
      var values = new Queue<int>();
      var pow = 0;
      while (accumulated_value > 0) {
        if (accumulated_value % 2 == 1)
          values.Enqueue(pow);
        accumulated_value /= 2;
        ++pow;
      }
      for (int i = 0; i < group.Count; ++i) {
        var j = UnityEngine.Random.Range(0, group.Count);
        (group[i], group[j]) = (group[j], group[i]);
      }
      foreach (var element in group) {
        var new_value = FieldElementsFactory.undefined_value;
        if (values.Count > 0)
          new_value = values.Dequeue();
        if (new_value == FieldElementsFactory.undefined_value)
          m_field[element.Item1, element.Item2] = FieldElementsFactory.empty_element;
        else
          m_field[element.Item1, element.Item2].value = new_value;
      }
    }
    return group_details;
  }

  public List<GroupDetails> ProcessGroups() {
    List<GroupDetails> group_details;
    switch (m_field_configuration.mode) {
      case FieldConfiguration.Mode.Classic:
        group_details = _RemoveGroups();
        break;
      case FieldConfiguration.Mode.Accumulated:
        group_details = _AccumulateGroups();
        break;
      default:
        throw new NotImplementedException();
    }
    _ProcessDestructibleElements(group_details);
    return group_details;
  }

  private void _ProcessDestructibleElements(List<GroupDetails> i_nearby_combinations) {
    var affected_destractable_elements = new HashSet<(int, int)>();
    var neighbor_offsets = new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) };
    foreach (var group in i_nearby_combinations)
      foreach (var element_position in group.group)
        foreach (var neighbor_offset in neighbor_offsets) {
          var neighbor_position = (element_position.Item1 + neighbor_offset.Item1, element_position.Item2 + neighbor_offset.Item2);
          if (!_IsValidElementPosition(neighbor_position.Item1, neighbor_position.Item2))
            continue;
          if (!this[neighbor_position].destructible)
            continue;
          affected_destractable_elements.Add(neighbor_position);
        }
    foreach (var (row, column) in affected_destractable_elements) {
      int old_value = m_field[row, column].value;
      int value = old_value - 1;
      if (value < 0)
        m_field[row, column] = FieldElementsFactory.empty_element;
      else
        m_field[row, column].value = value;
    }
  }

  private void _Init() {
    m_should_stay_empty = new List<(int, int)>();
    m_field = new FieldElement[m_field_configuration.height, m_field_configuration.width];
  }

  public void Reset() {
    System.IO.File.Delete(m_save_file_path);
    _Init();
  }

  private struct SerializableData {
    public string[] field;
  }

  private bool _Load() {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return false;
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        m_field[row_id, column_id] = FieldElement.FromString(data.field[row_id * m_field_configuration.width + column_id]);
    return true;
  }

  public void Save() {
    var data = new SerializableData {
      field = new string[m_field_configuration.width * m_field_configuration.height]
    };
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        int flat_id = row_id * m_field_configuration.width + column_id;
        data.field[flat_id] = m_field[row_id, column_id].ToString();
      }
    SaveLoad.Save(data, m_save_file_path);
  }
}