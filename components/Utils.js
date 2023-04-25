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

export function copy_array(array) {
  var array_copy = [];
  for (let element of array) {
    if (Array.isArray(element))
      array_copy.push(Object.assign([], copy_array(element)));
    else
      array_copy.push(element);
  }
  return array_copy;
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