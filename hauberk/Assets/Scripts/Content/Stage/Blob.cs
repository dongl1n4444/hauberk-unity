using System;
using System.Collections.Generic;
using System.Linq;

/// Uses an iterated cellular automata to generate organic blob-like shapes.
///
/// Note that this may generate unconnected regions.
class Blob
{
  public static Array2D<bool> make(int size)
  {
    Array2D<bool> blob = null;

    if (size >= 64)
    {
      // Truncate to nearest multiple of 8.
      size = (size / 8) * 8;
      blob = _make(size / 8, 2);
      blob = _make(size / 4, 3, blob);
      blob = _make(size / 2, 5, blob);
      blob = _make(size, 6, blob);
    }
    else if (size >= 32)
    {
      // Truncate to nearest multiple of 4.
      size = (size / 4) * 4;
      blob = _make(size / 4, 2);
      blob = _make(size / 2, 3, blob);
      blob = _make(size, 5, blob);
    }
    else if (size >= 16)
    {
      // Truncate to nearest multiple of 2.
      size = (size / 2) * 2;
      blob = _make(size / 2, 2);
      blob = _make(size, 3, blob);
    }
    else
    {
      blob = _make(size, 3, blob);
    }

    return _crop(blob);
  }

  static Array2D<bool> _make(int size, int smoothing, Array2D<bool> input = null)
  {
    var cells = new Array2D<bool>(size, size, false);
    var dest = new Array2D<bool>(size, size, false);

    if (input != null)
    {
      // Generate noise based on the input blob but scaled up x2. Doing this
      // repeatedly lets us generate larger structure than you tend to get
      // otherwise.

      // Must scale from exactly a half size.
      Debugger.assert(input.width == size / 2);

      foreach (var pos in cells.bounds.inflate(-1))
      {
        var value = input._get(pos.x / 2, pos.y / 2) ? 0.3 : 0.7;
        cells[pos] = Rng.rng.rfloat(1.0) > value;
      }
    }
    else
    {
      // Fill with noise weighted towards the center to generate a single
      // blob in the middle.
      var center = cells.bounds.center;
      var maxLength = (cells.bounds.topLeft - cells.bounds.center).length;
      foreach (var pos in cells.bounds.inflate(-1))
      {
        var distance = (pos - center).length * 1f / maxLength;

        cells[pos] = Rng.rng.rfloat(1.0) > distance;
      }
    }

    for (var i = 0; i < smoothing; i++)
    {
      foreach (var pos in cells.bounds.inflate(-1))
      {
        var walls = 0;
        if (cells[pos]) walls++;
        foreach (var neighbor in pos.neighbors)
        {
          if (cells[neighbor]) walls++;
        }

        dest[pos] = walls >= 5;
      }

      // Swap the buffers.
      var temp = cells;
      cells = dest;
      dest = temp;
    }

    return dest;
  }

  /// Removes edges of the array that are all solid.
  static Array2D<bool> _crop(Array2D<bool> blob)
  {
    var minX = blob.width;
    var maxX = -1;
    var minY = blob.height;
    var maxY = -1;

    foreach (var pos in blob.bounds)
    {
      if (blob[pos])
      {
        minX = Math.Min(minX, pos.x);
        maxX = Math.Max(maxX, pos.x);
        minY = Math.Min(minY, pos.y);
        maxY = Math.Max(maxY, pos.y);
      }
    }

    var result = new Array2D<bool>(maxX - minX + 1, maxY - minY + 1, false);
    foreach (var pos in result.bounds)
    {
      result[pos] = blob[pos.offset(minX, minY)];
    }

    return result;
  }
}
