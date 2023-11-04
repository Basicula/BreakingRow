public class FieldElementsFactory
{
  public const int undefined_value = -1;

  public static FieldElement CreateElement(FieldElement.Type i_type, int i_value = undefined_value)
  {
    switch (i_type)
    {
      case FieldElement.Type.Common:
        return new FieldElement(i_type, true, true, false, true, true, i_value);
      case FieldElement.Type.Empty:
        return new FieldElement(i_type, false, false, false, false, false, i_value);
      case FieldElement.Type.Hole:
        return new FieldElement(i_type, false, false, false, false, false, i_value);
      case FieldElement.Type.InteractableDestractable:
        return new FieldElement(i_type, true, true, true, false, true, i_value);
      case FieldElement.Type.ImmobileDestractable:
        return new FieldElement(i_type, false, false, true, false, true, i_value);
      default:
        throw new System.NotImplementedException($"Field elements factory can't create element with type {i_type} as it's not implemented yet");
    }
  }
}