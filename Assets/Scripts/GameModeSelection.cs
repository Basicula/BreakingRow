using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameModeSelection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  private struct GameModeInfo
  {
    public string name;
    public string description;
  }

  [SerializeField] private GameObject m_game_mode_info_prefab;

  private List<GameModeInfo> m_game_mode_infos;
  private int m_current_game_mode_id;

  private Vector2 m_swipe_start;

  void Start()
  {
    m_current_game_mode_id = 0;

    m_game_mode_infos = new List<GameModeInfo>();
    m_game_mode_infos.Add(new GameModeInfo
    {
      name = "Sandbox",
      description = "Configure your own match 3 game with available variety of options"
    });
    m_game_mode_infos.Add(new GameModeInfo
    {
      name = "Classic",
      description = "Simple match 3 game combined elements will be simply removed"
    });
    m_game_mode_infos.Add(new GameModeInfo
    {
      name = "Accumulated",
      description = "Match 3 game but combined elements will be accumulated like in 2048"
    });

    //m_game_mode_infos = new List<GameModeInfo>();
    //for (int i = 0; i < 9; ++i)
    //  m_game_mode_infos.Add(new GameModeInfo
    //  {
    //    name = $"Test{i}",
    //    description = $"Test{i}"
    //  });

    _Init();
  }

  private void _Init()
  {
    foreach (var game_mode_info in m_game_mode_infos)
    {
      var game_mode_info_instance = Instantiate(m_game_mode_info_prefab, transform);
      game_mode_info_instance.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = game_mode_info.name;
      game_mode_info_instance.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = game_mode_info.description;
      var rect_transform = game_mode_info_instance.GetComponent<RectTransform>();
      rect_transform.anchorMin = new Vector2(0.15f, 0.25f);
      rect_transform.anchorMax = new Vector2(0.85f, 0.9f);
    }

    StartCoroutine(_ChangeCurrentMode(0));

    var prev_game_mode_button = transform.GetChild(0).gameObject;
    var next_game_mode_button = transform.GetChild(1).gameObject;
    var play_button = transform.GetChild(2).gameObject;
    prev_game_mode_button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(_ChangeCurrentMode(-1)));
    next_game_mode_button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(_ChangeCurrentMode(1)));
    play_button.GetComponent<Button>().onClick.AddListener(() => { SceneLoader.LoadScene($"{m_game_mode_infos[m_current_game_mode_id].name}Game"); });
  }

  private System.Collections.IEnumerator _ChangeCurrentMode(int i_offset)
  {
    m_current_game_mode_id += i_offset;
    if (m_current_game_mode_id < 0)
      m_current_game_mode_id += m_game_mode_infos.Count;
    m_current_game_mode_id %= m_game_mode_infos.Count;

    var coroutines = new List<Coroutine>(m_game_mode_infos.Count);
    for (int i = 0; i < m_game_mode_infos.Count; ++i)
      coroutines.Add(StartCoroutine(_Move(i, i_offset)));
    for (int i = 0; i < m_game_mode_infos.Count; ++i)
      yield return coroutines[i];
    yield return null;
  }

  private System.Collections.IEnumerator _Move(int i_instance_id, int i_offset)
  {
    var rect_transform = transform.GetChild(3 + i_instance_id).GetComponent<RectTransform>();
    var from_offset = rect_transform.anchoredPosition.x;
    var anchor_id = m_game_mode_infos.Count / 2;
    var to_id = i_instance_id - m_current_game_mode_id;
    if (to_id < -anchor_id)
      to_id += m_game_mode_infos.Count;
    if (to_id > m_game_mode_infos.Count - 1 - anchor_id)
      to_id -= m_game_mode_infos.Count;
    var to_offset = to_id * Screen.width / 1.5f;
    var offset = from_offset;

    var from_scale = rect_transform.localScale.x;
    var to_scale = 1.0f / (1 + Mathf.Abs(to_id) / 2.0f);
    var scale = from_scale;

    var sorting_group = transform.GetChild(3 + i_instance_id).GetComponent<Canvas>();
    var direction = -Mathf.RoundToInt(Mathf.Sign(i_offset));
    sorting_group.sortingOrder = direction * to_id;

    float start_time = Time.time;
    while (offset != to_offset || scale != to_scale)
    {
      var t = 2 * (Time.time - start_time);
      offset = Mathf.Lerp(from_offset, to_offset, t);
      rect_transform.anchoredPosition = new Vector2(offset, 0);
      scale = Mathf.Lerp(from_scale, to_scale, t);
      rect_transform.localScale = new Vector3(scale, scale);
      yield return null;
    }
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    m_swipe_start = eventData.position;
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    var swipe_end = eventData.position;
    var delta = swipe_end - m_swipe_start;
    StartCoroutine(_ChangeCurrentMode(Mathf.RoundToInt(-Mathf.Sign(delta.x))));
  }
}