import { useRef, useEffect, useState, memo } from "react";
import { StyleSheet, View, Text, Platform, Dimensions } from 'react-native';

import { FieldData } from "./GameFieldData.js";
import GameField from "./GameField.js";
import { manhattan_distance } from "./Utils.js";
import { ElementStyleProvider } from "./ElementStyleProvider.js";
import GameOver from "./GameOver.js";
import { Abilities, AbilitiesVisualizer } from "./Abilities.js";

function map_coordinates(x, y, grid_step) {
  return [
    Math.floor(y / grid_step),
    Math.floor(x / grid_step)
  ];
}

const ScoreVisualizer = memo(function ({ score, moves_count }) {
  return (
    <View style={styles.score_container}>
      <Text style={styles.game_state_text_info_container}>Score</Text>
      <Text style={styles.game_state_text_info_container}>{score}</Text>
      <Text style={styles.game_state_text_info_container}>Moves count</Text>
      <Text style={styles.game_state_text_info_container}>{moves_count}</Text>
    </View>
  );
});

function Game({ width, height, score_bonuses, onStrike, onRestart }) {
  const request_animation_ref = useRef(null);
  const prev_animation_ref = useRef(null);
  const mouse_down_position_ref = useRef([]);
  const [game_state, set_game_state] = useState({
    field_data: new FieldData(width, height),
    moves_count: 0,
    selected_elements: [],
    step: -1,
    prev_step: -1,
    abilities: new Abilities(),
    score_state: {
      score: 0,
      total_score: 0,
      spent_score: 0
    }
  });
  const [element_style_provider, set_element_style_provider] = useState(undefined);
  const [grid_step, set_grid_step] = useState(0);
  const [element_offset, set_element_offset] = useState(0);
  const [is_game_over, set_is_game_over] = useState(false);
  const [autoplay, set_autoplay] = useState(false);

  useEffect(() => {
    const scale_factor = Platform.OS === "web" ? 0.75 : 1;
    var width = scale_factor * Dimensions.get("window").width;
    var height = scale_factor * Dimensions.get("window").height;
    const grid_x_step = Math.floor(width / game_state.field_data.width);
    const grid_y_step = Math.floor(height / game_state.field_data.height);
    const grid_step = Math.min(grid_x_step, grid_y_step);
    const element_offset = Math.floor(0.1 * grid_step);
    const element_size = grid_step - 2 * element_offset;
    if (!element_style_provider || element_size !== element_style_provider.size)
      set_element_style_provider(new ElementStyleProvider(element_size));
    set_grid_step(grid_step);
    set_element_offset(element_offset);
    request_animation_ref.current = requestAnimationFrame(update_game_state);
    return () => cancelAnimationFrame(request_animation_ref.current);
  }, [game_state.field_data, game_state.step, autoplay, game_state.selected_elements]);

  const check_for_game_over = () => {
    if (game_state.moves_count > 0)
      return;
    const abilities_prices = game_state.abilities.all_prices;
    for (let price of abilities_prices)
      if (price <= game_state.score_state.score)
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
    if (game_state.step !== -1)
      return;
    var moves = game_state.field_data.get_all_moves();
    if (moves.length > 0) {
      moves.sort((a, b) => b["strike"] - a["strike"]);
      const max_strike_value = moves[0]["strike"];
      let max_strike_value_count = 0;
      for (let move of moves) {
        if (move["strike"] < max_strike_value)
          break;
        ++max_strike_value_count;
      }
      const move_index = Math.trunc(Math.random() * max_strike_value_count);
      const move = moves[move_index]["move"];
      set_game_state({
        ...game_state,
        selected_elements: move,
        step: 3
      });
    } else {
      const cheapest_ability = game_state.abilities.cheapest;
      if (cheapest_ability === game_state.abilities.upgrade_generator ||
        game_state.abilities.upgrade_generator.price < game_state.score_state.score)
        upgrade_generator();
      else if (cheapest_ability === game_state.abilities.shuffle)
        shuffle();
      else if (cheapest_ability === game_state.abilities.bomb) {
        if (game_state.selected_elements.length === 0) {
          const row_id = Math.trunc(Math.random() * game_state.field_data.height);
          const column_id = Math.trunc(Math.random() * game_state.field_data.width);
          set_game_state({
            ...game_state,
            selected_elements: [[row_id, column_id]]
          });
        } else
          apply_bomb()
      } else if (cheapest_ability === game_state.abilities.remove_element) {
        if (game_state.selected_elements.length === 0) {
          const row_id = Math.trunc(Math.random() * game_state.field_data.height);
          const column_id = Math.trunc(Math.random() * game_state.field_data.width);
          set_game_state({
            ...game_state,
            selected_elements: [[row_id, column_id]]
          });
        } else
          remove_element()
      } else if (cheapest_ability === game_state.abilities.remove_elements_by_value) {
        if (game_state.selected_elements.length === 0) {
          const row_id = Math.trunc(Math.random() * game_state.field_data.height);
          const column_id = Math.trunc(Math.random() * game_state.field_data.width);
          set_game_state({
            ...game_state,
            selected_elements: [[row_id, column_id]]
          });
        } else
          remove_elements_by_value()
      }
    }
  };

  const update_game_state = (time) => {
    auto_move();
    prev_animation_ref.current = time;
    const steps = 4;
    let next_step = (game_state.step + 1) % steps;
    var prev_step = game_state.step;
    var field_data = undefined;
    var selected_elements = undefined;
    var swapping = game_state.swapping;
    var new_score_state = undefined;
    switch (game_state.step) {
      case -1:
        return;
      case 0:
        const removed_groups_details = game_state.field_data.accumulate_groups();
        const changed = removed_groups_details.length > 0;
        if (changed) {
          var total_score_delta = 0;
          for (let removed_group_details of removed_groups_details) {
            const value = 2 ** removed_group_details.value;
            const count = removed_group_details.size;
            onStrike(value, count);
            const bonus = count in score_bonuses ? score_bonuses[count] : 10;
            const score_delta = value * count * bonus;
            total_score_delta += score_delta;
          }
          new_score_state = {
            score: game_state.score_state.score + total_score_delta,
            total_score: game_state.score_state.total_score + total_score_delta,
            spent_score: game_state.score_state.spent_score
          }
          field_data = game_state.field_data.clone();
          if (game_state.prev_step === 3) {
            selected_elements = [];
            swapping = false;
          }
        } else if (game_state.prev_step === 3) {
          prev_step = 0;
          next_step = 3;
        } else if (game_state.prev_step != game_state.step) {
          next_step = -1;
          prev_step = -1;
          check_for_game_over();
        }
        break;
      case 1:
        game_state.field_data.move_elements();
        field_data = game_state.field_data.clone();
        break;
      case 2:
        game_state.field_data.spawn_new_values();
        field_data = game_state.field_data.clone();
        next_step = 0;
        break;
      case 3:
        if (game_state.selected_elements.length === 2) {
          game_state.field_data.swap_cells(
            game_state.selected_elements[0][0], game_state.selected_elements[0][1],
            game_state.selected_elements[1][0], game_state.selected_elements[1][1]
          );
          field_data = game_state.field_data.clone();
          if (game_state.prev_step === 0 && game_state.swapping) {
            selected_elements = [];
            swapping = false;
            next_step = -1;
            prev_step = -1;
          } else
            swapping = !swapping;
        }
        break;
      default:
        break;
    }
    set_game_state({
      ...game_state,
      field_data: field_data !== undefined ? field_data : game_state.field_data,
      moves_count: field_data !== undefined ? field_data.get_all_moves().length : game_state.moves_count,
      step: next_step,
      prev_step: prev_step,
      selected_elements: selected_elements !== undefined ? selected_elements : game_state.selected_elements,
      swapping: swapping,
      score_state: new_score_state !== undefined ? new_score_state : game_state.score_state
    });
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
      var selected_elements = [];
      var step = game_state.step;
      if (game_state.selected_elements.length === 1)
        distance = manhattan_distance(
          game_state.selected_elements[0][0], game_state.selected_elements[0][1],
          field_element_coordinates[0], field_element_coordinates[1]
        );
      if (game_state.selected_elements.length === 0 || distance > 1) {
        selected_elements.push(field_element_coordinates);
        mouse_down_position_ref.current = [x, y];
      }
      else if (distance === 0) {
        selected_elements = [];
        mouse_down_position_ref.current = [];
      }
      else if (game_state.selected_elements.length === 1) {
        selected_elements = [...game_state.selected_elements, field_element_coordinates];
        step = 3;
      }
      set_game_state({
        ...game_state,
        step: step,
        selected_elements: selected_elements
      });
    }
  };

  const on_mouse_move = (event) => {
    event.preventDefault();
  };

  const on_mouse_up = (event) => {
    if (game_state.swapping)
      return;
    if (mouse_down_position_ref.current.length === 0)
      return;
    if (game_state.selected_elements.length === 2)
      return;
    if (game_state.selected_elements.length === 0)
      return;
    const [x, y] = get_event_position(event);
    const dx = x - mouse_down_position_ref.current[0];
    const dy = y - mouse_down_position_ref.current[1];
    if (Math.abs(dx) < grid_step / 4 && Math.abs(dy) < grid_step / 4)
      return;
    var factors = [0, 0];
    if (Math.abs(dx) > Math.abs(dy))
      factors[0] = 1 * Math.sign(dx);
    else
      factors[1] = 1 * Math.sign(dy);
    set_game_state({
      ...game_state,
      step: 3,
      selected_elements: [...game_state.selected_elements,
      [
        game_state.selected_elements[0][0] + factors[1],
        game_state.selected_elements[0][1] + factors[0]
      ]]
    });
  };

  const shuffle = () => {
    if (game_state.score_state.score < game_state.abilities.shuffle.price)
      return;
    game_state.field_data.shuffle();
    const negative_score_value = game_state.abilities.shuffle.price;
    game_state.abilities.shuffle.next_price();
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      step: 0,
      abilities: game_state.abilities.clone(),
      score_state: {
        score: game_state.score_state.score - negative_score_value,
        total_score: game_state.score_state.total_score,
        spent_score: game_state.score_state.spent_score + negative_score_value
      }
    });
  };

  const upgrade_generator = () => {
    if (game_state.score_state.score < game_state.abilities.upgrade_generator.price)
      return;
    game_state.field_data.increase_values_interval();
    const small_value = game_state.field_data.values_interval[0] - 1;
    const values_count = game_state.field_data.remove_value(small_value);
    const positive_score_value = values_count * 2 ** small_value;
    const negative_score_value = game_state.abilities.upgrade_generator.price;
    game_state.abilities.upgrade_generator.next_price();
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      step: 1,
      abilities: game_state.abilities.clone(),
      score_state: {
        score: game_state.score_state.score + positive_score_value - negative_score_value,
        total_score: game_state.score_state.total_score + positive_score_value,
        spent_score: game_state.score_state.spent_score + negative_score_value
      }
    });
  };

  const apply_bomb = () => {
    if (game_state.score_state.score < game_state.abilities.bomb.price)
      return;
    if (game_state.selected_elements.length === 0)
      return;
    const removed_values = game_state.field_data.remove_zone(
      game_state.selected_elements[0][0] - 1, game_state.selected_elements[0][1] - 1,
      game_state.selected_elements[0][0] + 1, game_state.selected_elements[0][1] + 1
    );
    var positive_score_value = 0;
    for (let [value, count] of Object.entries(removed_values))
      positive_score_value += 2 ** value * count;
    const negative_score_value = game_state.abilities.bomb.price;
    game_state.abilities.bomb.next_price();
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      step: 1,
      selected_elements: [],
      abilities: game_state.abilities.clone(),
      score_state: {
        score: game_state.score_state.score + positive_score_value - negative_score_value,
        total_score: game_state.score_state.total_score + positive_score_value,
        spent_score: game_state.score_state.spent_score + negative_score_value
      }
    });
  };

  const remove_element = () => {
    if (game_state.score_state.score < game_state.abilities.remove_element.price)
      return;
    if (game_state.selected_elements.length === 0)
      return;
    const removed_values = game_state.field_data.remove_zone(
      game_state.selected_elements[0][0], game_state.selected_elements[0][1],
      game_state.selected_elements[0][0], game_state.selected_elements[0][1]
    );
    const positive_score_value = 2 ** Object.keys(removed_values)[0];
    const negative_score_value = game_state.abilities.remove_element.price;
    game_state.abilities.remove_element.next_price();
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      step: 1,
      selected_elements: [],
      abilities: game_state.abilities.clone(),
      score_state: {
        score: game_state.score_state.score + positive_score_value - negative_score_value,
        total_score: game_state.score_state.total_score + positive_score_value,
        spent_score: game_state.score_state.spent_score + negative_score_value
      }
    });
  };

  const remove_elements_by_value = () => {
    if (game_state.score_state.score < game_state.abilities.remove_elements_by_value.price)
      return;
    if (game_state.selected_elements.length === 0)
      return;
    const value_position = game_state.selected_elements[0];
    const value = game_state.field_data.at(value_position[0], value_position[1]);
    const removed_values_count = game_state.field_data.remove_value(value);
    const positive_score_value = removed_values_count * 2 ** value;
    const negative_score_value = game_state.abilities.remove_elements_by_value.price;
    game_state.abilities.remove_elements_by_value.next_price();
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      step: 1,
      selected_elements: [],
      abilities: game_state.abilities.clone(),
      score_state: {
        score: game_state.score_state.score + positive_score_value - negative_score_value,
        total_score: game_state.score_state.total_score + positive_score_value,
        spent_score: game_state.score_state.spent_score + negative_score_value
      }
    });
  };

  const restart = () => {
    set_game_state({
      field_data: new FieldData(width, height),
      step: -1,
      prev_step: -1,
      selected_elements: [],
      swapping: false,
      abilities: new Abilities(),
      score_state: {
        score: 0,
        total_score: 0,
        spent_score: 0
      }
    });
    set_is_game_over(false);
    onRestart();
  }

  return (
    <View style={styles.elements_container}>
      <GameOver
        total_score={game_state.score_state.total_score}
        spent_score={game_state.score_state.spent_score}
        visible={is_game_over}
        onRestart={restart}
      />
      <ScoreVisualizer
        score={game_state.score_state.score}
        moves_count={game_state.moves_count}
      />
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
          field_data={game_state.field_data}
          selected_elements={game_state.selected_elements}
          element_style_provider={element_style_provider}
        />}
      </View>
      <AbilitiesVisualizer
        abilities={game_state.abilities}
        score={game_state.score_state.score}
        onShuffle={shuffle}
        onRemoveElement={remove_element}
        onBomb={apply_bomb}
        onRemoveElementsByValue={remove_elements_by_value}
        onUpgradeGenerator={upgrade_generator}
        onAutoplay={() => set_autoplay(!autoplay)}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  elements_container: {
    flex: 1,
    flexDirection: 'column',
    alignContent: "center",
    justifyContent: "center",
  },

  score_container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    padding: 5,

    backgroundColor: '#aaa',
    fontSize: 48,
    borderTopLeftRadius: 10,
    borderTopRightRadius: 10,
  },

  game_state_text_info_container: {
    fontWeight: 'bold',
    textShadowColor: 'white',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 5,
  },

  canvas_container: {
    justifyContent: "center",
    alignContent: "center",
    flexWrap: "wrap"
  },
});

export default memo(Game);