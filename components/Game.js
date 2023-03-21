import { useRef, useEffect, useState } from "react";
import { StyleSheet, View, TouchableOpacity, Text, Platform, Dimensions, Modal } from 'react-native';

import { FieldData } from "./GameFieldData.js";
import GameField from "./GameField.js";
import { manhattan_distance } from "./Utils.js";
import { regular_polygon_path, star_path, circle_path } from "./CanvasUtils.js";

function map_coordinates(x, y, grid_step) {
  return [
    Math.floor(y / grid_step),
    Math.floor(x / grid_step)
  ];
}

class ElementStyleProvider {
  constructor(size) {
    this.size = size;
    const circle_shape_path = circle_path([size / 2, size / 2], size / 2);
    const triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2);
    const rounded_triangle_shape_path = regular_polygon_path([size / 2, 5 * size / 8], 11 * size / 16,
      3, -Math.PI / 2, Math.floor(0.1 * size));
    const rounded_square_shape_path = regular_polygon_path([size / 2, size / 2], 10 * size / 14,
      4, -Math.PI / 4, Math.floor(0.1 * size));
    const rounded_pentagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      5, -Math.PI / 2, Math.floor(0.1 * size));
    const rounded_hexagon_shape_path = regular_polygon_path([size / 2, size / 2], 9 * size / 16,
      6, 0, Math.floor(0.1 * size));
    const rounded_rotated_triangle_shape_path = regular_polygon_path([size / 2, 3 * size / 8], 11 * size / 16,
      3, Math.PI / 2, Math.floor(0.1 * size));
    const rounded_rotated_square_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      4, 0, Math.floor(0.1 * size));
    const rounded_octagon_shape_path = regular_polygon_path([size / 2, size / 2], 8 * size / 14,
      8, Math.PI / 8, Math.floor(0.1 * size));
    const star_5_shape_path = star_path([size / 2, size / 2], 8 * size / 14,
      5, Math.PI / 12);
    const rounded_star_5_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      5, Math.PI / 12, Math.floor(0.05 * size));
    const rounded_star_7_shape_path = star_path([size / 2, size / 2], 9 * size / 14,
      7, -Math.PI / 14, Math.floor(0.05 * size));
    this.shape_paths = [circle_shape_path, triangle_shape_path, star_5_shape_path,
      rounded_triangle_shape_path, rounded_square_shape_path, rounded_pentagon_shape_path,
      rounded_hexagon_shape_path, rounded_rotated_triangle_shape_path,
      rounded_rotated_square_shape_path, rounded_octagon_shape_path, rounded_star_5_shape_path,
      rounded_star_7_shape_path
    ];
    this.colors = ["#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA", "#FFFB28", "#FF6D0A"];
  }

  get(value) {
    return [this.colors[value % this.colors.length], this.shape_paths[value % this.shape_paths.length]];
  }
}

