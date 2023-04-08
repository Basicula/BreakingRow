import { memo, useEffect, useState, useRef } from 'react';
import { Animated, Easing, Platform, StyleSheet, View } from 'react-native';
import { Path, Svg, Text as SvgText, Rect, G, Defs, RadialGradient, Stop } from 'react-native-svg';

import { line_path } from "./SvgPath.js";
import { manhattan_distance, map_coordinates } from './Utils.js';

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

const GameElement = memo(function ({ x, y, value, size, color, shape_path, selected, highlighted }) {
  const is_3d_view = false;
  var value_text, start_color, end_color, shape_props, text_props;
  if (is_3d_view)
    [value_text, start_color, end_color, shape_props, text_props] =
      get_element_props(value, size, color, shape_path, is_3d_view);
  else
    [value_text, shape_props, text_props] =
      get_element_props(value, size, color, shape_path, is_3d_view);
  const default_scale_factor = 1;
  const default_rotation_angle = 0;
  const [shake_animation_scale] = useState(new Animated.Value(default_scale_factor));
  const [shake_animation_rotation] = useState(new Animated.Value(default_rotation_angle));
  useEffect(() => {
    if (!selected && !highlighted) {
      shake_animation_scale.stopAnimation();
      shake_animation_scale.setValue(default_scale_factor);
      shake_animation_rotation.stopAnimation();
      shake_animation_rotation.setValue(default_rotation_angle);
      return;
    }
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
    Animated.parallel([
      Animated.loop(Animated.sequence([
        Animated.timing(shake_animation_scale,
          scale_animation_config(default_scale_factor + scale_offset)),
        Animated.timing(shake_animation_scale,
          scale_animation_config(default_scale_factor)),
        Animated.timing(shake_animation_scale,
          scale_animation_config(default_scale_factor - scale_offset)),
        Animated.timing(shake_animation_scale,
          scale_animation_config(default_scale_factor)),
      ])),
      Animated.loop(Animated.sequence([
        Animated.timing(shake_animation_rotation,
          rotation_animation_config(rotation_angle)),
        Animated.timing(shake_animation_rotation,
          rotation_animation_config(default_rotation_angle)),
        Animated.timing(shake_animation_rotation,
          rotation_animation_config(-rotation_angle)),
        Animated.timing(shake_animation_rotation,
          rotation_animation_config(default_rotation_angle)),
      ]))
    ]).start();
  }, [selected, highlighted, shake_animation_rotation, shake_animation_scale]);
  return (
    <Svg>
      <G x={x + size / 2} y={y + size / 2} >
        <AnimatedG rotation={shake_animation_rotation} scale={shake_animation_scale}>
          {selected &&
            <Path
              d={shape_path}
              strokeWidth={1}
              fill="rgba(0,0,0,0.5)"
              scale={1.05}
              translate={[-size / 2, -size / 2]}
            />
          }
          {is_3d_view &&
            <Defs>
              <RadialGradient id={`radialgradient${value}`} cx="15%" cy="15%" r="50%" fx="25%" fy="25%">
                <Stop offset="0%" stopColor={start_color} stopOpacity="1" />
                <Stop offset="100%" stopColor={end_color} stopOpacity="1" />
              </RadialGradient>
            </Defs>
          }
          <Path {...shape_props} />
          <SvgText {...text_props}>
            {value_text}
          </SvgText>
        </AnimatedG>
      </G>
    </Svg>
  );
});

function GameField({ field_data, grid_step, element_offset, element_style_provider,
  highlighted_elements, onElementsSwap, onLayout }) {
  const width = grid_step * field_data.width;
  const height = grid_step * field_data.height;

  const mouse_down_position = useRef([]);
  const [selected_elements, set_selected_elements] = useState([]);

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
        new_selected_elements = [...selected_elements, field_element_coordinates];
        onElementsSwap(new_selected_elements);
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
    onElementsSwap([
      ...selected_elements,
      [selected_elements[0][0] + factors[1],selected_elements[0][1] + factors[0]]
    ]);
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
            const value = field_data.at(row_id, column_id);
            if (value === -1)
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
            return <GameElement
              key={row_id * field_data.width + column_id}
              x={column_id * grid_step + element_offset}
              y={row_id * grid_step + element_offset}
              value={value}
              size={element_style_provider.size}
              color={color}
              shape_path={shape_path}
              selected={is_selected}
              highlighted={is_highlighted}
            />;
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