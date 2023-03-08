import { useRef, useEffect } from "react";

import { init_array, manhattan_distance } from "./Utils.js";
import { draw_line, draw_rect } from "./CanvasUtils.js";

import './style/App.css';

class FieldElement {
  #value;
  #size;

  constructor(value, size) {
    this.#value = value;
    this.#size = size;
  }

  get value() {
    return this.#value;
  }

  render(context, x, y) {
    if (this.#value === -1)
      return;
    draw_rect(context, x, y, this.#size, this.#size);
    context.font = "48px serif";
    context.textAlign = "center";
    context.textBaseline = "middle";
    context.fillText(this.#value, x + this.#size / 2, y + this.#size / 2, this.#size);
  }
}

class GameField {
  #min_x;
  #min_y;
  #max_x;
  #max_y;
  #width;
  #height;
  #field;
  #grid_step;
  #element_offset;
  #element_size;
  #values_interval;
  #values_probability_mask;

  constructor() {
    this.#field = [];
    this.#grid_step = 100;
    this.#element_offset = 5;
    this.#element_size = this.#grid_step - 2 * this.#element_offset;

    this.#values_interval = [1, 2, 4];
    this.#values_probability_mask = [0.5, 0.3, 0.2];
  }

  init(context, width, height) {
    const context_width = context.canvas.width;
    const context_height = context.canvas.height;
    this.#width = width;
    this.#height = height;
    const grid_width = this.#grid_step * this.#width;
    const grid_height = this.#grid_step * this.#height;
    this.#min_x = (context_width - grid_width) / 2;
    this.#max_x = this.#min_x + grid_width;
    this.#min_y = (context_height - grid_height) / 2;
    this.#max_y = this.#min_y + grid_height;
    this.#field = init_array(this.#width, this.#height, undefined, () => {
      const value = this.#get_random_value();
      return new FieldElement(value, this.#element_size)
    });
    while (true) {
      const removed_groups_sizes = this.remove_groups();
      if (removed_groups_sizes.length === 0)
        break;
      this.spawn_new_values();
    }
  }

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

  #render_grid(context) {
    for (let line_id = 0; line_id <= this.#width || line_id <= this.#height; ++line_id) {
      if (line_id <= this.#width) {
        const x = line_id * this.#grid_step + this.#min_x;
        draw_line(context, x, this.#min_y, x, this.#max_y);
      }
      if (line_id <= this.#height) {
        const y = line_id * this.#grid_step + this.#min_y;
        draw_line(context, this.#min_x, y, this.#max_x, y);
      }
    }
  }

  #render_field_elements(context) {
    for (let row_id = 0; row_id < this.#field.length; ++row_id) {
      const y = row_id * this.#grid_step + this.#min_y + this.#element_offset;
      for (let column_id = 0; column_id < this.#field[0].length; ++column_id) {
        const x = column_id * this.#grid_step + this.#min_x + this.#element_offset;
        this.#field[row_id][column_id].render(context, x, y)
      }
    }
  }

  #get_groups() {
    var visited = init_array(this.#width, this.#height, false);
    var groups = [];
    for (let row_id = 0; row_id < this.#height; ++row_id) {
      for (let column_id = 0; column_id < this.#width; ++column_id) {
        if (visited[row_id][column_id])
          continue;
        var to_traverse = [[row_id, column_id]];
        var group = []
        const component_value = this.#field[row_id][column_id].value;
        if (component_value === -1)
          continue;
        while (to_traverse.length > 0) {
          const current = to_traverse.shift();
          const current_row_id = current[0];
          const current_column_id = current[1];
          if (current_column_id < 0 || current_column_id >= this.#width)
            continue;
          if (current_row_id < 0 || current_row_id >= this.#height)
            continue;
          if (this.#field[current_row_id][current_column_id].value !== component_value)
            continue;
          if (visited[current_row_id][current_column_id])
            continue;
          visited[current_row_id][current_column_id] = true;
          group.push(current);
          to_traverse.push([current_row_id - 1, current_column_id]);
          to_traverse.push([current_row_id + 1, current_column_id]);
          to_traverse.push([current_row_id, current_column_id + 1]);
          to_traverse.push([current_row_id, current_column_id - 1]);
        }
        if (group.length < 3)
          continue;
        var row_lengths = init_array(this.#height, 1, 0);
        var column_lengths = init_array(this.#width, 1, 0);
        for (let component_element of group) {
          ++row_lengths[component_element[0]];
          ++column_lengths[component_element[1]];
        }
        if (Math.max(...row_lengths) < 3 && Math.max(...column_lengths) < 3)
          continue;
        groups.push(group);
      }
    }
    return groups;
  }

  #get_cross_groups() {
    const check_row = (row_id, column_id) => {
      var r = column_id + 1;
      var l = column_id - 1;
      while (true) {
        if (r < this.#width &&
          this.#field[row_id][r].value === this.#field[row_id][column_id].value)
          ++r;
        else if (l >= 0 &&
          this.#field[row_id][l].value === this.#field[row_id][column_id].value)
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
          this.#field[r][column_id].value === this.#field[row_id][column_id].value)
          ++r;
        else if (l >= 0 &&
          this.#field[l][column_id].value === this.#field[row_id][column_id].value)
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
        const component_value = this.#field[row_id][column_id].value;
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

  #highlight_components(context) {
    var groups = this.#get_cross_groups();
    for (let group of groups) {
      for (let component_element of group) {
        const x = component_element[1] * this.#grid_step + this.#min_x + 2 * this.#element_offset;
        const y = component_element[0] * this.#grid_step + this.#min_y + 2 * this.#element_offset;
        draw_rect(context, x, y, this.#element_size - 2 * this.#element_offset, this.#element_size - 2 * this.#element_offset, 1, "#ff0000");
      }
    }
  }

  highlight(context, row, column) {
    const x = column * this.#grid_step + this.#min_x + 2 * this.#element_offset;
    const y = row * this.#grid_step + this.#min_y + 2 * this.#element_offset;
    draw_rect(context, x, y, this.#element_size - 2 * this.#element_offset, this.#element_size - 2 * this.#element_offset, 1, "#ff0000");
  }

  render(context) {
    const width = context.canvas.width;
    const height = context.canvas.height;
    draw_rect(context, 0, 0, width, height, 5);
    this.#render_grid(context);
    this.#render_field_elements(context);
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
        value: this.#field[group[0][0]][group[0][1]].value
      });
      for (let element of group)
        this.#field[element[0]][element[1]] = new FieldElement(-1, this.#element_size);
    }
    return group_details;
  }

  move_elements() {
    for (let column_id = 0; column_id < this.#width; ++column_id) {
      var empty_row_id = -1;
      for (let row_id = this.#height - 1; row_id >= 0;) {
        if (this.#field[row_id][column_id].value === -1 && empty_row_id === -1) {
          empty_row_id = row_id;
        } else if (empty_row_id !== -1 && this.#field[row_id][column_id].value !== -1) {
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
        if (this.#field[row_id][column_id].value === -1)
          this.#field[row_id][column_id] = new FieldElement(this.#get_random_value(), this.#element_size);
  }

  get_cell_coordinates(x, y) {
    if (x < this.#min_x || x > this.#max_x || y < this.#min_y || y > this.#max_y)
      return [];
    return [
      Math.floor((y - this.#min_y) / this.#grid_step),
      Math.floor((x - this.#min_x) / this.#grid_step)
    ];
  }

  swap_cells(row1, column1, row2, column2) {
    if (row1 < 0 || row1 > this.#height || column1 < 0 || column1 > this.#width ||
      row2 < 0 || row2 > this.#height || column2 < 0 || column2 > this.#width)
      return;
    if (this.#field[row1][column1].value === -1)
      return;
    if (this.#field[row2][column2].value === -1)
      return;
    [this.#field[row1][column1], this.#field[row2][column2]] =
      [this.#field[row2][column2], this.#field[row1][column1]];
  }
}

function App() {
  const canvas_ref = useRef(null);
  const score_ref = useRef(null);
  const elements_count_ref = useRef(null);

  var game_field = new GameField();
  var first_element = [];
  var second_element = [];
  var elements_count = {};
  var swapping = false;

  const update_elements_count = (value, count) => {
    if (value in elements_count)
      elements_count[value] += count;
    else
      elements_count[value] = count;
    const elements_counts_container = elements_count_ref.current;

  }

  const update_game_state = (context, prev_step, step) => {
    const steps = 4;
    let next_step = (step + 1) % steps;
    switch (step) {
      case 0:
        const removed_groups_details = game_field.remove_groups(1);
        const changed = removed_groups_details.length > 0;
        if (changed) {
          next_step = step;
          var score = parseInt(score_ref.current.textContent);
          for (let removed_group_details of removed_groups_details) {
            score += removed_group_details.size * removed_group_details.value;
            update_elements_count(removed_group_details.value, removed_group_details.size);
          }
          score_ref.current.textContent = score;
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
        game_field.move_elements();
        break;
      case 2:
        game_field.spawn_new_values();
        break;
      case 3:
        if (first_element.length !== 0 && second_element.length !== 0) {
          game_field.swap_cells(first_element[0], first_element[1], second_element[0], second_element[1]);
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
    context.clearRect(0, 0, context.canvas.width, context.canvas.height);
    if (first_element.length !== 0)
      game_field.highlight(context, first_element[0], first_element[1]);
    if (second_element.length !== 0)
      game_field.highlight(context, second_element[0], second_element[1]);
    game_field.render(context);
    setTimeout(update_game_state, 500, context, step, next_step);
  }

  useEffect(() => {
    const canvas = canvas_ref.current;
    const context = canvas.getContext("2d");
    const active_zone_fraction = 0.75;
    context.canvas.style.top = `${window.innerHeight * (1 - active_zone_fraction) / 2}px`;
    context.canvas.style.left = `${window.innerWidth * (1 - active_zone_fraction) / 2}px`;
    context.canvas.height = window.innerHeight * active_zone_fraction;
    context.canvas.width = window.innerWidth * active_zone_fraction;

    game_field.init(context, 7, 7);
    update_game_state(context, -1, -1);
  });

  const on_click = (event) => {
    const x = event.nativeEvent.offsetX;
    const y = event.nativeEvent.offsetY;

    const field_element_coordinates = game_field.get_cell_coordinates(x, y);
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
    <div className="app-container">
      <div className="stats-container">
        <div className="score-container">
          Score
          <div
            className="score-wrapper"
            ref={score_ref}>
            0
          </div>
        </div>
        <div
          className="elements-count-container"
          ref={elements_count_ref}>
        </div>
      </div>
      <canvas
        className="gamefield"
        ref={canvas_ref}
        onMouseDown={on_click}
        onMouseMove={on_mouse_move}
      />
    </div>
  );
}

export default App;
