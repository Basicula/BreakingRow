import { useEffect, useState } from 'react';
import { StyleSheet, View, Text, Switch, TouchableOpacity } from 'react-native';
import Slider from '@react-native-community/slider';
import AsyncStorage from '@react-native-async-storage/async-storage';

const listeners = new Set();
export function useSettings() {
  const settings_key = "Settings";
  const element_number_shown_key = "element_number";
  const element_style_3d = "elements_view";
  const animation_speed = "animation_speed";

  const default_settings = {
    elements_view: {
      name: "3D Elements",
      type: "bool",
      value: false
    },
    element_number: {
      name: "Element numbers",
      type: "bool",
      value: true
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
    },
    animation_speed: {
      name: "Animation speed",
      type: "interval",
      interval: [25, 1000],
      step: 25,
      value: 250
    }
  };

  const [initialized, set_initialized] = useState(false);
  const [settings, set_settings] = useState(default_settings);

  useEffect(() => {
    if (initialized)
      return;
    AsyncStorage.getItem(settings_key).then(json_data => {
      if (json_data === null) {
        AsyncStorage.setItem(settings_key, JSON.stringify(default_settings))
          .then(() => {
            set_settings(default_settings);
            set_initialized(true);
          });
      }
      else {
        const saved_settings = JSON.parse(json_data);
        const old_settings_keys = Object.keys(saved_settings);
        const new_settings_keys = Object.keys(default_settings);
        for (let new_setting_key of new_settings_keys)
          if (!old_settings_keys.includes(new_setting_key))
            saved_settings[new_setting_key] = default_settings[new_setting_key];
        for (let old_setting_key of old_settings_keys)
          if (!new_settings_keys.includes(old_setting_key))
            delete saved_settings[old_setting_key];
        AsyncStorage.setItem(settings_key, JSON.stringify(saved_settings))
          .then(() => {
            set_settings(saved_settings);
            set_initialized(true);
          });
      }
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

  return { settings, update_setting, element_number_shown_key, element_style_3d, animation_speed };
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
              {settings[setting_key].type === "interval" &&
                <View style={styles.slider_container}>
                  <Text style={styles.slider_value}>{settings[setting_key].value}</Text>
                  <Slider
                    style={styles.slider}
                    minimumValue={settings[setting_key].interval[0]}
                    maximumValue={settings[setting_key].interval[1]}
                    onValueChange={(value) => { update_setting(setting_key, value) }}
                    step={settings[setting_key].step}
                    value={settings[setting_key].value}
                  />
                </View>
              }
            </View>
          );
        })}
      {false && <TouchableOpacity onPress={()=>AsyncStorage.clear()}><Text>Clear all saves</Text></TouchableOpacity>}
    </View>
  );
}

const styles = StyleSheet.create({
  settings_container: {
    flexDirection: "column",
    justifyContent: "center",
    backgroundColor: "#dddddd",
    minWidth: 256,
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

  slider_container: {
    flex: 1,
    flexDirection: "column",
    justifyContent: "center",
    marginLeft: 8
  },

  slider_value: {
    textAlign: "center",
    fontWeight: "bold",
    fontSize: 16,
  }
});