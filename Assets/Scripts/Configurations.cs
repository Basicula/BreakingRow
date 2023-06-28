using UnityEngine;

public class Configurations : MonoBehaviour
{
  [SerializeReference] private GameField m_game_field;

  private IntegerInput m_width_input;
  private IntegerInput m_height_input;
  private IntegerInput m_active_elements_count_input;

  void Start()
  {
    m_width_input = transform.GetChild(0).gameObject.GetComponent<IntegerInput>();
    m_height_input = transform.GetChild(1).gameObject.GetComponent<IntegerInput>();
    m_active_elements_count_input = transform.GetChild(2).gameObject.GetComponent<IntegerInput>();

    m_width_input.value = m_game_field.width;
    m_width_input.on_value_change = () => _InitGameField();

    m_height_input.value = m_game_field.height;
    m_height_input.on_value_change = () => _InitGameField();

    m_active_elements_count_input.value = m_game_field.active_elements_count;
    m_active_elements_count_input.on_value_change = () => _InitGameField();
  }

  private void _InitGameField()
  {
    m_game_field.Init(m_width_input.value, m_height_input.value, m_active_elements_count_input.value);
  }
}