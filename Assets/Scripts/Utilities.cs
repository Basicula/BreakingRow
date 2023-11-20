public static class Utilities {
  public static string GetCurrentSceneName() {
    return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
  }

  public static string GetSavePath(string i_prefix) {
    return $"{UnityEngine.Application.persistentDataPath}/{GetCurrentSceneName()}{i_prefix}.json";
  }
}