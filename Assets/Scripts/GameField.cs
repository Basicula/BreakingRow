using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
  public int width;
  public int height;
  public GameObject game_element_prefab;
  public bool is_auto_play;

  private GameElement[,] field;
  private GameFieldData field_data;
  private ElementStyleProvider element_style_provider;
  private float grid_step;
  private float half_grid_step;
  private float element_offset;
  private float element_size;
  private Vector2 field_center;
  private List<(int, int)> to_create;
  private List<(int, int)> selected_elements;
  private Vector2 mouse_down_position;
  private ((int, int), (int, int))? reverse_move;

  public GameField()
  {
    this.to_create = new List<(int, int)>();
    this.selected_elements = new List<(int, int)>();
  }

  void Start()
  {
    Camera.main.orthographicSize = Screen.height / 2;
    Camera.main.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
    float screen_width = Screen.width;
    float screen_height = Screen.height;
    var min_dimmension = Mathf.Min(screen_width, screen_height);
    this.grid_step = Mathf.Min(screen_width / width, screen_height / height);
    this.half_grid_step = this.grid_step / 2;
    this.element_offset = this.grid_step * 0.1f;
    this.element_size = this.grid_step - 2 * this.element_offset;
    this.element_style_provider = new ElementStyleProvider(this.element_size);
    this.field = new GameElement[height, width];
    this.field_data = new GameFieldData(width, height);
    this.field_center = new Vector2(min_dimmension / 2, min_dimmension / 2);
    for (int row_id = 0; row_id < height; ++row_id)
      for (int column_id = 0; column_id < width; ++column_id)
      {
        Vector2 position = this.element_position(row_id, column_id);
        this.field[row_id, column_id] = Instantiate(game_element_prefab, position, Quaternion.identity).GetComponent<GameElement>();
        this.field[row_id, column_id].transform.parent = this.transform;
        this.init_element(row_id, column_id);
      }
    this.fit_collider();
  }

  void Update()
  {
    for (int i = 0; i < height; ++i)
      for (int j = 0; j < width; ++j)
        if (this.field[i, j].state != GameElement.State.Waiting)
          return;
    if (reverse_move.HasValue)
    {
      this.make_move(reverse_move.Value.Item1, reverse_move.Value.Item2);
      reverse_move = null;
    }
    if (this.to_create.Count != 0)
    {
      foreach (var (row_id, column_id) in this.to_create)
        this.init_element(row_id, column_id);
      this.to_create.Clear();
      return;
    }
    var element_move_changes = this.field_data.element_move_changes();
    if (element_move_changes.Count > 0)
    {
      this.field_data.move_elements();
      foreach (var element_move in element_move_changes)
      {
        var first = element_move.Item1;
        var second = element_move.Item2;
        (this.field[first.Item1, first.Item2], this.field[second.Item1, second.Item2]) =
          (this.field[second.Item1, second.Item2], this.field[first.Item1, first.Item2]);
        var target_position = this.field[first.Item1, first.Item2].transform.position;
        this.field[first.Item1, first.Item2].transform.position = this.field[second.Item1, second.Item2].transform.position;
        this.field[second.Item1, second.Item2].move_to(target_position);
      }
      return;
    }
    if (this.field_data.has_empty_cells())
    {
      var created_elements = this.field_data.spawn_new_values();
      foreach (var element_position in created_elements)
        this.init_element(element_position.Item1, element_position.Item2);
      return;
    }
    if (this.field_data.has_groups())
    {
      var groups_details = this.field_data.accumulate_groups();
      foreach (var group_details in groups_details)
        foreach (var element in group_details.group)
        {
          this.field[element.Item1, element.Item2].destroy();
          if (this.field_data.at(element.Item1, element.Item2) != -1)
            to_create.Add(element);
        }
      return;
    }
    this.auto_play();
  }

  private Vector2 get_mouse_event_position()
  {
    var world_mouse_event_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    return new Vector2(world_mouse_event_position.x, world_mouse_event_position.y);
  }

  private void OnMouseDown()
  {
    this.mouse_down_position = this.get_mouse_event_position();
    var position = this.element_position(this.mouse_down_position);
    if (position.Item1 < 0 || position.Item2 < 0 || position.Item1 >= this.height || position.Item2 >= this.width)
      return;
    this.selected_elements.Add(position);
    this.process_selected_elements();
  }

  private void OnMouseUp()
  {
    if (this.selected_elements.Count == 2)
      return;
    var mouse_up_position = this.get_mouse_event_position();
    var delta = mouse_up_position - mouse_down_position;
    if (delta.magnitude / this.grid_step < 0.5)
      return;
    delta.Normalize();
    var position = this.element_position(delta * this.grid_step + mouse_down_position);
    this.selected_elements.Add(position);
    this.process_selected_elements();
  }

  private void process_selected_elements()
  {
    string debug = "Before ";
    foreach (var selected in this.selected_elements)
      debug += $"({selected.Item1}, {selected.Item2}) ";
    Debug.Log(debug);
    if (this.selected_elements.Count != 2)
      return;

    var first = this.selected_elements[0];
    var second = this.selected_elements[1];
    var distance = Mathf.Abs(first.Item1 - second.Item1) + Mathf.Abs(first.Item2 - second.Item2);
    if (distance == 0)
      this.selected_elements.Clear();
    else if (distance == 1)
    {
      this.make_move(first, second);
      this.selected_elements.Clear();
    }
    else
      this.selected_elements.RemoveAt(0);

    debug = "After ";
    foreach (var selected in this.selected_elements)
      debug += $"({selected.Item1}, {selected.Item2}) ";
    Debug.Log(debug);
  }

  private void fit_collider()
  {
    var collider = GetComponent<BoxCollider2D>();
    collider.size = new Vector2(this.grid_step * this.width, this.grid_step * this.height);
  }

  private void auto_play()
  {
    if (!this.is_auto_play)
      return;
    var moves = this.field_data.get_all_moves();
    if (moves.Count > 0)
    {
      moves.Sort((GameFieldData.MoveDetails first, GameFieldData.MoveDetails second) => second.strike - first.strike);
      var max_strike_value = moves[0].strike;
      int max_strike_value_count = 0;
      foreach (var move_details in moves)
      {
        if (move_details.strike < max_strike_value)
          break;
        ++max_strike_value_count;
      }
      var move_index = Random.Range(0, max_strike_value_count);
      var move = moves[move_index];
      this.make_move(move.first, move.second);
    }
  }

  private void make_move((int, int) first, (int, int) second)
  {
    this.field_data.swap_cells(first.Item1, first.Item2, second.Item1, second.Item2);
    (this.field[first.Item1, first.Item2], this.field[second.Item1, second.Item2]) =
      (this.field[second.Item1, second.Item2], this.field[first.Item1, first.Item2]);
    this.field[first.Item1, first.Item2].move_to(this.field[second.Item1, second.Item2].transform.position);
    this.field[second.Item1, second.Item2].move_to(this.field[first.Item1, first.Item2].transform.position);
    if (!this.field_data.has_groups() && !reverse_move.HasValue)
      reverse_move = (first, second);
  }

  private Vector2 element_position(int row_id, int column_id)
  {
    return new Vector2(
      this.grid_step * column_id - this.field_center.x + this.half_grid_step,
      this.field_center.y - this.half_grid_step - this.grid_step * row_id
    );
  }

  private (int, int) element_position(Vector2 position)
  {
    var row_id = Mathf.FloorToInt((this.field_center.y - position.y) / this.grid_step);
    var column_id = Mathf.FloorToInt((this.field_center.x + position.x) / this.grid_step);
    return (row_id, column_id);
  }

  private void init_element(int row_id, int column_id)
  {
    int value = this.field_data.at(row_id, column_id);
    this.field[row_id, column_id].create(element_style_provider.get(value));
  }
}
