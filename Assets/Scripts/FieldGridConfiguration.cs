using UnityEngine;

public class FieldGridConfiguration {

  private int m_width;
  private int m_height;
  public readonly float grid_step;
  public readonly float half_grid_step;
  public readonly float element_offset;
  public readonly float element_size;
  public readonly float inner_grid_stroke_width;
  public readonly float outer_grid_stroke_width;
  private readonly Vector2 m_center;
  private readonly GameField.MaxActiveZone m_max_active_zone;

  public Vector3 position { get => m_max_active_zone.position; }

  public FieldGridConfiguration(FieldConfiguration i_field_configuration, GameField.MaxActiveZone i_max_active_zone) {
    m_max_active_zone = i_max_active_zone;
    grid_step = Mathf.Min(m_max_active_zone.rect.width / i_field_configuration.width, m_max_active_zone.rect.height / i_field_configuration.height);
    half_grid_step = grid_step / 2;
    element_offset = grid_step * 0.05f;
    element_size = grid_step - 2 * element_offset;
    inner_grid_stroke_width = element_offset / 4;
    outer_grid_stroke_width = element_offset;
    m_center = new Vector2(grid_step * i_field_configuration.width / 2 - m_max_active_zone.position.x, grid_step * i_field_configuration.height / 2 + m_max_active_zone.position.y);
    m_height = i_field_configuration.height;
    m_width = i_field_configuration.width;
  }

  public Vector2 GetElementPosition(int row_id, int column_id) {
    return new Vector2(grid_step * column_id - m_center.x + half_grid_step,
      m_center.y - half_grid_step - grid_step * row_id
    );
  }

  public Vector2 GetElementPosition((int, int) i_position) =>
    GetElementPosition(i_position.Item1, i_position.Item2);

  public (int, int) GetElementPosition(Vector2 i_position) {
    var row_id = Mathf.FloorToInt((m_center.y - i_position.y) / grid_step);
    var column_id = Mathf.FloorToInt((m_center.x + i_position.x) / grid_step);
    return (row_id, column_id);
  }

  public (Vector2, Vector2) GetAnchors() {
    var anchor_delta = m_max_active_zone.anchor_max - m_max_active_zone.anchor_min;
    var height_fraction = anchor_delta.y * (m_max_active_zone.rect.height - grid_step * m_height) / m_max_active_zone.rect.height / 2;
    var width_fraction = anchor_delta.x * (m_max_active_zone.rect.width - grid_step * m_width) / m_max_active_zone.rect.width / 2;
    var anchor_min = m_max_active_zone.anchor_min + new Vector2(width_fraction, height_fraction);
    var anchor_max = m_max_active_zone.anchor_max - new Vector2(width_fraction, height_fraction);
    return (anchor_min, anchor_max);
  }
}