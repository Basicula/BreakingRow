using UnityEngine;

public class StartGameMode : MonoBehaviour
{
  [SerializeReference] private GameModeSelection m_game_mode_selection;

  public void LoadScene()
  {
    SceneLoader.LoadScene($"{m_game_mode_selection.GetCurrentGameModeName()}Game");
  }
}