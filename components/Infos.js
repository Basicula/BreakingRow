import { useState } from 'react';
import { StyleSheet, View, Text, TouchableOpacity } from 'react-native';

export default function Infos({ score_bonuses, elements_count, strikes_statistics, shown }) {
  const [current_tab, set_current_tab] = useState(0);

  const score_bonuses_tab_id = 2;
  const statistics = [elements_count, strikes_statistics, score_bonuses]
  const tab_names = ["Elements count", "Strikes statistics", "Score bonuses"];

  return (shown &&
    <View style={styles.infos_container}>
      <View style={styles.tabs_container}>
        {tab_names.map((tab_name, i) => {
          if (i === current_tab)
            return (
              <Text style={styles.selected_tab_container} key={i} onClick={() => { }}>
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
                <Text style={styles.info_name_container}>{info_name}</Text>
                <Text>:</Text>
                <Text style={styles.info_data_container}>{statistics[current_tab][info_name]}</Text>
              </View>
            );
          })}
        {current_tab === score_bonuses_tab_id &&
          Object.keys(score_bonuses).map((score_bonus, i) => {
            return (
              <View style={styles.info_container} key={i}>
                <Text style={styles.info_condition_container}>x{score_bonus} strike</Text>
                <Text>:</Text>
                <Text style={styles.info_result_container}>x{score_bonuses[score_bonus]} score bonus</Text>
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
    flex: 1,

    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    border: '1px solid black',
    borderRadius: 5,
  },

  info_container: {
    flexDirection: 'row'
  },

  info_condition_container: {
    marginRight: 5
  },

  info_result_container: {
    marginLeft: 5
  },

  tabs_container: {
    flexDirection: 'row',
  },

  tab_container: {
    padding: 5,

    borderRadius: 5,
    backgroundColor: 'black',
  },

  tab_title_wrapper: {
    color: 'white'
  },

  selected_tab_container: {
    padding: 5,

    borderRadius: 5,
    color: 'white',
    backgroundColor: 'grey',
  },

  tab_content_container: {
    width: '100%',
    flexDirection: 'column',
    padding: 5,

    border: '2px solid black',
    borderRadius: 5,
  },

  info_container: {
    flexDirection: 'row',

    justifyContent: 'center',
  },

  info_name_container: {
    marginRight: 5,
  },

  info_data_container: {
    width: '100%',
    marginLeft: 5,
  },

  no_data_container: {
    width: '100%',
    textAlign: 'center',
  }
});

