export function draw_line(context, x1, y1, x2, y2, line_width = 1) {
  context.beginPath();
  context.lineWidth = line_width;
  context.moveTo(x1, y1);
  context.lineTo(x2, y2);
  context.stroke();
  context.closePath();
}

export function draw_rect(context, x, y, width, height, line_width = 1, color = "#000") {
  context.beginPath();
  context.lineWidth = line_width;
  context.strokeStyle = color;
  context.rect(x, y, width, height);
  context.stroke();
  context.closePath();
}

export function draw_regular_polygon(
  context,
  center,
  size,
  angle_count,
  start_angle = 0,
  rounding_radius = 0) {

  if (angle_count === 0) {
    context.arc(center[0], center[1], size, 0, 2 * Math.PI);
    return;
  }

  const angle_step = (Math.PI * 2) / angle_count;
  var points = [];
  for (let point_id = 0; point_id < angle_count; ++point_id) {
    const angle = start_angle + point_id * angle_step;
    points.push([
      center[0] + size * Math.cos(angle),
      center[1] + size * Math.sin(angle),
    ]);
  }

  if (rounding_radius === 0) {
    context.moveTo(points[0][0], points[0][1]);
    for (let point_id = 1; point_id < points.length; ++point_id)
      context.lineTo(points[point_id][0], points[point_id][1]);
    context.lineTo(points[0][0], points[0][1]);
    return;
  }

  const point_offset = Math.sin(angle_step / 2) * rounding_radius / Math.sin((Math.PI - angle_step) / 2);
  var points_with_offset = [];
  var arc_centers = [];
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
    points_with_offset.push([
      curr_point[0] + vector1_normalized[0] * point_offset,
      curr_point[1] + vector1_normalized[1] * point_offset]);
    points_with_offset.push([
      curr_point[0] + vector2_normalized[0] * point_offset,
      curr_point[1] + vector2_normalized[1] * point_offset]);

    const vector_to_arc_center = [
      vector1_normalized[0] + vector2_normalized[0],
      vector1_normalized[1] + vector2_normalized[1]
    ];
    const vector_to_arc_center_length = Math.sqrt(
      vector_to_arc_center[0] * vector_to_arc_center[0] +
      vector_to_arc_center[1] * vector_to_arc_center[1]
    );
    const vector_to_arc_center_normalized = [
      vector_to_arc_center[0] / vector_to_arc_center_length,
      vector_to_arc_center[1] / vector_to_arc_center_length
    ];
    const dist_to_arc_center = Math.sqrt(rounding_radius * rounding_radius + point_offset * point_offset);
    arc_centers.push([
      curr_point[0] + vector_to_arc_center_normalized[0] * dist_to_arc_center,
      curr_point[1] + vector_to_arc_center_normalized[1] * dist_to_arc_center
    ]);
  }

  context.moveTo(points_with_offset[0][0], points_with_offset[0][1]);
  for (let point_id = 1; point_id < points_with_offset.length; point_id += 2) {
    const prev_point = point_id - 1;
    var next_point_id = point_id + 1;
    if (next_point_id === points_with_offset.length)
      next_point_id = 0;
    const arc_center_id = Math.trunc(point_id / 2);
    const arc_center = arc_centers[arc_center_id];
    const arc_end_angle = Math.atan2(
      (points_with_offset[point_id][1] - arc_center[1]),
      (points_with_offset[point_id][0] - arc_center[0])
    );
    const arc_start_angle = Math.atan2(
      (points_with_offset[prev_point][1] - arc_center[1]),
      (points_with_offset[prev_point][0] - arc_center[0])
    );
    context.arc(arc_center[0], arc_center[1], rounding_radius, arc_start_angle, arc_end_angle);
    context.lineTo(points_with_offset[point_id][0], points_with_offset[point_id][1]);
    context.lineTo(points_with_offset[next_point_id][0], points_with_offset[next_point_id][1]);
  }
}