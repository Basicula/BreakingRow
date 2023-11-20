using System.Collections.Generic;

public abstract class IFieldElementsSpawner {
  protected FieldData m_field_data;
  protected string m_save_path;

  protected IFieldElementsSpawner(FieldData i_field_data) {
    m_field_data = i_field_data;
    m_save_path = Utilities.GetSavePath("FieldElementsSpawner");
    if (!Load())
      _Init();
  }

  protected abstract void _Init();

  public abstract List<(int, int)> SpawnElements();
  public abstract void InitElements();

  public abstract bool Load();
  public abstract void Save();
}