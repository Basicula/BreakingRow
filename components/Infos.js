import { useState } from 'react';
import { StyleSheet, View, Text, TouchableOpacity } from 'react-native';

export default function Infos({ score_bonuses, elements_count, strikes_statistics }) {
  const [current_tab, set_current_tab] = useState(0);

  const score_bonuses_tab_id = 2;
  const statistics = [elements_count, strikes_statistics, score_bonuses]
  const tab_names = ["Elements count", "Strikes statistics", "Score bonuses"];

  return (
    <View style={styles.infos_container}>
      <View style={styles.tabs_container}>
        {tab_names.map((tab_name, i) => {
          if (i === current_tab)
            return (
              <Text style={[styles.tab_container, styles.selected_tab_container]} key={i} onClick={() => { }}>
                {tab_name}
              </Text>
            );
          return (
            <TouchableOpacity style={styles.tab_container} key={i} onPress={() => set_current_tab(i)}>
              <Text style={styles.tab_title_wrapper}>{tab_name}</Text>
            </TouchableOpacity>
          );
        })}
      </View>
      <View style={styles.tab_content_container}>
        {Object.keys(statistics[current_tab]).length > 0 && current_tab !== score_bonuses_tab_id &&
          Object.keys(statistics[current_tab]).map((info_name, i) => {
            return (
              <View style={styles.info_container} key={info_name}>
                <Text style={styles.info_left_part}>{info_name}</Text>
                <Text>:</Text>
                <Text style={styles.info_right_part}>{statistics[current_tab][info_name]}</Text>
              </View>
            );
          })}
        {current_tab === score_bonuses_tab_id &&
          Object.keys(score_bonuses).map((score_bonus, i) => {
            return (
              <View style={styles.info_container} key={i}>
                <Text style={styles.info_left_part}>x{score_bonus} strike</Text>
                <Text>:</Text>
                <Text style={styles.info_right_part}>x{score_bonuses[score_bonus]} score bonus</Text>
              </View>);
          })}
        {Object.keys(statistics[current_tab]).length === 0 &&
          <Text style={styles.no_data_container}>No data</Text>}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  infos_container: {
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 5,
  },

  info_container: {
    flexDirection: 'row',
    justifyContent: 'center',
  },

  info_left_part: {
    marginRight: 5
  },

  info_right_part: {
    marginLeft: 5
  },

  tabs_container: {
    alignSelf: "stretch",
    flexDirection: 'row',
    backgroundColor: "#dddddd",
    justifyContent: "center"
  },

  tab_container: {
    padding: 5,
    backgroundColor: 'black',
  },

  tab_title_wrapper: {
    color: 'white'
  },

  selected_tab_container: {
    color: 'white',
    backgroundColor: 'grey',
  },

  tab_content_container: {
    alignSelf: "stretch",
    flexDirection: 'column',
    padding: 5,

    borderBottomLeftRadius: 5,
    borderBottomRightRadius: 5,
    backgroundColor: "#dddddd"
  },

  no_data_container: {
    textAlign: 'center',
  }
});

