using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour
{
  [SerializeReference] private TMP_Text score_text;
  [SerializeReference] private TMP_Text moves_count_text;
  [SerializeReference] private TMP_Text best_score_text;

  private int m_score;
  private int m_moves_count;
  private int m_best_score;

  private string m_save_file_path;
  private readonly Dictionary<int, int> m_score_bonuses = new() { { 3, 1 }, { 4, 2 }, { 5, 5 }, { 6, 5 }, { 7, 10 } };

  void Start()
  {
    m_score = 0;
    m_moves_count = 0;
    m_best_score = 0;
    m_save_file_path = Application.persistentDataPath + $"/{SceneManager.GetActiveScene().name}GameInfo.json";
    _Load();
    _Update();
  }

  public void UpdateScore(int i_value, int i_count)
  {
    int bonus_multiplier = 1;
    if (i_count > 7)
      bonus_multiplier = 25;
    else if (i_count > 2)
      bonus_multiplier = m_score_bonuses[i_count];
    m_score += Mathf.FloorToInt(Mathf.Pow(2, i_value)) * i_count * bonus_multiplier;
    if (m_score > m_best_score)
      m_best_score = m_score;
    _Update();
    _Save();
  }

  public void SpentScore(int i_spent_score)
  {
    m_score -= i_spent_score;
    _Update();
    _Save();
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
    _Update();
    _Save();
  }

  private void _Update()
  {
    score_text.text = m_score.ToString();
    moves_count_text.text = m_moves_count.ToString();
    best_score_text.text = m_best_score.ToString();
  }

  private struct SerializableData
  {
    public int score;
    public int best_score;
  }

  private void _Load()
  {
    var data = new SerializableData();
    if (!SaveLoad.Load(ref data, m_save_file_path))
      return;
    m_score = data.score;
    m_best_score = data.best_score;
  }

  private void _Save()
  {
    var data = new SerializableData();
    data.score = m_score;
    data.best_score = m_best_score;
    SaveLoad.Save(data, m_save_file_path);
  }
}
