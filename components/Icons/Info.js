import { memo } from "react";
import { Svg, Rect, Circle } from "react-native-svg";

function Info({ size }) {
  return (
    <Svg
      width={size}
      height={size}
      viewBox="0 0 48 48"
    >
      <Circle fill="#2196F3" cx={24} cy={24} r={21} />
      <Rect x={22} y={22} fill="#ffffff" width={4} height={11} />
      <Circle fill="#ffffff" cx={24} cy={16.5} r={2.5} />
    </Svg>
  );
}

export default memo(Info);