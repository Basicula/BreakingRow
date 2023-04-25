import { useRef, useEffect, useState, memo } from "react";
import { StatusBar, StyleSheet, View, Text, Platform, Dimensions } from 'react-native';

import { FieldData } from "./GameFieldData.js";
import GameField from "./GameField.js";
import { map_coordinates } from "./Utils.js";
import { ElementStyleProvider } from "./ElementStyleProvider.js";
import GameOver from "./GameOver.js";
import { Abilities, AbilitiesVisualizer } from "./Abilities.js";

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

const AbilityType = Object.freeze({
  RemoveElement: "RemoveElement",
  Bomb: "Bomb",
  RemoveElementsByValue: "RemoveElementsByValue",
  None: "None"
});

function useScore(init_score) {
  const [score, set_score] = useState(init_score);
  const [earned_score, set_earned_score] = useState(init_score);
  const [spent_score, set_spent_score] = useState(0);

  const update_score = (earned_score_value, spent_score_value) => {
    set_score(score + earned_score_value - spent_score_value);
    set_earned_score(earned_score_value + earned_score_value);
    set_spent_score(spent_score + spent_score_value);
  };

  const reset_score = (score_value = 0, spent_score_value = 0) => {
    set_score(score_value);
    set_earned_score(score_value);
    set_spent_score(spent_score_value);
  };

  return [score, earned_score, spent_score, update_score, reset_score];
}

