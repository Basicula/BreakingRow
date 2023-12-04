using System.Collections.Generic;
using UnityEngine;

public class GameFieldHoles : MonoBehaviour {
  [SerializeReference] private GameObject m_holes_fill_overlay;
  [SerializeReference] private GameObject m_holes_image;
  [SerializeReference] private GameObject m_field_image;
  [SerializeReference] private GameObject m_holes_contour_overlay;

  public void Init(FieldData i_field_data, FieldGridConfiguration i_grid_configuration) {
    var holes = _GetHoles(i_field_data);
    var stroke_svg = new SVG();
    var fill_svg = new SVG();
    var fill_color = "rgba(20, 20, 20, 0.5)";
    var offset_value = i_grid_configuration.outer_grid_stroke_width / 2;
    var hole_stroke = new SVGStrokeProps("#000000", i_grid_configuration.outer_grid_stroke_width);
    var no_stroke = new SVGStrokeProps("none", 0);

    foreach (var hole in holes) {
      var paths = _GetHolePaths(hole);
      var fill_path = new SVGPath {
        fill_color = fill_color,
        stroke_props = no_stroke
      };
      var stroke_path = new SVGPath {
        fill_color = "rgba(20, 20, 20, 0.25)",
        stroke_props = hole_stroke
      };
      foreach (var path_points in paths) {
        // Remove last point as it's same as first one
        path_points.RemoveAt(path_points.Count - 1);
        if (path_points[0].Item1 == -1 && path_points[0].Item2 == -1) {
          // Extend outer hole to cover all unused outer space
          path_points.Clear();
          var max_row_count = Mathf.RoundToInt(Screen.height / i_grid_configuration.grid_step) + 1;
          var max_column_count = Mathf.RoundToInt(Screen.width / i_grid_configuration.grid_step) + 1;
          var additional_rows = Mathf.Max(0, max_row_count - i_field_data.configuration.height) / 2 + 1;
          var additional_columns = Mathf.Max(0, max_column_count - i_field_data.configuration.width) / 2 + 1;
          path_points.Add((-additional_rows, -additional_columns));
          path_points.Add((-additional_rows, i_field_data.configuration.width + additional_columns));
          path_points.Add((i_field_data.configuration.height + additional_rows, i_field_data.configuration.width + additional_columns));
          path_points.Add((i_field_data.configuration.height + additional_rows, -additional_columns));
        }

        var offset_path_points = new List<Vector2>(path_points.Count);
        foreach (var (row_id, column_id) in path_points)
          offset_path_points.Add(new Vector2(column_id * i_grid_configuration.grid_step, -row_id * i_grid_configuration.grid_step));

        fill_path.MoveTo(offset_path_points[0]);
        for (int point_id = 1; point_id < offset_path_points.Count; ++point_id)
          fill_path.LineTo(offset_path_points[point_id]);
        fill_path.Close();

        var prev_offset_direction = new Vector2(0, 0);
        for (int point_id = 0; point_id <= path_points.Count; ++point_id) {
          var curr_point_id = point_id == path_points.Count ? 0 : point_id;
          var prev_point_id = point_id == 0 ? path_points.Count - 1 : point_id - 1;
          var edge_direction = new Vector2(path_points[curr_point_id].Item2, path_points[curr_point_id].Item1) -
            new Vector2(path_points[prev_point_id].Item2, path_points[prev_point_id].Item1);
          var offset_direction = new Vector2(-edge_direction.y, -edge_direction.x);
          offset_direction *= offset_value;
          if (prev_offset_direction == new Vector2(0, 0)) {
            prev_offset_direction = offset_direction;
            continue;
          }
          if (Vector2.Dot(prev_offset_direction, offset_direction) == 0.0f) {
            offset_path_points[curr_point_id] = offset_path_points[curr_point_id] + offset_direction;
            offset_path_points[point_id - 1] = offset_path_points[point_id - 1] + offset_direction;
          } else
            offset_path_points[curr_point_id] = offset_path_points[curr_point_id] + offset_direction;
          prev_offset_direction = offset_direction;
        }

        stroke_path.MoveTo(offset_path_points[0]);
        for (int point_id = 1; point_id < offset_path_points.Count; ++point_id)
          stroke_path.LineTo(offset_path_points[point_id]);
        stroke_path.Close();
      }
      fill_svg.Add(fill_path);
      stroke_svg.Add(stroke_path);
    }

    m_holes_fill_overlay.transform.localPosition = i_grid_configuration.position;
    var holes_fill_mask = m_holes_fill_overlay.GetComponent<SpriteMask>();
    holes_fill_mask.sprite = SVG.BuildSprite(fill_svg, i_grid_configuration.grid_step);

    var background_image_size = m_holes_image.GetComponent<SpriteRenderer>().sprite.rect.size;
    var x_scale = (Screen.width + 2 * i_grid_configuration.outer_grid_stroke_width) / background_image_size.x;
    var y_scale = (Screen.height + 2 * i_grid_configuration.outer_grid_stroke_width) / background_image_size.x;
    m_holes_image.transform.localScale = new Vector3(x_scale, y_scale, 1);
    m_field_image.transform.localScale = new Vector3(x_scale, y_scale, 1);

    m_holes_contour_overlay.transform.localPosition = i_grid_configuration.position;
    var sprite_renderer = m_holes_contour_overlay.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = SVG.BuildSprite(stroke_svg, i_grid_configuration.grid_step);
  }

