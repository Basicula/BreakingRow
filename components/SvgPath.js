export function line_path(x1, y1, x2, y2) {
  var svg_path = `M ${x1},${y1}\n`;
  svg_path += `L ${x2},${y2}`;
  return svg_path;
}

export function rounded_corners_path(points, rounding_radius) {
  var points_with_offset = [];
  var arc_directions = [];
  for (let point_id = 0; point_id < points.length; ++point_id) {
    const curr_point = points[point_id];
    var prev_point_id = point_id - 1;
    var next_point_id = point_id + 1;
    if (point_id === 0)
      prev_point_id = points.length - 1;
    if (point_id === points.length - 1)
      next_point_id = 0;
    const prev_point = points[prev_point_id];
    const next_point = points[next_point_id];
    const vector1 = [prev_point[0] - curr_point[0], prev_point[1] - curr_point[1]];
    const vector2 = [next_point[0] - curr_point[0], next_point[1] - curr_point[1]];
    const vector1_length = Math.sqrt(vector1[0] * vector1[0] + vector1[1] * vector1[1]);
    const vector2_length = Math.sqrt(vector2[0] * vector2[0] + vector2[1] * vector2[1]);
    const vector1_normalized = [vector1[0] / vector1_length, vector1[1] / vector1_length];
    const vector2_normalized = [vector2[0] / vector2_length, vector2[1] / vector2_length];
    const angle_at_point = Math.acos((vector1[0] * vector2[0] + vector1[1] * vector2[1]) / (vector1_length * vector2_length));
    const point_offset = Math.sin((Math.PI - angle_at_point) / 2) * rounding_radius / Math.sin(angle_at_point / 2);
    points_with_offset.push([
      curr_point[0] + vector1_normalized[0] * point_offset,
      curr_point[1] + vector1_normalized[1] * point_offset]);
    points_with_offset.push([
      curr_point[0] + vector2_normalized[0] * point_offset,
      curr_point[1] + vector2_normalized[1] * point_offset]);
    const cross = vector1[0] * vector2[1] - vector1[1] * vector2[0];
    arc_directions.push(cross > 0);
  }

  var svg_path = `M ${points_with_offset[0][0]},${points_with_offset[0][1]}\n`;
  for (let point_id = 1; point_id < points_with_offset.length; point_id += 2) {
    var next_point_id = point_id + 1;
    if (next_point_id === points_with_offset.length)
      next_point_id = 0;
    const arc_id = Math.trunc(point_id / 2);
    svg_path += `A ${rounding_radius} ${rounding_radius} 0 0 ${arc_directions[arc_id] ? 0 : 1} ${points_with_offset[point_id][0]},${points_with_offset[point_id][1]}\n`;
    svg_path += `L ${points_with_offset[next_point_id][0]},${points_with_offset[next_point_id][1]}\n`;
  }
  return svg_path;
}

export function rounded_edges_path(points, rounding_radius) {
  var svg_path = `M ${points[0][0]},${points[0][1]}\n`;
  for (let point_id = 1; point_id < points.length; ++point_id)
    svg_path += `A ${rounding_radius} ${rounding_radius} 0 0 1 ${points[point_id][0]},${points[point_id][1]}\n`;
  svg_path += `A ${rounding_radius} ${rounding_radius} 0 0 1 ${points[0][0]},${points[0][1]}\n`;
  return svg_path;
}

export function polyline_path(points) {
  var svg_path = `M ${points[0][0]},${points[0][1]}\n`;
  for (let point_id = 1; point_id < points.length; ++point_id)
    svg_path += `L ${points[point_id][0]},${points[point_id][1]}\n`;
  svg_path += `L ${points[0][0]},${points[0][1]}\n`;
  return svg_path;
}

export function circle_path(center, radius) {
  var svg_path = `M ${center[0] + radius},${center[1]}\n`;
  svg_path += `A ${radius} ${radius} 0 0 1 ${center[0] - radius},${center[1]}\n`;
  svg_path += `A ${radius} ${radius} 0 0 1 ${center[0] + radius},${center[1]}\n`;
  return svg_path;
}

export function regular_polygon_points(
  center,
  size,
  angle_count,
  start_angle = 0) {
  const angle_step = (Math.PI * 2) / angle_count;
  var points = [];
  for (let point_id = 0; point_id < angle_count; ++point_id) {
    const angle = start_angle + point_id * angle_step;
    points.push([
      center[0] + size * Math.cos(angle),
      center[1] + size * Math.sin(angle),
    ]);
  }
  return points;
}

export function regular_polygon_path(
  center,
  size,
  angle_count,
  start_angle = 0,
  rounding_radius = 0) {
  const angle_step = (Math.PI * 2) / angle_count;
  var points = [];
  for (let point_id = 0; point_id < angle_count; ++point_id) {
    const angle = start_angle + point_id * angle_step;
    points.push([
      center[0] + size * Math.cos(angle),
      center[1] + size * Math.sin(angle),
    ]);
  }

  if (rounding_radius === 0)
    return polyline_path(points);

  return rounded_corners_path(points, rounding_radius);
}

export function star_path(
  center,
  size,
  corner_count,
  start_angle = 0,
  rounding_radius = 0) {
  const angle_count = corner_count * 2;
  const angle_step = (Math.PI * 2) / angle_count;
  var points = [];
  for (let point_id = 0; point_id < angle_count; ++point_id) {
    const angle = start_angle + point_id * angle_step;
    const radius = point_id % 2 === 1 ? size : size / 2;
    points.push([
      center[0] + radius * Math.cos(angle),
      center[1] + radius * Math.sin(angle),
    ]);
  }

  if (rounding_radius === 0)
    polyline_path(points);

  return rounded_corners_path(points, rounding_radius);
}