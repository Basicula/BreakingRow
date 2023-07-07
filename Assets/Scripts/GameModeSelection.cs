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

  private List<GameModeInfo> m_game_mode_infos;
  private int m_current_game_mode_id;

  private TMP_Text m_name_text_element;
  private TMP_Text m_description_text_element;

  private Vector2 m_swipe_start;

  public string GetCurrentGameModeName()
  {
    return m_game_mode_infos[m_current_game_mode_id].name;
  }

  void Start()
  {
    m_current_game_mode_id = 0;

    m_game_mode_infos = new List<GameModeInfo>();
    m_game_mode_infos.Add(new GameModeInfo
    {
      name = "Sandbox",
      description = "Configure your own match 3 game with available variety of options"
    });
    m_game_mode_infos.Add(new GameModeInfo {
      name = "Classic",
      description = "Simple match 3 game combined elements will be simply removed"
    });
    m_game_mode_infos.Add(new GameModeInfo {
      name = "Accumulated",
      description = "Match 3 game but combined elements will be accumulated like in 2048"
    });

    _Init();
  }

  private void _Init()
  {
    m_name_text_element = transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
    m_description_text_element = transform.GetChild(1).gameObject.GetComponent<TMP_Text>();

    StartCoroutine(_ChangeCurrentMode(0));

    var prev_game_mode_button = transform.GetChild(2).gameObject;
    var next_game_mode_button = transform.GetChild(3).gameObject;
    prev_game_mode_button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(_ChangeCurrentMode(-1)));
    next_game_mode_button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(_ChangeCurrentMode(1)));
  }

  private System.Collections.IEnumerator _ChangeCurrentMode(int i_offset)
  {
    m_current_game_mode_id += i_offset;
    if (m_current_game_mode_id < 0)
      m_current_game_mode_id += m_game_mode_infos.Count;
    m_current_game_mode_id %= m_game_mode_infos.Count;

    yield return _TextFade(FadeMode.FadeOut);
    var current_game_mode_info = m_game_mode_infos[m_current_game_mode_id];
    m_name_text_element.text = current_game_mode_info.name;
    m_description_text_element.text = current_game_mode_info.description;
    yield return _TextFade(FadeMode.FadeIn);
  }

  enum FadeMode
  {
    FadeIn,
    FadeOut
  }

  private System.Collections.IEnumerator _TextFade(FadeMode i_fade_mode)
  {
    float min = 0;
    float max = 1;
    if (i_fade_mode == FadeMode.FadeOut)
      (min, max) = (max, min);
    float alpha = min;
    float start_time = Time.time;
    while (alpha != max)
    {
      alpha = Mathf.Lerp(min, max, 4 * (Time.time - start_time));
      m_name_text_element.color = new Color(1.0f, 1.0f, 1.0f, alpha);
      m_description_text_element.color = new Color(1.0f, 1.0f, 1.0f, alpha);
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