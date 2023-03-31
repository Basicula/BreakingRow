import { Modal, View, StyleSheet, Text, TouchableOpacity } from "react-native";
import { Svg, Circle, Line } from 'react-native-svg';

function PopupContainer({ children, visible, onClose, title = "" }) {
  const close_button_size = 25;
  const radius = close_button_size / 2;
  const coord = radius * Math.SQRT1_2 * 0.75;

  return (
    <Modal
      animationType="fade"
      transparent={true}
      visible={visible}
      onRequestClose={onClose}
    >
      <View style={styles.modal_container}>
        <View style={styles.content_container}>
          <View style={styles.header_container}>
            <Text style={styles.header_title}>{title}</Text>
            <TouchableOpacity style={styles.close_button} onPress={onClose}>
              <Svg width={close_button_size} height={close_button_size}>
                <Circle
                  r={radius}
                  cx={radius}
                  cy={radius}
                  fill="#ff0000"
                  stroke="#000000"
                />
                <Line
                  x1={radius - coord}
                  y1={radius - coord}
                  x2={radius + coord}
                  y2={radius + coord}
                  stroke="#000000"
                  strokeWidth={2}
                />
                <Line
                  x1={radius - coord}
                  y1={radius + coord}
                  x2={radius + coord}
                  y2={radius - coord}
                  stroke="#000000"
                  strokeWidth={2}
                />
              </Svg>
            </TouchableOpacity>
          </View>
          {children}
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  modal_container: {
    flex: 1,

    justifyContent: "center",
    alignItems: "center",

    backgroundColor: "rgba(0.25,0.25,0.25,0.75)"
  },

  content_container: {
    justifyContent: "center",
    flexWrap: "wrap"
  },

  header_container: {
    flexDirection: "row",
    backgroundColor: "#ffffff",
    justifyContent: "space-between",
    alignItems: "center",

    borderTopLeftRadius: 5,
    borderTopRightRadius: 5,

    paddingLeft: 5,
    paddingRight: 5,
  },

  header_title: {
    flex: 1,
    textAlign: "left",
    fontSize: 24,
    fontWeight: "bold"
  },

  close_button: {
    justifyItems: "center",
    alignItems: "center",
  }
});

export default PopupContainer;