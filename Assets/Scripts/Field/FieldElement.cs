using System;

[Serializable]
public class FieldElement {
  public enum Type {
    Hole = -2,
    Empty = -1,
    Common = 0,
    InteractableDestractable = 1,
    ImmobileDestractable = 2,
  }

  public int value;
  public readonly Type type;
  public readonly bool interactable; // If it can be moved by user, i.e. swap with other element that is also interactable
  public readonly bool movable; // If it can be moved by "gravity"
  public readonly bool combinable; // If it can be combined and processed as one of the match 3 game figure
  public readonly bool destructible; // If it can be damaged via nearby elements combination
  public readonly bool affected_by_ability; // If it can be damaged via special abilities

  public FieldElement(
    Type i_type,
    bool i_interactable,
    bool i_movable,
    bool i_destructible,
    bool i_combinable,
    bool i_affected_by_ability,
    int i_value = FieldElementsFactory.undefined_value) {
    type = i_type;
    interactable = i_interactable;
    movable = i_movable;
    destructible = i_destructible;
    combinable = i_combinable;
    affected_by_ability = i_affected_by_ability;
    value = i_value;
  }

  public static bool operator ==(FieldElement i_first, FieldElement i_second) {
    return i_first.type == i_second.type &&
      i_first.value == i_second.value &&
      i_first.interactable == i_second.interactable &&
      i_first.movable == i_second.movable &&
      i_first.destructible == i_second.destructible &&
      i_first.combinable == i_second.combinable &&
      i_first.affected_by_ability == i_second.affected_by_ability;
  }
  public static bool operator !=(FieldElement i_first, FieldElement i_second) => !(i_first == i_second);

  public override string ToString() {
    return $"{(int)type},{interactable},{movable},{destructible},{combinable},{affected_by_ability},{value}";
  }

  public static FieldElement FromString(string i_string) {
    var attributes = i_string.Split(',');
    return new FieldElement(
      (Type)int.Parse(attributes[0]),
      bool.Parse(attributes[1]),
      bool.Parse(attributes[2]),
      bool.Parse(attributes[3]),
      bool.Parse(attributes[4]),
      bool.Parse(attributes[5]),
      int.Parse(attributes[6])
    );
  }

  public override bool Equals(object i_obj) {
    return i_obj is FieldElement element &&
           type == element.type &&
           value == element.value &&
           interactable == element.interactable &&
           movable == element.movable &&
           destructible == element.destructible &&
           combinable == element.combinable &&
           affected_by_ability == element.affected_by_ability;
  }

  public override int GetHashCode() {
    return HashCode.Combine(value, type, interactable, movable, destructible, combinable, affected_by_ability);
  }
}