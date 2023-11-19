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

  public enum FillStrategy {
    MoveThenSpawn,
    SpawnThenMove
  }

  [SerializeField] public int width;
  [SerializeField] public int height;
  [SerializeField] public int active_elements_count;

  [SerializeField] public Mode mode;
  [SerializeField] public MoveDirection move_direction;
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
    var clone = new FieldConfiguration();
    clone.width = width;
    clone.height = height;
    clone.active_elements_count = active_elements_count;
    clone.mode = mode;
    clone.fill_strategy = fill_strategy;
    clone.move_direction = move_direction;
    clone.m_cells_configuration = (FieldElement.Type[,])m_cells_configuration.Clone();
    return clone;
  }

  public void Update(FieldConfiguration i_other) {
    width = i_other.width;
    height = i_other.height;
    active_elements_count = i_other.active_elements_count;
    mode = i_other.mode;
    fill_strategy = i_other.fill_strategy;
    move_direction = i_other.move_direction;
    m_cells_configuration = i_other.m_cells_configuration;
  }

  private struct SerializableData {
    public int width;
    public int height;
    public int active_elements_count;
    public string mode;
    public string move_direction;
    public string fill_strategy;
    public int[] cells_configuration;
  }

  public bool Load(string i_name) {
    var save_file_path = $"{Application.persistentDataPath}/{i_name}FieldConfiguration.json";
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, save_file_path))
      return false;
    width = data.width;
    height = data.height;
    active_elements_count = data.active_elements_count;
    m_cells_configuration = new FieldElement.Type[height, width];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        m_cells_configuration[row_id, column_id] = (FieldElement.Type)data.cells_configuration[row_id * width + column_id];
    mode = Enum.Parse<Mode>(data.mode);
    move_direction = Enum.Parse<MoveDirection>(data.move_direction);
    fill_strategy = Enum.Parse<FillStrategy>(data.fill_strategy);
    return true;
  }

  public void Save(string i_name) {
    var save_file_path = $"{Application.persistentDataPath}/{i_name}FieldConfiguration.json";
    var data = new SerializableData();
    data.width = width;
    data.height = height;
    data.active_elements_count = active_elements_count;
    data.cells_configuration = new int[width * height];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id) {
        int flat_id = row_id * width + column_id;
        data.cells_configuration[flat_id] = (int)m_cells_configuration[row_id, column_id];
      }
    data.mode = Enum.GetName(typeof(Mode), mode);
    data.move_direction = Enum.GetName(typeof(MoveDirection), move_direction);
    data.fill_strategy = Enum.GetName(typeof(FillStrategy), fill_strategy);
    SaveLoad.Save(data, save_file_path);
  }
}