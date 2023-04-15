import { useEffect, useState } from 'react';
import { StyleSheet, View, Text, Switch } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

const listeners = new Set();
export function useSettings() {
  const settings_key = "Settings";
  const element_number_shown_key = "element_number";
  const element_style_3d = "elements_view";

  const default_settings = {
    elements_view: {
      name: "3D Elements",
      type: "bool",
      value: false
    },
    element_number: {
      name: "Element numbers",
      type: "bool",
      values: true
    },
    sound: {
      name: "Sound",
      type: "bool",
      value: false
    },
    music: {
      name: "Music",
      type: "bool",
      value: false
    }
  };

  const [initialized, set_initialized] = useState(false);
  const [settings, set_settings] = useState(default_settings);

  useEffect(() => {
    if (initialized)
      return;
    AsyncStorage.getItem(settings_key).then(json_data => {
      if (json_data === null) {
        AsyncStorage.setItem(settings_key, JSON.stringify(default_settings));
        set_settings(default_settings);
      }
      else
        set_settings(JSON.parse(json_data));
      set_initialized(true);
    });
    const listener = () => {
      AsyncStorage.getItem(settings_key).then(json_data => {
        set_settings(JSON.parse(json_data))
      });
    };
    listeners.add(listener);
    return () => listeners.delete(listener);
  }, []);

  const on_change = () => {
    for (let listener of listeners)
      listener();
  };

  const update_setting = (setting_key, new_value) => {
    var new_settings = Object.assign({}, settings);
    new_settings[setting_key].value = new_value;
    AsyncStorage.setItem(settings_key, JSON.stringify(new_settings))
      .then(_ => set_settings(new_settings));
    on_change();
  };

  return { settings, update_setting, element_number_shown_key, element_style_3d };
}

export default function Settings() {
  const { settings, update_setting } = useSettings();

  return (
    <View style={styles.settings_container}>
      {settings &&
        Object.keys(settings).map((setting_key, i) => {
          return (
            <View style={styles.setting_container} key={i}>
              <Text style={styles.setting_name}>{settings[setting_key].name}</Text>
              {settings[setting_key].type === "bool" &&
                <Switch
                  trackColor={{ false: '#767577', true: '#81b0ff' }}
                  thumbColor={settings[setting_key].value ? '#f5dd4b' : '#f4f3f4'}
                  onValueChange={() => { update_setting(setting_key, !settings[setting_key].value) }}
                  value={settings[setting_key].value}
                />
              }
            </View>
          );
        })}
    </View>
  );
}

const styles = StyleSheet.create({
  settings_container: {
    flexDirection: "column",
    justifyContent: "center",
    backgroundColor: "#dddddd"
  },

  setting_container: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 2
  },

  setting_name: {
    fontWeight: "bold",
    fontSize: 16
  },
});