using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Ability : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
  public enum PriceChangeRule
  {
    Additive,
    Multiplicative
  }

  [SerializeReference] private PriceChangeRule m_price_change_rule;
  [SerializeReference] private int m_starting_price;
  [SerializeReference] private int m_price_step;
  [SerializeReference] private int m_current_price;
  [SerializeReference] private GameInfo m_game_info;

  private TMP_Text m_price_text;
  private Vector2 m_original_image_position;
  private RectTransform m_image_transform;
  private CanvasGroup m_canvas_group;
  private Button m_button;

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

  private void Start()
  {
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

  public void OnDrag(PointerEventData eventData)
  {
    m_image_transform.anchoredPosition += eventData.delta;
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    if (!m_button.interactable)
    {
      eventData.pointerDrag = null;
      return;
    }
    m_canvas_group.blocksRaycasts = false;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    m_image_transform.anchoredPosition = m_original_image_position;
    m_canvas_group.blocksRaycasts = true;
  }

  public int price
  {
    get => m_current_price;
  }

  private void Awake()
  {
    m_button = gameObject.GetComponent<Button>();
    m_image_transform = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
    m_canvas_group = transform.GetChild(0).gameObject.GetComponent<CanvasGroup>();
    m_original_image_position = m_image_transform.anchoredPosition;
    m_price_text = transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
  }

  private void _Update()
  {
    m_price_text.text = m_current_price.ToString();
  }

  private void OnDestroy()
  {
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
    var path = Application.persistentDataPath + $"/{gameObject.name}Ability.json";
    if (!System.IO.File.Exists(path))
      return;
    var json_data = System.IO.File.ReadAllText(path);
    var data = JsonUtility.FromJson<SerializableData>(json_data);
    m_price_change_rule = data.price_change_rule;
    m_starting_price = data.starting_price;
    m_price_step = data.price_step;
    m_current_price = data.current_price;
    return;
  }

  private void _Save()
  {
    var data = new SerializableData();
    data.price_change_rule = this.m_price_change_rule;
    data.starting_price = this.m_starting_price;
    data.price_step = this.m_price_step;
    data.current_price = this.m_current_price;
    var json = JsonUtility.ToJson(data);
    System.IO.File.WriteAllText(Application.persistentDataPath + $"/{gameObject.name}Ability.json", json);
  }
}