function GameOver({ total_score, spent_score, visible, onRestart }) {
  return (
    <Modal
      animationType="slide"
      transparent={true}
      visible={visible}
    >
      <View style={styles.game_over_container}>
        <View style={styles.game_over_view_container}>
          <Text style={styles.game_over_text_title}>Game Over</Text>
          <View style={styles.game_over_score_container}>
            <Text style={styles.game_over_score_text}>Total Earned Score: {total_score}</Text>
            <Text style={styles.game_over_score_text}>Spent Score: {spent_score}</Text>
          </View>
          <View style={styles.game_over_buttons_container}>
            <TouchableOpacity style={styles.game_over_button} onPress={onRestart}>
              <Text style={styles.game_over_button_text}>Restart</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.game_over_button}>
              <Text style={styles.game_over_button_text}>Free Shuffle</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

class Ability {
  #name;
  #starting_price;
  #price_step;
  #price_factor;
  #current_price;
  constructor(name, starting_price, price_step = undefined, price_factor = undefined) {
    this.#starting_price = starting_price;
    this.#price_step = price_step;
    this.#price_factor = price_factor;
    this.#name = name;
    this.#current_price = starting_price;
  }

  next_price() {
    if (this.#price_factor !== undefined && this.#price_step !== undefined) {
      console.log("Price factor and step can't be set simultaneously");
      return;
    }
    if (this.#price_step !== undefined)
      this.#current_price += this.#price_step;
    else if (this.#price_factor !== undefined)
      this.#current_price *= this.#price_factor;
  }

  reset() {
    this.#current_price = this.#starting_price;
  }

  get price() {
    return this.#current_price;
  }

  get name() {
    return this.#name;
  }
}

class Abilities {
  #all;
  constructor() {
    this.#all = [
      new Ability("Shuffle", 2 ** 8, undefined, 2),
      new Ability("Bomb", 2 ** 7, undefined, 2),
      new Ability("Upgrade generator", 2 ** 10, undefined, 4)
    ];
  }

  get shuffle() {
    return this.#all[0];
  }

  get bomb() {
    return this.#all[1];
  }

  get upgrade_generator() {
    return this.#all[2];
  }

  get all_prices() {
    var prices = [];
    for (let ability of this.#all)
      prices.push(ability.price);
    return prices;
  }

  clone() {
    var new_abilities = new Abilities();
    new_abilities.#all = this.#all;
    return new_abilities;
  }
}

export default function Game({ width, height, score_bonuses, onStrike }) {
  const request_animation_ref = useRef(null);
  const prev_animation_ref = useRef(null);
  const [grid_step, set_grid_step] = useState(0);
  const [element_offset, set_element_offset] = useState(0);
  const [mouse_down_position, set_mouse_down_position] = useState([]);
  const [field_data, set_field_data] = useState(new FieldData(width, height));
  const [element_style_provider, set_element_style_provider] = useState(undefined);
  const [selected_elements, set_selected_elements] = useState([]);
  const [swapping, set_swapping] = useState(false);
  const [step, set_step] = useState(-1);
  const [prev_step, set_prev_step] = useState(-1);
  const [score, set_score] = useState(0);
  const [total_score, set_total_score] = useState(0);
  const [spent_score, set_spent_score] = useState(0);
  const [moves_count, set_moves_count] = useState(field_data.get_all_moves().length);
  const [abilities, set_abilities] = useState(new Abilities());
  const [is_game_over, set_is_game_over] = useState(false);
  const [autoplay, set_autoplay] = useState(false);

  useEffect(() => {
    const scale_factor = Platform.OS === "web" ? 0.75 : 1;
    var width = scale_factor * Dimensions.get("window").width;
    var height = scale_factor * Dimensions.get("window").height;
    const grid_x_step = Math.floor(width / field_data.width);
    const grid_y_step = Math.floor(height / field_data.height);
    const grid_step = Math.min(grid_x_step, grid_y_step);
    const element_offset = Math.floor(0.1 * grid_step);
    const element_size = grid_step - 2 * element_offset;
    if (!element_style_provider || element_size !== element_style_provider.size)
      set_element_style_provider(new ElementStyleProvider(element_size));
    set_grid_step(grid_step);
    set_element_offset(element_offset);
    request_animation_ref.current = requestAnimationFrame(update_game_state);
    return () => cancelAnimationFrame(request_animation_ref.current);
  });

  const check_for_game_over = () => {
    if (field_data.get_all_moves().length > 0)
      return;
    const abilities_prices = abilities.all_prices;
    for (let price of abilities_prices)
      if (price <= score)
        return;
    set_is_game_over(true);
  };

  const auto_move = () => {
    if (!autoplay)
      return;
    if (is_game_over) {
      set_autoplay(false);
      return;
    }
    if (step !== -1)
      return;
    const moves = field_data.get_all_moves();
    if (moves.length > 0) {
      const index = Math.trunc(Math.random() * moves.length);
      const move = moves[index];
      set_selected_elements(move);
      set_step(3);
    } else if (score > abilities.upgrade_generator.price)
      upgrade_generator();
    else if (score > abilities.shuffle.price)
      shuffle();
    else if (score > abilities.bomb.price) {
      if (selected_elements.length === 0) {
        const row_id = Math.trunc(Math.random() * field_data.height);
        const column_id = Math.trunc(Math.random() * field_data.width);
        set_selected_elements([[row_id, column_id]]);
      } else
        apply_bomb()
    }
  };

  const update_game_state = (time) => {
    auto_move();
    prev_animation_ref.current = time;
    const steps = 4;
    let next_step = (step + 1) % steps;
    switch (step) {
      case 0:
        const removed_groups_details = field_data.accumulate_groups();
        const changed = removed_groups_details.length > 0;
        set_prev_step(step);
        if (changed) {
          for (let removed_group_details of removed_groups_details) {
            const value = 2 ** removed_group_details.value;
            const count = removed_group_details.size;
            onStrike(value, count);
            const bonus = count in score_bonuses ? score_bonuses[count] : 10;
            const score_delta = value * count * bonus;
            set_score(score + score_delta);
            set_total_score(total_score + score_delta);
          }
          set_field_data(field_data.clone());
          if (prev_step === 3) {
            set_selected_elements([]);
            set_swapping(false);
          }
        } else if (prev_step === 3) {
          set_prev_step(0);
          set_step(3);
        } else if (prev_step != step) {
          set_step(-1);
          set_prev_step(-1);
          check_for_game_over();
        } else
          set_step(1);
        set_moves_count(field_data.get_all_moves().length);
        break;
      case 1:
        field_data.move_elements();
        set_field_data(field_data.clone());
        set_prev_step(step);
        set_step(next_step);
        break;
      case 2:
        field_data.spawn_new_values();
        set_field_data(field_data.clone());
        set_prev_step(step);
        set_step(0);
        break;
      case 3:
        if (selected_elements.length === 2) {
          field_data.swap_cells(
            selected_elements[0][0], selected_elements[0][1],
            selected_elements[1][0], selected_elements[1][1]
          );
          set_field_data(field_data.clone());
          if (prev_step === 0 && swapping) {
            set_selected_elements([]);
            set_swapping(false);
            set_step(-1);
            set_prev_step(-1);
          } else {
            set_prev_step(step);
            set_step(0);
            set_swapping(!swapping);
          }
        }
        break;
      default:
        break;
    }
  }

  const get_event_position = (event) => {
    var x, y;
    const native_event = event.nativeEvent;
    if (event.type === "mousedown" || event.type === "mouseup") {
      x = native_event.offsetX;
      y = native_event.offsetY;
    } else {
      x = native_event.locationX;
      y = native_event.locationY;
    }
    return [x, y];
  }

  const on_mouse_down = (event) => {
    const [x, y] = get_event_position(event);
    const field_element_coordinates = map_coordinates(x, y, grid_step);
    if (field_element_coordinates.length !== 0) {
      var distance = -1;
      if (selected_elements.length === 1)
        distance = manhattan_distance(
          selected_elements[0][0], selected_elements[0][1],
          field_element_coordinates[0], field_element_coordinates[1]
        );
      if (selected_elements.length === 0 || distance > 1) {
        set_selected_elements([field_element_coordinates]);
        set_mouse_down_position([x, y]);
      }
      else if (distance === 0) {
        set_selected_elements([]);
        set_mouse_down_position([]);
      }
      else if (selected_elements.length === 1) {
        set_selected_elements([...selected_elements, field_element_coordinates]);
        set_step(3);
      }
    }
  };

  const on_mouse_move = (event) => {
    event.preventDefault();
  };

  const on_mouse_up = (event) => {
    set_mouse_down_position([]);
    if (swapping)
      return;
    if (mouse_down_position.length === 0)
      return;
    if (selected_elements.length === 2)
      return;
    if (selected_elements.length === 0)
      return;
    const [x, y] = get_event_position(event);
    const dx = x - mouse_down_position[0];
    const dy = y - mouse_down_position[1];
    if (Math.abs(dx) < grid_step / 4 && Math.abs(dy) < grid_step / 4)
      return;
    var factors = [0, 0];
    if (Math.abs(dx) > Math.abs(dy))
      factors[0] = 1 * Math.sign(dx);
    else
      factors[1] = 1 * Math.sign(dy);
    set_selected_elements([
      ...selected_elements,
      [selected_elements[0][0] + factors[1], selected_elements[0][1] + factors[0]]
    ])
    set_step(3);
  };

  const shuffle = () => {
    if (score < abilities.shuffle.price)
      return;
    field_data.shuffle();
    set_field_data(field_data.clone());
    set_step(0);
    set_score(score - abilities.shuffle.price);
    set_spent_score(spent_score + abilities.shuffle.price);
    abilities.shuffle.next_price();
    set_abilities(abilities.clone());
  };

  const upgrade_generator = () => {
    if (score < abilities.upgrade_generator.price)
      return;
    field_data.increase_values_interval();
    const small_value = field_data.values_interval[0] - 1;
    const values_count = field_data.remove_value(small_value);
    set_field_data(field_data.clone());
    set_step(1);
    const positive_score_value = values_count * 2 ** small_value;
    const negative_score_value = abilities.upgrade_generator.price;
    set_score(score + positive_score_value - negative_score_value);
    set_total_score(total_score + positive_score_value);
    set_spent_score(spent_score + negative_score_value)
    abilities.upgrade_generator.next_price();
    set_abilities(abilities.clone());
  };

  const apply_bomb = () => {
    if (score < abilities.bomb.price)
      return;
    if (selected_elements.length === 0)
      return;
    const removed_values = field_data.remove_zone(
      selected_elements[0][0] - 1, selected_elements[0][1] - 1,
      selected_elements[0][0] + 1, selected_elements[0][1] + 1
    );
    var positive_score_value = 0;
    for (let [value, count] of Object.entries(removed_values))
      positive_score_value += 2 ** value * count;
    const negative_score_value = abilities.bomb.price;
    set_score(score + positive_score_value - negative_score_value);
    set_total_score(total_score + positive_score_value);
    set_spent_score(spent_score + negative_score_value);
    set_field_data(field_data.clone());
    abilities.bomb.next_price();
    set_abilities(abilities.clone());
    set_step(1);
    set_selected_elements([]);
  };

  const restart = () => {
    set_score(0);
    set_total_score(0);
    set_spent_score(0);
    set_field_data(new FieldData(width, height));
    set_step(-1);
    set_prev_step(-1);
    set_is_game_over(false);
    set_abilities(new Abilities());
  }

  return (
    <View style={styles.elements_container}>
      <GameOver
        total_score={total_score}
        spent_score={spent_score}
        visible={is_game_over}
        onRestart={restart}
      />
      <View style={styles.score_container}>
        <Text style={styles.score_title_container}>Score</Text>
        <Text style={styles.score_value_container}>{score}</Text>
        <Text style={styles.moves_count_title_container}>Moves count</Text>
        <Text style={styles.moves_count_value_container}>{moves_count}</Text>
      </View>
      <View
        style={styles.canvas_container}
        onMouseDown={on_mouse_down}
        onTouchStart={on_mouse_down}
        onMouseUp={on_mouse_up}
        onTouchEnd={on_mouse_up}
        onMouseMove={on_mouse_move}
      >
        {grid_step > 0 && <GameField
          grid_step={grid_step}
          element_offset={element_offset}
          field_data={field_data}
          selected_elements={selected_elements}
          element_style_provider={element_style_provider}
        />}
      </View>
      <View style={styles.abilities_container}>
        <TouchableOpacity style={styles.ability_button} onPress={shuffle}>
          <Text style={styles.ability_button_text}>{abilities.shuffle.name}</Text>
          <Text style={styles.ability_button_price}>{abilities.shuffle.price}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.ability_button} onPress={apply_bomb}>
          <Text style={styles.ability_button_text}>{abilities.bomb.name}</Text>
          <Text style={styles.ability_button_price}>{abilities.bomb.price}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.ability_button} onPress={upgrade_generator}>
          <Text style={styles.ability_button_text}>{abilities.upgrade_generator.name}</Text>
          <Text style={styles.ability_button_price}>{abilities.upgrade_generator.price}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.ability_button} onPress={() => set_autoplay(!autoplay)}>
          <Text style={styles.ability_button_text}>Autoplay</Text>
          <Text style={styles.ability_button_price}>0</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  elements_container: {
    flexDirection: 'column',
    alignContent: "center"
  },

  score_container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    padding: 5,
    marginBottom: 2,

    backgroundColor: '#aaa',
    fontSize: 48,
    borderRadius: 10,
  },

  score_title_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  score_value_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  moves_count_title_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  moves_count_value_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  canvas_container: {
  },

  abilities_container: {
    flexDirection: 'row',
    justifyContent: 'center'
  },

  ability_button: {
    borderWidth: 1,
    borderRadius: 5,
    borderColor: "black",
    backgroundColor: "#007AFF",
    paddingLeft: 5,
    paddingRight: 5,
    margin: 1,
    flexDirection: 'column'
  },

  ability_button_text: {
    fontSize: 18,
    fontWeight: 'bold'
  },

  ability_button_price: {
    fontSize: 14,
    fontWeight: 'bold',
    textAlign: 'center'
  },

  game_over_container: {
    flex: 1,
    flexDirection: "column",
    alignContent: "center",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(0.25,0.25,0.25,0.75)",
  },

  game_over_view_container: {
    padding: 10,
    backgroundColor: "#222222",
    borderRadius: 15,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.25,
    shadowRadius: 4,
    elevation: 5,
  },

  game_over_text_title: {
    fontSize: 80
  },

  game_over_score_container: {
    flexDirection: "row"
  },

  game_over_score_text: {
    fontSize: 24,
    marginLeft: 5,
    marginRight: 5
  },

  game_over_buttons_container: {
    flexDirection: "row",
    justifyContent: "center"
  },

  game_over_button: {
    marginLeft: 1,
    marginRight: 1,
    padding: 5,
    borderRadius: 5,
    backgroundColor: "#000000"
  },

  game_over_button_text: {
    textAlign: "center",
    fontWeight: "bold",
    fontSize: 24,
    color: "#ffffff"
  }
});