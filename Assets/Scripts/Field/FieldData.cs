using System;
using System.Collections.Generic;

public class FieldData {
  private readonly FieldConfiguration m_field_configuration;
  private FieldElement[,] m_field;

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

  public List<(int, int)> RemoveSameAs(FieldElement i_reference_element) {
    var removed = new List<(int, int)>();
    var it = new FieldDataIterator(m_field_configuration.move_direction, m_field_configuration.height, m_field_configuration.width);
    while (!it.Finished()) {
      if (this[it.current] == i_reference_element) {
        this[it.current] = FieldElementsFactory.empty_element;
        removed.Add(it.current);
      }
      it.Increment(true);
    }
    return removed;
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
    Utilities.InitArray(visited, false);
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
        if (m_field[row_id, column_id] == FieldElementsFactory.empty_element)
          return true;
    return false;
  }

  public bool IsMoveAvailable((int, int) i_from, (int, int) i_to) {
    FieldElement from = m_field[i_from.Item1, i_from.Item2];
    FieldElement to = m_field[i_to.Item1, i_to.Item2];
    return from.interactable && to.interactable;
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
    if (!IsValidElementPosition(row1, column1) || !IsValidElementPosition(row2, column2))
      return;

    (m_field[row1, column1], m_field[row2, column2]) =
      (m_field[row2, column2], m_field[row1, column1]);
  }

  public void SwapCells((int, int) i_first_cell, (int, int) i_second_cell) {
    SwapCells(i_first_cell.Item1, i_first_cell.Item2, i_second_cell.Item1, i_second_cell.Item2);
  }

  public bool IsValidElementPosition(int i_row, int i_column) {
    return i_row >= 0 && i_column >= 0 &&
      i_row < m_field_configuration.height && i_column < m_field_configuration.width;
  }

  public bool IsValidElementPosition((int, int) i_position) {
    return IsValidElementPosition(i_position.Item1, i_position.Item2);
  }

  public void Shuffle() {
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id) {
        var other_row_id = UnityEngine.Random.Range(0, m_field_configuration.height);
        var other_column_id = UnityEngine.Random.Range(0, m_field_configuration.width);
        SwapCells(row_id, column_id, other_row_id, other_column_id);
      }
  }

  private void _Init() {
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