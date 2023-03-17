import { useState } from 'react';
import { StyleSheet, View, Text, TouchableOpacity } from 'react-native';

export default function Statistics({ elements_count, strikes_statistics }) {
  const [current_tab, set_current_tab] = useState(0);

  const statistics = [elements_count, strikes_statistics]
  const tab_names = ["Elements count", "Strikes statistics"];

  return (
    <View style={styles.statistics_container}>
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
        {Object.keys(statistics[current_tab]).length > 0 &&
          Object.keys(statistics[current_tab]).map((info_name, i) => {
            return (
              <View style={styles.info_container} key={info_name}>
                <Text style={styles.info_name_container}>{info_name}</Text>
                <Text>:</Text>
                <Text style={styles.info_data_container}>{statistics[current_tab][info_name]}</Text>
              </View>
            );
          })}
        {Object.keys(statistics[current_tab]).length === 0 &&
          <Text style={styles.no_data_container}>No data</Text>}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  statistics_container: {
    minWidth: 100,

    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    border: '1px solid black',
    borderRadius: 5,
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