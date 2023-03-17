import { useRef, useEffect, useState } from "react";
import { StyleSheet, View, TouchableOpacity, Text, Platform, Dimensions } from 'react-native';
import Canvas, {Path2D as ReactNativeCanvasPath2D} from 'react-native-canvas';

import { FieldData } from "./GameFieldData.js";
import { manhattan_distance } from "./Utils.js";
import { draw_line, draw_regular_polygon, draw_star } from "./CanvasUtils.js";

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
  const triangle_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2, Math.floor(0.1 * size));
  };
  const square_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + size / 2], 10 * size / 14,
      4, -Math.PI / 4, Math.floor(0.1 * size));
  };
  const pentagon_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + size / 2], 9 * size / 16,
      5, -Math.PI / 2, Math.floor(0.1 * size));
  };
  const hexagon_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + size / 2], 9 * size / 16,
      6, 0, Math.floor(0.1 * size));
  };
  const rotated_triangle_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + 3 * size / 8], 11 * size / 16,
      3, Math.PI / 2, Math.floor(0.1 * size));
  };
  const rotated_square_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + size / 2], 8 * size / 14,
      4, 0, Math.floor(0.1 * size));
  };
  const octagon_drawer = (path, x, y, size) => {
    draw_regular_polygon(path, [x + size / 2, y + size / 2], 8 * size / 14,
      8, Math.PI / 8, Math.floor(0.1 * size));
  }
  const star_5 = (path, x, y, size) => {
    draw_star(path, [x + size / 2, y + size / 2], 8 * size / 14,
      5, Math.PI / 12, Math.floor(0.05 * size));
  };
  const star_7 = (path, x, y, size) => {
    draw_star(path, [x + size / 2, y + size / 2], 9 * size / 14,
      7, -Math.PI / 14, Math.floor(0.05 * size));
  }
  const drawers = [triangle_drawer, square_drawer, pentagon_drawer, hexagon_drawer,
    rotated_triangle_drawer, rotated_square_drawer, octagon_drawer, star_5, star_7];
  const colors = ["#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA", "#FFFB28", "#FF6D0A"];
  return [colors[value % colors.length], drawers[value % drawers.length]];
}

