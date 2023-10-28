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

  [SerializeField] public int width;
  [SerializeField] public int height;
  [SerializeField] public int active_elements_count;

  [SerializeField] public Mode mode;
  [SerializeField] public MoveDirection move_direction;
  [SerializeField] public SpawnMoveScenario spawn_move_scenario;

  private bool[,] m_cells;

  public bool[,] GetCells()
  {
    if (m_cells is null)
      InitCells();
    if (m_cells.GetLength(0) != height || m_cells.GetLength(1) != width)
      InitCells();
    return m_cells;
  }

  public void InitCells()
  {
    m_cells = new bool[height, width];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        m_cells[row_id, column_id] = true;
  }

  public void ElementAt(int i_row, int i_column, bool i_exists)
  {
    m_cells[i_row, i_column] = i_exists;
  }

  public FieldConfiguration Clone()
  {
    var clone = new FieldConfiguration();
    clone.width = width;
    clone.height = height;
    clone.active_elements_count = active_elements_count;
    clone.mode = mode;
    clone.spawn_move_scenario = spawn_move_scenario;
    clone.move_direction = move_direction;
    clone.m_cells = (bool[,])m_cells.Clone();
    return clone;
  }
}