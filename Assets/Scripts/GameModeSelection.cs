using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameModeSelection : MonoBehaviour
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

  public string GetCurrentGameModeName()
  {
    return m_game_mode_infos[m_current_game_mode_id].name;
  }

  void Start()
  {
    m_current_game_mode_id = 0;

    m_game_mode_infos = new List<GameModeInfo>();
    m_game_mode_infos.Add(new GameModeInfo {
      name = "Classic",
      description = "Simple match 3 game combined elements will be simply removed"
    });
    m_game_mode_infos.Add(new GameModeInfo {
      name = "Accumulated",
      description = "Match 3 game but combined elements will be accumulated like in 2048"
    });

    var game_mode_panel = gameObject.transform.GetChild(2).gameObject;
    m_name_text_element = game_mode_panel.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
    m_description_text_element = game_mode_panel.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();

    _ChangeCurrentMode(0);

    var prev_game_mode_button = gameObject.transform.GetChild(0).gameObject;
    var next_game_mode_button = gameObject.transform.GetChild(1).gameObject;
    prev_game_mode_button.GetComponent<Button>().onClick.AddListener(() => _ChangeCurrentMode(-1));
    next_game_mode_button.GetComponent<Button>().onClick.AddListener(() => _ChangeCurrentMode(1));
  }

  private void _ChangeCurrentMode(int i_offset)
  {
    m_current_game_mode_id += i_offset;
    if (m_current_game_mode_id < 0)
      m_current_game_mode_id += m_game_mode_infos.Count;
    m_current_game_mode_id %= m_game_mode_infos.Count;

    var current_game_mode_info = m_game_mode_infos[m_current_game_mode_id];
    m_name_text_element.text = current_game_mode_info.name;
    m_description_text_element.text = current_game_mode_info.description;
  }
}