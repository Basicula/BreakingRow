public static class Utilities {
  public static string GetCurrentSceneName() {
    return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
  }

  public static string GetSavePath(string i_prefix) {
    return $"{UnityEngine.Application.persistentDataPath}/{GetCurrentSceneName()}{i_prefix}.json";
  }

  public static void InitArray<T>(T[,] array, T default_value) {
    for (int i = 0; i < array.GetLength(0); ++i)
      for (int j = 0; j < array.GetLength(1); ++j)
        array[i, j] = default_value;
  }
}