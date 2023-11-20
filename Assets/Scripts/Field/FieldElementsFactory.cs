public class FieldElementsFactory {
  public const int undefined_value = -1;

  public static readonly FieldElement hole_element = new(FieldElement.Type.Hole, false, false, false, false, false, undefined_value);
  public static readonly FieldElement empty_element = new(FieldElement.Type.Empty, false, false, false, false, false, undefined_value);

  public static FieldElement CreateElement(FieldElement.Type i_type, int i_value = undefined_value) {
    switch (i_type) {
      case FieldElement.Type.Common:
        return new FieldElement(i_type, true, true, false, true, true, i_value);
      case FieldElement.Type.InteractableDestractable:
        return new FieldElement(i_type, true, true, true, false, true, i_value);
      case FieldElement.Type.ImmobileDestractable:
        return new FieldElement(i_type, false, false, true, false, true, i_value);
      case FieldElement.Type.Empty:
        throw new System.ArgumentException($"No need to create empty element, please use public static readonly field with corresponding info");
      case FieldElement.Type.Hole:
        throw new System.ArgumentException($"No need to create hole element, please use public static readonly field with corresponding info");
      default:
        throw new System.NotImplementedException($"Field elements factory can't create element with type {i_type} as it's not implemented yet");
    }
  }
}