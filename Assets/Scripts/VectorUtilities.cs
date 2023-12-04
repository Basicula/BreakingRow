using System.Collections.Generic;
using UnityEngine;

public class VectorUtilities {
  public static Vector3 Lerp(List<Vector3> i_control_points, float i_max_value, float i_value) {
    var step = i_max_value / (i_control_points.Count - 1);
    i_value %= i_max_value;
    var first_id = Mathf.FloorToInt(i_value / step);
    var second_id = first_id + 1;
    return Vector3.Lerp(i_control_points[first_id], i_control_points[second_id], (i_value - step * first_id) / step);
  }
}