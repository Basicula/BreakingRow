import {
  regular_polygon_points, rounded_corners_path, rounded_edges_path, polyline_path, star_path
} from "./SvgPath.js";

export class ElementStyleProvider {
  constructor(size) {
    this.size = size;

    const triangle_path = polyline_path(
      regular_polygon_points([size / 2, 5 * size / 8], 10 * size / 16, 3, -Math.PI / 2)
    );
    const rounded_corners_triangle_path = rounded_corners_path(
      regular_polygon_points([size / 2, 5 * size / 8], 11 * size / 16, 3, -Math.PI / 2),
      Math.floor(0.1 * size)
    );
    const rounded_edges_triangle_path = rounded_edges_path(
      regular_polygon_points([size / 2, 5 * size / 8], 10 * size / 16, 3, -Math.PI / 2),
      Math.floor(2 * size)
    );

    const rotated_triangle_path = polyline_path(
      regular_polygon_points([size / 2, 3 * size / 8], 10 * size / 16, 3, Math.PI / 2)
    );
    const rounded_corners_rotated_triangle_path = rounded_corners_path(
      regular_polygon_points([size / 2, 3 * size / 8], 11 * size / 16, 3, Math.PI / 2),
      Math.floor(0.1 * size)
    );
    const rounded_edges_rotated_triangle_path = rounded_edges_path(
      regular_polygon_points([size / 2, 3 * size / 8], 10 * size / 16, 3, Math.PI / 2),
      Math.floor(2 * size)
    );

    const square_path = polyline_path(
      regular_polygon_points([size / 2, size / 2], 10 * size / 14, 4, -Math.PI / 4)
    );
    const rounded_corners_square_path = rounded_corners_path(
      regular_polygon_points([size / 2, size / 2], 10 * size / 14, 4, -Math.PI / 4),
      Math.floor(0.1 * size)
    );
    const rounded_edges_square_path = rounded_edges_path(
      regular_polygon_points([size / 2, size / 2], 10 * size / 14, 4, -Math.PI / 4),
      Math.floor(2 * size)
    );

    const rotated_square_path = polyline_path(
      regular_polygon_points([size / 2, size / 2], 18 * size / 32, 4, 0)
    );
    const rounded_corners_rotated_square_path = rounded_corners_path(
      regular_polygon_points([size / 2, size / 2], 8 * size / 14, 4, 0),
      Math.floor(0.1 * size)
    );
    const rounded_edges_rotated_square_path = rounded_edges_path(
      regular_polygon_points([size / 2, size / 2], 18 * size / 32, 4, 0),
      Math.floor(1.5 * size)
    );

    const pentagon_path = polyline_path(
      regular_polygon_points([size / 2, 9 * size / 16], 9 * size / 16, 5, -Math.PI / 2)
    );
    const rounded_corners_pentagon_path = rounded_corners_path(
      regular_polygon_points([size / 2, size / 2], 9 * size / 16, 5, -Math.PI / 2),
      Math.floor(0.1 * size)
    );
    const rounded_edges_pentagon_path = rounded_edges_path(
      regular_polygon_points([size / 2, size / 2], 9 * size / 16, 5, -Math.PI / 2),
      Math.floor(2 * size)
    );

    const hexagon_path = polyline_path(
      regular_polygon_points([size / 2, size / 2], 9 * size / 16, 6, 0)
    );
    const rounded_corners_hexagon_path = rounded_corners_path(
      regular_polygon_points([size / 2, size / 2], 9 * size / 16, 6, 0),
      Math.floor(0.1 * size)
    );
    const rounded_edges_hexagon_path = rounded_edges_path(
      regular_polygon_points([size / 2, size / 2], 9 * size / 16, 6, 0),
      Math.floor(2 * size)
    );

    const octagon_path = polyline_path(
      regular_polygon_points([size / 2, size / 2], 8 * size / 14, 8, Math.PI / 8)
    );
    const rounded_corners_octagon_path = rounded_corners_path(
      regular_polygon_points([size / 2, size / 2], 8 * size / 14, 8, Math.PI / 8),
      Math.floor(0.1 * size)
    );
    const rounded_edges_octagon_path = rounded_edges_path(
      regular_polygon_points([size / 2, size / 2], 8 * size / 14, 8, Math.PI / 8),
      Math.floor(2 * size)
    );

    const star_5_path = star_path([size / 2, size / 2], 8 * size / 14,
      5, Math.PI / 12);
    const rounded_corners_star_5_path = star_path([size / 2, size / 2], 9 * size / 14,
      5, Math.PI / 12, Math.floor(0.05 * size));

    const star_7_path = star_path([size / 2, size / 2], 9 * size / 16,
      7, -Math.PI / 14);
    const rounded_corners_star_7_path = star_path([size / 2, size / 2], 9 * size / 14,
      7, -Math.PI / 14, Math.floor(0.05 * size));

    this.paths = [
      triangle_path, square_path, pentagon_path, star_5_path,
      rotated_triangle_path, hexagon_path, rotated_square_path,
      octagon_path, star_7_path,

      rounded_corners_triangle_path, rounded_corners_square_path, rounded_corners_pentagon_path,
      rounded_corners_star_5_path, rounded_corners_rotated_triangle_path, rounded_corners_hexagon_path,
      rounded_corners_rotated_square_path, rounded_corners_octagon_path, rounded_corners_star_7_path
    ];
    this.colors = [
      "#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA",
      "#FFFB28", "#FF6D0A", "#CB0032", "#00990A", "#990054"
    ];
  }

  get(value) {
    return [
      this.colors[value % this.colors.length],
      this.paths[value % this.paths.length]
    ];
  }
}