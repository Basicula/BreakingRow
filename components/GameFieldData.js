import { init_array } from "./Utils.js";

export class FieldData {
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

    this.#values_interval = [0, 1, 2, 3];
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

  increase_values_interval() {
    for (let i = 0; i < this.#values_interval.length; ++i)
      ++this.#values_interval[i];
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

  accumulate_groups(count = -1) {
    const groups = this.#get_cross_groups();
    if (count === -1)
      count = groups.length;
    count = Math.min(count, groups.length);
    if (count === 0)
      return [];
    var group_details = [];
    for (let group_id = 0; group_id < count; ++group_id) {
      const group = groups[group_id];
      const value = this.#field[group[0][0]][group[0][1]];
      group_details.push({
        size: group.length,
        value: value
      });
      var accumulated_value = 2 ** value * group.length;
      var values = [];
      var pow = 0;
      while (accumulated_value > 0) {
        if (accumulated_value & 1 === 1)
          values.push(pow);
        accumulated_value = accumulated_value / 2;
        ++pow;
      }
      for (let i = 0; i < group.length; ++i) {
        const j = Math.floor(Math.random() * group.length);
        [group[i], group[j]] = [group[j], group[i]];
      }
      for (let element of group) {
        var new_value = -1;
        if (values.length > 0)
          new_value = values.pop();
        this.#field[element[0]][element[1]] = new_value;
      }
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
    if (row1 < 0 || row1 >= this.#height || column1 < 0 || column1 >= this.#width ||
      row2 < 0 || row2 >= this.#height || column2 < 0 || column2 >= this.#width)
      return;
    if (this.#field[row1][column1] === -1)
      return;
    if (this.#field[row2][column2] === -1)
      return;
    [this.#field[row1][column1], this.#field[row2][column2]] =
      [this.#field[row2][column2], this.#field[row1][column1]];
  }

  get_all_moves() {
    const neighbors = [[0, 1], [0, -1], [1, 0], [-1, 0]];
    var moves = [];
    for (let row_id = 0; row_id < this.#height; ++row_id)
      for (let column_id = 0; column_id < this.#width; ++column_id)
        for (let neighbor of neighbors) {
          const neighbor_row_id = row_id + neighbor[0];
          const neighbor_column_id = column_id + neighbor[1];
          if (neighbor_row_id < 0 || neighbor_row_id >= this.#height ||
            neighbor_column_id < 0 || neighbor_column_id >= this.#width)
            continue;
          this.swap_cells(row_id, column_id, neighbor_row_id, neighbor_column_id);
          if (this.#get_cross_groups().length > 0) {
            let move_exists = false;
            for (let move of moves)
              if (row_id === move[1][0] && column_id === move[1][1] &&
                neighbor_row_id === move[0][0] && column_id === move[0][1]) {
                move_exists = true;
                break;
              }
            if (!move_exists)
              moves.push([[row_id, column_id], [neighbor_row_id, neighbor_column_id]]);
          }
          this.swap_cells(row_id, column_id, neighbor_row_id, neighbor_column_id);
        }
    return moves;
  }

  shuffle() {
    do {
      for (let row_id = 0; row_id < this.#height; ++row_id)
        for (let column_id = 0; column_id < this.#width; ++column_id) {
          const other_row_id = Math.floor(Math.random() * this.#height);
          const other_column_id = Math.floor(Math.random() * this.#width);
          this.swap_cells(row_id, column_id, other_row_id, other_column_id);
        }
    } while (this.get_all_moves().length === 0);
  }
}