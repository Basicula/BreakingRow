import { regular_polygon_path, star_path, circle_path } from "./SvgPath.js";

export class ElementStyleProvider {
  constructor(size) {
    this.size = size;
    const circle_shape_path = circle_path([size / 2, size / 2], size / 2);

    const triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 10 * size / 16,
      3, -Math.PI / 2);
    const rounded_triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2, Math.floor(0.1 * size));
    const rotated_triangle_shape_path = regular_polygon_path([size / 2, 3 * size / 8], 10 * size / 16,
      3, Math.PI / 2);
    const rounded_rotated_triangle_shape_path = regular_polygon_path([size / 2, 3 * size / 8], 11 * size / 16,
      3, Math.PI / 2, Math.floor(0.1 * size));

    const square_shape_path = regular_polygon_path([size / 2, size / 2], 10 * size / 14,
      4, -Math.PI / 4);
    const rounded_square_shape_path = regular_polygon_path([size / 2, size / 2], 10 * size / 14,
      4, -Math.PI / 4, Math.floor(0.1 * size));
    const rotated_square_shape_path = regular_polygon_path([size / 2, size / 2], 18 * size / 32,
      4, 0);
    const rounded_rotated_square_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      4, 0, Math.floor(0.1 * size));

    const pentagon_shape_path = regular_polygon_path([size / 2, 9 * size / 16], 9 * size / 16,
      5, -Math.PI / 2);
    const rounded_pentagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      5, -Math.PI / 2, Math.floor(0.1 * size));

    const hexagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16, 6, 0);
    const rounded_hexagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      6, 0, Math.floor(0.1 * size));

    const octagon_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      8, Math.PI / 8);
    const rounded_octagon_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      8, Math.PI / 8, Math.floor(0.1 * size));

    const star_5_shape_path = star_path([size / 2, size / 2], 8 * size / 14,
      5, Math.PI / 12);
    const rounded_star_5_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      5, Math.PI / 12, Math.floor(0.05 * size));

    const star_7_shape_path = star_path([size / 2, size / 2], 9 * size / 16,
      7, -Math.PI / 14);
    const rounded_star_7_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      7, -Math.PI / 14, Math.floor(0.05 * size));

    this.shape_paths = [
      triangle_shape_path, square_shape_path, pentagon_shape_path, star_5_shape_path,
      rotated_triangle_shape_path, hexagon_shape_path, rotated_square_shape_path,
      octagon_shape_path, star_7_shape_path,

      rounded_triangle_shape_path, rounded_square_shape_path, rounded_pentagon_shape_path,
      rounded_star_5_shape_path, rounded_rotated_triangle_shape_path, rounded_hexagon_shape_path,
      rounded_rotated_square_shape_path, rounded_octagon_shape_path, rounded_star_7_shape_path
    ];
    this.colors = [
      "#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA",
      "#FFFB28", "#FF6D0A", "#CB0032", "#00990A", "#990054"
    ];
  }

  get(value) {
    return [
      this.colors[value % this.colors.length],
      this.shape_paths[value % this.shape_paths.length]
    ];
  }
}