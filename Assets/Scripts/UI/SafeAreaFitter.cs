using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
  private void Awake()
  {
    var safe_area_rect = Screen.safeArea;
    var anchor_min = safe_area_rect.min;
    var anchor_max = safe_area_rect.max;
    anchor_min.x /= Screen.width;
    anchor_max.x /= Screen.width;
    anchor_min.y /= Screen.height;
    anchor_max.y /= Screen.height;

    var rect = GetComponent<RectTransform>();
    rect.anchorMin = anchor_min;
    rect.anchorMax = anchor_max;
  }
}