export function init_array(width, height, value, value_generator) {
  var array = [];

  if (height === 1) {
    for (let i = 0; i < width; ++i)
      if (value === undefined)
        array.push(value_generator());
      else
        array.push(value);
    return array;
  }

  for (let i = 0; i < height; ++i) {
    var row = [];
    for (let j = 0; j < width; ++j)
      if (value === undefined)
        row.push(value_generator());
      else
        row.push(value);
    array.push(row);
  }
  return array;
}

export function manhattan_distance(x1, y1, x2, y2) {
  return Math.abs(x1 - x2) + Math.abs(y1 - y2);
}

export function map_coordinates(x, y, grid_step) {
  return [
    Math.floor(y / grid_step),
    Math.floor(x / grid_step)
  ];
}