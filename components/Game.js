import { useRef, useEffect, useState } from "react";
import { StyleSheet, View, TouchableOpacity, Text, Platform, Dimensions } from 'react-native';
import { Path, Svg, Text as SvgText, Rect } from 'react-native-svg';

import { FieldData } from "./GameFieldData.js";
import { manhattan_distance } from "./Utils.js";
import { regular_polygon_path, star_path, circle_path, line_path } from "./CanvasUtils.js";

function map_coordinates(x, y, grid_step) {
  return [
    Math.floor(y / grid_step),
    Math.floor(x / grid_step)
  ];
}

function grid_path(width, height, field_data, grid_step) {
  var total_grid_path = "";
  for (let line_id = 0; line_id <= field_data.width || line_id <= field_data.height; ++line_id) {
    if (line_id <= field_data.width) {
      const x = line_id * grid_step;
      total_grid_path += line_path(x, 0, x, height);
    }
    if (line_id <= field_data.height) {
      const y = line_id * grid_step;
      total_grid_path += line_path(0, y, width, y);
    }
  }
  return total_grid_path;
}


class ElementStyleProvider {
  constructor(size) {
    this.size = size;
    const circle_shape_path = circle_path([size / 2, size / 2], size / 2);
    const triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2);
    const rounded_triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2, Math.floor(0.1 * size));
    const rounded_square_shape_path = regular_polygon_path([size / 2, size / 2], 10 * size / 14,
      4, -Math.PI / 4, Math.floor(0.1 * size));
    const rounded_pentagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      5, -Math.PI / 2, Math.floor(0.1 * size));
    const rounded_hexagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      6, 0, Math.floor(0.1 * size));
    const rounded_rotated_triangle_shape_path = regular_polygon_path([size / 2, 3 * size / 8], 11 * size / 16,
      3, Math.PI / 2, Math.floor(0.1 * size));
    const rounded_rotated_square_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      4, 0, Math.floor(0.1 * size));
    const rounded_octagon_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      8, Math.PI / 8, Math.floor(0.1 * size));
    const star_5_shape_path = star_path([size / 2, size / 2], 8 * size / 14,
      5, Math.PI / 12);
    const rounded_star_5_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      5, Math.PI / 12, Math.floor(0.05 * size));
    const rounded_star_7_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      7, -Math.PI / 14, Math.floor(0.05 * size));
    this.shape_paths = [circle_shape_path, triangle_shape_path, star_5_shape_path,
      rounded_triangle_shape_path, rounded_square_shape_path, rounded_pentagon_shape_path,
      rounded_hexagon_shape_path, rounded_rotated_triangle_shape_path,
      rounded_rotated_square_shape_path, rounded_octagon_shape_path, rounded_star_5_shape_path,
      rounded_star_7_shape_path
    ];
    this.colors = ["#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA", "#FFFB28", "#FF6D0A"];
  }

  get(value) {
    return [this.colors[value % this.colors.length], this.shape_paths[value % this.shape_paths.length]];
  }
}

const GameElement = ({ x, y, value, size, color, shape_path, selected }) => {
  const exponents = {
    "0": "⁰", "1": "¹", "2": "²", "3": "³", "4": "⁴",
    "5": "⁵", "6": "⁶", "7": "⁷", "8": "⁸", "9": "⁹"
  }
  let regex = RegExp(`[${Object.keys(exponents).join("")}]`, "g");
  const exponent = value.toString().replace(regex, number => exponents[number]);
  const value_text = value > 12 ? `2${exponent}` : (2 ** value).toString();
  var specific_text_style = {};
  if (Platform.OS === "web")
    specific_text_style = {
      cursor: "default"
    };
  return (
    <Svg>
      {selected && <Path
        d={shape_path}
        strokeWidth={1}
        fill="rgba(0,0,0,0.5)"
        scale={1.1}
        translate={[x - size * 0.05, y - size * 0.05]}
      />}
      <Path
        d={shape_path}
        strokeWidth={1}
        stroke="#000000"
        fill={color}
        opacity={1}
        translate={[x, y]}
      />
      <Path
        d={shape_path}
        strokeWidth={1}
        fill="#000000"
        opacity={0.075}
        translate={[x, y]}
      />
      {selected && <Path
        d={shape_path}
        strokeWidth={1}
        fill={color}
        opacity={1}
        scale={0.9}
        translate={[x + size * 0.05, y + size * 0.05]}
      />}
      <SvgText
        x={x + size / 2}
        y={y + size / 2}
        stroke="#000000"
        fill="#ffffff"
        alignmentBaseline="central"
        textAnchor="middle"
        fontFamily="candara"
        fontSize={size * 0.5}
        style={specific_text_style}
      >
        {value_text}
      </SvgText>
    </Svg>
  );
}

