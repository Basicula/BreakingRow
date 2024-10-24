﻿using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Configurations : MonoBehaviour {
  [SerializeReference] private GameField m_game_field;
  [SerializeReference] private Button m_field_shape_apply;

  private IntegerInput m_width_input;
  private IntegerInput m_height_input;
  private IntegerInput m_active_elements_count_input;

  [SerializeReference] private TMP_Dropdown m_fill_strategy;
  [SerializeReference] private TMP_Dropdown m_mode;
  [SerializeReference] private TMP_Dropdown m_move_type;
  [SerializeReference] private TMP_Dropdown m_move_direction;
  [SerializeReference] private Button m_edit_field_shape_button;
  [SerializeReference] private EditFieldShape m_edit_field_shape;

  void Start() {
    m_width_input = transform.GetChild(0).gameObject.GetComponent<IntegerInput>();
    m_height_input = transform.GetChild(1).gameObject.GetComponent<IntegerInput>();
    m_active_elements_count_input = transform.GetChild(2).gameObject.GetComponent<IntegerInput>();
    m_edit_field_shape_button.onClick.AddListener(() => _InitEditFieldShape());

    m_width_input.value = m_game_field.field_configuration.width;
    m_width_input.on_value_change = () => _InitGameField();

    m_height_input.value = m_game_field.field_configuration.height;
    m_height_input.on_value_change = () => _InitGameField();

    m_active_elements_count_input.value = m_game_field.field_configuration.active_elements_count;
    m_active_elements_count_input.on_value_change = () => _InitGameField();

    _InitDropdown(ref m_fill_strategy, m_game_field.field_configuration.fill_strategy);
    m_fill_strategy.onValueChanged.AddListener((option_id) => _InitGameField());
    _InitDropdown(ref m_mode, m_game_field.field_configuration.mode);
    m_mode.onValueChanged.AddListener((option_id) => _InitGameField());
    _InitDropdown(ref m_move_direction, m_game_field.field_configuration.move_direction);
    m_move_direction.onValueChanged.AddListener((option_id) => _InitGameField());
    _InitDropdown(ref m_move_type, m_game_field.field_configuration.move_type);
    m_move_type.onValueChanged.AddListener((option_id) => _InitGameField());
  }

  private void _InitDropdown<ValueType>(ref TMP_Dropdown io_dropdown, ValueType i_value) {
    var options = System.Enum.GetNames(typeof(ValueType)).ToList();
    io_dropdown.ClearOptions();
    io_dropdown.AddOptions(options);
    var current_option_name = System.Enum.GetName(typeof(ValueType), i_value);
    io_dropdown.SetValueWithoutNotify(options.IndexOf(current_option_name));
  }

  private void _InitGameField() {
    var new_field_configuration = m_game_field.field_configuration.Clone();
    new_field_configuration.width = m_width_input.value;
    new_field_configuration.height = m_height_input.value;
    if (m_width_input.value != m_game_field.field_configuration.width ||
      m_height_input.value != m_game_field.field_configuration.height)
      new_field_configuration.InitCellsConfiguration();
    new_field_configuration.active_elements_count = m_active_elements_count_input.value;
    var fill_strategy_option = m_fill_strategy.options[m_fill_strategy.value].text;
    new_field_configuration.fill_strategy = System.Enum.Parse<FieldConfiguration.FillStrategy>(fill_strategy_option);
    var mode_option = m_mode.options[m_mode.value].text;
    new_field_configuration.mode = System.Enum.Parse<FieldConfiguration.Mode>(mode_option);
    var move_direction_option = m_move_direction.options[m_move_direction.value].text;
    new_field_configuration.move_direction = System.Enum.Parse<FieldConfiguration.MoveDirection>(move_direction_option);
    var move_type_option = m_move_type.options[m_move_type.value].text;
    new_field_configuration.move_type = System.Enum.Parse<FieldConfiguration.MoveType>(move_type_option);
    m_game_field.Init(new_field_configuration);
  }

  private void _InitEditFieldShape() {
    m_edit_field_shape.Init(m_game_field.field_configuration.Clone());
    m_field_shape_apply.onClick.AddListener(() => {
      var field_configuration = m_game_field.field_configuration.Clone();
      var cells = m_edit_field_shape.GetCells();
      for (int row_id = 0; row_id < field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < field_configuration.width; ++column_id)
          field_configuration.SetElementTypeAt(row_id, column_id, cells[row_id, column_id]);
      m_game_field.Init(field_configuration);
    });
  }
}