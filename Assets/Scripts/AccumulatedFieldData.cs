using System;
using System.Collections.Generic;

public class AccumulatedFieldData : FieldBase
{
  public AccumulatedFieldData(int width, int height)
    : base(width, height)
  {
    //for (int row_id = 0; row_id < m_height; ++row_id)
    //  for (int column_id = 0; column_id < m_width; ++column_id)
    //    m_field[row_id, column_id] = row_id * width + column_id;
  }

  public override List<GroupDetails> ProcessGroups()
  {
    var groups = _GetCrossGroups();
    var group_details = new List<GroupDetails>();
    for (int group_id = 0; group_id < groups.Count; ++group_id)
    {
      var group = groups[group_id];
      var value = m_field[group[0].Item1, group[0].Item2];
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
        var new_value = -1;
        if (values.Count > 0)
          new_value = values.Dequeue();
        m_field[element.Item1, element.Item2] = new_value;
      }
    }
    return group_details;
  }

  protected override void _InitIdentifier()
  {
    m_identifier = "Accumulated";
  }

  protected override void _InitIntervals()
  {
    m_values_interval = new int[4] { 0, 1, 2, 3 };
    m_values_probability_interval = new float[4] { 0.4f, 0.3f, 0.2f, 0.2f };
  }
}