const GameField = ({ field_data, grid_step, element_offset, selected_elements, element_style_provider }) => {
  const width = grid_step * field_data.width;
  const height = grid_step * field_data.height;
  return (
    <Svg
      width={width}
      height={height}
      viewBox={`0 0 ${width} ${height}`}>
      <Rect
        x={0}
        y={0}
        width={width}
        height={height}
        fill="#dddddd"
        strokeWidth={5}
        stroke="#000000"
      />
      <Path
        d={grid_path(width, height, field_data, grid_step)}
        strokeWidth={2}
        stroke="black"
        fill="grey"
      />
      {element_style_provider && Array.from(Array(field_data.height)).map((_, row_id) => {
        return Array.from(Array(field_data.width)).map((_, column_id) => {
          const value = field_data.at(row_id, column_id);
          if (value === -1)
            return;
          var is_selected = false;
          for (let selected_element of selected_elements)
            if (selected_element[0] == row_id && selected_element[1] == column_id) {
              is_selected = true;
              break;
            }
          const [color, shape_path] = element_style_provider.get(value);
          return <GameElement
            key={row_id * field_data.width + column_id}
            x={column_id * grid_step + element_offset}
            y={row_id * grid_step + element_offset}
            value={value}
            size={element_style_provider.size}
            color={color}
            shape_path={shape_path}
            selected={is_selected}
          />;
        });
      })}
    </Svg>
  );
}

export default function Game({ width, height, score_bonuses, onStrike }) {
  const request_animation_ref = useRef(null);
  const prev_animation_ref = useRef(null);
  const [grid_step, set_grid_step] = useState(0);
  const [element_offset, set_element_offset] = useState(0);
  const [mouse_down_position, set_mouse_down_position] = useState([]);
  const [field_data, set_field_data] = useState(new FieldData(width, height));
  const [element_style_provider, set_element_style_provider] = useState(undefined);
  const [first_element, set_first_element] = useState([]);
  const [second_element, set_second_element] = useState([]);
  const [swapping, set_swapping] = useState(false);
  const [step, set_step] = useState(-1);
  const [prev_step, set_prev_step] = useState(-1);
  const [score, set_score] = useState(0);
  const [moves_count, set_moves_count] = useState(field_data.get_all_moves().length);
  const [shuffle_price, set_shuffle_price] = useState(1024);
  const [generator_upgrade_price, set_generator_upgrade_price] = useState(1024);

  var selected_elements = [];
  if (first_element.length > 0)
    selected_elements.push(first_element);
  if (second_element.length > 0)
    selected_elements.push(second_element);

  useEffect(() => {
    const scale_factor = 1;
    var width = scale_factor * Dimensions.get("window").width;
    var height = scale_factor * Dimensions.get("window").height;
    const grid_x_step = Math.floor(width / field_data.width);
    const grid_y_step = Math.floor(height / field_data.height);
    const grid_step = Math.min(grid_x_step, grid_y_step);
    const element_offset = Math.floor(0.1 * grid_step);
    const element_size = grid_step - 2 * element_offset;
    if (!element_style_provider || element_size !== element_style_provider.size)
      set_element_style_provider(new ElementStyleProvider(element_size));
    set_grid_step(grid_step);
    set_element_offset(element_offset);
    request_animation_ref.current = requestAnimationFrame(update_game_state);
    return () => cancelAnimationFrame(request_animation_ref.current);
  });

  const update_game_state = (time) => {
    prev_animation_ref.current = time;
    const steps = 4;
    let next_step = (step + 1) % steps;
    switch (step) {
      case 0:
        const removed_groups_details = field_data.accumulate_groups();
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
    const native_event = event.nativeEvent;
    if (event.type === "touchstart" || event.type === "touchend") {
      x = native_event.touches[0].pageX - native_event.touches[0].clientX;
      y = native_event.touches[0].pageY - native_event.touches[0].clientY;
    } else if (event.type === "mousedown" || event.type === "mouseup") {
      x = native_event.offsetX;
      y = native_event.offsetY;
    } else {
      x = native_event.locationX;
      y = native_event.locationY;
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

  const on_mouse_move = (event) => {
    event.preventDefault();
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
        {grid_step > 0 && <GameField
          grid_step={grid_step}
          element_offset={element_offset}
          field_data={field_data}
          selected_elements={selected_elements}
          element_style_provider={element_style_provider}
        />
        }
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
    flexDirection: 'column',
    alignContent: "center"
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