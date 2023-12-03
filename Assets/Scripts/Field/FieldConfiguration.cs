using System;
using UnityEngine;

[Serializable]
public class FieldConfiguration {
  public enum Mode {
    Classic,
    Accumulated
  }

  public enum MoveDirection {
    TopToBottom,
    RightToLeft,
    BottomToTop,
    LeftToRight
  }

  public enum MoveType {
    Immobile,
    Fall,
    FallAndSlide
  }

  public enum FillStrategy {
    MoveThenSpawn,
    SpawnThenMove
  }

  [SerializeField] public int width;
  [SerializeField] public int height;
  [SerializeField] public int active_elements_count;

  [SerializeField] public Mode mode;
  [SerializeField] public MoveDirection move_direction;
  [SerializeField] public MoveType move_type;
  [SerializeField] public FillStrategy fill_strategy;

  private FieldElement.Type[,] m_cells_configuration;

  public FieldElement.Type[,] GetCellsConfiguration() {
    if (m_cells_configuration is null)
      InitCellsConfiguration();
    if (m_cells_configuration.GetLength(0) != height || m_cells_configuration.GetLength(1) != width)
      InitCellsConfiguration();
    return m_cells_configuration;
  }

  public void InitCellsConfiguration() {
    m_cells_configuration = new FieldElement.Type[height, width];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        m_cells_configuration[row_id, column_id] = FieldElement.Type.Common;
  }

  public void SetElementTypeAt(int i_row, int i_column, FieldElement.Type i_type) {
    m_cells_configuration[i_row, i_column] = i_type;
  }

  public FieldConfiguration Clone() {
    var clone = new FieldConfiguration {
      width = width,
      height = height,
      active_elements_count = active_elements_count,
      mode = mode,
      fill_strategy = fill_strategy,
      move_direction = move_direction,
      move_type = move_type,
      m_cells_configuration = (FieldElement.Type[,])m_cells_configuration.Clone()
    };
    return clone;
  }

  public void Update(FieldConfiguration i_other) {
    width = i_other.width;
    height = i_other.height;
    active_elements_count = i_other.active_elements_count;
    mode = i_other.mode;
    fill_strategy = i_other.fill_strategy;
    move_direction = i_other.move_direction;
    move_type = i_other.move_type;
    m_cells_configuration = i_other.m_cells_configuration;
  }

  private struct SerializableData {
    public int width;
    public int height;
    public int active_elements_count;
    public Mode mode;
    public MoveDirection move_direction;
    public MoveType move_type;
    public FillStrategy fill_strategy;
    public FieldElement.Type[] cells_configuration;
  }

  public bool Load() {
    var save_file_path = Utilities.GetSavePath("FieldConfiguration");
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, save_file_path))
      return false;
    width = data.width;
    height = data.height;
    active_elements_count = data.active_elements_count;
    m_cells_configuration = new FieldElement.Type[height, width];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        m_cells_configuration[row_id, column_id] = data.cells_configuration[row_id * width + column_id];
    mode = data.mode;
    move_direction = data.move_direction;
    move_type = data.move_type;
    fill_strategy = data.fill_strategy;
    return true;
  }

  public void Save() {
    var save_file_path = Utilities.GetSavePath("FieldConfiguration");
    var data = new SerializableData {
      width = width,
      height = height,
      active_elements_count = active_elements_count,
      cells_configuration = new FieldElement.Type[width * height],
      mode = mode,
      move_direction = move_direction,
      move_type = move_type,
      fill_strategy = fill_strategy,
    };
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id) {
        int flat_id = row_id * width + column_id;
        data.cells_configuration[flat_id] = m_cells_configuration[row_id, column_id];
      }
    SaveLoad.Save(data, save_file_path);
  }
}