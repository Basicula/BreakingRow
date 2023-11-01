public class FieldElementsFactory
{
  public const int undefined_value = -1;

  public const int common_element_class_id = 0;
  public const int empty_element_class_id = -1;
  public const int hole_element_class_id = -2;

  public static FieldElement CreateElementByClassId(int i_class_id, int i_value = undefined_value)
  {
    switch (i_class_id)
    {
      case common_element_class_id: return CreateCommonElement(i_value);
      case empty_element_class_id: return CreateEmptyElement(i_value);
      case hole_element_class_id: return CreateHoleElement(i_value);
      default: return null;
    }
  }

  public static FieldElement CreateCommonElement(int i_value)
  {
    return new FieldElement(common_element_class_id, true, true, false, true, true, i_value);
  }

  public static FieldElement CreateEmptyElement(int i_value = undefined_value)
  {
    return new FieldElement(empty_element_class_id, false, false, false, false, false, i_value);
  }

  public static FieldElement CreateHoleElement(int i_value = undefined_value)
  {
    return new FieldElement(hole_element_class_id, false, false, false, false, false, i_value);
  }
}