using System.Collections.Generic;
using UnityEngine;

public class ElementStyleProvider
{
  public struct ElementProps
  {
    public Sprite sprite;
    public string number;
    public float text_zone_size;
  }

  private string[] m_colors;
  private List<SVGPath> m_paths;
  private Dictionary<int, ElementProps> m_sprite_cache;
  private float m_size;
  private float m_line_width;

  public ElementStyleProvider(float size)
  {
    m_size = size;
    m_line_width = m_size / 25;
    m_colors = new string[10]
    {
      "#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA",
      "#FFFB28", "#FF6D0A", "#CB0032", "#00990A", "#990054"
    };
    // m_colors = new string[10]
    // {
    //   "#111111", "#222222", "#333333", "#444444", "#555555",
    //   "#666666", "#777777", "#888888", "#999999", "#aaaaaa"
    // };
    m_sprite_cache = new Dictionary<int, ElementProps>();

    var shape_provider = new ShapeProvider(size, m_line_width);

    var corner_rounding_radius = Mathf.Floor(0.1f * size);
    var edge_rounding_radius = Mathf.Floor(2 * size);

    ShapeProvider.ShapeType current_shape = ShapeProvider.ShapeType.Triangle;
    SVGPath triangle_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_triangle_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_triangle_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_triangle_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.RotatedTriangle;
    SVGPath rotated_triangle_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_rotated_triangle_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_rotated_triangle_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_rotated_triangle_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.Square;
    SVGPath square_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_square_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_square_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_square_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.RotatedSquare;
    SVGPath rotated_square_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_rotated_square_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_rotated_square_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_rotated_square_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.Pentagon;
    SVGPath pentagon_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_pentagon_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_pentagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_pentagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.Hesxagon;
    SVGPath hexagon_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_hexagon_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_hexagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_hexagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.Octagon;
    SVGPath octagon_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_octagon_path = shape_provider.RoundCornersShape(current_shape, corner_rounding_radius);
    SVGPath rounded_convex_edges_octagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, false);
    SVGPath rounded_concave_edges_octagon_path = shape_provider.RoundEdgesShape(current_shape, edge_rounding_radius, true);

    var star_corner_rounding_radius = corner_rounding_radius / 2;
    var star_edge_rounding_radius = edge_rounding_radius / 4;
    current_shape = ShapeProvider.ShapeType.Star5;
    SVGPath star_5_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_star_5_path = shape_provider.RoundCornersShape(current_shape, star_corner_rounding_radius);
    SVGPath rounded_convex_edges_star_5_path = shape_provider.RoundEdgesShape(current_shape, star_edge_rounding_radius, false);
    SVGPath rounded_concave_edges_star_5_path = shape_provider.RoundEdgesShape(current_shape, star_edge_rounding_radius, true);

    current_shape = ShapeProvider.ShapeType.Star7;
    SVGPath star_7_path = shape_provider.RegularShape(current_shape);
    SVGPath rounded_corners_star_7_path = shape_provider.RoundCornersShape(current_shape, star_corner_rounding_radius);
    SVGPath rounded_convex_edges_star_7_path = shape_provider.RoundEdgesShape(current_shape, star_edge_rounding_radius, false);
    SVGPath rounded_concave_edges_star_7_path = shape_provider.RoundEdgesShape(current_shape, star_edge_rounding_radius, true);

    m_paths = new List<SVGPath>() {
      triangle_path, square_path, pentagon_path, star_5_path,
      rotated_triangle_path, hexagon_path, rotated_square_path,
      octagon_path, star_7_path,

      rounded_corners_triangle_path, rounded_corners_square_path, rounded_corners_pentagon_path,
      rounded_corners_star_5_path, rounded_corners_rotated_triangle_path, rounded_corners_hexagon_path,
      rounded_corners_rotated_square_path, rounded_corners_octagon_path, rounded_corners_star_7_path,

      rounded_convex_edges_triangle_path, rounded_convex_edges_square_path, rounded_convex_edges_pentagon_path,
      rounded_convex_edges_star_5_path, rounded_convex_edges_rotated_triangle_path, rounded_convex_edges_hexagon_path,
      rounded_convex_edges_rotated_square_path, rounded_convex_edges_octagon_path, rounded_convex_edges_star_7_path,

      rounded_concave_edges_triangle_path, rounded_concave_edges_square_path, rounded_concave_edges_pentagon_path,
      rounded_concave_edges_star_5_path, rounded_concave_edges_rotated_triangle_path, rounded_concave_edges_hexagon_path,
      rounded_concave_edges_rotated_square_path, rounded_concave_edges_octagon_path, rounded_concave_edges_star_7_path,
    };
  }

  public ElementProps Get(int value)
  {
    if (m_sprite_cache.ContainsKey(value))
      return m_sprite_cache[value];
    SVG svg = new SVG();
    {
      // Unity ignores this information but leave it as it's needed for correct SVG
      svg.width = m_size;
      svg.height = m_size;
      svg.viewbox_min = new Vector2(-m_size / 2, -m_size / 2);
      svg.viewbox_size = new Vector2(m_size, m_size);
    }
    SVGPath path = m_paths[value % m_paths.Count];
    path.fill_color = m_colors[value % m_colors.Length];
    path.stroke_props = new SVGStrokeProps("#000000", m_line_width);
    svg.Add(path);

    var element_props = new ElementProps();
    _FillElementNumberText(ref element_props, value);
    element_props.sprite = SVG.BuildSprite(svg, m_size / 100, m_size / 100);
    element_props.text_zone_size = m_size * 0.5f;
    m_sprite_cache[value] = element_props;
    return m_sprite_cache[value];
  }

  private void _FillElementNumberText(ref ElementProps io_element_props, int i_value)
  {
    if (i_value > 20)
    {
      char[] exponents = new char[10] { '⁰', '¹', '²', '³', '⁴', '⁵', '⁶', '⁷', '⁸', '⁹' };
      string exponent = "";
      while (i_value > 0)
      {
        exponent = exponents[i_value % 10] + exponent;
        i_value /= 10;
      }
      io_element_props.number = $"2{exponent}";
    }
    else
      io_element_props.number = Mathf.FloorToInt(Mathf.Pow(2, i_value)).ToString();
  }
}
