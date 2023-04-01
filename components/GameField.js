import { memo } from 'react';
import { Platform } from 'react-native';
import { Path, Svg, Text as SvgText, Rect, Defs, RadialGradient, LinearGradient, Stop } from 'react-native-svg';

import { line_path } from "./SvgPath.js";

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

function get_element_props(x, y, value, size, color, shape_path, with_volume_props = false) {
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
    translate: [x, y],
    stroke: "#000000"
  };
  var text_props = {
    x: x + size / 2,
    y: y + size / 2,
    stroke:"#000000",
    fill:"#ffffff",
    alignmentBaseline:"central",
    textAnchor:"middle",
    fontFamily:"candara",
    fontSize:size * 0.5,
    fontWeight:"bold",
    style: Platform.OS === "web" ? {cursor: "default"} : {},
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

const GameElement = memo(function ({ x, y, value, size, color, shape_path, selected }) {
  const is_3d_view = false;
  var value_text, start_color, end_color, shape_props, text_props;
  if (is_3d_view)
    [value_text, start_color, end_color, shape_props, text_props] =
      get_element_props(x, y, value, size, color, shape_path, is_3d_view);
  else
    [value_text, shape_props, text_props] = 
      get_element_props(x, y, value, size, color, shape_path, is_3d_view);
  return (
    <Svg>
      {selected && <Path
        d={shape_path}
        strokeWidth={1}
        fill="rgba(0,0,0,0.5)"
        scale={1.05}
        translate={[x, y]}
      />}
      {is_3d_view &&
        <Defs>
          <LinearGradient id={`lineargradient${value}`} x1="0" y1="0" x2="1" y2="1">
            <Stop offset="0" stopColor={start_color} stopOpacity="1" />
            <Stop offset="1" stopColor={end_color} stopOpacity="1" />
          </LinearGradient>
          <RadialGradient id={`radialgradient${value}`} cx="15%" cy="15%" r="50%" fx="25%" fy="25%">
            <Stop offset="0%" stopColor={start_color} stopOpacity="1" />
            <Stop offset="100%" stopColor={end_color} stopOpacity="1" />
          </RadialGradient>
        </Defs>
      }
      <Path {...shape_props}/>
      <SvgText {...text_props}>
        {value_text}
      </SvgText>
    </Svg>
  );
});

function GameField({ field_data, grid_step, element_offset, selected_elements, element_style_provider }) {
  const width = grid_step * field_data.width;
  const height = grid_step * field_data.height;
  return (
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
};

export default memo(GameField);