using UnityEngine;
using TMPro;

public class GameInfo : MonoBehaviour
{
  public TMP_Text score_text;
  public TMP_Text moves_count_text;
  public TMP_Text highest_score_text;

  private int m_score;
  private int m_moves_count;
  private int m_highest_score;

  private const string m_score_key = "Score";
  private const string m_highest_score_key = "HighestScore";

  void Start()
  {
    m_score = PlayerPrefs.GetInt(m_score_key, 0);
    m_moves_count = 0;
    m_highest_score = PlayerPrefs.GetInt(m_highest_score_key, 0);
  }

  public void UpdateScore(int i_value, int i_count)
  {
    m_score += Mathf.FloorToInt(Mathf.Pow(2, i_value)) * i_count;
    PlayerPrefs.SetInt(m_score_key, m_score);
    if (m_score > m_highest_score)
    {
      m_highest_score = m_score;
      PlayerPrefs.SetInt(m_highest_score_key, m_highest_score);
    }
    PlayerPrefs.Save();
    this._UpdateTexts();
  }

  public int moves_count
  {
    set {
      m_moves_count = value;
      this._UpdateTexts();
    }
  }

  public void Reset()
  {
    m_score = 0;
    m_moves_count = 0;
    this._UpdateTexts();
  }

  private void _UpdateTexts()
  {
    score_text.text = m_score.ToString();
    moves_count_text.text = m_moves_count.ToString();
    highest_score_text.text = m_highest_score.ToString();
  }
}
