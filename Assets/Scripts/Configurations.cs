using UnityEngine;
using TMPro;
using System.Linq;

public class Configurations : MonoBehaviour
{
  [SerializeReference] private GameField m_game_field;

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

    m_width_input.value = m_game_field.width;
    m_width_input.on_value_change = () => _InitGameField();

    m_height_input.value = m_game_field.height;
    m_height_input.on_value_change = () => _InitGameField();

    m_active_elements_count_input.value = m_game_field.active_elements_count;
    m_active_elements_count_input.on_value_change = () => _InitGameField();

    _InitDropdown(ref m_spawn_move_scenario, m_game_field.spawn_move_scenario);
    _InitDropdown(ref m_mode, m_game_field.mode);
    _InitDropdown(ref m_move_direction, m_game_field.move_direction);
  }

  private void _InitDropdown<ValueType>(ref TMP_Dropdown io_dropdown, ValueType i_value)
  {
    var options = System.Enum.GetNames(typeof(ValueType)).ToList();
    io_dropdown.ClearOptions();
    io_dropdown.AddOptions(options);
    var current_option_name = System.Enum.GetName(typeof(ValueType), i_value);
    io_dropdown.SetValueWithoutNotify(options.IndexOf(current_option_name));
    io_dropdown.onValueChanged.AddListener((option_id) => _InitGameField());
  }

  private void _InitGameField()
  {
    var spawn_move_scenario_option = m_spawn_move_scenario.options[m_spawn_move_scenario.value].text;
    var scenario = (GameField.SpawnMoveScenario)System.Enum.Parse(typeof(GameField.SpawnMoveScenario), spawn_move_scenario_option);
    var mode_option = m_mode.options[m_mode.value].text;
    var mode = (FieldData.Mode)System.Enum.Parse(typeof(FieldData.Mode), mode_option);
    var move_direction_option = m_move_direction.options[m_move_direction.value].text;
    var move_direction = (FieldData.MoveDirection)System.Enum.Parse(typeof(FieldData.MoveDirection), move_direction_option);
    m_game_field.Init(
      m_width_input.value,
      m_height_input.value,
      m_active_elements_count_input.value,
      scenario,
      mode,
      move_direction
    );
  }
}