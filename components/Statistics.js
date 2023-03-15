import { useState } from 'react';
import { StyleSheet, View, Text } from 'react-native';

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
              <Text style={styles.selected_tab_container} key={i} onClick={() => set_current_tab(i)}>
                {tab_name}
              </Text>
            );
          return (<Text style={styles.tab_container} key={i} onClick={() => set_current_tab(i)}>{tab_name}</Text>);
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
    minWidth: '100px',

    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    border: '1px solid black',
    borderRadius: '5px',
  },

  tabs_container: {
    flexDirection: 'row',
  },

  tab_container: {
    padding: '5px',
    boxSizing: 'border-box',

    borderRadius: '5px',
    color: 'white',
    backgroundColor: 'black',

    cursor: 'pointer',
  },

  selected_tab_container: {
    padding: '5px',
    boxSizing: 'border-box',

    borderRadius: '5px',
    color: 'white',
    backgroundColor: 'grey',

    cursor: 'default',
  },

  tab_content_container: {
    width: '100%',
    flexDirection: 'column',
    padding: '5px',
    boxSizing: 'border-box',

    border: '2px solid black',
    borderRadius: '5px',
  },

  info_container: {
    flexDirection: 'row',

    justifyContent: 'center',
  },

  info_name_container: {
    marginRight: '5px',
  },

  info_data_container: {
    width: '100%',
    marginLeft: '5px',
  },

  no_data_container: {
    width: '100%',
    textAlign: 'center',
  }
});