import { Svg, Circle, Line } from 'react-native-svg';

function Close({ size }) {
  const radius = size / 2;
  const coord = radius * Math.SQRT1_2 * 0.75;
  return (
    <Svg width={size} height={size}>
      <Circle
        r={radius}
        cx={radius}
        cy={radius}
        fill="#ff0000"
        stroke="#000000"
      />
      <Line
        x1={radius - coord}
        y1={radius - coord}
        x2={radius + coord}
        y2={radius + coord}
        stroke="#000000"
        strokeWidth={2}
      />
      <Line
        x1={radius - coord}
        y1={radius + coord}
        x2={radius + coord}
        y2={radius - coord}
        stroke="#000000"
        strokeWidth={2}
      />
    </Svg>
  );
}

export default Close;