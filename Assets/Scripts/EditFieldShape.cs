using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditFieldShape : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler
{
  public enum ShapePreset
  {
    Circle,
    Random
  }

  [SerializeReference] private TMP_Dropdown m_shape_preset_selector;
  [SerializeReference] private TMP_Dropdown m_field_element_selector;
  [SerializeReference] private Button m_shape_preset_apply;

  private FieldConfiguration m_field_configuration;

  private GameObject[,] m_tiles;
  private FieldElement.Type m_current_element_type;

  private float m_grid_size;
  private Vector2 m_field_offset;
  private bool[,] m_drag_visited_tiles;

  private readonly Dictionary<FieldElement.Type, Color> m_tile_color_by_element_id;

  public EditFieldShape()
  {
    m_tile_color_by_element_id = new Dictionary<FieldElement.Type, Color> {
      { FieldElement.Type.Common, Color.black },
      { FieldElement.Type.Empty, Color.black },
      { FieldElement.Type.Hole, Color.white },
      { FieldElement.Type.InteractableDestractable, Color.blue },
      { FieldElement.Type.ImmobileDestractable, Color.yellow },
    };
    m_current_element_type = FieldElement.Type.Common;
  }

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
        image.color = m_tile_color_by_element_id[cells[row_id, column_id]];
        tile.transform.SetParent(transform);
        var tile_transform = tile.GetComponent<RectTransform>();
        tile_transform.sizeDelta = new Vector2(tile_size, tile_size);
        tile_transform.anchoredPosition = new Vector2(column_id * m_grid_size + half_square_size, total_height - (row_id * m_grid_size + half_square_size)) + m_field_offset;
        tile_transform.anchorMin = new Vector2(0, 0);
        tile_transform.anchorMax = new Vector2(0, 0);
        m_tiles[row_id, column_id] = tile;
      }
  }

  public void Start()
  {
    var options = Enum.GetNames(typeof(ShapePreset)).ToList();
    m_shape_preset_selector.ClearOptions();
    m_shape_preset_selector.AddOptions(options);
    var current_option_name = Enum.GetName(typeof(ShapePreset), ShapePreset.Circle);
    m_shape_preset_selector.SetValueWithoutNotify(options.IndexOf(current_option_name));
    m_shape_preset_apply.onClick.AddListener(() =>
    {
      var preset = m_shape_preset_selector.options[m_shape_preset_selector.value].text;
      _ApplyPreset(Enum.Parse<ShapePreset>(preset));
    });

    options = Enum.GetNames(typeof(FieldElement.Type)).ToList();
    options.Remove(Enum.GetName(typeof(FieldElement.Type), FieldElement.Type.Empty));
    m_field_element_selector.ClearOptions();
    m_field_element_selector.AddOptions(options);
    current_option_name = Enum.GetName(typeof(FieldElement.Type), m_current_element_type);
    m_shape_preset_selector.SetValueWithoutNotify(options.IndexOf(current_option_name));
    m_field_element_selector.onValueChanged.AddListener((option_id) =>
    {
      var field_element = m_field_element_selector.options[option_id].text;
      m_current_element_type = Enum.Parse<FieldElement.Type>(field_element);
    });
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

  public FieldElement.Type[,] GetCells()
  {
    var cells = new FieldElement.Type[m_field_configuration.height, m_field_configuration.width];
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var color = m_tiles[row_id, column_id].GetComponent<RawImage>().color;
        cells[row_id, column_id] = m_tile_color_by_element_id.FirstOrDefault(pair => pair.Value == color).Key;
      }
    return cells;
  }

  private void _ApplyPreset(ShapePreset i_preset)
  {
    switch (i_preset)
    {
      case ShapePreset.Circle:
        _CircleShape();
        break;
      case ShapePreset.Random:
        _RandomPreset();
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
        var element_type = x * x + y * y <= sqr_radius ? FieldElement.Type.Common : FieldElement.Type.Hole;
        m_field_configuration.ElementAt(row_id, column_id, element_type);
      }
  }

  private void _RandomPreset()
  {
    var keys = m_tile_color_by_element_id.Keys.ToArray();
    System.Random random = new System.Random();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
        m_field_configuration.ElementAt(row_id, column_id, keys[random.Next(keys.Length)]);
  }

  private void _UpdateTiles()
  {
    var cells = m_field_configuration.GetCellsConfiguration();
    for (int row_id = 0; row_id < m_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < m_field_configuration.width; ++column_id)
      {
        var image = m_tiles[row_id, column_id].GetComponent<RawImage>();
        image.color = m_tile_color_by_element_id[(FieldElement.Type)cells[row_id, column_id]];
      }
  }

  private void _ToggleTile((int, int) i_position)
  {
    var tile = m_tiles[i_position.Item1, i_position.Item2];
    var tile_image = tile.GetComponent<RawImage>();
    tile_image.color = m_tile_color_by_element_id[m_current_element_type];
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