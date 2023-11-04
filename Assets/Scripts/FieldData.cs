using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldData
{
  private FieldConfiguration m_field_configuration;
  private FieldElement[,] m_field;

  private int[] m_values_interval;
  private float[] m_values_probability_interval;

  private string m_save_file_path;

  private static readonly FieldElement m_hole_element = FieldElementsFactory.CreateElement(FieldElement.Type.Hole);
  private static readonly FieldElement m_empty_element = FieldElementsFactory.CreateElement(FieldElement.Type.Empty);

  public FieldData(FieldConfiguration i_field_configuration, string i_custom_identificator = "")
  {
    m_field_configuration = i_field_configuration;
    if (i_custom_identificator.Length == 0)
      m_save_file_path = $"{Application.persistentDataPath}/{m_field_configuration.mode}FieldData({m_field_configuration.width}, {m_field_configuration.height}).json";
    else
      m_save_file_path = $"{Application.persistentDataPath}/{i_custom_identificator}FieldData.json";
    if (!_Load())
      _Init();

    //for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
    //  for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
    //    m_field[row_id, column_id] = FieldElementsFactory.CreateCommonElement(row_id * m_field_configuration.width + column_id);
  }

  public FieldElement At(int row_id, int column_id)
  {
    return m_field[row_id, column_id];
  }

  public FieldConfiguration field_configuration
  {
    get => m_field_configuration;
    set
    {
      var old = m_field_configuration;
      m_field_configuration = value;
      if (m_field_configuration.height != old.height ||
        m_field_configuration.width != old.width ||
        m_field_configuration.active_elements_count != old.active_elements_count)
        _Init();
    }
  }

  public int[] values_interval
  {
    get => m_values_interval;
  }

  public void IncreaseValuesInterval()
  {
    for (int i = 0; i < m_values_interval.Length; ++i)
      ++m_values_interval[i];
  }

  public int RemoveValue(int value)
  {
    int count = 0;
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        if (m_field[row_id, column_id].value == value)
        {
          ++count;
          m_field[row_id, column_id] = m_empty_element;
        }
    return count;
  }

  public Dictionary<int, int> RemoveZone(int row1, int column1, int row2, int column2)
  {
    row1 = Math.Min(Math.Max(row1, 0), m_field_configuration.height - 1);
    row2 = Math.Min(Math.Max(row2, 0), m_field_configuration.height - 1);
    column1 = Math.Min(Math.Max(column1, 0), m_field_configuration.width - 1);
    column2 = Math.Min(Math.Max(column2, 0), m_field_configuration.width - 1);
    var removed_values = new Dictionary<int, int>();
    for (int row_id = row1; row_id <= row2; ++row_id)
      for (int column_id = column1; column_id <= column2; ++column_id)
      {
        int value = m_field[row_id, column_id].value;
        if (removed_values.ContainsKey(value))
          ++removed_values[value];
        else
          removed_values[value] = 1;
        m_field[row_id, column_id] = m_empty_element;
      }
    return removed_values;
  }

  public struct GroupDetails
  {
    public List<(int, int)> group;
    public int value;

    public GroupDetails(List<(int, int)> group, int value)
    {
      this.group = group;
      this.value = value;
    }
  }

  public List<List<(int, int)>> GetHoles()
  {
    var hole_groups = new List<List<(int, int)>>();
    var visited = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(visited, false);
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        if (m_field[row_id, column_id] != m_hole_element || visited[row_id, column_id])
          continue;
        var group = new List<(int, int)>();
        var to_check = new Queue<(int, int)>();
        to_check.Enqueue((row_id, column_id));
        while (to_check.Count > 0)
        {
          var (row, column) = to_check.Dequeue();
          if (row < 0 || column < 0 || row >= m_field_configuration.height || column >= m_field_configuration.width)
            continue;
          if (m_field[row, column] != m_hole_element || visited[row, column])
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

  public bool HasGroups()
  {
    return _GetCrossGroups().Count > 0;
  }

  public bool HasEmptyCells()
  {
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        if (m_field[row_id, column_id] == m_empty_element)
          return true;
    return false;
  }

  public bool IsMoveAvailable((int, int) i_from, (int, int) i_to)
  {
    FieldElement from = m_field[i_from.Item1, i_from.Item2];
    FieldElement to = m_field[i_to.Item1, i_to.Item2];
    return from.interactable && to.interactable;
  }

  public void MoveElements()
  {
    var changes = ElementsMoveChanges();
    foreach (var change in changes)
      SwapCells(change.Item1.Item1, change.Item1.Item2, change.Item2.Item1, change.Item2.Item2);
  }

  public (int, int) GetMoveDirection()
  {
    switch (m_field_configuration.move_direction)
    {
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

  public List<((int, int), (int, int))> ElementsMoveChanges()
  {
    var values = m_field.Clone() as FieldElement[,];
    var changes = new List<((int, int), (int, int))>();

    var start_element = (0, 0);
    var end_element = (m_field_configuration.height, m_field_configuration.width);
    var direction = GetMoveDirection();
    switch (m_field_configuration.move_direction)
    {
      case FieldConfiguration.MoveDirection.TopToBottom:
        start_element = (m_field_configuration.height - 1, 0);
        end_element = (-1, m_field_configuration.width - 1);
        break;
      case FieldConfiguration.MoveDirection.RightToLeft:
        start_element = (0, 0);
        end_element = (m_field_configuration.height - 1, m_field_configuration.width);
        break;
      case FieldConfiguration.MoveDirection.BottomToTop:
        start_element = (0, m_field_configuration.width - 1);
        end_element = (m_field_configuration.height, 0);
        break;
      case FieldConfiguration.MoveDirection.LeftToRight:
        start_element = (m_field_configuration.height - 1, m_field_configuration.width - 1);
        end_element = (0, -1);
        break;
      default:
        throw new NotImplementedException();
    }

    var curr_element = start_element;
    var empty_element = (-1, -1);
    while (curr_element != end_element)
    {
      if (curr_element.Item1 < 0)
      {
        curr_element.Item1 = start_element.Item1;
        ++curr_element.Item2;
        empty_element = (-1, -1);
      }
      if (curr_element.Item1 >= m_field_configuration.height)
      {
        curr_element.Item1 = start_element.Item1;
        --curr_element.Item2;
        empty_element = (-1, -1);
      }
      if (curr_element.Item2 < 0)
      {
        curr_element.Item2 = start_element.Item2;
        --curr_element.Item1;
        empty_element = (-1, -1);
      }
      if (curr_element.Item2 >= m_field_configuration.width)
      {
        curr_element.Item2 = start_element.Item2;
        ++curr_element.Item1;
        empty_element = (-1, -1);
      }

      if (values[curr_element.Item1, curr_element.Item2] == m_empty_element && empty_element == (-1, -1))
        empty_element = curr_element;
      else if (values[curr_element.Item1, curr_element.Item2].movable && empty_element != (-1, -1))
      {
        (values[curr_element.Item1, curr_element.Item2], values[empty_element.Item1, empty_element.Item2]) =
          (values[empty_element.Item1, empty_element.Item2], values[curr_element.Item1, curr_element.Item2]);
        changes.Add((curr_element, empty_element));
        curr_element = empty_element;
        empty_element = (-1, -1);
      }

      curr_element.Item1 += direction.Item1;
      curr_element.Item2 += direction.Item2;
    }
    return changes;
  }

  public List<(int, int)> SpawnNewValues()
  {
    var created = new List<(int, int)>();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        if (m_field[row_id, column_id] == m_empty_element)
        {
          m_field[row_id, column_id] = FieldElementsFactory.CreateElement(FieldElement.Type.Common, _GetRandomValue());
          created.Add((row_id, column_id));
        }
    return created;
  }

  public void SwapCells(int row1, int column1, int row2, int column2)
  {
    if (row1 < 0 || row1 >= m_field_configuration.height || column1 < 0 || column1 >= m_field_configuration.width ||
      row2 < 0 || row2 >= m_field_configuration.height || column2 < 0 || column2 >= m_field_configuration.width)
      return;

    (m_field[row1, column1], m_field[row2, column2]) =
      (m_field[row2, column2], m_field[row1, column1]);
  }

  public struct MoveDetails
  {
    public (int, int) first;
    public (int, int) second;
    public int strike;
    public MoveDetails((int, int) first, (int, int) second, int strike)
    {
      this.first = first;
      this.second = second;
      this.strike = strike;
    }
  }

  public List<MoveDetails> GetAllMoves()
  {
    var neighbors = new (int, int)[4] { (0, 1), (0, -1), (1, 0), (-1, 0) };
    var moves_data = new List<MoveDetails>();
    Func<MoveDetails, bool> is_move_exists = (MoveDetails new_move) =>
    {
      foreach (var move_data in moves_data)
      {
        if (move_data.first.Item1 == new_move.first.Item1 && move_data.first.Item2 == new_move.first.Item2 &&
          move_data.second.Item1 == new_move.second.Item1 && move_data.second.Item2 == new_move.second.Item2 ||
          move_data.first.Item1 == new_move.second.Item1 && move_data.first.Item2 == new_move.second.Item2 &&
          move_data.second.Item1 == new_move.first.Item1 && move_data.second.Item2 == new_move.first.Item2)
          return true;
      }
      return false;
    };
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        foreach (var neighbor in neighbors)
        {
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
          if (first_cross_group.Count > 0 || second_cross_group.Count > 0)
          {
            new_move.strike = Math.Max(first_cross_group.Count, second_cross_group.Count);
            if (!is_move_exists(new_move))
              moves_data.Add(new_move);
          }
          SwapCells(row_id, column_id, neighbor_row_id, neighbor_column_id);
        }
    return moves_data;
  }

  public int CheckMove((int, int) first, (int, int) second)
  {
    SwapCells(first.Item1, first.Item2, second.Item1, second.Item2);
    var first_cross_group = _CrossGroupAt(first.Item1, first.Item2);
    var second_cross_group = _CrossGroupAt(second.Item1, second.Item2);
    SwapCells(first.Item1, first.Item2, second.Item1, second.Item2);
    return Convert.ToInt32(first_cross_group.Count > 0) + Convert.ToInt32(second_cross_group.Count > 0);
  }

  public void Shuffle()
  {
    do
    {
      for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        {
          var other_row_id = UnityEngine.Random.Range(0, m_field_configuration.height);
          var other_column_id = UnityEngine.Random.Range(0, m_field_configuration.width);
          SwapCells(row_id, column_id, other_row_id, other_column_id);
        }
    } while (GetAllMoves().Count == 0);
  }

  private int _GetRandomValue()
  {
    float random = UnityEngine.Random.Range(0.0f, 1.0f);
    float accumulated_probability = 0.0f;
    for (int i = 0; i < m_values_interval.Length; ++i)
    {
      if (random <= accumulated_probability)
        return m_values_interval[i - 1];
      accumulated_probability += m_values_probability_interval[i];
    }
    return m_values_interval[m_values_interval.Length - 1];
  }

  private static void _InitArray<T>(T[,] array, T default_value)
  {
    for (int i = 0; i < array.GetLength(0); ++i)
      for (int j = 0; j < array.GetLength(1); ++j)
        array[i, j] = default_value;
  }

  private static void _InitArray<T>(T[,] array, Func<T> default_value_generator)
  {
    for (int i = 0; i < array.GetLength(0); ++i)
      for (int j = 0; j < array.GetLength(1); ++j)
        array[i, j] = default_value_generator();
  }

  private List<(int, int)> _CrossGroupAt(int row_id, int column_id)
  {
    int target_value = m_field[row_id, column_id].value;

    Func<int, int, int, bool, (int, int)> get_coordinates = (int row_id, int column_id, int range_index, bool check_row) =>
    {
      if (check_row)
        return (row_id, range_index);
      return (range_index, column_id);
    };
    Func<int, int, bool, (int, int)> check_line = (row_id, column_id, check_row) =>
    {
      var r = check_row ? column_id + 1 : row_id + 1;
      var l = check_row ? column_id - 1 : row_id - 1;
      var max_r = check_row ? m_field_configuration.width : m_field_configuration.height;
      while (true)
      {
        var right_coordinates = get_coordinates(row_id, column_id, r, check_row);
        if (r < max_r && At(right_coordinates.Item1, right_coordinates.Item2).value == target_value)
        {
          ++r;
          continue;
        }
        var left_coordinates = get_coordinates(row_id, column_id, l, check_row);
        if (l >= 0 && At(left_coordinates.Item1, left_coordinates.Item2).value == target_value)
        {
          --l;
          continue;
        }
        break;
      }
      return (r, l);
    };

    bool[,] taken = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(taken, false);
    Queue<(int, int, bool)> to_check = new Queue<(int, int, bool)>();
    to_check.Enqueue((row_id, column_id, true));
    to_check.Enqueue((row_id, column_id, false));
    List<(int, int)> group = new List<(int, int)>();
    while (to_check.Count > 0)
    {
      var current_element = to_check.Dequeue();
      if (m_field[current_element.Item1, current_element.Item2].value < 0)
        continue;
      (int line_r, int line_l) = check_line(current_element.Item1, current_element.Item2, current_element.Item3);
      if (line_r - line_l > 3)
      {
        for (int i = line_l + 1; i < line_r; ++i)
        {
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

  private List<List<(int, int)>> _GetCrossGroups()
  {
    var taken = new bool[m_field_configuration.height, m_field_configuration.width];
    _InitArray(taken, false);
    var groups = new List<List<(int, int)>>();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
    {
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
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

  private List<GroupDetails> _RemoveGroups(int count = -1)
  {
    var groups = _GetCrossGroups();
    if (count == -1)
      count = groups.Count;
    count = Math.Min(count, groups.Count);
    var group_details = new List<GroupDetails>();
    if (count == 0)
      return group_details;
    for (int group_id = 0; group_id < count; ++group_id)
    {
      var group = groups[group_id];
      group_details.Add(new GroupDetails(group, m_field[group[0].Item1, group[0].Item2].value));
      foreach (var element in group)
        m_field[element.Item1, element.Item2] = m_empty_element;
    }
    return group_details;
  }

  private List<GroupDetails> _AccumulateGroups()
  {
    var groups = _GetCrossGroups();
    var group_details = new List<GroupDetails>();
    for (int group_id = 0; group_id < groups.Count; ++group_id)
    {
      var group = groups[group_id];
      var value = m_field[group[0].Item1, group[0].Item2].value;
      group_details.Add(new GroupDetails(group, value));
      var accumulated_value = (int)Math.Pow(2, value) * group.Count;
      var values = new Queue<int>();
      var pow = 0;
      while (accumulated_value > 0)
      {
        if (accumulated_value % 2 == 1)
          values.Enqueue(pow);
        accumulated_value = accumulated_value / 2;
        ++pow;
      }
      for (int i = 0; i < group.Count; ++i)
      {
        var j = UnityEngine.Random.Range(0, group.Count);
        (group[i], group[j]) = (group[j], group[i]);
      }
      foreach (var element in group)
      {
        var new_value = FieldElementsFactory.undefined_value;
        if (values.Count > 0)
          new_value = values.Dequeue();
        if (new_value == FieldElementsFactory.undefined_value)
          m_field[element.Item1, element.Item2] = m_empty_element;
        else
          m_field[element.Item1, element.Item2].value = new_value;
      }
    }
    return group_details;
  }

  private void _InitIntervals()
  {
    m_values_interval = Enumerable.Range(0, m_field_configuration.active_elements_count).ToArray();
    m_values_probability_interval = new float[m_field_configuration.active_elements_count];
    var mean = 0;
    var deviation = m_field_configuration.active_elements_count / 2;

    Func<float, float> normal_distribution = (x) =>
      Mathf.Exp(-Mathf.Pow((x - mean) / deviation, 2) / 2) / (deviation * Mathf.Sqrt(2 * Mathf.PI));

    for (int x = 0; x < m_field_configuration.active_elements_count; ++x)
    {
      m_values_probability_interval[x] = normal_distribution(x);
      if (x > 0)
        m_values_probability_interval[x] += normal_distribution(-x);
    }
  }

  public List<GroupDetails> ProcessGroups()
  {
    switch (m_field_configuration.mode)
    {
      case FieldConfiguration.Mode.Classic:
        return _RemoveGroups();
      case FieldConfiguration.Mode.Accumulated:
        return _AccumulateGroups();
      default:
        throw new NotImplementedException();
    }
  }

  private void _Init()
  {
    m_field = new FieldElement[m_field_configuration.height, m_field_configuration.width];
    _InitIntervals();
    var cells_configuration = m_field_configuration.GetCellsConfiguration();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var element_type = (FieldElement.Type)cells_configuration[row_id, column_id];
        int value = FieldElementsFactory.undefined_value;
        if (element_type == FieldElement.Type.Common)
          value = _GetRandomValue();
        m_field[row_id, column_id] = FieldElementsFactory.CreateElement(element_type, value);
      }
    while (true)
    {
      var removed_groups_sizes = _RemoveGroups();
      if (removed_groups_sizes.Count == 0)
        break;
      SpawnNewValues();
    }
  }

  public void Reset()
  {
    System.IO.File.Delete(m_save_file_path);
    _Init();
  }

  private struct SerializableData
  {
    public int width;
    public int height;
    public int active_elements_count;
    public string[] field;
    public int[] values_interval;
    public float[] values_probability_mask;
    public string mode;
    public string move_direction;
    public string spawn_move_scenario;
  }

  private bool _Load()
  {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return false;
    m_field_configuration.width = data.width;
    m_field_configuration.height = data.height;
    m_field_configuration.active_elements_count = data.active_elements_count;
    m_field = new FieldElement[m_field_configuration.height, m_field_configuration.width];
    m_field_configuration.InitCellsConfiguration();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        m_field[row_id, column_id] = FieldElement.FromString(data.field[row_id * m_field_configuration.width + column_id]);
        m_field_configuration.ElementAt(row_id, column_id, m_field[row_id, column_id].type);
      }
    m_values_interval = data.values_interval;
    m_values_probability_interval = data.values_probability_mask;
    m_field_configuration.mode = Enum.Parse<FieldConfiguration.Mode>(data.mode);
    m_field_configuration.move_direction = Enum.Parse<FieldConfiguration.MoveDirection>(data.move_direction);
    m_field_configuration.spawn_move_scenario = Enum.Parse<FieldConfiguration.SpawnMoveScenario>(data.spawn_move_scenario);
    return true;
  }

  public void Save()
  {
    var data = new SerializableData();
    data.width = m_field_configuration.width;
    data.height = m_field_configuration.height;
    data.active_elements_count = m_field_configuration.active_elements_count;
    data.field = new string[data.width * data.height];
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        int flat_id = row_id * data.width + column_id;
        data.field[flat_id] = m_field[row_id, column_id].ToString();
      }
    data.values_interval = m_values_interval;
    data.values_probability_mask = m_values_probability_interval;
    data.mode = Enum.GetName(typeof(FieldConfiguration.Mode), m_field_configuration.mode);
    data.move_direction = Enum.GetName(typeof(FieldConfiguration.MoveDirection), m_field_configuration.move_direction);
    data.spawn_move_scenario = Enum.GetName(typeof(FieldConfiguration.SpawnMoveScenario), m_field_configuration.spawn_move_scenario);
    SaveLoad.Save(data, m_save_file_path);
  }
}