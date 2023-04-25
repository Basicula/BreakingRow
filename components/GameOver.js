import { StyleSheet, View, TouchableOpacity, Text, Modal } from 'react-native';

export default function GameOver({ earned_score, spent_score, visible, onRestart }) {
  return (
    <Modal
      animationType="slide"
      transparent={true}
      visible={visible}
    >
      <View style={styles.game_over_container}>
        <View style={styles.game_over_view_container}>
          <Text style={styles.game_over_text_title}>Game Over</Text>
          <View style={styles.game_over_score_container}>
            <Text style={styles.game_over_score_text}>Total Earned Score: {earned_score}</Text>
            <Text style={styles.game_over_score_text}>Spent Score: {spent_score}</Text>
          </View>
          <View style={styles.game_over_buttons_container}>
            <TouchableOpacity style={styles.game_over_button} onPress={onRestart}>
              <Text style={styles.game_over_button_text}>Restart</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.game_over_button}>
              <Text style={styles.game_over_button_text}>Free Shuffle</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  game_over_container: {
    flex: 1,
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(0.25,0.25,0.25,0.75)",
  },

  game_over_view_container: {
    alignItems: "center",
    justifyContent: "center",
    padding: 10,
    margin: 10,
    backgroundColor: "#222222",
    borderRadius: 15,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.25,
    shadowRadius: 4,
    elevation: 5,
  },

  game_over_text_title: {
    fontSize: 64,
    color: "#bbbbbb"
  },

  game_over_score_container: {
    flexDirection: "row",
    flexWrap: "wrap",
    alignItems: "center",
    justifyContent: "center"
  },

  game_over_score_text: {
    fontSize: 24,
    marginLeft: 5,
    marginRight: 5,
    color: "#dddddd"
  },

  game_over_buttons_container: {
    flexDirection: "row",
    justifyContent: "center"
  },

  game_over_button: {
    marginLeft: 1,
    marginRight: 1,
    padding: 5,
    borderRadius: 5,
    backgroundColor: "#000000"
  },

  game_over_button_text: {
    textAlign: "center",
    fontWeight: "bold",
    fontSize: 24,
    color: "#ffffff"
  }
});