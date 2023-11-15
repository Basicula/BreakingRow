using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderIntegerInput : MonoBehaviour {
  [SerializeReference] private int m_min;
  [SerializeReference] private int m_max;
  [SerializeReference] private int m_value;

  public System.Action on_value_change;

  private TMP_Text m_value_text;

  private void Start() {
    m_value_text = gameObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
    var min_label = gameObject.transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
    var max_label = gameObject.transform.GetChild(4).gameObject.GetComponent<TMP_Text>();
    m_value_text.text = m_value.ToString();
    min_label.text = m_min.ToString();
    max_label.text = m_max.ToString();

    var slider = gameObject.transform.GetChild(2).gameObject.GetComponent<Slider>();
    slider.minValue = m_min;
    slider.maxValue = m_max;
    slider.onValueChanged.AddListener((value) => _ChangeValue(Mathf.RoundToInt(value)));
  }

  private void _ChangeValue(int i_new_value) {
    if (i_new_value == m_value)
      return;
    if (i_new_value < m_min || i_new_value > m_max)
      return;
    m_value = i_new_value;
    m_value_text.text = m_value.ToString();
    if (!(on_value_change is null))
      on_value_change();
  }

  public int value {
    set {
      m_value = value;
      m_value_text.text = m_value.ToString();
    }
    get => m_value;
  }
}