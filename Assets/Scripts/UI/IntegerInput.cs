using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntegerInput : MonoBehaviour {
  [SerializeReference] private int m_min;
  [SerializeReference] private int m_max;
  [SerializeReference] private int m_value;

  public System.Action on_value_change;

  private TMP_InputField m_input_field;

  private void Start() {
    m_input_field = gameObject.transform.GetChild(1).gameObject.GetComponent<TMP_InputField>();
    m_input_field.text = m_value.ToString();
    m_input_field.onValueChanged.AddListener((value_text) => _ChangeValue(int.Parse(value_text)));

    var increment = gameObject.transform.GetChild(2).gameObject;
    var decrement = gameObject.transform.GetChild(3).gameObject;
    increment.GetComponent<Button>().onClick.AddListener(() => _ChangeValue(m_value + 1));
    decrement.GetComponent<Button>().onClick.AddListener(() => _ChangeValue(m_value - 1));
  }

  private void Update() {
    if (m_value.ToString() != m_input_field.text)
      m_input_field.text = m_value.ToString();
  }

  private void _ChangeValue(int i_new_value) {
    i_new_value = Mathf.Clamp(i_new_value, m_min, m_max);
    m_input_field.text = i_new_value.ToString();
    if (i_new_value == m_value)
      return;
    m_value = i_new_value;
    if (!(on_value_change is null))
      on_value_change();
  }

  public int value {
    set => m_value = value;
    get => m_value;
  }
}