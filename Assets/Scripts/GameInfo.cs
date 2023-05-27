using UnityEngine;
using TMPro;

public class GameInfo : MonoBehaviour
{
  public TMP_Text score_text;
  public TMP_Text moves_count_text;
  public TMP_Text highest_score_text;

  private long m_score;
  private long m_moves_count;
  private long m_highest_score;

  void Start()
  {
    m_score = 0;
    m_moves_count = 0;
    m_highest_score = 0;
  }

  public void UpdateScore(long i_value, long i_count)
  {
    m_score += Mathf.FloorToInt(Mathf.Pow(2, i_value)) * i_count;
    if (m_score > m_highest_score)
      m_highest_score = m_score;
    this._UpdateTexts();
  }

  public long moves_count
  {
    set {
      m_moves_count = value;
      this._UpdateTexts();
    }
  }

  private void _UpdateTexts()
  {
    score_text.text = m_score.ToString();
    moves_count_text.text = m_moves_count.ToString();
    highest_score_text.text = m_highest_score.ToString();
  }
}
