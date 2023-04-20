import { memo, useEffect, useState, useRef } from 'react';
import { Animated, Easing, Platform, StyleSheet, View } from 'react-native';
import {
  Path, Svg, Text as SvgText, Rect, G,
  Defs, RadialGradient, Stop
} from 'react-native-svg';

import { line_path } from "./SvgPath.js";
import { init_array, manhattan_distance, map_coordinates } from './Utils.js';
import { useSettings } from './Settings.js';

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

function get_element_props(value, size, color, shape_path, with_volume_props = false) {
  const exponents = {
    "0": "⁰", "1": "¹", "2": "²", "3": "³", "4": "⁴",
    "5": "⁵", "6": "⁶", "7": "⁷", "8": "⁸", "9": "⁹"
  }
  let regex = RegExp(`[${Object.keys(exponents).join("")}]`, "g");
  const exponent = value.toString().replace(regex, number => exponents[number]);
  const value_text = value > 16 ? `2${exponent}` : (2 ** value).toString();
  var shape_props = {
    d: shape_path,
    strokeWidth: 1,
    fill: color,
    opacity: 1,
    translate: [-size / 2, -size / 2],
    stroke: "#000000",
  };
  var text_props = {
    stroke: "#000000",
    fill: "#ffffff",
    alignmentBaseline: "central",
    textAnchor: "middle",
    fontFamily: "candara",
    fontSize: size * 0.5,
    fontWeight: "bold",
    style: Platform.OS === "web" ? { cursor: "default" } : {},
  };
  if (with_volume_props) {
    function rgb_to_hex(r, g, b) {
      return "#" + (1 << 24 | r << 16 | g << 8 | b).toString(16).slice(1);
    }
    const rgb = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i
      .exec(color)
      .map(hex => parseInt(hex, 16))
      .slice(1);
    const color_component_diff_factor = 0.25;
    const start_rgb = rgb.slice(0).map(rgb_component =>
      Math.min(255, Math.floor(rgb_component + color_component_diff_factor * rgb_component)));
    const start_color = rgb_to_hex(...start_rgb);
    const end_rgb = rgb.slice(0).map(rgb_component =>
      Math.max(0, Math.floor(rgb_component - color_component_diff_factor * rgb_component)));
    const end_color = rgb_to_hex(...end_rgb);
    shape_props.fill = `url(#radialgradient${value})`;
    delete shape_props.stroke;
    return [value_text, start_color, end_color, shape_props, text_props];
  }
  return [value_text, shape_props, text_props];
}

const AnimatedG = Animated.createAnimatedComponent(G);

