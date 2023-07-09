using UnityEngine;
using UnityEngine.UI;

public class ToggleSprite : MonoBehaviour
{
  [SerializeField] private Sprite m_sprite_on;
  [SerializeField] private Sprite m_sprite_off;
  [SerializeField] private bool m_is_on;

  private void Start()
  {
    var button = GetComponent<Button>();
    var image = GetComponent<Image>();
    button.onClick.AddListener(() =>
    {
      m_is_on = !m_is_on;
      image.sprite = m_is_on ? m_sprite_on : m_sprite_off;
    });
    image.sprite = m_is_on ? m_sprite_on : m_sprite_off;
  }
}