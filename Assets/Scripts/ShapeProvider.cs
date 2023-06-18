using UnityEngine;
using System;
using System.Collections.Generic;

class ShapeProvider
{
  public enum ShapeType
  {
    Triangle = 0,
    RotatedTriangle = 1,
    Square = 2,
    RotatedSquare = 3,
    Pentagon = 4,
    Hesxagon = 5,
    Octagon = 6,
    Star5 = 7,
    Star7 = 8
  }

  private float m_size;

  private Dictionary<ShapeType, List<Vector2>> m_shapes;

  public ShapeProvider(float i_size, float i_line_width)
  {
    m_size = i_size - i_line_width;

    m_shapes = new Dictionary<ShapeType, List<Vector2>>();

    m_shapes[ShapeType.Triangle] = _RegularPolygon(3, Mathf.PI / 2);
    m_shapes[ShapeType.RotatedTriangle] = _RegularPolygon(3, -Mathf.PI / 2);
    m_shapes[ShapeType.Square] = _RegularPolygon(4, Mathf.PI / 4);
    m_shapes[ShapeType.RotatedSquare] = _RegularPolygon(4);
    m_shapes[ShapeType.Pentagon] = _RegularPolygon(5, Mathf.PI / 2);
    m_shapes[ShapeType.Hesxagon] = _RegularPolygon(6);
    m_shapes[ShapeType.Octagon] = _RegularPolygon(8, -Mathf.PI / 8);
    m_shapes[ShapeType.Star5] = _StarPolygon(5, Mathf.PI / 10);
    m_shapes[ShapeType.Star7] = _StarPolygon(7, -Mathf.PI / 14);
  }

  public SVGPath RegularShape(ShapeType i_shape)
  {
    var points = m_shapes[i_shape];
    var actual_bounds = _GetBounds(points);
    _Scale(actual_bounds, ref points);
    SVGPath svg_path = new SVGPath();
    var shape_points = m_shapes[i_shape];
    svg_path.MoveTo(shape_points[0]);
    for (int point_id = 1; point_id < shape_points.Count; ++point_id)
      svg_path.LineTo(shape_points[point_id]);
    svg_path.LineTo(shape_points[0]);
    svg_path.Close();
    return svg_path;
  }

  public SVGPath RoundCornersShape(ShapeType i_shape, float i_rounding_radius)
  {
    List<Vector2> rounded_points = new List<Vector2>();
    List<Vector2> arc_centers = new List<Vector2>();
    List<bool> arc_directions = new List<bool>();
    var shape_points = m_shapes[i_shape];
    for (int point_id = 0; point_id < shape_points.Count; ++point_id)
    {
      Vector2 curr_point = shape_points[point_id];
      int prev_point_id = point_id - 1;
      int next_point_id = point_id + 1;
      if (point_id == 0)
        prev_point_id = shape_points.Count - 1;
      if (point_id == shape_points.Count - 1)
        next_point_id = 0;
      Vector2 prev_point = shape_points[prev_point_id];
      Vector2 next_point = shape_points[next_point_id];
      Vector2 vector1 = prev_point - curr_point;
      Vector2 vector2 = next_point - curr_point;
      Vector2 direction_to_arc_center = (vector1 + vector2).normalized;
      float vector1_length = vector1.magnitude;
      float vector2_length = vector2.magnitude;
      Vector2 vector1_normalized = vector1.normalized;
      Vector2 vector2_normalized = vector2.normalized;
      float angle_at_point = Mathf.Acos((vector1[0] * vector2[0] + vector1[1] * vector2[1]) / (vector1_length * vector2_length));
      float point_offset = Mathf.Sin((Mathf.PI - angle_at_point) / 2) * i_rounding_radius / Mathf.Sin(angle_at_point / 2);
      float distance_to_arc_center = Mathf.Sqrt(point_offset * point_offset + i_rounding_radius * i_rounding_radius);
      rounded_points.Add(curr_point + vector1_normalized * point_offset);
      arc_centers.Add(curr_point + direction_to_arc_center * distance_to_arc_center);
      rounded_points.Add(curr_point + vector2_normalized * point_offset);
      float cross = vector1[0] * vector2[1] - vector1[1] * vector2[0];
      arc_directions.Add(cross > 0);
    }
    var rounded_corners = new List<Vector2>();
    var support_directions = new Vector2[4] {
        new Vector2(i_rounding_radius, 0),
        new Vector2(0, i_rounding_radius),
        new Vector2(-i_rounding_radius, 0),
        new Vector2(0, -i_rounding_radius),
      };
    for (int corner_id = 0; corner_id < arc_centers.Count; ++corner_id)
    {
      rounded_corners.Add(rounded_points[2 * corner_id]);
      foreach (var direction in support_directions)
        rounded_corners.Add(arc_centers[corner_id] + direction);
      rounded_corners.Add(rounded_points[2 * corner_id + 1]);
    }
    var bounds = _GetBounds(rounded_corners);
    var scale_factor = _GetFitScale(bounds);
    i_rounding_radius *= scale_factor;
    _Scale(bounds, ref rounded_points);

    SVGPath svg_path = new SVGPath();
    svg_path.MoveTo(rounded_points[0]);
    for (int point_id = 1; point_id < rounded_points.Count; point_id += 2)
    {
      int next_point_id = point_id + 1;
      if (next_point_id == rounded_points.Count)
        next_point_id = 0;
      int arc_id = (int)(point_id / 2);
      svg_path.ArcTo(i_rounding_radius, arc_directions[arc_id], rounded_points[point_id]);
      svg_path.LineTo(rounded_points[next_point_id]);
    }
    svg_path.Close();
    return svg_path;
  }

