import { useRef, useEffect, useState, memo } from "react";
import { StatusBar, StyleSheet, View, Text, Platform, Dimensions } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

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
  const score_key = "Score";
  const [score_state, set_score_state] = useState({});

  useEffect(() => {
    AsyncStorage.getItem(score_key).then(json_data => {
      if (json_data === null) {
        const init_score_state = {
          score: init_score,
          earned_score: init_score,
          spent_score: 0
        };
        AsyncStorage.setItem(score_key, JSON.stringify(init_score_state));
        set_score_state(init_score_state);
      }
      else
        set_score_state(JSON.parse(json_data));
    });
  }, []);

  const update_score = (earned_score_value, spent_score_value) => {
    const new_score_state = {
      score: score_state.score + earned_score_value - spent_score_value,
      earned_score: score_state.earned_score + earned_score_value,
      spent_score: score_state.spent_score + spent_score_value
    };
    AsyncStorage.setItem(score_key, JSON.stringify(new_score_state));
    set_score_state(new_score_state);
  };

  const reset_score = (score_value = 0, spent_score_value = 0) => {
    const new_score_state = {
      score: score_value,
      earned_score: score_value,
      spent_score: spent_score_value
    };
    AsyncStorage.setItem(score_key, JSON.stringify(new_score_state));
    set_score_state(new_score_state);
  };

  return [score_state.score, score_state.earned_score, score_state.spent_score, update_score, reset_score];
}

function useFieldData(width, height) {
  const field_data_key = "FieldData";
  const [field_data, set_field_data] = useState(new FieldData(width, height));
  const [moves_count, set_moves_count] = useState(field_data.get_all_moves().length);

  useEffect(() => {
    AsyncStorage.getItem(field_data_key).then(json_data => {
      if (json_data === null) {
        const new_field_data = new FieldData(width, height);
        AsyncStorage.setItem(field_data_key, new_field_data.stringify());
        set_field_data(new_field_data);
        set_moves_count(new_field_data.get_all_moves().length);
      }
      else {
        var new_field_data = FieldData.parse(json_data);
        set_field_data(new_field_data);
        set_moves_count(new_field_data.get_all_moves().length);
      }
    });
  }, []);

  const update_field_data = (new_field_data) => {
    AsyncStorage.setItem(field_data_key, new_field_data.stringify());
    set_field_data(new_field_data);
    set_moves_count(new_field_data.get_all_moves().length);
  };

  return [field_data, moves_count, update_field_data];
}

function useAbilities() {
  const abilities_key = "Abilities";
  const [abilities, set_abilities] = useState(new Abilities());

  useEffect(() => {
    AsyncStorage.getItem(abilities_key).then(json_data => {
      if (json_data === null) {
        const new_abilities = new Abilities();
        AsyncStorage.setItem(abilities_key, new_abilities.stringify());
        set_abilities(new_abilities);
      }
      else
        set_abilities(Abilities.parse(json_data));
    });
  }, []);

  const update_abilities = (new_abilities) => {
    AsyncStorage.setItem(abilities_key, new_abilities.stringify());
    set_abilities(new_abilities);
  };

  return [abilities, update_abilities];
}

