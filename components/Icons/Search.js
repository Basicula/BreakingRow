import { memo } from "react";
import { Svg, Circle, Line } from "react-native-svg";

function Search({ size }) {
  return (
    <Svg
      width={size}
      height={size}
      viewBox="0 0 32 32"
    >
      <Circle
        fill="none"
        stroke="#000000"
        strokeWidth={2}
        strokeMiterlimit={10}
        cx={19.5}
        cy={12.5}
        r={8.5}
      />
      <Line
        fill="none"
        stroke="#000000"
        strokeWidth={2}
        strokeMiterlimit={10}
        x1={4}
        y1={28}
        x2={14}
        y2={18}
      />
    </Svg>
  );
}

export default memo(Search);