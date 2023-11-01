using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditFieldShape : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler
{
  public enum ShapePreset
  {
    Circle
  }

  private FieldConfiguration m_field_configuration;

  private GameObject[,] m_tiles;

  private float m_grid_size;
  private Vector2 m_field_offset;
  private bool[,] m_drag_visited_tiles;

  public void Init(FieldConfiguration i_field_configuration)
  {
    m_field_configuration = i_field_configuration;
    m_tiles = new GameObject[m_field_configuration.height, m_field_configuration.width];
    for (var child_id = 0; child_id < transform.childCount; ++child_id)
      Destroy(transform.GetChild(child_id).gameObject);
    var active_zone = gameObject.GetComponent<RectTransform>();
    m_grid_size = Mathf.Min(active_zone.rect.width / m_field_configuration.width, active_zone.rect.height / m_field_configuration.height);
    m_field_offset = new Vector2(
      active_zone.rect.width - m_grid_size * m_field_configuration.width,
      active_zone.rect.height - m_grid_size * m_field_configuration.height
    ) / 2;
    var half_square_size = m_grid_size / 2;
    var padding = m_grid_size / 10;
    var tile_size = m_grid_size - padding / 2;
    var total_height = m_grid_size * m_field_configuration.height;
    var cells = m_field_configuration.GetCellsConfiguration();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var tile = new GameObject();
        var image = tile.AddComponent<RawImage>();
        image.color = cells[row_id, column_id] == FieldElementsFactory.common_element_id ? Color.black : Color.white;
        tile.transform.SetParent(transform);
        var tile_transform = tile.GetComponent<RectTransform>();
        tile_transform.sizeDelta = new Vector2(tile_size, tile_size);
        tile_transform.anchoredPosition = new Vector2(column_id * m_grid_size + half_square_size, total_height - (row_id * m_grid_size + half_square_size)) + m_field_offset;
        tile_transform.anchorMin = new Vector2(0, 0);
        tile_transform.anchorMax = new Vector2(0, 0);
        m_tiles[row_id, column_id] = tile;
      }
  }

  public void Reset()
  {
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        m_tiles[row_id, column_id].GetComponent<RawImage>().color = Color.black;
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    m_drag_visited_tiles = new bool[m_field_configuration.height, m_field_configuration.width];
  }

  public void OnDrag(PointerEventData eventData)
  {
    var tile_position = _GetTilePosition(eventData.position);
    if (tile_position.Item1 < 0 || tile_position.Item1 >= m_field_configuration.height ||
      tile_position.Item2 < 0 || tile_position.Item2 >= m_field_configuration.width)
      return;

    if (m_drag_visited_tiles[tile_position.Item1, tile_position.Item2])
      return;

    _ToggleTile(tile_position);
    m_drag_visited_tiles[tile_position.Item1, tile_position.Item2] = true;
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.dragging)
      return;

    var tile_position = _GetTilePosition(eventData.position);
    _ToggleTile(tile_position);
  }

  public int[,] GetCells()
  {
    var cells = new int[m_field_configuration.height, m_field_configuration.width];
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var color = m_tiles[row_id, column_id].GetComponent<RawImage>().color;
        if (color == Color.black)
          cells[row_id, column_id] = FieldElementsFactory.common_element_id;
        else if (color == Color.white)
          cells[row_id, column_id] = FieldElementsFactory.hole_element_id;
      }
    return cells;
  }

  public void ApplyPreset(ShapePreset i_preset)
  {
    switch (i_preset)
    {
      case ShapePreset.Circle:
        _CircleShape();
        break;
      default:
        throw new NotImplementedException();
    }
    _UpdateTiles();
  }

  private void _CircleShape()
  {
    var radius = Mathf.Min(m_field_configuration.width, m_field_configuration.height) / 2.0f;
    var sqr_radius = radius * radius;
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var x = column_id - m_field_configuration.width / 2.0f + 0.5;
        var y = row_id - m_field_configuration.height / 2.0f + 0.5;
        int is_exists_element = x * x + y * y <= sqr_radius ? FieldElementsFactory.common_element_id : FieldElementsFactory.hole_element_id;
        m_field_configuration.ElementAt(row_id, column_id, is_exists_element);
      }
  }

  private void _UpdateTiles()
  {
    var cells = m_field_configuration.GetCellsConfiguration();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var image = m_tiles[row_id, column_id].GetComponent<RawImage>();
        image.color = cells[row_id, column_id] == FieldElementsFactory.common_element_id ? Color.black : Color.white;
      }
  }

  private void _ToggleTile((int, int) i_position)
  {
    var tile = m_tiles[i_position.Item1, i_position.Item2];
    var tile_image = tile.GetComponent<RawImage>();
    if (tile_image.color == Color.black)
      tile_image.color = Color.white;
    else
      tile_image.color = Color.black;
  }

  private (int, int) _GetTilePosition(Vector2 i_position)
  {
    var field_center = new Vector2(transform.position.x, transform.position.y);
    var field_rect = gameObject.GetComponent<RectTransform>().rect;
    var field_min = field_center + field_rect.min + m_field_offset;
    var delta = i_position - field_min;
    var row_id = Mathf.FloorToInt(m_field_configuration.height - delta.y / m_grid_size);
    var column_id = Mathf.FloorToInt(delta.x / m_grid_size);
    return (row_id, column_id);
  }
}