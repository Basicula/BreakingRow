using System.Collections.Generic;

class AccumulativeFieldResolver : IFieldResolver {
  public AccumulativeFieldResolver(FieldData i_field_data) : base(i_field_data) { }

  protected override FieldChanges _Process() {
    var groups = _GetGroups();
    var changes = new FieldChanges();
    if (groups.Count == 0)
      return changes;
    foreach (var group in groups) {
      var value = m_field_data[group[0]].value;
      changes.combined.Add((value, group));
      var accumulated_value = (int)System.Math.Pow(2, value) * group.Count;
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
          m_field_data[element] = FieldElementsFactory.empty_element;
        else {
          m_field_data[element].value = new_value;
          changes.created.Add(element);
        }
      }
    }
    return changes;
  }
}