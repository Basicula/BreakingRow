import { memo } from "react";
import { Svg, Path } from "react-native-svg";

function Lightning({ size }) {
  return (
    <Svg
      width={size}
      height={size}
      viewBox="0 0 16 16"
    >
      <Path
        fill="#2222FF"
        stroke="#FFFF00"
        strokeWidth={0.5}
        d="M7.99 0l-7.010 9.38 6.020-0.42-4.96 7.040 12.96-10-7.010 0.47 7.010-6.47h-7.010z"
      />
    </Svg>
  );
}

export default memo(Lightning);