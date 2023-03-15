import { StyleSheet, View, Text } from 'react-native';

export default function Infos({ score_bonuses }) {
  return (
    <View style={styles.infos_container}>
      {Object.keys(score_bonuses).map((score_bonus, i) => {
        return (
          <View style={styles.info_container} key={i}>
            <Text style={styles.info_condition_container}>x{score_bonus} strike</Text>
            <Text>:</Text>
            <Text style={styles.info_result_container}>x{score_bonuses[score_bonus]} score bonus</Text>
          </View>);
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  infos_container: {
    flexDirection: 'column'
  },

  info_container: {
    flexDirection: 'row'
  },

  info_condition_container: {
    marginRight: '5px'
  },

  info_result_container: {
    marginLeft: '5px'
  }
});

