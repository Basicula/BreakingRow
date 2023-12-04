using System;
using System.Collections.Generic;
using UnityEngine;

class ShapeProvider {
  public enum ShapeType {
    Triangle = 0,
    RotatedTriangle = 1,
    Square = 2,
    RotatedSquare = 3,
    Pentagon = 4,
    Hesxagon = 5,
    Octagon = 6,
    Star5 = 7,
    Star7 = 8,
    Moon = 9,
    Circle = 10
  }

  private class BoundingRect {
    public Vector2 min;
    public Vector2 max;

    public BoundingRect() {
      min = new Vector2(float.MaxValue, float.MaxValue);
      max = new Vector2(float.MinValue, float.MinValue);
    }

    public BoundingRect(Vector2 i_min, Vector2 i_max) {
      min = i_min;
      max = i_max;
    }

    public Vector2 GetSize() {
      return new Vector2(max.x - min.x, max.y - min.y);
    }

    public void AddPoint(Vector2 i_point) {
      min.x = Mathf.Min(min.x, i_point.x);
      min.y = Mathf.Min(min.y, i_point.y);
      max.x = Mathf.Max(max.x, i_point.x);
      max.y = Mathf.Max(max.y, i_point.y);
    }

    public void Merge(BoundingRect i_other) {
      min.x = Mathf.Min(min.x, i_other.min.x);
      min.y = Mathf.Min(min.y, i_other.min.y);
      max.x = Mathf.Max(max.x, i_other.max.x);
      max.y = Mathf.Max(max.y, i_other.max.y);
    }
  }

  private abstract class ShapeTransition {
    public Vector2 point;

    public ShapeTransition(Vector2 i_point) {
      point = i_point;
    }

    public virtual void Scale(float i_scale) {
      point.Scale(new Vector2(i_scale, i_scale));
    }

    public abstract void FillSVG(ref SVGPath io_svg_path);
  }

  private class Move : ShapeTransition {
    public Move(Vector2 i_point) : base(i_point) {
    }

    public override void FillSVG(ref SVGPath io_svg_path) {
      io_svg_path.MoveTo(point);
    }
  }
  private class Line : ShapeTransition {
    public Line(Vector2 i_point) : base(i_point) {
    }

    public override void FillSVG(ref SVGPath io_svg_path) {
      io_svg_path.LineTo(point);
    }
  }
  private class CircularArc : ShapeTransition {
    public Vector2 arc_center;
    public float radius;
    public bool arc_direction;
    public bool large_arc;

    public CircularArc(Vector2 i_point, Vector2 i_arc_center, float i_radius, bool i_arc_direction, bool i_large_arc) : base(i_point) {
      radius = i_radius;
      arc_direction = i_arc_direction;
      arc_center = i_arc_center;
      large_arc = i_large_arc;
    }

    public override void FillSVG(ref SVGPath io_svg_path) {
      io_svg_path.ArcTo(radius, large_arc, arc_direction, point);
    }

    public override void Scale(float i_scale) {
      point.Scale(new Vector2(i_scale, i_scale));
      radius *= i_scale;
    }
  }

  private class Shape {
    public List<ShapeTransition> transitions;

    public Shape() {
      transitions = new List<ShapeTransition>();
    }

    public void Add(ShapeTransition i_transition) {
      transitions.Add(i_transition);
    }

