import { memo, useEffect, useState } from 'react';
import { Platform } from 'react-native';
import { Path, Svg, Text as SvgText, Rect } from 'react-native-svg';

import { line_path } from "./CanvasUtils.js";

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

const GameElement = memo(function ({ x, y, value, size, color, shape_path, selected }) {
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
});

function GameField({ field_data, grid_step, element_offset, selected_elements, element_style_provider }) {
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
};

export default memo(GameField);