function render_element(context, x, y, value, element_size, element_offset, is_highlighted = false) {
  const [color, shape_drawer] = element_style_by_value(value);

  if (!is_highlighted) {
    context.shadowBlur = element_offset;
    context.shadowColor = "rgba(0,0,0,1)";
  }

  context.fillStyle = color;
  var shape_path = undefined;
  if (Platform.OS === "web")
    shape_path = new Path2D();
  else
    shape_path = new ReactNativeCanvasPath2D(context.canvas);
  shape_drawer(shape_path, x, y, element_size);
  context.fill(shape_path);

  context.lineWidth = 1;
  context.strokeStyle = "#000000";
  context.stroke(shape_path);
  context.stroke(shape_path);
  context.stroke(shape_path);

  context.shadowBlur = 0;

  context.font = `${element_size * 0.75}px candara`;
  context.textAlign = "center";
  context.textBaseline = "middle";
  context.fillStyle = "#ffffff";
  const exponents = {
    "0": "⁰", "1": "¹", "2": "²", "3": "³", "4": "⁴",
    "5": "⁵", "6": "⁶", "7": "⁷", "8": "⁸", "9": "⁹"
  }
  let regex = RegExp(`[${Object.keys(exponents).join("")}]`, "g");
  const exponent = value.toString().replace(regex, number => exponents[number]);
  const value_text = value > 12 ? `2${exponent}` : (2 ** value).toString();
  context.fillText(value_text, x + element_size / 2, y + element_size / 2, element_size);

  context.lineWidth = 1;
  context.strokeStyle = "#000000";
  context.strokeText(value_text, x + element_size / 2, y + element_size / 2, element_size);
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

export default function Game({ width, height, score_bonuses, onStrike }) {
  const canvas_ref = useRef(null);
  const [mouse_down_position, set_mouse_down_position] = useState([]);
  const [field_data, set_field_data] = useState(new FieldData(width, height));
  const [first_element, set_first_element] = useState([]);
  const [second_element, set_second_element] = useState([]);
  const [swapping, set_swapping] = useState(false);
  const [step, set_step] = useState(-1);
  const [prev_step, set_prev_step] = useState(-1);
  const [score, set_score] = useState(0);
  const [moves_count, set_moves_count] = useState(0);
  const [shuffle_price, set_shuffle_price] = useState(1024);
  const [generator_upgrade_price, set_generator_upgrade_price] = useState(1024);

  var grid_step = 0;
  var element_offset = 0;

  useEffect(() => {
    if (!canvas_ref.current)
      return;
    const canvas = canvas_ref.current;
    const context = canvas.getContext("2d");
    const active_zone_fraction = 0.75;
    if (Platform.OS === "web") {
      context.canvas.height = window.innerHeight * active_zone_fraction;
      context.canvas.width = window.innerWidth * active_zone_fraction;
    } else {
      context.canvas.height = Dimensions.get("window").height * active_zone_fraction;
      context.canvas.width = Dimensions.get("window").width * active_zone_fraction;
    }
    const grid_x_step = Math.floor(context.canvas.width / field_data.width);
    const grid_y_step = Math.floor(context.canvas.height / field_data.height);
    grid_step = Math.min(grid_x_step, grid_y_step);
    element_offset = Math.floor(0.1 * grid_step);
    context.canvas.width = grid_step * field_data.width;
    context.canvas.height = grid_step * field_data.height;

    var highlighted_elements = [];
    if (first_element.length > 0)
      highlighted_elements.push(first_element);
    if (second_element.length > 0)
      highlighted_elements.push(second_element);
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
          for (let removed_group_details of removed_groups_details) {
            const value = 2 ** removed_group_details.value;
            const count = removed_group_details.size;
            onStrike(value, count);
            const bonus = count in score_bonuses ? score_bonuses[count] : 10;
            const new_score = score + value * count * bonus;
            set_score(new_score);
          }
          set_field_data(field_data.clone());
          if (prev_step === 3) {
            set_first_element([]);
            set_second_element([]);
            set_swapping(false);
          }
        } else if (prev_step === 3) {
          set_prev_step(0);
          set_step(3);
        } else if (prev_step != step) {
          set_step(-1);
          set_prev_step(-1);
        } else
          set_step(1);
        set_moves_count(field_data.get_all_moves().length);
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

  const get_event_position = (event) => {
    var x, y;
    if (Platform.OS === "web") {
      x = event.nativeEvent.offsetX;
      y = event.nativeEvent.offsetY;
    } else {
      x = event.nativeEvent.locationX;
      y = event.nativeEvent.locationY;
    }
    return [x, y];
  }

  const on_mouse_down = (event) => {
    const [x, y] = get_event_position(event);
    const field_element_coordinates = map_coordinates(x, y, grid_step);
    if (field_element_coordinates.length !== 0) {
      const distance = manhattan_distance(
        first_element[0], first_element[1],
        field_element_coordinates[0], field_element_coordinates[1]
      );
      if (first_element.length === 0 || distance > 1) {
        set_first_element(field_element_coordinates);
        set_mouse_down_position([x, y]);
      }
      else if (distance === 0) {
        set_first_element([]);
        set_mouse_down_position([]);
      }
      else if (second_element.length === 0) {
        set_second_element(field_element_coordinates);
        set_step(3);
      }
    }
  };

  const on_mouse_move = () => {
  };

  const on_mouse_up = (event) => {
    if (swapping)
      return;
    if (mouse_down_position.length === 0)
      return;
    if (second_element.length > 0)
      return;
    if (first_element.length === 0)
      return;
    const [x, y] = get_event_position(event);
    const dx = x - mouse_down_position[0];
    const dy = y - mouse_down_position[1];
    if (Math.abs(dx) < grid_step / 4 && Math.abs(dy) < grid_step / 4)
      return;
    var factors = [0, 0];
    if (Math.abs(dx) > Math.abs(dy))
      factors[0] = 1 * Math.sign(dx);
    else
      factors[1] = 1 * Math.sign(dy);
    set_second_element([first_element[0] + factors[1], first_element[1] + factors[0]])
    set_step(3);
    set_mouse_down_position([]);
  };

  const shuffle = () => {
    if (score < shuffle_price)
      return;
    field_data.shuffle();
    set_field_data(field_data.clone());
    set_step(0);
    set_score(score - shuffle_price);
    set_shuffle_price(shuffle_price * 2);
  };

  const upgrade_generator = () => {
    if (score < generator_upgrade_price)
      return;
    field_data.increase_values_interval();
    const small_value = field_data.values_interval[0] - 1;
    const values_count = field_data.remove_value(small_value);
    set_field_data(field_data.clone());
    set_step(1);
    const new_score = score - generator_upgrade_price + values_count * 2 ** small_value;
    set_score(new_score);
    set_generator_upgrade_price(generator_upgrade_price * 4);
  };

  return (
    <View style={styles.elements_container}>
      <View style={styles.score_container}>
        <Text style={styles.score_title_container}>Score</Text>
        <Text style={styles.score_value_container}>{score}</Text>
        <Text style={styles.moves_count_title_container}>Moves count</Text>
        <Text style={styles.moves_count_value_container}>{moves_count}</Text>
      </View>
      <View
        style={styles.canvas_container}
        onMouseDown={on_mouse_down}
        onTouchStart={on_mouse_down}
        onMouseUp={on_mouse_up}
        onTouchEnd={on_mouse_up}
        onMouseMove={on_mouse_move}
      >
        {Platform.OS === 'web' && <canvas ref={canvas_ref} />}
        {Platform.OS === 'android' && <Canvas ref={canvas_ref} />}
      </View>
      <View style={styles.abilities_container}>
        <TouchableOpacity style={styles.ability_button} onPress={shuffle} >
          <Text style={styles.ability_button_text}>Shuffle</Text>
          <Text style={styles.ability_button_price}>{shuffle_price}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.ability_button} onPress={upgrade_generator}>
          <Text style={styles.ability_button_text}>Upgrade generator</Text>
          <Text style={styles.ability_button_price}>{generator_upgrade_price}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  elements_container: {
    flexDirection: 'column'
  },

  score_container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    padding: 5,
    marginBottom: 2,

    backgroundColor: '#aaa',
    fontSize: 48,
    borderRadius: 10,
  },

  score_title_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },
  score_value_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  moves_count_title_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },
  moves_count_value_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  canvas_container: {

  },

  abilities_container: {
    flexDirection: 'row',
    justifyContent: 'center'
  },

  ability_button: {
    borderWidth: 1,
    borderRadius: 5,
    borderColor: "black",
    backgroundColor: "#007AFF",
    paddingLeft: 5,
    paddingRight: 5,
    margin: 1,
    flexDirection: 'column'
  },

  ability_button_text: {
    fontSize: 18,
    fontWeight: 'bold'
  },

  ability_button_price: {
    fontSize: 14,
    fontWeight: 'bold',
    textAlign: 'center'
  }
});