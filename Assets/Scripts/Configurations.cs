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

  void Start()
  {
    m_width_input = transform.GetChild(0).gameObject.GetComponent<IntegerInput>();
    m_height_input = transform.GetChild(1).gameObject.GetComponent<IntegerInput>();
    m_active_elements_count_input = transform.GetChild(2).gameObject.GetComponent<IntegerInput>();
    m_spawn_move_scenario = transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>();

    m_width_input.value = m_game_field.width;
    m_width_input.on_value_change = () => _InitGameField();

    m_height_input.value = m_game_field.height;
    m_height_input.on_value_change = () => _InitGameField();

    m_active_elements_count_input.value = m_game_field.active_elements_count;
    m_active_elements_count_input.on_value_change = () => _InitGameField();

    var options = System.Enum.GetNames(typeof(GameField.SpawnMoveScenario)).ToList();
    m_spawn_move_scenario.ClearOptions();
    m_spawn_move_scenario.AddOptions(options);
    var current_option_name = System.Enum.GetName(typeof(GameField.SpawnMoveScenario), m_game_field.spawn_move_scenario);
    m_spawn_move_scenario.SetValueWithoutNotify(options.IndexOf(current_option_name));
    m_spawn_move_scenario.onValueChanged.AddListener((option_id) => _InitGameField());
  }

  private void _InitGameField()
  {
    var scenario = (GameField.SpawnMoveScenario)System.Enum.Parse(typeof(GameField.SpawnMoveScenario), m_spawn_move_scenario.options[m_spawn_move_scenario.value].text);
    m_game_field.Init(
      m_width_input.value,
      m_height_input.value,
      m_active_elements_count_input.value,
      scenario
    );
  }
}