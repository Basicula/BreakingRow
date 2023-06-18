using UnityEngine;
using UnityEngine.EventSystems;

public class MovableAbility : AbilityBase, IDragHandler, IBeginDragHandler, IEndDragHandler
{
  private Vector2 m_original_image_position;
  private RectTransform m_image_transform;
  private CanvasGroup m_canvas_group;

  public void OnDrag(PointerEventData eventData)
  {
    m_image_transform.anchoredPosition += eventData.delta;
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    if (!m_button.interactable)
    {
      eventData.pointerDrag = null;
      return;
    }
    m_canvas_group.blocksRaycasts = false;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    m_image_transform.anchoredPosition = m_original_image_position;
    m_canvas_group.blocksRaycasts = true;
  }

  protected override void _Init()
  {
    m_image_transform = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
    m_canvas_group = transform.GetChild(0).gameObject.GetComponent<CanvasGroup>();
    m_original_image_position = m_image_transform.anchoredPosition;
  }
}