  public SVGPath RoundEdgesShape(ShapeType i_shape, float i_rounding_radius, bool i_is_concave)
  {
    var points = m_shapes[i_shape];
    if (!i_is_concave)
    {
      var rounding_points = new List<Vector2>();
      var support_directions = new Vector2[5] {
          new Vector2(i_rounding_radius, 0),
          new Vector2(0, i_rounding_radius),
          new Vector2(-i_rounding_radius, 0),
          new Vector2(0, -i_rounding_radius),
          new Vector2(i_rounding_radius, 0)
        };
      Func<Vector2, float> get_angle = (Vector2 i_point) =>
      {
        var angle = Mathf.Atan2(i_point.y, i_point.x);
        if (angle < 0)
          angle += 2 * Mathf.PI;
        return angle;
      };
      for (int point_id = 0; point_id < points.Count; ++point_id)
      {
        Vector2 curr_point = points[point_id];
        int next_point_id = point_id + 1;
        if (point_id == points.Count - 1)
          next_point_id = 0;
        Vector2 next_point = points[next_point_id];
        var a = (next_point - curr_point).magnitude / 2;
        var dist_to_arc_center = Mathf.Sqrt(i_rounding_radius * i_rounding_radius - a * a);
        var middle_point = (curr_point + next_point) / 2;
        var first_angle = get_angle(curr_point);
        var second_angle = get_angle(next_point);
        if (first_angle > second_angle)
          second_angle += 2 * Mathf.PI;
        var arc_center = middle_point - (middle_point.normalized * dist_to_arc_center);
        rounding_points.Add(curr_point);
        for (int direction_id = 0; direction_id < support_directions.Length; ++direction_id)
        {
          var direction = support_directions[direction_id];
          var support_point = arc_center + direction;
          var point_angle = get_angle(support_point);
          if (point_angle < first_angle || point_angle > second_angle)
            continue;
          rounding_points.Add(support_point);
        }
      }
      string debug = "";
      foreach (var point in rounding_points)
        debug += $"{point} ";
      var bounds = _GetBounds(rounding_points);
      var scale = _GetFitScale(bounds);
      _Scale(bounds, ref points);
      i_rounding_radius *= scale;
    }

    SVGPath svg_path = new SVGPath();
    svg_path.MoveTo(points[0]);
    // Additional loop needs due to bad Unity resolve closed paths for this case
    // TODO find better way to avoid such strange behavior
    for (int point_id = 1; point_id < 2 * points.Count; ++point_id)
      svg_path.ArcTo(i_rounding_radius, i_is_concave, points[point_id % points.Count]);
    svg_path.ArcTo(i_rounding_radius, i_is_concave, points[0]);
    return svg_path;
  }

  private List<Vector2> _RegularPolygon(int angle_count, float start_angle = 0)
  {
    float angle_step = (Mathf.PI * 2) / angle_count;
    List<Vector2> points = new List<Vector2>();
    for (int point_id = 0; point_id < angle_count; ++point_id)
    {
      float angle = start_angle + point_id * angle_step;
      var new_point = _UnitVector(angle);
      points.Add(new_point);
    }
    return points;
  }

  private List<Vector2> _StarPolygon(int i_spikes_count, float i_start_angle = 0)
  {
    float angle_count = i_spikes_count * 2;
    float angle_step = (Mathf.PI * 2) / angle_count;
    List<Vector2> points = new List<Vector2>();
    for (int point_id = 0; point_id < angle_count; ++point_id)
    {
      float angle = i_start_angle + point_id * angle_step;
      var point = _UnitVector(angle);
      if (point_id % 2 == 1)
        point /= 2;
      points.Add(point);
    }
    return points;
  }

  private float _GetFitScale(Rect i_bounds)
  {
    return Mathf.Min(m_size / i_bounds.width, m_size / i_bounds.height);
  }

  private void _Scale(Rect i_bounds, ref List<Vector2> i_points)
  {
    var scale_factor = _GetFitScale(i_bounds);
    var scale = new Vector2(scale_factor, scale_factor);
    for (int point_id = 0; point_id < i_points.Count; ++point_id)
    {
      var point = i_points[point_id];
      point.Scale(scale);
      i_points[point_id] = point;
    }
  }

  static private Rect _GetBounds(List<Vector2> i_points)
  {
    Vector2 min = i_points[0];
    Vector2 max = i_points[0];
    for (int point_id = 1; point_id < i_points.Count; ++point_id)
    {
      min.x = Mathf.Min(min.x, i_points[point_id].x);
      min.y = Mathf.Min(min.y, i_points[point_id].y);
      max.x = Mathf.Max(max.x, i_points[point_id].x);
      max.y = Mathf.Max(max.y, i_points[point_id].y);
    }
    return new Rect(min, max - min);
  }

  static private Vector2 _UnitVector(float angle)
  {
    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
  }
}