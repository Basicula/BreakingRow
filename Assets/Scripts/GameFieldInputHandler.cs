using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameFieldInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IDropHandler
{
  public Action<Vector2> on_input_down;
  public Action<Vector2> on_input_up;
  public Action<string, Vector2> on_ability_move;
  public Action<string, AbilityBase, Vector2> on_ability_apply;

  public void OnDrop(PointerEventData eventData)
  {
    var active_ability_game_object = eventData.pointerPress;
    var ability = active_ability_game_object.GetComponent<MovableAbility>();
    on_ability_apply(active_ability_game_object.name, ability, eventData.position);
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    on_input_down(eventData.position);
  }

  public void OnPointerMove(PointerEventData eventData)
  {
    if (eventData.dragging && eventData.pointerDrag)
    {
      var active_ability = eventData.pointerDrag;
      on_ability_move(active_ability.name, eventData.position);
    }
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    on_input_up(eventData.position);
  }
}