function Game({ width, height, score_bonuses, onStrike, onRestart }) {
  const [highlighted_elements, set_highlighted_elements] = useState([]);
  const [game_state, set_game_state] = useState({
    field_data: new FieldData(width, height),
    moves_count: 0,
    abilities: new Abilities()
  });
  const [score, earned_score, spent_score, update_score, reset_score] = useScore(0);
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
    set_game_state({ ...game_state, moves_count: game_state.field_data.get_all_moves().length });
  }, []);

  useEffect(() => {
    auto_move();
    check_for_game_over();
  }, [game_state.field_data, autoplay]);

  const game_field_offset = useRef({ x: 0, y: 0 });
  const on_game_field_layout = (event) => {
    if (Platform.OS === "web")
      event.nativeEvent.target.measure((x, y, width, height, pageX, pageY) => {
        game_field_offset.current.x = pageX;
        game_field_offset.current.y = pageY;
      });
    else if (Platform.OS === "android") {
      game_field_offset.current.x = event.nativeEvent.layout.x;
      game_field_offset.current.y = event.nativeEvent.layout.y + StatusBar.currentHeight;
    }
  };

  const check_for_game_over = () => {
    if (game_state.field_data.get_all_moves().length > 0)
      return;
    const abilities_prices = game_state.abilities.all_prices;
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
    if (game_state.field_data.has_empty_cells())
      return;
    if (game_state.field_data.has_groups())
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
      game_state.field_data.swap_cells(...move[0], ...move[1]);
      set_game_state({
        ...game_state,
        field_data: game_state.field_data.clone(),
      });
    } else {
      const cheapest_ability = game_state.abilities.cheapest;
      if (cheapest_ability === game_state.abilities.upgrade_generator ||
        game_state.abilities.upgrade_generator.price < score)
        upgrade_generator();
      else if (cheapest_ability === game_state.abilities.shuffle)
        shuffle();
      else {
        var ability_type = AbilityType.None;
        if (cheapest_ability === game_state.abilities.bomb)
          ability_type = AbilityType.Bomb;
        else if (cheapest_ability === game_state.abilities.remove_element)
          ability_type = AbilityType.RemoveElement;
        else if (cheapest_ability === game_state.abilities.remove_elements_by_value)
          ability_type = AbilityType.RemoveElementsByValue;
        const row_id = Math.trunc(Math.random() * game_state.field_data.height);
        const column_id = Math.trunc(Math.random() * game_state.field_data.width);
        apply_ability([row_id, column_id], ability_type);
      }
    }
  };

  const on_ability_move = (x, y, ability_type) => {
    x -= game_field_offset.current.x;
    y -= game_field_offset.current.y;
    const field_element_coordinates = map_coordinates(x, y, grid_step);

    if (field_element_coordinates[0] >= game_state.field_data.height ||
      field_element_coordinates[0] < 0 ||
      field_element_coordinates[1] >= game_state.field_data.width ||
      field_element_coordinates[1] < 0) {
      set_highlighted_elements([]);
      return
    }

    var new_highlighted_elements = [];
    switch (ability_type) {
      case AbilityType.RemoveElement:
        new_highlighted_elements.push(field_element_coordinates);
        break;
      case AbilityType.Bomb:
        for (let row_offset = -1; row_offset < 2; ++row_offset)
          for (let column_offset = -1; column_offset < 2; ++column_offset)
            new_highlighted_elements.push([
              field_element_coordinates[0] + row_offset,
              field_element_coordinates[1] + column_offset
            ]);
        break;
      case AbilityType.RemoveElementsByValue:
        const value = game_state.field_data.at(field_element_coordinates[0], field_element_coordinates[1]);
        for (let row_id = 0; row_id < game_state.field_data.height; ++row_id)
          for (let column_id = 0; column_id < game_state.field_data.width; ++column_id)
            if (game_state.field_data.at(row_id, column_id) === value)
              new_highlighted_elements.push([row_id, column_id]);
        break;
      case AbilityType.None:
        break;
    }
    set_highlighted_elements(new_highlighted_elements);
  };

  const apply_ability = (element_coordinates, ability_type) => {
    if (element_coordinates === undefined)
      return;
    switch (ability_type) {
      case AbilityType.RemoveElement:
        remove_element(element_coordinates);
        break;
      case AbilityType.Bomb:
        apply_bomb(element_coordinates);
        break;
      case AbilityType.RemoveElementsByValue:
        remove_elements_by_value(element_coordinates);
        break;
      default:
        break;
    }
    set_highlighted_elements([]);
  };

  const shuffle = () => {
    if (score < game_state.abilities.shuffle.price)
      return;
    game_state.field_data.shuffle();
    game_state.abilities.shuffle.next_price();
    update_score(0, game_state.abilities.shuffle.price);
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      abilities: game_state.abilities.clone()
    });
  };

  const upgrade_generator = () => {
    if (score < game_state.abilities.upgrade_generator.price)
      return;
    game_state.field_data.increase_values_interval();
    const small_value = game_state.field_data.values_interval[0] - 1;
    const values_count = game_state.field_data.remove_value(small_value);
    const earned_score_value = values_count * 2 ** small_value;
    const spent_score_value = game_state.abilities.upgrade_generator.price;
    game_state.abilities.upgrade_generator.next_price();
    update_score(earned_score_value, spent_score_value);
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      abilities: game_state.abilities.clone()
    });
  };

  const apply_bomb = (element_coordinates) => {
    if (score < game_state.abilities.bomb.price)
      return;
    const removed_values = game_state.field_data.remove_zone(
      element_coordinates[0] - 1, element_coordinates[1] - 1,
      element_coordinates[0] + 1, element_coordinates[1] + 1
    );
    var earned_score_value = 0;
    for (let [value, count] of Object.entries(removed_values))
      earned_score_value += 2 ** value * count;
    const spent_score_value = game_state.abilities.bomb.price;
    game_state.abilities.bomb.next_price();
    update_score(earned_score_value, spent_score_value);
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      abilities: game_state.abilities.clone()
    });
  };

  const remove_element = (element_coordinates) => {
    if (score < game_state.abilities.remove_element.price)
      return;
    const removed_values = game_state.field_data.remove_zone(
      element_coordinates[0], element_coordinates[1],
      element_coordinates[0], element_coordinates[1]
    );
    const earned_score_value = 2 ** Object.keys(removed_values)[0];
    const spent_score_value = game_state.abilities.remove_element.price;
    game_state.abilities.remove_element.next_price();
    update_score(earned_score_value, spent_score_value);
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      abilities: game_state.abilities.clone()
    });
  };

  const remove_elements_by_value = (element_coordinates) => {
    if (score < game_state.abilities.remove_elements_by_value.price)
      return;
    const value = game_state.field_data.at(element_coordinates[0], element_coordinates[1]);
    const removed_values_count = game_state.field_data.remove_value(value);
    const earned_score_value = removed_values_count * 2 ** value;
    const spent_score_value = game_state.abilities.remove_elements_by_value.price;
    game_state.abilities.remove_elements_by_value.next_price();
    update_score(earned_score_value, spent_score_value);
    set_game_state({
      ...game_state,
      field_data: game_state.field_data.clone(),
      abilities: game_state.abilities.clone()
    });
  };

  const highlight_move = () => {
    const moves = game_state.field_data.get_all_moves();
    const move_index = Math.trunc(Math.random() * moves.length);
    const move = moves[move_index]["move"];
    set_highlighted_elements(move);
    setTimeout(() => set_highlighted_elements([]), 1000);
  };

  const accumulate_elements = (new_field_data, removed_groups_details) => {
    var earned_score_value = 0;
    for (let removed_group_details of removed_groups_details) {
      const value = 2 ** removed_group_details.value;
      const count = removed_group_details.group.length;
      onStrike(value, count);
      const bonus = count in score_bonuses ? score_bonuses[count] : 10;
      const score_delta = value * count * bonus;
      earned_score_value += score_delta;
    }
    update_score(earned_score_value, 0);
    set_game_state({
      ...game_state,
      field_data: new_field_data,
      moves_count: new_field_data.get_all_moves().length
    });
  };

  const restart = () => {
    set_highlighted_elements([]);
    set_game_state({
      field_data: new FieldData(width, height),
      abilities: new Abilities()
    });
    reset_score();
    set_is_game_over(false);
    onRestart();
  }

  return (
    <View style={styles.elements_container}>
      <GameOver
        earned_score={earned_score}
        spent_score={spent_score}
        visible={is_game_over}
        onRestart={restart}
      />
      <ScoreVisualizer
        score={score}
        moves_count={game_state.moves_count}
      />
      {grid_step > 0 &&
        <GameField
          grid_step={grid_step}
          element_offset={element_offset}
          field_data={game_state.field_data.clone()}
          highlighted_elements={highlighted_elements}
          element_style_provider={element_style_provider}
          onFieldDataChange={(new_field_data) => set_game_state({
            ...game_state,
            field_data: new_field_data,
            moves_count: new_field_data.get_all_moves().length
          })}
          onAccumulateElements={accumulate_elements}
          onLayout={on_game_field_layout}
        />
      }
      <AbilitiesVisualizer
        abilities={game_state.abilities}
        score={score}
        onRemoveElement={() => apply_ability(highlighted_elements[0], AbilityType.RemoveElement)}
        onBomb={() => apply_ability(highlighted_elements[4], AbilityType.Bomb)}
        onRemoveElementsByValue={() => apply_ability(highlighted_elements[0], AbilityType.RemoveElementsByValue)}
        onRemoveElementMove={(x, y) => on_ability_move(x, y, AbilityType.RemoveElement)}
        onBombMove={(x, y) => on_ability_move(x, y, AbilityType.Bomb)}
        onRemoveElementsByValueMove={(x, y) => on_ability_move(x, y, AbilityType.RemoveElementsByValue)}
        onShuffle={shuffle}
        onUpgradeGenerator={upgrade_generator}
        onSearch={highlight_move}
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
  }
});

export default memo(Game);