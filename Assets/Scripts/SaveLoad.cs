public static class SaveLoad {
  public static bool Load<Data>(ref Data o_data, string i_path) {
    if (!System.IO.File.Exists(i_path))
      return false;
    var json_data = System.IO.File.ReadAllText(i_path);
    o_data = UnityEngine.JsonUtility.FromJson<Data>(json_data);
    return true;
  }

  public static void Save<Data>(Data i_data, string i_path) {
    var json = UnityEngine.JsonUtility.ToJson(i_data, true);
    System.IO.File.WriteAllText(i_path, json);
  }
}