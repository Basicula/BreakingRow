using System;
using UnityEngine;

[Serializable]
public class FieldConfiguration
{
  public enum Mode
  {
    Classic,
    Accumulated
  }

  public enum MoveDirection
  {
    TopToBottom,
    RightToLeft,
    BottomToTop,
    LeftToRight
  }

  public enum SpawnMoveScenario
  {
    MoveThenSpawn,
    SpawnThenMove
  }

  public enum CellType
  {
    Emptiness,
    Element
  }

  [SerializeField] public int width;
  [SerializeField] public int height;
  [SerializeField] public int active_elements_count;

  [SerializeField] public Mode mode;
  [SerializeField] public MoveDirection move_direction;
  [SerializeField] public SpawnMoveScenario spawn_move_scenario;

  private CellType[,] m_cells;
}