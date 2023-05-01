import { useState } from "react";
import { StyleSheet, View, Text, TouchableOpacity, StatusBar } from 'react-native';

import PopupContainer from "./components/PopupContainer.js";
import Settings from "./components/Settings.js";
import Game from "./components/Game.js";
import Infos from "./components/Infos.js";
import Gear from "./components/Icons/Gear.js";
import Info from "./components/Icons/Info.js";

function MenuBar({ onInfo, onSettings }) {
  const menu_icon_size = 25;
  return (
    <View style={styles.menu_bar_container}>
      <TouchableOpacity style={styles.menu_info_button} onPress={onInfo}>
        <Info size={menu_icon_size} />
      </TouchableOpacity>
      <TouchableOpacity style={styles.menu_settings_button} onPress={onSettings}>
        <Gear size={menu_icon_size} />
      </TouchableOpacity>
    </View>
  );
}

export default function App() {
  const [statistics, set_statistics] = useState({
    elements_count: {},
    strikes: {}
  });
  const [info_visible, set_info_visible] = useState(false);
  const [settings_visible, set_settings_visible] = useState(false);

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

  return (
    <View style={styles.app_container}>
      <StatusBar
        animated={true}
        backgroundColor="#aaddff"
        barStyle="dark-content"
        showHideTransition={true}
        hidden={false}
      />
      <MenuBar
        onInfo={() => set_info_visible(true)}
        onSettings={() => set_settings_visible(true)}
      />
      <Game
        width={8}
        height={8}
        score_bonuses={score_bonuses}
        onStrike={update_statistics}
        onRestart={() => set_statistics({ elements_count: {}, strikes: {} })}
      />
      <PopupContainer
        visible={info_visible}
        title="Info"
        onClose={() => set_info_visible(false)}
      >
        <Infos
          score_bonuses={score_bonuses}
          elements_count={statistics.elements_count}
          strikes_statistics={statistics.strikes}
        />
      </PopupContainer>
      <PopupContainer
        visible={settings_visible}
        title="Settings"
        onClose={() => set_settings_visible(false)}
      >
        <Settings />
      </PopupContainer>
    </View>
  );
}

const styles = StyleSheet.create({
  app_container: {
    flex: 1,

    flexDirection: 'column',
    alignItems: "center",
    backgroundColor: "#aaddff"
  },

  menu_bar_container: {
    flexDirection: "row",
    justifyContent: "flex-end",
    alignSelf: "stretch"
  },
  menu_info_button: {

  },
  menu_settings_button: {

  },
});
