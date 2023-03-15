import { useRef, useEffect, useState } from "react";
import { StyleSheet, View } from 'react-native';

import { FieldData } from "./GameFieldData.js";
import { manhattan_distance } from "./Utils.js";
import { draw_line, draw_regular_polygon } from "./CanvasUtils.js";

function map_coordinates(x, y, grid_step) {
  return [
    Math.floor(y / grid_step),
    Math.floor(x / grid_step)
  ];
}

function render_grid(context, field_data, grid_step) {
  const width = context.canvas.width;
  const height = context.canvas.height;
  for (let line_id = 0; line_id <= field_data.width || line_id <= field_data.height; ++line_id) {
    if (line_id <= field_data.width) {
      const x = line_id * grid_step;
      draw_line(context, x, 0, x, height);
    }
    if (line_id <= field_data.height) {
      const y = line_id * grid_step;
      draw_line(context, 0, y, width, y);
    }
  }
}

function element_style_by_value(value) {
  var pow = 0;
  while (value > 1) {
    ++pow;
    value = value >> 1;
  }
  const triangle_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2, Math.floor(0.1 * size));
  };
  const square_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + size / 2], 10 * size / 14,
      4, -Math.PI / 4, Math.floor(0.1 * size));
  };
  const pentagon_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + size / 2], 9 * size / 16,
      5, -Math.PI / 2, Math.floor(0.1 * size));
  };
  const hexagon_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + size / 2], 9 * size / 16,
      6, 0, Math.floor(0.1 * size));
  };
  const rotated_triangle_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + 3 * size / 8], 11 * size / 16,
      3, Math.PI / 2, Math.floor(0.1 * size));
  };
  const rotated_square_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + size / 2], 8 * size / 14,
      4, 0, Math.floor(0.1 * size));
  };
  const octagon_drawer = (context, x, y, size) => {
    draw_regular_polygon(context, [x + size / 2, y + size / 2], 8 * size / 14,
      8, Math.PI / 8, Math.floor(0.1 * size));
  }
  const drawers = [triangle_drawer, square_drawer, pentagon_drawer, hexagon_drawer,
    rotated_triangle_drawer, rotated_square_drawer, octagon_drawer];
  const colors = ["#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA", "#FFFB28", "#FF6D0A"];
  return [colors[pow % colors.length], drawers[pow % drawers.length]];
}

function render_element(context, x, y, value, element_size, element_offset, is_highlighted = false) {
  const [color, shape_drawer] = element_style_by_value(value);

  context.beginPath();

  if (!is_highlighted) {
    context.shadowBlur = element_offset;
    context.shadowColor = "rgba(0,0,0,1)";
  }

  context.fillStyle = color;
  shape_drawer(context, x, y, element_size);
  context.fill();

  context.lineWidth = 1;
  context.strokeStyle = "#000000";
  shape_drawer(context, x, y, element_size);
  context.stroke();
  context.stroke();
  context.stroke();

  context.shadowBlur = 0;

  context.font = `${element_size * 0.75}px candara`;
  context.textAlign = "center";
  context.textBaseline = "middle";
  context.fillStyle = "#ffffff";
  context.fillText(value, x + element_size / 2, y + element_size / 2, element_size);

  context.lineWidth = 1;
  context.strokeStyle = "#000000";
  context.strokeText(value, x + element_size / 2, y + element_size / 2, element_size);

  context.closePath();
}

function render_field_elements(context, field_data, grid_step, element_offset, highlighted_elements) {
  const element_size = grid_step - 2 * element_offset;
  for (let row_id = 0; row_id < field_data.height; ++row_id) {
    const y = row_id * grid_step + element_offset;
    for (let column_id = 0; column_id < field_data.width; ++column_id) {
      const x = column_id * grid_step + element_offset;
      const value = field_data.at(row_id, column_id);
      if (value === -1)
        continue;
      var is_highlighted = false;
      for (let highlighted_element of highlighted_elements)
        if (row_id === highlighted_element[0] && column_id === highlighted_element[1]) {
          is_highlighted = true;
          break;
        }
      render_element(context, x, y, value, element_size, element_offset, is_highlighted);
    }
  }
}

function render(context, field_data, grid_step, element_offset, highlighted_elements) {
  const width = context.canvas.width;
  const height = context.canvas.height;
  context.clearRect(0, 0, width, height);
  context.lineWidth = 5;
  context.fillStyle = "#dddddd";
  context.strokeStyle = "#000000";
  context.rect(0, 0, width, height);
  context.fill();
  context.stroke();
  render_grid(context, field_data, grid_step);
  render_field_elements(context, field_data, grid_step, element_offset, highlighted_elements);
}