    public BoundingRect GetBounds() {
      BoundingRect bounds = new BoundingRect();
      Func<Vector2, float> get_angle = (Vector2 i_point) => {
        var angle = Mathf.Atan2(i_point.y, i_point.x);
        if (angle < 0)
          angle += 2 * Mathf.PI;
        return angle;
      };
      for (int i = 0; i < transitions.Count; ++i) {
        var transition_type = transitions[i].GetType();
        if (transition_type == typeof(CircularArc)) {
          var transition = (CircularArc)transitions[i];
          List<Vector2> extreme_points = new List<Vector2> {
            transition.arc_center + new Vector2(transition.radius, 0),
            transition.arc_center + new Vector2(0, transition.radius),
            transition.arc_center + new Vector2(-transition.radius, 0),
            transition.arc_center + new Vector2(0, -transition.radius)
          };
          var from = transitions[i - 1].point;
          var to = transition.point;
          var first_angle = get_angle(from);
          var second_angle = get_angle(to);
          if (first_angle > second_angle)
            second_angle += 2 * Mathf.PI;
          foreach (var extreme_point in extreme_points) {
            var point_angle = get_angle(extreme_point);
            if (point_angle < first_angle || point_angle > second_angle)
              continue;
            bounds.AddPoint(extreme_point);
          }
        }
        bounds.AddPoint(transitions[i].point);
      }
      return bounds;
    }

    public void Scale(float i_scale) {
      foreach (var transition in transitions)
        transition.Scale(i_scale);
    }

    public void FillSVG(ref SVGPath io_svg_path) {
      foreach (var transition in transitions)
        transition.FillSVG(ref io_svg_path);
    }
  }

  private float m_size;
  private Dictionary<ShapeType, Shape> m_shapes;

  public ShapeProvider(float i_size, float i_line_width) {
    m_size = i_size - i_line_width;

    m_shapes = new Dictionary<ShapeType, Shape>();

    m_shapes[ShapeType.Triangle] = _RegularPolygon(3, Mathf.PI / 2);
    m_shapes[ShapeType.RotatedTriangle] = _RegularPolygon(3, -Mathf.PI / 2);
    m_shapes[ShapeType.Square] = _RegularPolygon(4, Mathf.PI / 4);
    m_shapes[ShapeType.RotatedSquare] = _RegularPolygon(4);
    m_shapes[ShapeType.Pentagon] = _RegularPolygon(5, Mathf.PI / 2);
    m_shapes[ShapeType.Hesxagon] = _RegularPolygon(6);
    m_shapes[ShapeType.Octagon] = _RegularPolygon(8, -Mathf.PI / 8);
    m_shapes[ShapeType.Star5] = _StarPolygon(5, Mathf.PI / 10);
    m_shapes[ShapeType.Star7] = _StarPolygon(7, -Mathf.PI / 14);
    m_shapes[ShapeType.Moon] = _MoonPolygon();
    m_shapes[ShapeType.Circle] = _Circle();
  }

  public SVGPath RegularShape(ShapeType i_shape) {
    var shape = m_shapes[i_shape];
    _Scale(ref shape);
    SVGPath svg_path = new SVGPath();
    shape.FillSVG(ref svg_path);
    svg_path.Close();
    return svg_path;
  }

  public SVGPath RoundCornersShape(ShapeType i_shape, float i_rounding_radius) {
    var shape = m_shapes[i_shape];
    Shape rounded_shape = new Shape();
    for (int transition_id = 0; transition_id < shape.transitions.Count; ++transition_id) {
      Vector2 curr_point = shape.transitions[transition_id].point;
      int prev_point_id = transition_id - 1;
      int next_point_id = transition_id + 1;
      if (transition_id == 0)
        prev_point_id = shape.transitions.Count - 1;
      if (transition_id == shape.transitions.Count - 1)
        next_point_id = 0;
      Vector2 prev_point = shape.transitions[prev_point_id].point;
      Vector2 next_point = shape.transitions[next_point_id].point;
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
      Vector2 arc_start = curr_point + vector1_normalized * point_offset;
      Vector2 arc_end = curr_point + vector2_normalized * point_offset;
      Vector2 arc_center = curr_point + direction_to_arc_center * distance_to_arc_center;
      float cross = vector1[0] * vector2[1] - vector1[1] * vector2[0];
      bool arc_direction = cross > 0;
      rounded_shape.Add(transition_id == 0 ? new Move(arc_start) : new Line(arc_start));
      rounded_shape.Add(new CircularArc(arc_end, arc_center, i_rounding_radius, arc_direction, false));
    }
    _Scale(ref rounded_shape);

    SVGPath svg_path = new SVGPath();
    rounded_shape.FillSVG(ref svg_path);
    svg_path.Close();
    return svg_path;
  }

