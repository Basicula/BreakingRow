using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class Configurations : MonoBehaviour
{
  [SerializeReference] private GameField m_game_field;
  [SerializeReference] private TMP_Dropdown m_shape_preset_selector;
  [SerializeReference] private TMP_Dropdown m_field_element_selector;
  [SerializeReference] private Button m_shape_preset_apply;
  [SerializeReference] private Button m_field_shape_apply;

  private IntegerInput m_width_input;
  private IntegerInput m_height_input;
  private IntegerInput m_active_elements_count_input;

  private TMP_Dropdown m_spawn_move_scenario;
  private TMP_Dropdown m_mode;
  private TMP_Dropdown m_move_direction;

  void Start()
  {
    m_width_input = transform.GetChild(0).gameObject.GetComponent<IntegerInput>();
    m_height_input = transform.GetChild(1).gameObject.GetComponent<IntegerInput>();
    m_active_elements_count_input = transform.GetChild(2).gameObject.GetComponent<IntegerInput>();
    m_spawn_move_scenario = transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>();
    m_mode = transform.GetChild(4).gameObject.GetComponent<TMP_Dropdown>();
    m_move_direction = transform.GetChild(5).gameObject.GetComponent<TMP_Dropdown>();
    var edit_field_shape = transform.GetChild(6).gameObject.GetComponent<Button>();
    edit_field_shape.onClick.AddListener(() => _InitEditFieldShape());

    m_width_input.value = m_game_field.field_configuration.width;
    m_width_input.on_value_change = () => _InitGameField();

    m_height_input.value = m_game_field.field_configuration.height;
    m_height_input.on_value_change = () => _InitGameField();

    m_active_elements_count_input.value = m_game_field.field_configuration.active_elements_count;
    m_active_elements_count_input.on_value_change = () => _InitGameField();

    _InitDropdown(ref m_spawn_move_scenario, m_game_field.field_configuration.spawn_move_scenario);
    m_spawn_move_scenario.onValueChanged.AddListener((option_id) => _InitGameField());
    _InitDropdown(ref m_mode, m_game_field.field_configuration.mode);
    m_mode.onValueChanged.AddListener((option_id) => _InitGameField());
    _InitDropdown(ref m_move_direction, m_game_field.field_configuration.move_direction);
    m_move_direction.onValueChanged.AddListener((option_id) => _InitGameField());
  }

  private void _InitDropdown<ValueType>(ref TMP_Dropdown io_dropdown, ValueType i_value)
  {
    var options = System.Enum.GetNames(typeof(ValueType)).ToList();
    io_dropdown.ClearOptions();
    io_dropdown.AddOptions(options);
    var current_option_name = System.Enum.GetName(typeof(ValueType), i_value);
    io_dropdown.SetValueWithoutNotify(options.IndexOf(current_option_name));
  }

  private void _InitGameField()
  {
    var new_field_configuration = m_game_field.field_configuration.Clone();
    new_field_configuration.width = m_width_input.value;
    new_field_configuration.height = m_height_input.value;
    if (m_width_input.value != m_game_field.field_configuration.width ||
      m_height_input.value != m_game_field.field_configuration.height)
      new_field_configuration.InitCellsConfiguration();
    new_field_configuration.active_elements_count = m_active_elements_count_input.value;
    var spawn_move_scenario_option = m_spawn_move_scenario.options[m_spawn_move_scenario.value].text;
    new_field_configuration.spawn_move_scenario = System.Enum.Parse<FieldConfiguration.SpawnMoveScenario>(spawn_move_scenario_option);
    var mode_option = m_mode.options[m_mode.value].text;
    new_field_configuration.mode = System.Enum.Parse<FieldConfiguration.Mode>(mode_option);
    var move_direction_option = m_move_direction.options[m_move_direction.value].text;
    new_field_configuration.move_direction = System.Enum.Parse<FieldConfiguration.MoveDirection>(move_direction_option);
    m_game_field.Init(new_field_configuration);
  }

  private void _InitEditFieldShape()
  {
    var edit_field_shape = transform.GetChild(8).GetChild(0).GetComponent<EditFieldShape>();
    edit_field_shape.Init(m_game_field.field_configuration.Clone());
    m_field_shape_apply.onClick.AddListener(() =>
    {
      var field_configuration = m_game_field.field_configuration.Clone();
      var cells = edit_field_shape.GetCells();
      for (int row_id = 0; row_id < field_configuration.height; ++row_id)
        for (int column_id = 0; column_id < field_configuration.width; ++column_id)
          field_configuration.ElementAt(row_id, column_id, cells[row_id, column_id]);
      m_game_field.Init(field_configuration);
    });

    _InitDropdown(ref m_shape_preset_selector, EditFieldShape.ShapePreset.Circle);
    m_shape_preset_apply.onClick.AddListener(() =>
    {
      var preset = m_shape_preset_selector.options[m_shape_preset_selector.value].text;
      edit_field_shape.ApplyPreset(System.Enum.Parse<EditFieldShape.ShapePreset>(preset));
    });

    _InitDropdown(ref m_field_element_selector, edit_field_shape.element_type);
    m_field_element_selector.onValueChanged.AddListener((option_id) =>
    {
      var field_element = m_field_element_selector.options[option_id].text;
      edit_field_shape.element_type = System.Enum.Parse<EditFieldShape.FieldElementType>(field_element);
    });
  }
}