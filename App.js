import { useState, useEffect } from "react";
import { Platform, StyleSheet, View } from 'react-native';

import Game from "./components/Game.js";
import Statistics from "./components/Statistics.js";
import Infos from "./components/Infos.js";

export default function App() {
  const [window_size, set_window_size] = useState([]);
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
    var new_elements_count = Object.assign({}, elements_count);
    if (value in elements_count)
      new_elements_count[value] += count;
    else
      new_elements_count[value] = count;
    set_elements_count(new_elements_count);
    var new_strike_statistics = Object.assign({}, strikes_statistics);
    if (count in strikes_statistics)
      ++new_strike_statistics[count]
    else
      new_strike_statistics[count] = 1
    set_strike_statistics(new_strike_statistics);
  }

  useEffect(() => {
    if (Platform.OS === "web") {
      function updateSize() {
        set_window_size([window.innerWidth, window.innerHeight]);
      }
      window.addEventListener('resize', updateSize);
      updateSize();
      return () => window.removeEventListener('resize', updateSize);
    }
  }, []);

  return (
    <View style={styles.app_container}>
      <Statistics elements_count={elements_count} strikes_statistics={strikes_statistics} />
      <Game
        width={7}
        height={7}
        score_bonuses={score_bonuses}
        onStrike={update_statistics}
      />
      <Infos score_bonuses={score_bonuses}></Infos>
    </View>
  );
}

const styles = StyleSheet.create({
  app_container: {
    flex: 1,

    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
  }
});
