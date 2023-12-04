using System.Collections.Generic;

public class FieldChanges {
  public List<(int, List<(int, int)>)> combined;
  public List<(int, int)> destroyed;
  public List<(int, int)> created;
  public List<(int, int)> updated;
  public List<((int, int), List<(int, int)>)> moved;

  public FieldChanges() {
    combined = new();
    destroyed = new();
    created = new();
    updated = new();
    moved = new();
  }
}