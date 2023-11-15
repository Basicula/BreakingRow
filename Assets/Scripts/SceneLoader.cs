using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
  public static void LoadScene(string i_name) {
    SceneManager.LoadScene(i_name);
  }
}