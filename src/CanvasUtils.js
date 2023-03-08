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