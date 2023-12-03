using TMPro;
using UnityEngine;
using UnityEngine.UI;

abstract public class AbilityBase : MonoBehaviour {
  public enum PriceChangeRule {
    Additive,
    Multiplicative
  }

  [SerializeReference] protected PriceChangeRule m_price_change_rule;
  [SerializeReference] protected int m_starting_price;
  [SerializeReference] protected int m_price_step;
  [SerializeReference] protected int m_current_price;
  [SerializeReference] protected int m_cooldown_time;
  [SerializeReference] private GameInfo m_game_info;

  protected Button m_button;
  private TMP_Text m_price_text;
  private Image m_cooldown_overlay;
  private TMP_Text m_cooldown_timer;
  private float m_cooldown_start_time;
  private string m_save_file_path;

  public void NextPrice() {
    switch (m_price_change_rule) {
      case PriceChangeRule.Additive:
        m_current_price += m_price_step;
        break;
      case PriceChangeRule.Multiplicative:
        m_current_price *= m_price_step;
        break;
      default:
        break;
    }
    _StartCooldown(Time.time);
    _Update();
  }

  abstract protected void _Init();

  private void Start() {
    m_button = gameObject.GetComponent<Button>();
    m_price_text = transform.GetChild(0).GetChild(2).gameObject.GetComponent<TMP_Text>();
    m_cooldown_overlay = transform.GetChild(0).GetChild(3).gameObject.GetComponent<Image>();
    m_cooldown_timer = transform.GetChild(0).GetChild(4).gameObject.GetComponent<TMP_Text>();
    _Init();

    m_save_file_path = Utilities.GetSavePath($"{gameObject.name}Ability.json");

    if (_Load()) {
      if (Time.time - m_cooldown_start_time < m_cooldown_time)
        _StartCooldown(m_cooldown_start_time);
    } else
      m_cooldown_start_time = Time.time - m_cooldown_time;
    _Update();
  }

  private void Update() {
    var elapsed_cooldown_time = Time.time - m_cooldown_start_time;
    if (elapsed_cooldown_time < m_cooldown_time) {
      m_cooldown_timer.text = Mathf.CeilToInt(m_cooldown_time - elapsed_cooldown_time).ToString();
      m_cooldown_overlay.fillAmount = 1.0f - elapsed_cooldown_time / m_cooldown_time;
      _Save();
    } else if (m_cooldown_timer.IsActive()) {
      m_cooldown_timer.gameObject.SetActive(false);
      m_cooldown_overlay.gameObject.SetActive(false);
    } else if (m_game_info.score < m_current_price)
      m_button.interactable = false;
    else
      m_button.interactable = true;
  }

  public void Reset() {
    m_current_price = m_starting_price;
    m_cooldown_start_time = -2 * m_cooldown_time;
    _Update();
  }

  public int price {
    get => m_current_price;
  }
  private void _Update() {
    m_price_text.text = m_current_price.ToString();
    _Save();
  }

  private void _StartCooldown(float i_custom_start) {
    m_button.interactable = false;
    m_cooldown_timer.gameObject.SetActive(true);
    m_cooldown_overlay.gameObject.SetActive(true);
    m_cooldown_timer.text = m_cooldown_time.ToString();
    m_cooldown_overlay.fillAmount = 1.0f;
    m_cooldown_start_time = i_custom_start;
  }

  struct SerializableData {
    public PriceChangeRule price_change_rule;
    public int starting_price;
    public int price_step;
    public int current_price;
    public int cooldown_elapsed_time;
  }

  private bool _Load() {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return false;
    m_price_change_rule = data.price_change_rule;
    m_starting_price = data.starting_price;
    m_price_step = data.price_step;
    m_current_price = data.current_price;
    m_cooldown_start_time = Time.time - data.cooldown_elapsed_time;
    return true;
  }

  private void _Save() {
    var data = new SerializableData();
    data.price_change_rule = m_price_change_rule;
    data.starting_price = m_starting_price;
    data.price_step = m_price_step;
    data.current_price = m_current_price;
    data.cooldown_elapsed_time = Mathf.CeilToInt(Time.time - m_cooldown_start_time);
    SaveLoad.Save(data, m_save_file_path);
  }
}