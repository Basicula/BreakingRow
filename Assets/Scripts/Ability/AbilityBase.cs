using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

abstract public class AbilityBase : MonoBehaviour {
  public enum PriceChangeRule {
    Additive,
    Multiplicative
  }

  [Serializable]
  public struct AbilityConfiguration {
    public PriceChangeRule price_change_rule;
    public int starting_price;
    public int price_step;
    public int current_price;
    public int cooldown_time;
    public void NextPrice() {
      switch (price_change_rule) {
        case PriceChangeRule.Additive:
          current_price += price_step;
          break;
        case PriceChangeRule.Multiplicative:
          current_price *= price_step;
          break;
        default:
          throw new NotImplementedException();
      }
    }

    public void Reset() {
      current_price = starting_price;
    }
  }

  [SerializeField] protected AbilityConfiguration m_configuration;
  [SerializeReference] private GameInfo m_game_info;

  protected Button m_button;
  [SerializeReference] private TMP_Text m_price_text;
  [SerializeReference] private Image m_cooldown_overlay;
  [SerializeReference] private TMP_Text m_cooldown_timer;
  private float m_cooldown_start_time;
  private string m_save_file_path;

  public void NextPrice() {
    m_configuration.NextPrice();
    _StartCooldown(Time.time);
    _Update();
  }

  abstract protected void _Init();

  private void Start() {
    m_button = gameObject.GetComponent<Button>();
    _Init();

    m_save_file_path = Utilities.GetSavePath($"{gameObject.name}Ability");

    if (_Load()) {
      if (Time.time - m_cooldown_start_time < m_configuration.cooldown_time)
        _StartCooldown(m_cooldown_start_time);
    } else
      m_cooldown_start_time = Time.time - m_configuration.cooldown_time;
    _Update();
  }

  private void Update() {
    var elapsed_cooldown_time = Time.time - m_cooldown_start_time;
    if (elapsed_cooldown_time < m_configuration.cooldown_time) {
      m_cooldown_timer.text = Mathf.CeilToInt(m_configuration.cooldown_time - elapsed_cooldown_time).ToString();
      m_cooldown_overlay.fillAmount = 1.0f - elapsed_cooldown_time / m_configuration.cooldown_time;
      _Save();
    } else if (m_cooldown_timer.IsActive()) {
      m_cooldown_timer.gameObject.SetActive(false);
      m_cooldown_overlay.gameObject.SetActive(false);
    } else if (m_game_info.score < m_configuration.current_price)
      m_button.interactable = false;
    else
      m_button.interactable = true;
  }

  public void Reset() {
    m_configuration.Reset();
    m_cooldown_start_time = -2 * m_configuration.cooldown_time;
    _Update();
  }

  public int price {
    get => m_configuration.current_price;
  }
  private void _Update() {
    m_price_text.text = m_configuration.current_price.ToString();
    _Save();
  }

  private void _StartCooldown(float i_custom_start) {
    m_button.interactable = false;
    m_cooldown_timer.gameObject.SetActive(true);
    m_cooldown_overlay.gameObject.SetActive(true);
    m_cooldown_timer.text = m_configuration.cooldown_time.ToString();
    m_cooldown_overlay.fillAmount = 1.0f;
    m_cooldown_start_time = i_custom_start;
  }

  struct SerializableData {
    public AbilityConfiguration configuration;
    public int cooldown_elapsed_time;
  }

  private bool _Load() {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return false;
    m_configuration = data.configuration;
    m_cooldown_start_time = Time.time - data.cooldown_elapsed_time;
    return true;
  }

  private void _Save() {
    var data = new SerializableData();
    data.configuration = m_configuration;
    data.cooldown_elapsed_time = Mathf.CeilToInt(Time.time - m_cooldown_start_time);
    SaveLoad.Save(data, m_save_file_path);
  }
}