using UnityEngine;

public class GameFieldBackgroundGrid : MonoBehaviour {
  [SerializeReference] private GameObject m_background_image;
  public void Init(FieldConfiguration i_field_configuration, FieldGridConfiguration i_grid_configuration) {
    var svg = new SVG();
    var rect_size = new Vector2(i_grid_configuration.grid_step, i_grid_configuration.grid_step);
    var rect_color = "rgba(20, 20, 20, 0.5)";
    var rect_stroke_props = new SVGStrokeProps("#000000", i_grid_configuration.inner_grid_stroke_width);
    for (int row_id = 0; row_id < i_field_configuration.height; ++row_id)
      for (int column_id = 0; column_id < i_field_configuration.width; ++column_id)
        svg.Add(new SVGRect(new Vector2(column_id * i_grid_configuration.grid_step, row_id * i_grid_configuration.grid_step), rect_size, rect_color, rect_stroke_props));

    m_background_image.transform.localPosition = i_grid_configuration.position;
    var sprite_renderer = m_background_image.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = SVG.BuildSprite(svg, i_grid_configuration.grid_step);
  }
}