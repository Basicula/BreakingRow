import { useState, useEffect } from "react";
import { StyleSheet, View, Text } from 'react-native';

import GameField from "./components/GameField.js";
import Statistics from "./components/Statistics.js";
import Infos from "./components/Infos.js";

export default function App() {
  const [window_size, set_window_size] = useState([]);
  const [score, set_score] = useState(0);
  const [moves_count, set_moves_count] = useState(0);
  const [elements_count, set_elements_count] = useState({});
  const [strikes_statistics, set_strike_statistics] = useState({});

  const score_bonuses = {
    3: 1,
    4: 2,
    5: 5,
    6: 5,
    7: 10
  };

  const update_statistics = (value, count) => {
    set_score(score + value * count * score_bonuses[count]);
    var new_elements_count = elements_count;
    if (value in elements_count)
      new_elements_count[value] += count;
    else
      new_elements_count[value] = count;
    set_elements_count(new_elements_count);
    var new_strike_statistics = strikes_statistics;
    if (count in strikes_statistics)
      ++new_strike_statistics[count]
    else
      new_strike_statistics[count] = 1
    set_strike_statistics(new_strike_statistics);
  }


  useEffect(() => {
    function updateSize() {
      set_window_size([window.innerWidth, window.innerHeight]);
    }
    window.addEventListener('resize', updateSize);
    updateSize();
    return () => window.removeEventListener('resize', updateSize);
  }, []);

  return (
    <View style={styles.app_container}>
      <Statistics elements_count={elements_count} strikes_statistics={strikes_statistics} />
      <View style={styles.game_field_container}>
        <View style={styles.score_container}>
          <Text style={styles.score_title_container}>Score</Text>
          <Text style={styles.score_value_container}>{score}</Text>
          <Text style={styles.moves_count_title_container}>Moves count</Text>
          <Text style={styles.moves_count_value_container}>{moves_count}</Text>
        </View>
        <GameField
          width={7}
          height={7}
          onStrike={update_statistics}
          onMovesCountChange={count => set_moves_count(count)}
        />
      </View>
      <Infos score_bonuses={score_bonuses}></Infos>
    </View>
  );
}

const styles = StyleSheet.create({
  app_container: {
    width: '100%',
    height: '100%',

    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
  },

  score_container: {
    width: '100%',

    flexDirection: 'row',
    justifyContent: 'space-between',
    padding: '5px',
    marginBottom: '2px',
    boxSizing: 'border-box',

    backgroundColor: '#aaa',

    fontSize: '48px',

    borderRadius: '10px',
  },

  score_title_container: {
    fontWeight: 'bold',
    textShadow: '2px 2px 5px white',
  },
  score_value_container: {
    fontWeight: 'bold',
    textShadow: '2px 2px 5px white',
  },

  moves_count_title_container: {
    fontWeight: 'bold',
    textShadow: '2px 2px 5px white',
  },
  moves_count_value_container: {
    fontWeight: 'bold',
    textShadow: '2px 2px 5px white',
  },

  game_field_container: {
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    padding: '5px',
  }
});
