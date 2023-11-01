﻿using System;
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

  private int[,] m_cells_configuration;

  public int[,] GetCellsConfiguration()
  {
    if (m_cells_configuration is null)
      InitCellsConfiguration();
    if (m_cells_configuration.GetLength(0) != height || m_cells_configuration.GetLength(1) != width)
      InitCellsConfiguration();
    return m_cells_configuration;
  }

  public void InitCellsConfiguration()
  {
    m_cells_configuration = new int[height, width];
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
        m_cells_configuration[row_id, column_id] = FieldElementsFactory.common_element_class_id;
  }

  public void ElementAt(int i_row, int i_column, int i_element_id)
  {
    m_cells_configuration[i_row, i_column] = i_element_id;
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
    clone.m_cells_configuration = (int[,])m_cells_configuration.Clone();
    return clone;
  }
}