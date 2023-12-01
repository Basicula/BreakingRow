public class ImmobileMover : IFieldElementsMover {
  public ImmobileMover(FieldData i_field_data) : base(i_field_data) {
  }

  public override FieldChanges Move() {
    return new();
  }
}