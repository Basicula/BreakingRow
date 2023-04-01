import { memo } from "react";
import { StyleSheet, View, Text, TouchableOpacity } from 'react-native';

import Bomb from "./Icons/Bomb.js";
import Shuffle from "./Icons/Shuffle.js";
import Hammer from "./Icons/Hammer.js";
import Upgrade from "./Icons/Upgrade.js";
import Lightning from "./Icons/Lightning.js";

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

export class Abilities {
  #all;
  constructor() {
    this.#all = [
      new Ability("Shuffle", 2 ** 8, undefined, 2),
      new Ability("Remove element", 2, undefined, 2),
      new Ability("Bomb", 2 ** 7, undefined, 2),
      new Ability("Upgrade generator", 2 ** 12, undefined, 4),
      new Ability("Remove elements by value", 2 ** 8, undefined, 4),
    ];
  }

  get shuffle() {
    return this.#all[0];
  }

  get remove_element() {
    return this.#all[1]
  }

  get bomb() {
    return this.#all[2];
  }

  get upgrade_generator() {
    return this.#all[3];
  }

  get remove_elements_by_value() {
    return this.#all[4]
  }

  get all_prices() {
    var prices = [];
    for (let ability of this.#all)
      prices.push(ability.price);
    return prices;
  }

  get cheapest() {
    var cheapest_ability = this.#all[0];
    for (let i = 1; i < this.#all.length; ++i) {
      const ability = this.#all[i];
      if (ability.price < cheapest_ability.price)
        cheapest_ability = ability;
    }
    return cheapest_ability;
  }

  clone() {
    var new_abilities = new Abilities();
    new_abilities.#all = this.#all;
    return new_abilities;
  }
}

export const AbilitiesVisualizer = memo(function ({ abilities, score,
  onShuffle, onRemoveElement, onBomb, onRemoveElementsByValue, onUpgradeGenerator, onAutoplay }) {
  const disabled_opacity = 0.5;
  const ability_icon_size = 32;
  const abilities_data = [
    {
      icon: <Shuffle size={ability_icon_size} />,
      name: abilities.shuffle.name,
      price: abilities.shuffle.price,
      callback: onShuffle
    },
    {
      icon: <Hammer size={ability_icon_size} />,
      name: abilities.remove_element.name,
      price: abilities.remove_element.price,
      callback: onRemoveElement
    },
    {
      icon: <Bomb size={ability_icon_size} />,
      name: abilities.bomb.name,
      price: abilities.bomb.price,
      callback: onBomb
    },
    {
      icon: <Lightning size={ability_icon_size} />,
      name: abilities.remove_elements_by_value.name,
      price: abilities.remove_elements_by_value.price,
      callback: onRemoveElementsByValue
    },
    {
      icon: <Upgrade size={ability_icon_size} />,
      name: abilities.upgrade_generator.name,
      price: abilities.upgrade_generator.price,
      callback: onUpgradeGenerator
    },
  ];
  return (
    <View style={styles.abilities_container}>
      {abilities_data.map((ability_data, i) => {
        return (
          <TouchableOpacity
            style={{
              ...styles.ability_button,
              opacity: score < ability_data.price ? disabled_opacity : 1
            }}
            disabled={score < ability_data.price}
            onPress={ability_data.callback}
            key={i}
          >
            {ability_data.icon}
            <Text style={styles.ability_button_text}>{ability_data.name}</Text>
            <Text style={styles.ability_button_price}>{ability_data.price}</Text>
          </TouchableOpacity>
        );
      })}
      <TouchableOpacity style={styles.ability_button} onPress={onAutoplay}>
        <Text style={styles.ability_button_text}>Autoplay</Text>
        <Text style={styles.ability_button_price}>0</Text>
      </TouchableOpacity>
    </View>
  );
});

const styles = StyleSheet.create({
  abilities_container: {
    flexDirection: 'row',
    justifyContent: 'center',
    flexWrap: "wrap"
  },

  ability_button: {
    alignItems: "center",
    justifyContent: "center",
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
    fontSize: 8,
  },

  ability_button_price: {
    fontSize: 10,
    fontWeight: 'bold',
    textAlign: 'center'
  },
});