const GameElement = memo(function ({ value, size, color, shape_path,
  selected, highlighted, to_create, to_destroy }) {
  const { settings, element_number_shown_key, element_style_3d } = useSettings();
  var value_text, start_color, end_color, shape_props, text_props;
  if (settings[element_style_3d].value)
    [value_text, start_color, end_color, shape_props, text_props] =
      get_element_props(value, size, color, shape_path, settings[element_style_3d].value);
  else
    [value_text, shape_props, text_props] =
      get_element_props(value, size, color, shape_path, settings[element_style_3d].value);
  const default_scale_factor = 1;
  const default_rotation_angle = 0;
  const [animation_scale] = useState(new Animated.Value(default_scale_factor));
  const [animation_rotation] = useState(new Animated.Value(default_rotation_angle));
  useEffect(() => {
    const scale_offset = 0.05;
    const rotation_angle = 10;
    const rotate_duration = 113;
    const scale_duration = 219;
    const use_native_driver = false;
    const scale_animation_config = (scale) => {
      return {
        toValue: scale,
        duration: scale_duration,
        useNativeDriver: use_native_driver
      };
    };
    const rotation_animation_config = (angle) => {
      return {
        toValue: angle,
        duration: rotate_duration,
        easing: Easing.quad,
        useNativeDriver: use_native_driver
      };
    };

    if (!selected && !highlighted && !to_create && !to_destroy) {
      animation_scale.stopAnimation();
      animation_scale.setValue(default_scale_factor);
      animation_rotation.stopAnimation();
      animation_rotation.setValue(default_rotation_angle);
      return;
    }

    Animated.parallel([
      Animated.loop(Animated.sequence([
        Animated.timing(animation_scale,
          scale_animation_config(default_scale_factor + scale_offset)),
        Animated.timing(animation_scale,
          scale_animation_config(default_scale_factor)),
        Animated.timing(animation_scale,
          scale_animation_config(default_scale_factor - scale_offset)),
        Animated.timing(animation_scale,
          scale_animation_config(default_scale_factor)),
      ])),
      Animated.loop(Animated.sequence([
        Animated.timing(animation_rotation,
          rotation_animation_config(rotation_angle)),
        Animated.timing(animation_rotation,
          rotation_animation_config(default_rotation_angle)),
        Animated.timing(animation_rotation,
          rotation_animation_config(-rotation_angle)),
        Animated.timing(animation_rotation,
          rotation_animation_config(default_rotation_angle)),
      ]))
    ]).start();
  }, [selected, highlighted, to_create, to_destroy]);
  return (
    <AnimatedG rotation={animation_rotation} scale={animation_scale}>
      {selected &&
        <Path
          d={shape_path}
          strokeWidth={1}
          fill="rgba(0,0,0,0.5)"
          scale={1.05}
          translate={[-size / 2, -size / 2]}
        />
      }
      {settings[element_style_3d].value &&
        <Defs>
          <RadialGradient id={`radialgradient${value}`} cx="15%" cy="15%" r="75%" fx="25%" fy="25%">
            <Stop offset="0%" stopColor={start_color} stopOpacity="1" />
            <Stop offset="100%" stopColor={end_color} stopOpacity="1" />
          </RadialGradient>
        </Defs>
      }
      <Path {...shape_props} />
      {settings[element_number_shown_key].value &&
        <SvgText {...text_props}>
          {value_text}
        </SvgText>
      }
    </AnimatedG>
  );
});