  private List<List<(int, int)>> _GetHoles(FieldData i_field_data) {
    var holes = i_field_data.GetHoles();
    var outer_hole = new List<(int, int)>();
    for (int row_id = -1; row_id <= i_field_data.configuration.height; ++row_id)
      for (int column_id = -1; column_id <= i_field_data.configuration.width; ++column_id) {
        if (row_id >= 0 && row_id < i_field_data.configuration.height &&
          column_id >= 0 && column_id < i_field_data.configuration.width)
          continue;
        outer_hole.Add((row_id, column_id));
      }
    for (int hole_id = 0; hole_id < holes.Count; ++hole_id) {
      bool is_outer = false;
      foreach (var (row_id, column_id) in holes[hole_id]) {
        if (row_id == 0 || row_id == i_field_data.configuration.height - 1 ||
          column_id == 0 || column_id == i_field_data.configuration.width - 1) {
          is_outer = true;
          break;
        }
      }
      if (is_outer) {
        outer_hole.AddRange(holes[hole_id].ToArray());
        holes.RemoveAt(hole_id);
        --hole_id;
      }
    }
    holes.Add(outer_hole);
    return holes;
  }

  private List<List<(int, int)>> _GetHolePaths(List<(int, int)> i_hole) {
    var hole_edges = new List<((int, int), (int, int))>();
    foreach (var (row_id, column_id) in i_hole) {
      hole_edges.Add(((row_id, column_id), (row_id, column_id + 1)));
      hole_edges.Add(((row_id, column_id + 1), (row_id + 1, column_id + 1)));
      hole_edges.Add(((row_id + 1, column_id + 1), (row_id + 1, column_id)));
      hole_edges.Add(((row_id + 1, column_id), (row_id, column_id)));
    }
    for (int i = 0; i < hole_edges.Count; ++i)
      for (int j = i + 1; j < hole_edges.Count; ++j)
        if (hole_edges[i].Item1 == hole_edges[j].Item2 && hole_edges[i].Item2 == hole_edges[j].Item1) {
          hole_edges.RemoveAt(j);
          hole_edges.RemoveAt(i);
          --i;
          break;
        }
    var paths = new List<List<(int, int)>>();
    int edge_id = hole_edges.Count;
    while (hole_edges.Count > 0) {
      if (edge_id >= hole_edges.Count) {
        paths.Add(new List<(int, int)>());
        paths[^1].Add(hole_edges[0].Item1);
        paths[^1].Add(hole_edges[0].Item2);
        hole_edges.RemoveAt(0);
        edge_id = 0;
        continue;
      }
      if (paths[^1][^1] == hole_edges[edge_id].Item1) {
        paths[^1].Add(hole_edges[edge_id].Item2);
        hole_edges.RemoveAt(edge_id);
        edge_id = 0;
      } else
        ++edge_id;
    }
    return paths;
  }
}