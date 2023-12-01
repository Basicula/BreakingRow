public abstract class IFieldElementsMover {
  protected FieldData m_field_data;

  protected IFieldElementsMover(FieldData i_field_data) {
    m_field_data = i_field_data;
  }

  public abstract FieldChanges Move();
}