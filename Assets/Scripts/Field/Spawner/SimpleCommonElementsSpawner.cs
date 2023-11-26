using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class SimpleCommonElementsSpawner : IFieldElementsSpawner {
  private int[] m_values_interval;
  private float[] m_values_probability_interval;

  public SimpleCommonElementsSpawner(FieldData i_field_data) : base(i_field_data) { }

  public override void Reset() {
    m_values_interval = Enumerable.Range(0, m_field_data.configuration.active_elements_count).ToArray();
  }

  public override void InitElements() {
    var field_configuration = m_field_data.configuration;
    var it = new FieldDataIterator(field_configuration.move_direction, field_configuration.height, field_configuration.width);
    var cells_configuration = field_configuration.GetCellsConfiguration();
    while (!it.Finished()) {
      if (m_field_data[it.current] is null) {
        var cell_type = cells_configuration[it.current.Item1, it.current.Item2];
        m_field_data[it.current] = FieldElementsFactory.CreateElement(cell_type, _GetRandomValue());
      }
      it.Increment(true);
    }
  }

  public override List<(int, int)> SpawnElements() {
    var field_configuration = m_field_data.configuration;
    var created = new List<(int, int)>();
    var it = new FieldDataIterator(field_configuration.move_direction, field_configuration.height, field_configuration.width);
    while (!it.Finished()) {
      if (m_field_data[it.current] == FieldElementsFactory.empty_element && !m_field_data.ShouldBeEmpty(it.current)) {
        m_field_data[it.current] = FieldElementsFactory.CreateElement(FieldElement.Type.Common, _GetRandomValue());
        created.Add(it.current);
      }
      it.Increment(true);
    }
    return created;
  }

  public override List<(int, int)> Upgrade() {
    var elements_to_remove = FieldElementsFactory.CreateElement(FieldElement.Type.Common, m_values_interval[0]);
    for (int i = 0; i < m_values_interval.Length; ++i)
      ++m_values_interval[i];
    return m_field_data.RemoveSameAs(elements_to_remove);
  }

  private int _GetRandomValue() {
    float random = UnityEngine.Random.Range(0.0f, 1.0f);
    float accumulated_probability = 0.0f;
    for (int i = 0; i < m_values_interval.Length; ++i) {
      if (random <= accumulated_probability)
        return m_values_interval[i - 1];
      accumulated_probability += m_values_probability_interval[i];
    }
    return m_values_interval[^1];
  }

  protected override void _Init() {
    var field_configuration = m_field_data.configuration;
    m_values_interval = Enumerable.Range(0, field_configuration.active_elements_count).ToArray();
    m_values_probability_interval = new float[field_configuration.active_elements_count];
    var mean = 0;
    var deviation = field_configuration.active_elements_count / 2;

    float normal_distribution(float x) =>
      Mathf.Exp(-Mathf.Pow((x - mean) / deviation, 2) / 2) / (deviation * Mathf.Sqrt(2 * Mathf.PI));

    for (int x = 0; x < field_configuration.active_elements_count; ++x) {
      m_values_probability_interval[x] = normal_distribution(x);
      if (x > 0)
        m_values_probability_interval[x] += normal_distribution(-x);
    }
  }

  private struct SerializableData {
    public int[] values_interval;
    public float[] values_probability_mask;
  }

  public override bool Load() {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_path))
      return false;
    m_values_interval = data.values_interval;
    m_values_probability_interval = data.values_probability_mask;
    return true;
  }

  public override void Save() {
    var data = new SerializableData {
      values_interval = m_values_interval,
      values_probability_mask = m_values_probability_interval
    };
    SaveLoad.Save(data, m_save_path);
  }
}