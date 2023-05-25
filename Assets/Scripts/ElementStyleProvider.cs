using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

public class ElementStyleProvider
{
  enum ShapeType
  {
    RegularPolygon,
    RegularStar,
    RoundedRegularPolygon,
    RoundedStar
  };

  private string[] colors;
  private List<SVGPath> paths;
  private float size;

  public ElementStyleProvider(float size)
  {
    this.size = size;
    this.colors = new string[10]
    {
      "#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA",
      "#FFFB28", "#FF6D0A", "#CB0032", "#00990A", "#990054"
    };

    SVGPath triangle_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, 5 * size / 8), 10 * size / 16, 3, Mathf.PI / 2)
    );
    SVGPath rounded_corners_triangle_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, 5 * size / 8), 11 * size / 16, 3, Mathf.PI / 2),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_triangle_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, 5 * size / 8), 10 * size / 16, 3, Mathf.PI / 2),
      Mathf.Floor(2 * size)
    );

    SVGPath rotated_triangle_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, 3 * size / 8), 10 * size / 16, 3, -Mathf.PI / 2)
    );
    SVGPath rounded_corners_rotated_triangle_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, 3 * size / 8), 11 * size / 16, 3, -Mathf.PI / 2),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_rotated_triangle_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, 3 * size / 8), 10 * size / 16, 3, -Mathf.PI / 2),
      Mathf.Floor(2 * size)
    );

    SVGPath square_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 10 * size / 14, 4, Mathf.PI / 4)
    );
    SVGPath rounded_corners_square_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 10 * size / 14, 4, Mathf.PI / 4),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_square_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 10 * size / 14, 4, Mathf.PI / 4),
      Mathf.Floor(2 * size)
    );

    SVGPath rotated_square_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 18 * size / 32, 4, 0)
    );
    SVGPath rounded_corners_rotated_square_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 8 * size / 14, 4, 0),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_rotated_square_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 18 * size / 32, 4, 0),
      Mathf.Floor(1.5f * size)
    );

    SVGPath pentagon_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, 9 * size / 16), 9 * size / 16, 5, Mathf.PI / 2)
    );
    SVGPath rounded_corners_pentagon_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 9 * size / 16, 5, Mathf.PI / 2),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_pentagon_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 9 * size / 16, 5, Mathf.PI / 2),
      Mathf.Floor(2 * size)
    );

    SVGPath hexagon_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 9 * size / 16, 6, 0)
    );
    SVGPath rounded_corners_hexagon_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 9 * size / 16, 6, 0),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_hexagon_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 9 * size / 16, 6, 0),
      Mathf.Floor(2 * size)
    );

    SVGPath octagon_path = polyline_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 8 * size / 14, 8, -Mathf.PI / 8)
    );
    SVGPath rounded_corners_octagon_path = rounded_corners_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 8 * size / 14, 8, -Mathf.PI / 8),
      Mathf.Floor(0.1f * size)
    );
    SVGPath rounded_edges_octagon_path = rounded_edges_path(
      regular_polygon_points(new Vector2(size / 2, size / 2), 8 * size / 14, 8, -Mathf.PI / 8),
      Mathf.Floor(2 * size)
    );

    SVGPath star_5_path = polyline_path(star_points(new Vector2(size / 2, size / 2), 8 * size / 14, 5, -Mathf.PI / 12));
    SVGPath rounded_corners_star_5_path = rounded_corners_path(
      star_points(new Vector2(size / 2, size / 2), 9 * size / 14, 5, -Mathf.PI / 12),
      Mathf.Floor(0.05f * size)
    );

    SVGPath star_7_path = polyline_path(star_points(new Vector2(size / 2, size / 2), 9 * size / 16, 7, Mathf.PI / 14));
    SVGPath rounded_corners_star_7_path = rounded_corners_path(
      star_points(new Vector2(size / 2, size / 2), 9 * size / 14, 7, Mathf.PI / 14),
      Mathf.Floor(0.05f * size)
    );

    this.paths = new List<SVGPath>() {
      triangle_path, square_path, pentagon_path, star_5_path,
      rotated_triangle_path, hexagon_path, rotated_square_path,
      octagon_path, star_7_path,

      rounded_corners_triangle_path, rounded_corners_square_path, rounded_corners_pentagon_path,
      rounded_corners_star_5_path, rounded_corners_rotated_triangle_path, rounded_corners_hexagon_path,
      rounded_corners_rotated_square_path, rounded_corners_octagon_path, rounded_corners_star_7_path
    };
  }

  public Sprite get(int value)
  {
    SVG svg = new SVG();
    svg.width = this.size;
    svg.height = this.size;
    svg.view_box = "0 0 " + this.size + " " + this.size;
    SVGPath path = this.paths[value % this.paths.Count];
    path.fill_color = this.colors[value % this.colors.Length];
    path.stroke_color = "#000000";
    path.stroke_width = this.size / 20;
    svg.Add(path);
    const string svg_header = "<?xml version=\"1.0\" encoding=\"utf - 8\"?>";
    string svg_string = svg_header + svg.GetXML();
    using StringReader textReader = new StringReader(svg_string);
    var sceneInfo = SVGParser.ImportSVG(textReader);
    var geometries = VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
    {
      StepDistance = this.size / 1000,
      SamplingStepSize = this.size / 1000,
      MaxCordDeviation = 0.0f,
      MaxTanAngleDeviation = 0.0f
    });
    return VectorUtils.BuildSprite(geometries, 1, VectorUtils.Alignment.Center, Vector2.zero, 128, false);
  }

  private Vector2 unit_vector(float angle, float size = 1.0f)
  {
    return new Vector2(size * Mathf.Cos(angle), size * Mathf.Sin(angle));
  }

  private List<Vector2> regular_polygon_points(Vector2 center, float size, int angle_count, float start_angle = 0)
  {
    float angle_step = (Mathf.PI * 2) / angle_count;
    List<Vector2> points = new List<Vector2>();
    for (int point_id = 0; point_id < angle_count; ++point_id)
    {
      float angle = start_angle + point_id * angle_step;
      points.Add(center + unit_vector(angle, size));
    }
    return points;
  }

  private List<Vector2> star_points(Vector2 center, float size, float corner_count, float start_angle = 0)
  {
    float angle_count = corner_count * 2;
    float angle_step = (Mathf.PI * 2) / angle_count;
    List<Vector2> points = new List<Vector2>();
    for (int point_id = 0; point_id < angle_count; ++point_id)
    {
      float angle = start_angle + point_id * angle_step;
      float radius = point_id % 2 == 1 ? size : size / 2;
      points.Add(center + unit_vector(angle, radius));
    }
    return points;
  }

  private SVGPath rounded_corners_path(List<Vector2> points, float rounding_radius)
  {
    List<Vector2> points_with_offset = new List<Vector2>();
    List<bool> arc_directions = new List<bool>();
    for (int point_id = 0; point_id < points.Count; ++point_id)
    {
      Vector2 curr_point = points[point_id];
      int prev_point_id = point_id - 1;
      int next_point_id = point_id + 1;
      if (point_id == 0)
        prev_point_id = points.Count - 1;
      if (point_id == points.Count - 1)
        next_point_id = 0;
      Vector2 prev_point = points[prev_point_id];
      Vector2 next_point = points[next_point_id];
      Vector2 vector1 = prev_point - curr_point;
      Vector2 vector2 = next_point - curr_point;
      float vector1_length = vector1.magnitude;
      float vector2_length = vector2.magnitude;
      Vector2 vector1_normalized = vector1.normalized;
      Vector2 vector2_normalized = vector2.normalized;
      float angle_at_point = Mathf.Acos((vector1[0] * vector2[0] + vector1[1] * vector2[1]) / (vector1_length * vector2_length));
      float point_offset = Mathf.Sin((Mathf.PI - angle_at_point) / 2) * rounding_radius / Mathf.Sin(angle_at_point / 2);
      points_with_offset.Add(curr_point + vector1_normalized * point_offset);
      points_with_offset.Add(curr_point + vector2_normalized * point_offset);
      float cross = vector1[0] * vector2[1] - vector1[1] * vector2[0];
      arc_directions.Add(cross > 0);
    }

    SVGPath svg_path = new SVGPath();
    svg_path.MoveTo(points_with_offset[0]);
    for (int point_id = 1; point_id < points_with_offset.Count; point_id += 2)
    {
      int next_point_id = point_id + 1;
      if (next_point_id == points_with_offset.Count)
        next_point_id = 0;
      int arc_id = (int)(point_id / 2);
      svg_path.ArcTo(rounding_radius, arc_directions[arc_id], points_with_offset[point_id]);
      svg_path.LineTo(points_with_offset[next_point_id]);
    }
    return svg_path;
  }

  private SVGPath rounded_edges_path(List<Vector2> points, float rounding_radius)
  {
    SVGPath svg_path = new SVGPath();
    svg_path.MoveTo(points[0]);
    for (int point_id = 1; point_id < points.Count; ++point_id)
      svg_path.ArcTo(rounding_radius, true, points[point_id]);
    svg_path.ArcTo(rounding_radius, true, points[0]);
    return svg_path;
  }

  private SVGPath polyline_path(List<Vector2> points)
  {
    SVGPath svg_path = new SVGPath();
    svg_path.MoveTo(points[0]);
    for (int point_id = 1; point_id < points.Count; ++point_id)
      svg_path.LineTo(points[point_id]);
    svg_path.LineTo(points[0]);
    return svg_path;
  }
}
