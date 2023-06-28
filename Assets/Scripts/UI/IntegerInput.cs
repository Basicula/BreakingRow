using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntegerInput : MonoBehaviour
{
  [SerializeReference] private int m_min;
  [SerializeReference] private int m_max;
  [SerializeReference] private int m_value;

  public System.Action on_value_change;

  private TMP_InputField m_input_field;

  private void Start()
  {
    var input = gameObject.transform.GetChild(1).gameObject;
    m_input_field = input.GetComponent<TMP_InputField>();
    m_input_field.text = m_value.ToString();
    m_input_field.onValueChanged.AddListener((value_text) => _ChangeValue(int.Parse(value_text)));

    var increment = gameObject.transform.GetChild(2).gameObject;
    var decrement = gameObject.transform.GetChild(3).gameObject;
    increment.GetComponent<Button>().onClick.AddListener(() => _ChangeValue(m_value + 1));
    decrement.GetComponent<Button>().onClick.AddListener(() => _ChangeValue(m_value - 1));
  }

  private void _ChangeValue(int i_new_value)
  {
    if (i_new_value == m_value)
      return;
    if (i_new_value < m_min || i_new_value > m_max)
      return;
    m_value = i_new_value;
    m_input_field.text = m_value.ToString();
    if (!(on_value_change is null))
      on_value_change();
  }

  public int value
  {
    set
    {
      m_value = value;
      m_input_field.text = m_value.ToString();
    }
    get => m_value;
  }
}