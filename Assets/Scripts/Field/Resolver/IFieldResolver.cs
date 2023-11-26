using System.Collections.Generic;

public abstract class IFieldResolver {
  protected FieldData m_field_data;

  public class FieldChanges {
    public List<(int, List<(int, int)>)> combined;
    public List<(int, int)> destroyed;
    public List<(int, int)> created;
    public List<(int, int)> updated;

    public FieldChanges() {
      combined = new();
      destroyed = new();
      created = new();
      updated = new();
    }
  }

  protected IFieldResolver(FieldData i_field_data) {
    m_field_data = i_field_data;
  }

  public FieldChanges Process() {
    var field_changes = _Process();
    if (field_changes.combined != null)
      _ProcessDestructibleElements(field_changes);
    return field_changes;
  }

  public struct MoveDetails {
    public (int, int) first;
    public (int, int) second;
    public int strike;
    public MoveDetails((int, int) i_first, (int, int) i_second, int i_strike) {
      first = i_first;
      second = i_second;
      strike = i_strike;
    }
  }

  public List<MoveDetails> GetAllMoves() {
    var field_configuration = m_field_data.configuration;
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
    for (int row_id = 0; row_id < field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < field_configuration.width; ++column_id)
        foreach (var neighbor in neighbors) {
          var neighbor_row_id = row_id + neighbor.Item1;
          var neighbor_column_id = column_id + neighbor.Item2;
          if (neighbor_row_id < 0 || neighbor_row_id >= field_configuration.height ||
              neighbor_column_id < 0 || neighbor_column_id >= field_configuration.width)
            continue;
          var new_move = new MoveDetails((row_id, column_id), (neighbor_row_id, neighbor_column_id), 0);
          if (!m_field_data.IsMoveAvailable(new_move.first, new_move.second))
            continue;
          m_field_data.SwapCells(row_id, column_id, neighbor_row_id, neighbor_column_id);
          var first_cross_group = _GroupAt(row_id, column_id);
          var second_cross_group = _GroupAt(neighbor_row_id, neighbor_column_id);
          if (first_cross_group.Count > 0 || second_cross_group.Count > 0) {
            new_move.strike = System.Math.Max(first_cross_group.Count, second_cross_group.Count);
            if (!is_move_exists(new_move))
              moves_data.Add(new_move);
          }
          m_field_data.SwapCells(row_id, column_id, neighbor_row_id, neighbor_column_id);
        }
    return moves_data;
  }

  protected abstract FieldChanges _Process();

  protected List<List<(int, int)>> _GetGroups() {
    var field_configuration = m_field_data.configuration;
    var taken = new bool[field_configuration.height, field_configuration.width];
    Utilities.InitArray(taken, false);
    var groups = new List<List<(int, int)>>();
    for (int row_id = 0; row_id < field_configuration.height; ++row_id) {
      for (int column_id = 0; column_id < field_configuration.width; ++column_id) {
        if (taken[row_id, column_id])
          continue;
        var group = _GroupAt(row_id, column_id);
        if (group.Count == 0)
          continue;
        foreach (var element in group)
          taken[element.Item1, element.Item2] = true;
        groups.Add(group);
      }
    }
    return groups;
  }

  // Virtual because way of group detection may be different
  // Here common way of group detection is proposed
  protected virtual List<(int, int)> _GroupAt(int row_id, int column_id) {
    var field_configuration = m_field_data.configuration;
    var target_element = m_field_data[row_id, column_id];

    (int, int) get_coordinates(int row_id, int column_id, int range_index, bool check_row) {
      if (check_row)
        return (row_id, range_index);
      return (range_index, column_id);
    }
    (int, int) check_line(int row_id, int column_id, bool check_row) {
      var r = check_row ? column_id + 1 : row_id + 1;
      var l = check_row ? column_id - 1 : row_id - 1;
      var max_r = check_row ? field_configuration.width : field_configuration.height;
      while (true) {
        var right_coordinates = get_coordinates(row_id, column_id, r, check_row);
        if (r < max_r && m_field_data[right_coordinates] == target_element) {
          ++r;
          continue;
        }
        var left_coordinates = get_coordinates(row_id, column_id, l, check_row);
        if (l >= 0 && m_field_data[left_coordinates] == target_element) {
          --l;
          continue;
        }
        break;
      }
      return (r, l);
    }

    bool[,] taken = new bool[field_configuration.height, field_configuration.width];
    Utilities.InitArray(taken, false);
    var to_check = new Queue<(int, int, bool)>();
    to_check.Enqueue((row_id, column_id, true));
    to_check.Enqueue((row_id, column_id, false));
    var group = new List<(int, int)>();
    while (to_check.Count > 0) {
      var current_element = to_check.Dequeue();
      if (!m_field_data[current_element.Item1, current_element.Item2].combinable)
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

  private void _ProcessDestructibleElements(FieldChanges i_field_changes) {
    var affected_destractable_elements = new HashSet<(int, int)>();
    var neighbor_offsets = new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) };
    foreach (var (group_value, group) in i_field_changes.combined)
      foreach (var element_position in group)
        foreach (var neighbor_offset in neighbor_offsets) {
          var neighbor_position = (element_position.Item1 + neighbor_offset.Item1, element_position.Item2 + neighbor_offset.Item2);
          if (!m_field_data.IsValidElementPosition(neighbor_position))
            continue;
          if (!m_field_data[neighbor_position].destructible)
            continue;
          affected_destractable_elements.Add(neighbor_position);
        }
    foreach (var element_position in affected_destractable_elements) {
      int old_value = m_field_data[element_position].value;
      int value = old_value - 1;
      if (value < 0) {
        m_field_data[element_position] = FieldElementsFactory.empty_element;
        i_field_changes.destroyed.Add(element_position);
      } else {
        m_field_data[element_position].value = value;
        i_field_changes.updated.Add(element_position);
      }
    }
  }
}