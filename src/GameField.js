import { useRef, useEffect, useState } from "react";

import { init_array, manhattan_distance } from "./Utils.js";
import { draw_line, draw_rect } from "./CanvasUtils.js";

class FieldData {
  #field;
  #width;
  #height;

  #values_interval;
  #values_probability_mask;

  #get_random_value() {
    const random = Math.random();
    var accumulated_probability = 0;
    for (let i = 0; i < this.#values_interval.length; ++i) {
      if (random <= accumulated_probability)
        return this.#values_interval[i - 1];
      accumulated_probability += this.#values_probability_mask[i];
    }
    return this.#values_interval[this.#values_interval.length - 1];
  }

  #get_cross_groups() {
    const check_row = (row_id, column_id) => {
      var r = column_id + 1;
      var l = column_id - 1;
      while (true) {
        if (r < this.#width &&
          this.#field[row_id][r] === this.#field[row_id][column_id])
          ++r;
        else if (l >= 0 &&
          this.#field[row_id][l] === this.#field[row_id][column_id])
          --l;
        else
          break;
      }
      return [r, l];
    };
    const check_column = (row_id, column_id) => {
      var r = row_id + 1;
      var l = row_id - 1;
      while (true) {
        if (r < this.#height &&
          this.#field[r][column_id] === this.#field[row_id][column_id])
          ++r;
        else if (l >= 0 &&
          this.#field[l][column_id] === this.#field[row_id][column_id])
          --l;
        else
          break;
      }
      return [r, l];
    };
    var taken = init_array(this.#width, this.#height, false);
    var groups = [];
    for (let row_id = 0; row_id < this.#height; ++row_id) {
      for (let column_id = 0; column_id < this.#width; ++column_id) {
        const component_value = this.#field[row_id][column_id];
        if (component_value === -1)
          continue;
        if (taken[row_id][column_id])
          continue;
        var group = [];
        var [row_r, row_l] = check_row(row_id, column_id);
        var [column_r, column_l] = check_column(row_id, column_id);
        if (row_r - row_l >= 4) {
          for (let i = row_l + 1; i < row_r; ++i) {
            group.push([row_id, i]);
            taken[row_id][i] = true;
            var [sub_column_r, sub_column_l] = check_column(row_id, i);
            if (sub_column_r - sub_column_l >= 4)
              for (let j = sub_column_l + 1; j < sub_column_r; ++j) {
                if (j === row_id)
                  continue;
                group.push([j, i]);
                taken[j][i] = true;
              }
          }
        } else if (column_r - column_l >= 4) {
          for (let i = column_l + 1; i < column_r; ++i) {
            group.push([i, column_id]);
            taken[i][column_id] = true;
            var [sub_row_r, sub_row_l] = check_row(i, column_id);
            if (sub_row_r - sub_row_l >= 4)
              for (let j = sub_row_l + 1; j < sub_row_r; ++j) {
                if (j === column_id)
                  continue;
                group.push([i, j]);
                taken[i][j] = true;
              }
          }
        }
        else
          continue;
        groups.push(group);
      }
    }
    return groups;
  }

  constructor(width, height) {
    this.#width = width;
    this.#height = height;

    this.#values_interval = [1, 2, 4, 8];
    this.#values_probability_mask = [0.4, 0.3, 0.2, 0.2];

    this.#field = init_array(this.#width, this.#height, undefined, () => {
      return this.#get_random_value();
    });

    while (true) {
      const removed_groups_sizes = this.remove_groups();
      if (removed_groups_sizes.length === 0)
        break;
      this.spawn_new_values();
    }
  }

  clone() {
    var new_field_data = new FieldData(0, 0);
    new_field_data.#field = this.#field;
    new_field_data.#width = this.#width;
    new_field_data.#height = this.#height;
    new_field_data.#values_interval = this.#values_interval;
    new_field_data.#values_probability_mask = this.#values_probability_mask;
    return new_field_data;
  }

  get width() {
    return this.#width;
  }

  get height() {
    return this.#height;
  }

  at(row, column) {
    return this.#field[row][column];
  }

  remove_groups(count = -1) {
    const groups = this.#get_cross_groups();
    if (count === -1)
      count = groups.length;
    count = Math.min(count, groups.length);
    if (count === 0)
      return [];
    var group_details = [];
    for (let group_id = 0; group_id < count; ++group_id) {
      const group = groups[group_id];
      group_details.push({
        size: group.length,
        value: this.#field[group[0][0]][group[0][1]]
      });
      for (let element of group)
        this.#field[element[0]][element[1]] = -1;
    }
    return group_details;
  }

  move_elements() {
    for (let column_id = 0; column_id < this.#width; ++column_id) {
      var empty_row_id = -1;
      for (let row_id = this.#height - 1; row_id >= 0;) {
        if (this.#field[row_id][column_id] === -1 && empty_row_id === -1) {
          empty_row_id = row_id;
        } else if (empty_row_id !== -1 && this.#field[row_id][column_id] !== -1) {
          [this.#field[row_id][column_id], this.#field[empty_row_id][column_id]] =
            [this.#field[empty_row_id][column_id], this.#field[row_id][column_id]];
          row_id = empty_row_id;
          empty_row_id = -1;
        }
        --row_id;
      }
    }
  }

  spawn_new_values() {
    for (let row_id = 0; row_id < this.#height; ++row_id)
      for (let column_id = 0; column_id < this.#width; ++column_id)
        if (this.#field[row_id][column_id] === -1)
          this.#field[row_id][column_id] = this.#get_random_value();
  }

  swap_cells(row1, column1, row2, column2) {
    if (row1 < 0 || row1 > this.#height || column1 < 0 || column1 > this.#width ||
      row2 < 0 || row2 > this.#height || column2 < 0 || column2 > this.#width)
      return;
    if (this.#field[row1][column1] === -1)
      return;
    if (this.#field[row2][column2] === -1)
      return;
    [this.#field[row1][column1], this.#field[row2][column2]] =
      [this.#field[row2][column2], this.#field[row1][column1]];
  }
}

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

function render_field_elements(context, field_data, grid_step, element_offset) {
  const element_size = grid_step - 2 * element_offset;
  for (let row_id = 0; row_id < field_data.height; ++row_id) {
    const y = row_id * grid_step + element_offset;
    for (let column_id = 0; column_id < field_data.width; ++column_id) {
      const x = column_id * grid_step + element_offset;
      const value = field_data.at(row_id, column_id);
      if (value === -1)
        continue;
      draw_rect(context, x, y, element_size, element_size);
      context.font = `${element_size * 0.75}px serif`;
      context.textAlign = "center";
      context.textBaseline = "middle";
      context.fillText(value, x + element_size / 2, y + element_size / 2, element_size);
    }
  }
}

function highlight(context, row, column, grid_step, element_offset) {
  const element_size = grid_step - 2 * element_offset;
  const x = column * grid_step + 2 * element_offset;
  const y = row * grid_step + 2 * element_offset;
  draw_rect(context, x, y, element_size - 2 * element_offset, element_size - 2 * element_offset, 1, "#ff0000");
}

function render(context, field_data, grid_step, element_offset, highlighted_elements) {
  const width = context.canvas.width;
  const height = context.canvas.height;
  context.clearRect(0, 0, width, height);
  for (const highlighted_element of highlighted_elements)
    highlight(context, highlighted_element[0], highlighted_element[1], grid_step, element_offset);
  draw_rect(context, 0, 0, width, height, 5);
  render_grid(context, field_data, grid_step);
  render_field_elements(context, field_data, grid_step, element_offset);
}

export default function GameField({ width, height, onStrike }) {
  const canvas_ref = useRef(null);

  var field_data = new FieldData(width, height);
  var first_element = [];
  var second_element = [];
  var swapping = false;

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
    element_offset = Math.floor(0.05 * grid_step);
    context.canvas.width = grid_step * field_data.width;
    context.canvas.height = grid_step * field_data.height;
    context.canvas.style.top = `${(window.innerHeight - context.canvas.width) / 2}px`;
    context.canvas.style.left = `${(window.innerWidth - context.canvas.height) / 2}px`;

    update_game_state(context, -1, -1);
  });

  const update_game_state = (context, prev_step, step) => {
    const steps = 4;
    let next_step = (step + 1) % steps;
    switch (step) {
      case 0:
        const removed_groups_details = field_data.remove_groups(1);
        const changed = removed_groups_details.length > 0;
        if (changed) {
          next_step = step;
          for (let removed_group_details of removed_groups_details)
            onStrike(removed_group_details.value, removed_group_details.size);
        }
        if (prev_step === 3) {
          if (!changed)
            next_step = 3;
          else {
            first_element = [];
            second_element = [];
          }
        }
        break;
      case 1:
        field_data.move_elements();
        break;
      case 2:
        field_data.spawn_new_values();
        break;
      case 3:
        if (first_element.length !== 0 && second_element.length !== 0) {
          field_data.swap_cells(first_element[0], first_element[1], second_element[0], second_element[1]);
          if (prev_step === 0 && swapping) {
            first_element = [];
            second_element = [];
            swapping = false;
          } else
            swapping = !swapping;
        }
        break;
      default:
        break;
    }
    var highlighted_elements = [];
    if (first_element.length > 0)
      highlighted_elements.push(first_element);
    if (second_element.length > 0)
      highlighted_elements.push(second_element);
    render(context, field_data, grid_step, element_offset, highlighted_elements);
    setTimeout(update_game_state, 500, context, step, next_step);
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
        first_element = field_element_coordinates;
      else
        second_element = field_element_coordinates;
    }
  };

  const on_mouse_move = (event) => {
  };

  return (
    <canvas
      className="gamefield"
      ref={canvas_ref}
      onMouseDown={on_click}
      onMouseMove={on_mouse_move}
    />
  );
}