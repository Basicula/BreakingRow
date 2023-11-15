using UnityEngine;

public class StaticAbility : AbilityBase {
  [SerializeReference] private GameField m_game_field;
  override protected void _Init() {
    m_button.onClick.AddListener(() => { m_game_field.HandleStaticAbility(gameObject.name, this); });
  }
}