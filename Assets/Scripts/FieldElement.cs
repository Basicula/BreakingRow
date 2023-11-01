[System.Serializable]
public class FieldElement
{
  public int value;
  public readonly int id;
  public readonly bool interactable; // If it can be moved by user, i.e. swap with other element that is also interactable
  public readonly bool movable; // If it can be moved by "gravity"
  public readonly bool destracable; // If it can be damaged via nearby elements combination
  public readonly bool combinable; // If it can be combined and processed as one of the match 3 game figure
  public readonly bool destroyable; // If it can be damaged via special abilities

  public FieldElement(
    int i_id,
    bool i_interactable,
    bool i_movable,
    bool i_destracable,
    bool i_combinable,
    bool i_destroyable,
    int i_value = FieldElementsFactory.undefined_value)
  {
    id = i_id;
    interactable = i_interactable;
    movable = i_movable;
    destracable = i_destracable;
    combinable = i_combinable;
    destroyable = i_destroyable;
    value = i_value;
  }

  public static bool operator ==(FieldElement i_first, FieldElement i_second)
  {
    return i_first.id == i_second.id &&
      i_first.value == i_second.value &&
      i_first.interactable == i_second.interactable &&
      i_first.movable == i_second.movable &&
      i_first.destracable == i_second.destracable &&
      i_first.combinable == i_second.combinable &&
      i_first.destroyable == i_second.destroyable;
  }
  public static bool operator !=(FieldElement i_first, FieldElement i_second) => !(i_first == i_second);

  public override string ToString()
  {
    return $"{id},{interactable},{movable},{destracable},{combinable},{destroyable},{value}";
  }

  public static FieldElement FromString(string i_string)
  {
    var attributes = i_string.Split(',');
    return new FieldElement(
      int.Parse(attributes[0]),
      bool.Parse(attributes[1]),
      bool.Parse(attributes[2]),
      bool.Parse(attributes[3]),
      bool.Parse(attributes[4]),
      bool.Parse(attributes[5]),
      int.Parse(attributes[6])
    );
  }
}