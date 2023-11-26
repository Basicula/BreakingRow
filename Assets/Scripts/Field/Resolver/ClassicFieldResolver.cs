using System.Collections.Generic;

class ClassicFieldResolver : IFieldResolver {
  public ClassicFieldResolver(FieldData i_field_data) : base(i_field_data) { }

  protected override FieldChanges _Process() {
    var groups = _GetGroups();
    var changes = new FieldChanges();
    if (groups.Count == 0)
      return changes;
    foreach (var group in groups) {
      changes.combined.Add((m_field_data[group[0]].value, group));
      foreach (var element in group)
        m_field_data[element.Item1, element.Item2] = FieldElementsFactory.empty_element;
    }
    return changes;
  }
}