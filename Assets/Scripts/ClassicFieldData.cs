using System.Collections.Generic;

public class ClassicFieldData : FieldBase
{
  public ClassicFieldData(int i_width, int i_height)
    : base(i_width, i_height)
  { }

  public override List<GroupDetails> ProcessGroups()
  {
    return _RemoveGroups();
  }

  protected override void _InitIdentifier()
  {
    m_identifier = "Classic";
  }

  protected override void _InitIntervals()
  {
    m_values_interval = new int[4] { 0, 1, 2, 3 };
    m_values_probability_interval = new float[4] { 0.4f, 0.3f, 0.2f, 0.2f };
  }
}