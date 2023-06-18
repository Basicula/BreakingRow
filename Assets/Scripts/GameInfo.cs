using System.Collections.Generic;
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
  private readonly Dictionary<int, int> m_score_bonuses = new() { { 3, 1 }, { 4, 2 }, { 5, 5 }, { 6, 5 }, { 7, 10 } };

  void Start()
  {
    m_score = PlayerPrefs.GetInt(m_score_key, 0);
    m_moves_count = 0;
    m_highest_score = PlayerPrefs.GetInt(m_highest_score_key, 0);
  }

  public void UpdateScore(int i_value, int i_count)
  {
    int bonus_multiplier = 1;
    if (i_count > 7)
      bonus_multiplier = 25;
    else if (i_count > 2)
      bonus_multiplier = m_score_bonuses[i_count];
    m_score += Mathf.FloorToInt(Mathf.Pow(2, i_value)) * i_count * bonus_multiplier;
    if (m_score > m_highest_score)
      m_highest_score = m_score;
    this._Update();
  }

  public void SpentScore(int i_spent_score)
  {
    m_score -= i_spent_score;
    this._Update();
  }

  public int moves_count
  {
    set
    {
      m_moves_count = value;
      this._Update();
    }
  }

  public int score
  {
    get => m_score;
  }

  public void Reset()
  {
    m_score = 0;
    m_moves_count = 0;
    this._Update();
  }

  private void _Update()
  {
    PlayerPrefs.SetInt(m_score_key, m_score);
    PlayerPrefs.SetInt(m_highest_score_key, m_highest_score);
    PlayerPrefs.Save();
    score_text.text = m_score.ToString();
    moves_count_text.text = m_moves_count.ToString();
    highest_score_text.text = m_highest_score.ToString();
  }
}
