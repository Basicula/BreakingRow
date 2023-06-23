using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

abstract public class AbilityBase : MonoBehaviour
{
  public enum PriceChangeRule
  {
    Additive,
    Multiplicative
  }

  [SerializeReference] protected PriceChangeRule m_price_change_rule;
  [SerializeReference] protected int m_starting_price;
  [SerializeReference] protected int m_price_step;
  [SerializeReference] protected int m_current_price;
  [SerializeReference] private GameInfo m_game_info;

  protected Button m_button;
  private TMP_Text m_price_text;
  private string m_save_file_path;

  public void NextPrice()
  {
    switch (m_price_change_rule)
    {
      case PriceChangeRule.Additive:
        m_current_price += m_price_step;
        break;
      case PriceChangeRule.Multiplicative:
        m_current_price *= m_price_step;
        break;
      default:
        break;
    }
    this._Update();
  }

  abstract protected void _Init();

  private void Start()
  {
    m_button = gameObject.GetComponent<Button>();
    m_price_text = transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
    this._Init();

    m_save_file_path = Application.persistentDataPath + $"/{SceneManager.GetActiveScene().name}{gameObject.name}Ability.json";

    this._Load();
    this._Update();
  }

  private void Update()
  {
    if (m_game_info.score < m_current_price)
      m_button.interactable = false;
    else
      m_button.interactable = true;
  }

  public void Reset()
  {
    m_current_price = m_starting_price;
    this._Update();
  }

  public int price
  {
    get => m_current_price;
  }
  private void _Update()
  {
    m_price_text.text = m_current_price.ToString();
    this._Save();
  }

  struct SerializableData
  {
    public PriceChangeRule price_change_rule;
    public int starting_price;
    public int price_step;
    public int current_price;
  }

  private void _Load()
  {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return;
    m_price_change_rule = data.price_change_rule;
    m_starting_price = data.starting_price;
    m_price_step = data.price_step;
    m_current_price = data.current_price;
  }

  private void _Save()
  {
    var data = new SerializableData();
    data.price_change_rule = this.m_price_change_rule;
    data.starting_price = this.m_starting_price;
    data.price_step = this.m_price_step;
    data.current_price = this.m_current_price;
    SaveLoad.Save(data, m_save_file_path);
  }
}