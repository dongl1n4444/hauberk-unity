using System;
using System.Collections;
using System.Collections.Generic;
using Mathf = UnityEngine.Mathf;
using num = System.Double;

/// Calculates the hero's field of view of the dungeon, which tiles are occluded
/// by other tiles and which are not.
public class Fov
{
  public const int _maxViewDistance = 24;

  public static Vec[][] _octantCoordinates = new Vec[][]{
    // y, x
    new Vec[2]{new Vec(0, -1), new Vec(1, 0)},
    new Vec[2]{new Vec(1, 0), new Vec(0, -1)},
    new Vec[2]{new Vec(1, 0), new Vec(0, 1)},
    new Vec[2]{new Vec(0, 1), new Vec(1, 0)},
    new Vec[2]{new Vec(0, 1), new Vec(-1, 0)},
    new Vec[2]{new Vec(-1, 0), new Vec(0, 1)},
    new Vec[2]{new Vec(-1, 0), new Vec(0, -1)},
    new Vec[2]{new Vec(0, -1), new Vec(-1, 0)},
};

  public Stage _stage;

  List<_Shadow> _shadows = new List<_Shadow>(); // Temporary value.

  public Fov(Stage _stage)
  {
    this._stage = _stage;
  }

  /// Updates the visible flags in [_stage] given the hero's [pos].
  public void refresh(Vec pos)
  {
    if (_stage.game.hero.blindness.isActive)
    {
      _hideAll();
      return;
    }

    // Sweep through the octants.
    for (var octant = 0; octant < 8; octant++)
    {
      _refreshOctant(pos, octant);
    }

    // The starting position is always visible.
    _stage.setVisibility(pos, false, 0);
  }

  void _hideAll()
  {
    foreach (var pos in _stage.bounds)
    {
      _stage.setVisibility(pos, true, 0);
    }

    // The hero knows where they are.
    _stage.setVisibility(_stage.game.hero.pos, false, 0);
  }

  void _refreshOctant(Vec start, int octant)
  {
    // Figure out which direction to increment based on the octant. Octant 0
    // starts at 12 - 2 o'clock, and octants proceed clockwise from there.

    var rowInc = _octantCoordinates[octant][0];
    var colInc = _octantCoordinates[octant][1];

    _shadows = new List<_Shadow> { };

    var bounds = _stage.bounds;
    var fullShadow = false;

    // Sweep through the rows ('rows' may be vertical or horizontal based on
    // the incrementors). Start at row 1 to skip the center position.
    for (var row = 1; ; row++)
    {
      var pos = start + (rowInc * row);

      // If we've traversed out of bounds, bail.
      // Note: this improves performance, but works on the assumption that the
      // starting tile of the FOV is in bounds.
      if (!bounds.contains(pos)) break;

      // If we've reached a tile that is past the maximum view distance, we
      // know the rest of the tiles in the column will be too since they are
      // always farther.
      var pastMaxDistance = false;

      for (var col = 0; col <= row; col++)
      {
        var fallOff = 255;

        if (fullShadow || pastMaxDistance)
        {
          // If we know the entire row is in shadow, we don't need to be more
          // specific.
          _stage.setVisibility(pos, true, fallOff);
        }
        else
        {
          fallOff = 0;
          var distance = (start - pos).length;
          if (distance > _maxViewDistance)
          {
            fallOff = 255;
            pastMaxDistance = true;
          }
          else
          {
            var normalized = distance / _maxViewDistance;
            normalized = normalized * normalized;
            fallOff = (int)(normalized * 255);
          }

          var projection = getProjection(col, row);
          _stage.setVisibility(pos, _isInShadow(projection), fallOff);

          // Add any opaque tiles to the shadow map.
          if (_stage[pos].blocksView)
          {
            fullShadow = _addShadow(projection);
          }
        }

        // Move to the next column.
        pos += colInc;

        // If we've traversed out of bounds, bail on this row. This improves
        // performance, but assumes the starting tile of the FOV is in bounds.
        if (!bounds.contains(pos)) break;
      }
    }
  }

  /// Creates a [_Shadow] that corresponds to the projected silhouette of the
  /// given tile. This is used both to determine visibility (if any of the
  /// projection is visible, the tile is) and to add the tile to the shadow map.
  ///
  /// The maximal projection of a square is always from the two opposing
  /// corners. From the perspective of octant zero, we know the square is
  /// above and to the right of the viewpoint, so it will be the top left and
  /// bottom right corners.
  static _Shadow getProjection(int col, int row)
  {
    // The top edge of row 0 is 2 wide.
    var topLeft = col * 1f / (row + 2);

    // The bottom edge of row 0 is 1 wide.
    var bottomRight = (col + 1) * 1f / (row + 1);

    return new _Shadow(topLeft, bottomRight);
  }

  bool _isInShadow(_Shadow projection)
  {
    // Check the shadow list.
    foreach (var shadow in _shadows)
    {
      if (shadow.contains(projection)) return true;
    }

    return false;
  }

  bool _addShadow(_Shadow shadow)
  {
    var index = 0;
    for (index = 0; index < _shadows.Count; index++)
    {
      // See if we are at the insertion point for this shadow.
      if (_shadows[index].start > shadow.start)
      {
        // Break out and handle inserting below.
        break;
      }
    }

    // The new shadow is going here. See if it overlaps the previous or next.
    var overlapsPrev = (index > 0) && (_shadows[index - 1].end > shadow.start);
    var overlapsNext =
        (index < _shadows.Count) && (_shadows[index].start < shadow.end);

    // Insert and unify with overlapping shadows.
    if (overlapsNext)
    {
      if (overlapsPrev)
      {
        // Overlaps both, so unify one and delete the other.
        _shadows[index - 1].end =
            Math.Max(_shadows[index - 1].end, _shadows[index].end);
        _shadows.RemoveAt(index);
      }
      else
      {
        // Just overlaps the next shadow, so unify it with that.
        _shadows[index].start = Math.Min(_shadows[index].start, shadow.start);
      }
    }
    else
    {
      if (overlapsPrev)
      {
        // Just overlaps the previous shadow, so unify it with that.
        _shadows[index - 1].end = Math.Max(_shadows[index - 1].end, shadow.end);
      }
      else
      {
        // Does not overlap anything, so insert.
        _shadows.Insert(index, shadow);
      }
    }

    // See if we are now shadowing everything.
    return (_shadows.Count == 1) &&
        (_shadows[0].start == 0) &&
        (_shadows[0].end == 1);
  }
}

/// Represents the 1D projection of a 2D shadow onto a normalized line. In
/// other words, a range from 0.0 to 1.0.
class _Shadow
{
  public num start;
  public num end;

  public _Shadow(num start, num end)
  {
    this.start = start;
    this.end = end;
  }

  public override string ToString() => $"({start}-{end})";

  public bool contains(_Shadow projection)
  {
    return (start <= projection.start) && (end >= projection.end);
  }
}