function GameField({ field_data, grid_step, element_offset, element_style_provider,
  highlighted_elements, onLayout,
  onFieldDataChange, onAccumulateElements }) {
  const width = grid_step * field_data.width;
  const height = grid_step * field_data.height;

  const element_positions = useRef([]).current;
  const element_scales = useRef(init_array(field_data.width, field_data.height, undefined, () => new Animated.Value(0))).current;
  const animation_running = useRef(false);
  const animation_duration = 500;
  const use_native_driver = false;

  const mouse_down_position = useRef([]);
  const [selected_elements, set_selected_elements] = useState([]);

  const get_element_position = (row, column) => {
    const x = column * grid_step + element_offset + element_style_provider.size / 2;
    const y = row * grid_step + element_offset + element_style_provider.size / 2;
    return { x: x, y: y };
  };

  const reset_positions = () => {
    if (element_positions.length === 0) {
      for (let row_id = 0; row_id < field_data.height; ++row_id) {
        var row = [];
        for (let column_id = 0; column_id < field_data.width; ++column_id)
          row.push(new Animated.ValueXY(get_element_position(row_id, column_id)));
        element_positions.push(row);
      }
    } else {
      for (let row_id = 0; row_id < field_data.height; ++row_id)
        for (let column_id = 0; column_id < field_data.width; ++column_id)
          element_positions[row_id][column_id].setValue(get_element_position(row_id, column_id));
    }
  };

  if (element_positions.length === 0)
    reset_positions();

  const swap_animation = (first, second) => {
    const first_position = element_positions[first[0]][first[1]];
    const second_position = element_positions[second[0]][second[1]];
    return Animated.parallel([
      Animated.timing(first_position,
        {
          toValue: {
            x: second_position.x._value,
            y: second_position.y._value
          },
          duration: animation_duration,
          useNativeDriver: use_native_driver
        }),
      Animated.timing(second_position,
        {
          toValue: {
            x: first_position.x._value,
            y: first_position.y._value
          },
          duration: animation_duration,
          useNativeDriver: use_native_driver
        })
    ]);
  };

  const destroy_animation = (row_id, column_id) => {
    element_scales[row_id][column_id].setValue(1);
    return Animated.timing(element_scales[row_id][column_id], {
      toValue: 0,
      duration: animation_duration,
      useNativeDriver: use_native_driver
    });
  };

  const create_animation = (row_id, column_id) => {
    element_scales[row_id][column_id].setValue(0);
    return Animated.timing(element_scales[row_id][column_id], {
      toValue: 1,
      duration: animation_duration,
      useNativeDriver: use_native_driver
    });
  };

  const swap_elements = (first, second) => {
    swap_animation(first, second).start(({ finished }) => {
      if (!finished)
        return;
      const move_result = field_data.check_move(first, second);
      if (move_result > 0) {
        field_data.swap_cells(...first, ...second);
        reset_positions();
        onFieldDataChange(field_data);
      }
      else
        swap_animation(first, second).start(({ finished }) => {
          if (!finished)
            return;
        });
    });
  };

  const accumulate_elements = () => {
    const removed_groups_details = field_data.accumulate_groups();
    var to_destroy_animations = []
    for (let removed_group_details of removed_groups_details) {
      for (let cell_coordinate of removed_group_details.group)
        to_destroy_animations.push(destroy_animation(...cell_coordinate));
    }
    Animated.parallel(to_destroy_animations).start(({ finished }) => {
      if (!finished)
        return;
      onAccumulateElements(field_data, removed_groups_details);
    });
  };

  const move_elements = (element_move_changes) => {
    var move_animations = [];
    const duration_over_cell = animation_duration;
    for (let element_move_change of element_move_changes) {
      const old_coordinates = element_move_change[0];
      const new_coordinates = element_move_change[1];
      [element_scales[old_coordinates[0]][old_coordinates[1]], element_scales[new_coordinates[0]][new_coordinates[1]]] =
        [element_scales[new_coordinates[0]][new_coordinates[1]], element_scales[old_coordinates[0]][old_coordinates[1]]];
      const new_element_position = get_element_position(...new_coordinates);
      const element_position = element_positions[old_coordinates[0]][old_coordinates[1]];
      const duration = duration_over_cell * (new_coordinates[0] - old_coordinates[0])
      move_animations.push(
        Animated.timing(element_position,
          {
            toValue: new_element_position,
            duration: duration,
            useNativeDriver: use_native_driver,
          })
      );
    }
    Animated.parallel(move_animations).start(({ finished }) => {
      if (!finished)
        return;
      field_data.move_elements()
      onFieldDataChange(field_data);
      reset_positions();
    });
  };

  const spawn_elements = () => {
    field_data.spawn_new_values();
    onFieldDataChange(field_data);
  };

  useEffect(() => {
    animation_running.current = true;
    const update_field_data = () => {
      const element_move_changes = field_data.element_move_changes();
      if (element_move_changes.length > 0) {
        move_elements(element_move_changes);
        return;
      }
      if (field_data.has_empty_cells()) {
        spawn_elements();
        return;
      }
      if (field_data.has_groups()) {
        accumulate_elements();
        return;
      }
      animation_running.current = false;
    };
    var to_create_animations = [];
    for (let row_id = 0; row_id < field_data.height; ++row_id)
      for (let column_id = 0; column_id < field_data.width; ++column_id) {
        if (element_scales[row_id][column_id]._value === 0 &&
          field_data.at(row_id, column_id) >= 0)
          to_create_animations.push(create_animation(row_id, column_id));
      }
    if (to_create_animations.length > 0) {
      Animated.parallel(to_create_animations).start(({ finished }) => {
        if (!finished)
          return;
        update_field_data();
      });
    }
    else
      update_field_data()
  }, [field_data]);

  const get_event_position = (event) => {
    const native_event = event.nativeEvent;
    switch (event.type) {
      case "mousedown":
      case "mouseup":
      case "mousemove":
        return [native_event.offsetX, native_event.offsetY]
      default:
        return [native_event.locationX, native_event.locationY];
    }
  }

  const on_mouse_down = (event) => {
    if (animation_running.current)
      return;
    const [x, y] = get_event_position(event);
    const field_element_coordinates = map_coordinates(x, y, grid_step);
    if (field_element_coordinates.length !== 0) {
      var distance = -1;
      var new_selected_elements = [];
      if (selected_elements.length === 1)
        distance = manhattan_distance(
          selected_elements[0][0], selected_elements[0][1],
          field_element_coordinates[0], field_element_coordinates[1]
        );
      if (mouse_down_position.current.length === 0 || distance > 1) {
        new_selected_elements.push(field_element_coordinates);
        mouse_down_position.current = [x, y];
      }
      else if (distance === 0) {
        new_selected_elements = [];
        mouse_down_position.current = [];
      }
      else if (selected_elements.length === 1) {
        swap_elements(selected_elements[0], field_element_coordinates);
        mouse_down_position.current = [];
        new_selected_elements = [];
      }
      set_selected_elements(new_selected_elements);
    }
  };

  const on_mouse_move = (event) => {
    event.preventDefault();
  };

  const on_mouse_up = (event) => {
    if (animation_running.current)
      return;
    if (mouse_down_position.current.length === 0)
      return;
    const [x, y] = get_event_position(event);
    const dx = x - mouse_down_position.current[0];
    const dy = y - mouse_down_position.current[1];
    if (Math.abs(dx) < grid_step / 4 && Math.abs(dy) < grid_step / 4)
      return;
    var factors = [0, 0];
    if (Math.abs(dx) > Math.abs(dy))
      factors[0] = 1 * Math.sign(dx);
    else
      factors[1] = 1 * Math.sign(dy);
    const next_selected_element = [
      selected_elements[0][0] + factors[1],
      selected_elements[0][1] + factors[0]
    ];
    if (next_selected_element[0] >= 0 && next_selected_element[0] < field_data.height &&
      next_selected_element[1] >= 0 && next_selected_element[1] < field_data.width)
      swap_elements(
        selected_elements[0],
        next_selected_element
      );
    set_selected_elements([]);
    mouse_down_position.current = [];
  };
  return (
    <View
      style={styles.canvas_container}
      onLayout={onLayout}
      onMouseDown={on_mouse_down}
      onMouseUp={on_mouse_up}
      onMouseMove={on_mouse_move}
      onTouchStart={on_mouse_down}
      onTouchEnd={on_mouse_up}
    >
      <Svg
        width={width}
        height={height}
        viewBox={`0 0 ${width} ${height}`}
      >
        <Rect
          x={0}
          y={0}
          width={width}
          height={height}
          fill="#dddddd"
          strokeWidth={2}
          stroke="#000000"
        />
        <Path
          d={grid_path(width, height, field_data, grid_step)}
          strokeWidth={1}
          stroke="black"
          fill="grey"
        />
        {element_style_provider && Array.from(Array(field_data.height)).map((_, row_id) => {
          return Array.from(Array(field_data.width)).map((_, column_id) => {
            var value = field_data.at(row_id, column_id);
            if (value < 0)
              return;
            var is_selected = false;
            for (let selected_element of selected_elements)
              if (selected_element[0] === row_id && selected_element[1] === column_id) {
                is_selected = true;
                break;
              }
            var is_highlighted = false;
            for (let highlighted_element of highlighted_elements)
              if (highlighted_element[0] === row_id && highlighted_element[1] === column_id) {
                is_highlighted = true;
                break;
              }
            const [color, shape_path] = element_style_provider.get(value);
            return (
              <AnimatedG
                key={row_id * field_data.width + column_id}
                style={{
                  transform: [
                    { translateX: element_positions[row_id][column_id].x },
                    { translateY: element_positions[row_id][column_id].y },
                    { scale: element_scales[row_id][column_id] }
                  ]
                }}
              >
                <GameElement
                  value={value}
                  size={element_style_provider.size}
                  color={color}
                  shape_path={shape_path}
                  selected={is_selected}
                  highlighted={is_highlighted}
                />
              </AnimatedG>
            );
          });
        })}
      </Svg>
    </View>
  );
};

const styles = StyleSheet.create({
  canvas_container: {
    justifyContent: "center",
    alignContent: "center",
    flexWrap: "wrap"
  },
});

export default memo(GameField);