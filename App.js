import { useState, useEffect } from "react";
import { Platform, StyleSheet, View } from 'react-native';

import Game from "./components/Game.js";
import Infos from "./components/Infos.js";

export default function App() {
  const [window_size, set_window_size] = useState([]);
  const [statistics, set_statistics] = useState({
    elements_count: {},
    strikes: {}
  });

  const score_bonuses = {
    3: 1,
    4: 2,
    5: 5,
    6: 5,
    7: 10
  };

  const update_statistics = (value, count) => {
    var new_elements_count = Object.assign({}, statistics.elements_count);
    if (value in new_elements_count)
      new_elements_count[value] += count;
    else
      new_elements_count[value] = count;
    var new_strike_statistics = Object.assign({}, statistics.strikes);
    if (count in new_strike_statistics)
      ++new_strike_statistics[count]
    else
      new_strike_statistics[count] = 1
    set_statistics({
      elements_count: new_elements_count,
      strikes: new_strike_statistics
    });
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
      <Game
        width={7}
        height={7}
        score_bonuses={score_bonuses}
        onStrike={update_statistics}
      />
      <Infos
        score_bonuses={score_bonuses}
        elements_count={statistics.elements_count}
        strikes_statistics={statistics.strikes}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  app_container: {
    flex: 1,

    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: "#aaddff"
  }
});