function Game({ width, height, score_bonuses, onStrike, onRestart }) {
  const [highlighted_elements, set_highlighted_elements] = useState([]);
  const [field_data, moves_count, set_field_data] = useFieldData(width, height);
  const [abilities, set_abilities] = useAbilities();
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
    const grid_x_step = Math.floor(width / field_data.width);
    const grid_y_step = Math.floor(height / field_data.height);
    const grid_step = Math.min(grid_x_step, grid_y_step);
    const element_offset = Math.floor(0.1 * grid_step);
    const element_size = grid_step - 2 * element_offset;
    if (!element_style_provider || element_size !== element_style_provider.size)
      set_element_style_provider(new ElementStyleProvider(element_size));
    set_grid_step(grid_step);
    set_element_offset(element_offset);
  }, []);

  useEffect(() => {
    check_for_game_over();
    auto_move();
  }, [field_data, autoplay]);

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
    if (field_data.has_empty_cells())
      return;
    if (field_data.has_groups())
      return;
    if (field_data.get_all_moves().length > 0)
      return;
    const abilities_prices = abilities.all_prices;
    for (let price of abilities_prices)
      if (price <= score)
        return;
    set_is_game_over(true);
    set_autoplay(false);
  };

  const auto_move = () => {
    if (!autoplay)
      return;
    if (field_data.has_empty_cells())
      return;
    if (field_data.has_groups())
      return;
    var moves = field_data.get_all_moves();
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
      field_data.swap_cells(...move[0], ...move[1]);
      set_field_data(field_data.clone());
    } else {
      const cheapest_ability = abilities.cheapest;
      if (cheapest_ability === abilities.upgrade_generator || abilities.upgrade_generator.price < score)
        upgrade_generator();
      else if (cheapest_ability === abilities.shuffle)
        shuffle();
      else {
        var ability_type = AbilityType.None;
        if (cheapest_ability === abilities.bomb)
          ability_type = AbilityType.Bomb;
        else if (cheapest_ability === abilities.remove_element)
          ability_type = AbilityType.RemoveElement;
        else if (cheapest_ability === abilities.remove_elements_by_value)
          ability_type = AbilityType.RemoveElementsByValue;
        const row_id = Math.trunc(Math.random() * field_data.height);
        const column_id = Math.trunc(Math.random() * field_data.width);
        apply_ability([row_id, column_id], ability_type);
      }
    }
  };

  const on_ability_move = (x, y, ability_type) => {
    x -= game_field_offset.current.x;
    y -= game_field_offset.current.y;
    const field_element_coordinates = map_coordinates(x, y, grid_step);

    if (field_element_coordinates[0] >= field_data.height ||
      field_element_coordinates[0] < 0 ||
      field_element_coordinates[1] >= field_data.width ||
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
        const value = field_data.at(field_element_coordinates[0], field_element_coordinates[1]);
        for (let row_id = 0; row_id < field_data.height; ++row_id)
          for (let column_id = 0; column_id < field_data.width; ++column_id)
            if (field_data.at(row_id, column_id) === value)
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
    if (score < abilities.shuffle.price)
      return;
    field_data.shuffle();
    abilities.shuffle.next_price();
    update_score(0, abilities.shuffle.price);
    set_field_data(field_data.clone());
    set_abilities(abilities.clone());
  };

  const upgrade_generator = () => {
    if (score < abilities.upgrade_generator.price)
      return;
    field_data.increase_values_interval();
    const small_value = field_data.values_interval[0] - 1;
    const values_count = field_data.remove_value(small_value);
    const earned_score_value = values_count * 2 ** small_value;
    const spent_score_value = abilities.upgrade_generator.price;
    abilities.upgrade_generator.next_price();
    update_score(earned_score_value, spent_score_value);
    set_field_data(field_data.clone());
    set_abilities(abilities.clone());
  };

  const apply_bomb = (element_coordinates) => {
    if (score < abilities.bomb.price)
      return;
    const removed_values = field_data.remove_zone(
      element_coordinates[0] - 1, element_coordinates[1] - 1,
      element_coordinates[0] + 1, element_coordinates[1] + 1
    );
    var earned_score_value = 0;
    for (let [value, count] of Object.entries(removed_values))
      earned_score_value += 2 ** value * count;
    const spent_score_value = abilities.bomb.price;
    abilities.bomb.next_price();
    update_score(earned_score_value, spent_score_value);
    set_field_data(field_data.clone());
    set_abilities(abilities.clone());
  };

  const remove_element = (element_coordinates) => {
    if (score < abilities.remove_element.price)
      return;
    const removed_values = field_data.remove_zone(
      element_coordinates[0], element_coordinates[1],
      element_coordinates[0], element_coordinates[1]
    );
    const earned_score_value = 2 ** Object.keys(removed_values)[0];
    const spent_score_value = abilities.remove_element.price;
    abilities.remove_element.next_price();
    update_score(earned_score_value, spent_score_value);
    set_field_data(field_data.clone());
    set_abilities(abilities.clone());
  };

  const remove_elements_by_value = (element_coordinates) => {
    if (score < abilities.remove_elements_by_value.price)
      return;
    const value = field_data.at(element_coordinates[0], element_coordinates[1]);
    const removed_values_count = field_data.remove_value(value);
    const earned_score_value = removed_values_count * 2 ** value;
    const spent_score_value = abilities.remove_elements_by_value.price;
    abilities.remove_elements_by_value.next_price();
    update_score(earned_score_value, spent_score_value);
    set_field_data(field_data.clone());
    set_abilities(abilities.clone());
  };

  const highlight_move = () => {
    const moves = field_data.get_all_moves();
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
    set_field_data(new_field_data);
  };

  const restart = () => {
    set_highlighted_elements([]);
    var new_field_data = new FieldData(width, height);
    set_field_data(new_field_data);
    set_abilities(new Abilities());
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
        moves_count={moves_count}
      />
      {grid_step > 0 &&
        <GameField
          grid_step={grid_step}
          element_offset={element_offset}
          field_data={field_data.clone()}
          highlighted_elements={highlighted_elements}
          element_style_provider={element_style_provider}
          onFieldDataChange={(new_field_data) => set_field_data(new_field_data)}
          onAccumulateElements={accumulate_elements}
          onLayout={on_game_field_layout}
        />
      }
      <AbilitiesVisualizer
        abilities={abilities}
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