export default function GameField({ width, height, onStrike, onMovesCountChange }) {
  const canvas_ref = useRef(null);

  const [field_data, set_field_data] = useState(new FieldData(width, height));
  const [first_element, set_first_element] = useState([]);
  const [second_element, set_second_element] = useState([]);
  const [swapping, set_swapping] = useState(false);
  const [step, set_step] = useState(-1);
  const [prev_step, set_prev_step] = useState(-1);

  var grid_step = 0;
  var element_offset = 0;

  useEffect(() => {
    const canvas = canvas_ref.current;
    const context = canvas.getContext("2d");
    const active_zone_fraction = 0.75;
    context.canvas.height = window.innerHeight * active_zone_fraction;
    context.canvas.width = window.innerWidth * active_zone_fraction;
    const grid_x_step = Math.floor(context.canvas.width / field_data.width);
    const grid_y_step = Math.floor(context.canvas.height / field_data.height);
    grid_step = Math.min(grid_x_step, grid_y_step);
    element_offset = Math.floor(0.1 * grid_step);
    context.canvas.width = grid_step * field_data.width;
    context.canvas.height = grid_step * field_data.height;
    context.canvas.style.top = `${(window.innerHeight - context.canvas.width) / 2}px`;
    context.canvas.style.left = `${(window.innerWidth - context.canvas.height) / 2}px`;

    var highlighted_elements = [];
    if (first_element.length > 0)
      highlighted_elements.push(first_element);
    if (second_element.length > 0)
      highlighted_elements.push(second_element);
    const moves = field_data.get_all_moves();
    onMovesCountChange(moves.length);
    render(context, field_data, grid_step, element_offset, highlighted_elements);
    update_game_state();
  });

  const update_game_state = () => {
    const steps = 4;
    let next_step = (step + 1) % steps;
    switch (step) {
      case 0:
        const removed_groups_details = field_data.accumulate_groups(1);
        const changed = removed_groups_details.length > 0;
        set_prev_step(step);
        if (changed) {
          for (let removed_group_details of removed_groups_details)
            onStrike(removed_group_details.value, removed_group_details.size);
          set_field_data(field_data.clone());
          if (prev_step === 3) {
            set_first_element([]);
            set_second_element([]);
          }
        } else if (prev_step === 3) {
          set_prev_step(0);
          set_step(3);
        } else if (prev_step != step) {
          set_step(-1);
          set_prev_step(-1);
        } else
          set_step(1);
        break;
      case 1:
        field_data.move_elements();
        set_field_data(field_data.clone());
        set_prev_step(step);
        set_step(next_step);
        break;
      case 2:
        field_data.spawn_new_values();
        set_field_data(field_data.clone());
        set_prev_step(step);
        set_step(0);
        break;
      case 3:
        if (first_element.length !== 0 && second_element.length !== 0) {
          field_data.swap_cells(first_element[0], first_element[1], second_element[0], second_element[1]);
          set_field_data(field_data.clone());
          if (prev_step === 0 && swapping) {
            set_first_element([]);
            set_second_element([]);
            set_swapping(false);
            set_step(-1);
            set_prev_step(-1);
          } else {
            set_prev_step(step);
            set_step(0);
            set_swapping(!swapping);
          }
        }
        break;
      default:
        break;
    }
  }

  const on_click = (event) => {
    const x = event.nativeEvent.offsetX;
    const y = event.nativeEvent.offsetY;

    const field_element_coordinates = map_coordinates(x, y, grid_step);
    if (field_element_coordinates.length !== 0) {
      const distance = manhattan_distance(
        first_element[0], first_element[1],
        field_element_coordinates[0], field_element_coordinates[1]
      );
      if (first_element.length === 0 || distance > 1)
        set_first_element(field_element_coordinates);
      else if (distance === 0)
        set_first_element([]);
      else if (second_element.length === 0) {
        set_second_element(field_element_coordinates);
        set_step(3);
      }
    }
  };

  const on_mouse_move = (event) => {
  };

  const shuffle = () => {
    field_data.shuffle();
    set_field_data(field_data.clone());
    set_step(0);
  }

  return (
    <View style={styles.elements_container}>
      <canvas
        ref={canvas_ref}
        onMouseDown={on_click}
        onMouseMove={on_mouse_move}
      />
      <View style={styles.abilities_container}>
        <button onClick={shuffle}>Shuffle</button>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  elements_container: {
    flexDirection: 'column'
  },

  abilities_container: {
    flexDirection: 'row'
  }
});