  public SVGPath RoundEdgesShape(ShapeType i_shape, float i_rounding_radius, bool i_is_concave) {
    var shape = m_shapes[i_shape];
    Shape rounded_shape = new Shape();
    rounded_shape.Add(new Move(shape.transitions[0].point));
    for (int transition_id = 1; transition_id <= shape.transitions.Count; ++transition_id) {
      Vector2 prev_point = shape.transitions[transition_id - 1].point;
      Vector2 curr_point = shape.transitions[transition_id == shape.transitions.Count ? 0 : transition_id].point;
      var a = (curr_point - prev_point).magnitude / 2;
      var dist_to_arc_center = Mathf.Sqrt(i_rounding_radius * i_rounding_radius - a * a);
      var middle_point = (prev_point + curr_point) / 2;
      var arc_center = middle_point - (middle_point.normalized * dist_to_arc_center);
      rounded_shape.Add(new CircularArc(curr_point, arc_center, i_rounding_radius, i_is_concave, false));
    }
    _Scale(ref rounded_shape);

    SVGPath svg_path = new SVGPath();
    rounded_shape.FillSVG(ref svg_path);
    return svg_path;
  }

  private Shape _RegularPolygon(int angle_count, float start_angle = 0) {
    float angle_step = (Mathf.PI * 2) / angle_count;
    Shape polyline = new Shape();
    for (int point_id = 0; point_id < angle_count; ++point_id) {
      float angle = start_angle + point_id * angle_step;
      var new_point = _UnitVector(angle);
      polyline.Add(point_id == 0 ? new Move(new_point) : new Line(new_point));
    }
    return polyline;
  }

  private Shape _StarPolygon(int i_spikes_count, float i_start_angle = 0) {
    float angle_count = i_spikes_count * 2;
    float angle_step = (Mathf.PI * 2) / angle_count;
    Shape polyline = new Shape();
    for (int point_id = 0; point_id < angle_count; ++point_id) {
      float angle = i_start_angle + point_id * angle_step;
      var point = _UnitVector(angle);
      if (point_id % 2 == 1)
        point /= 2;
      polyline.Add(point_id == 0 ? new Move(point) : new Line(point));
    }
    return polyline;
  }

  private Shape _MoonPolygon() {
    Shape moon = new Shape();
    float radius = 1;
    float phase = -1.75f;
    float x_intersection = phase / 2;
    float y_intersection = Mathf.Sqrt(radius - x_intersection * x_intersection);
    moon.Add(new Move(new Vector2(x_intersection, -y_intersection)));
    moon.Add(new CircularArc(new Vector2(x_intersection, y_intersection), new Vector2(0.0f, 0.0f), radius, false, true));
    moon.Add(new CircularArc(new Vector2(x_intersection, -y_intersection), new Vector2(-phase, 0.0f), radius, true, false));
    return moon;
  }

  private Shape _Circle() {
    Shape moon = new Shape();
    moon.Add(new Move(_UnitVector(-Mathf.PI / 2)));
    moon.Add(new CircularArc(_UnitVector(Mathf.PI / 2), new Vector2(0.0f, 0.0f), 1, false, false));
    moon.Add(new CircularArc(_UnitVector(-Mathf.PI / 2), new Vector2(0.0f, 0.0f), 1, false, false));
    return moon;
  }


  private float _GetFitScale(BoundingRect i_bounds) {
    Vector2 size = i_bounds.GetSize();
    return Mathf.Min(m_size / size.x, m_size / size.y);
  }

  private void _Scale(ref Shape i_shape) {
    var scale_factor = _GetFitScale(i_shape.GetBounds());
    i_shape.Scale(scale_factor);
  }

  static private Vector2 _UnitVector(float angle) {
    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
  }
}