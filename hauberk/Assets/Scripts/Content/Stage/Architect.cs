using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: Consider regions that are randomly placed blobs in the middle too.
public class Region
{
  public string name;

  /// Cover the whole stage.
  public static Region everywhere = new Region("everywhere");
  public static Region n = new Region("n");
  public static Region ne = new Region("ne");
  public static Region e = new Region("e");
  public static Region se = new Region("se");
  public static Region s = new Region("s");
  public static Region sw = new Region("sw");
  public static Region w = new Region("w");
  public static Region nw = new Region("nw");

  public static Region[] directions = new Region[] { n, ne, e, se, s, sw, w, nw };

  Region(string name)
  {
    this.name = name;
  }
}

/// The main class that orchestrates painting and populating the stage.
public class Architect
{
  static Array2D<Architecture> debugOwners;

  public Lore lore;
  public Stage stage;
  public int depth;
  public Array2D<Architecture> _owners;

  public int _carvedTiles = 0;

  public Architect(Lore lore, Stage stage, int depth)
  {
    this.lore = lore;
    this.stage = stage;
    this.depth = depth;
    _owners = new Array2D<Architecture>(stage.width, stage.height, null);
    debugOwners = _owners;
  }

  public IEnumerator buildStage(System.Action<Vec> placeHero)
  {
    // Initialize the stage with an edge of solid and everything else open but
    // fillable.
    foreach (var pos in stage.bounds)
    {
      stage[pos].type = Tiles.unformed;
    }

    var styles = ArchitecturalStyle.pick(depth);

    var lastFillable = -1;
    for (var i = styles.Count - 1; i >= 0; i--)
    {
      if (styles[i].canFill)
      {
        lastFillable = i;
        break;
      }
    }

    // Pick unique regions for each style. The last non-aquatic one always
    // gets "everywhere" to ensure the entire stage is covered.
    var possibleRegions = Region.directions.ToList();
    var regions = new List<Region> { };
    for (var i = 0; i < styles.Count; i++)
    {
      if (i == lastFillable || !styles[i].canFill)
      {
        regions.Add(Region.everywhere);
      }
      else
      {
        regions.Add(Rng.rng.take<Region>(possibleRegions));
      }
    }

    for (var i = 0; i < styles.Count; i++)
    {
      var architect = styles[i].create(this, regions[i]);
      yield return Main.Inst.StartCoroutine(architect.build());
    }

    foreach (var pos in stage.bounds.trace())
    {
      stage[pos].type = Tiles.solid;
    }

    // Fill in the remaining fillable tiles and keep everything connected.
    var unownedPassages = new List<Vec> { };

    yield return Main.Inst.StartCoroutine(_fillPassages(unownedPassages));
    yield return Main.Inst.StartCoroutine(_addShortcuts(unownedPassages));
    yield return Main.Inst.StartCoroutine(_claimPassages(unownedPassages));

    var decorator = new Decorator(this);
    yield return Main.Inst.StartCoroutine(decorator.decorate());
    
    placeHero(decorator.heroPos);
  }

  public Architecture ownerAt(Vec pos) => _owners[pos];

  /// Marks the tile at [x], [y] as open floor for [architecture].
  public void _carve(Architecture architecture, int x, int y, TileType tile)
  {
    Debugger.assert(_owners._get(x, y) == null || _owners._get(x, y) == architecture);
    Debugger.assert(stage.get(x, y).type == Tiles.unformed);

    stage.get(x, y).type = tile ?? Tiles.open;
    _carvedTiles++;

    // Claim all neighboring dry tiles too. This way the architecture can paint
    // the surrounding solid tiles however it wants.
    _owners._set(x, y, architecture);
    foreach (var dir in Direction.all)
    {
      var here = dir.offset(x, y);
      if (_owners.bounds.contains(here) &&
          stage[here].type != Tiles.unformedWet)
      {
        _owners[here] = architecture;
      }
    }
  }

  public bool _canCarve(Architecture architecture, Vec pos)
  {
    if (!stage.bounds.contains(pos)) return false;

    // Can't already be in use.
    if (_owners[pos] != null) return false;

    // Or water.
    if (stage[pos].type == Tiles.unformedWet) return false;

    // Need at least one tile of padding between other dry architectures so that
    // this one can have a ring of solid tiles around itself without impinging
    // on the other architecture. This means that there will be at least two
    // solid tiles between two open tiles of different architectures, one owned
    // by each. That way, if they style their walls differently, one doesn't
    // bleed into the other.
    foreach (var here in pos.neighbors)
    {
      if (!stage.bounds.contains(here)) continue;

      if (stage[here].type == Tiles.unformedWet) continue;

      var owner = _owners[here];
      if (owner != null && owner != architecture) return false;
    }

    return true;
  }

  /// Takes all of the remaining fillable tiles and fills them randomly with
  /// solid tiles or open tiles, making sure to preserve reachability.
  IEnumerator _fillPassages(List<Vec> unownedPassages)
  {
    var openCount = 0;
    var start = Vec.zero;
    var startDistance = 99999;

    var unformed = new List<Vec> { };
    foreach (var pos in stage.bounds.inflate(-1))
    {
      var tile = stage[pos].type;
      if (tile == Tiles.open)
      {
        openCount++;

        // Prefer a starting tile near the center.
        var distance = (pos - stage.bounds.center).rookLength;
        if (distance < startDistance)
        {
          start = pos;
          startDistance = distance;
        }
      }
      else if (!_isFormed(tile))
      {
        unformed.Add(pos);
      }
    }

    Rng.rng.shuffle(unformed);

    var reachability = new Reachability(stage, start);

    var count = 0;
    foreach (var pos in unformed)
    {
      var tile = stage[pos];

      // We may have already processed it.
      if (_isFormed(tile.type)) continue;

      // Try to fill this tile.
      if (tile.type == Tiles.unformed)
      {
        tile.type = Tiles.solid;
      }
      else if (tile.type == Tiles.unformedWet)
      {
        tile.type = Tiles.solidWet;
      }
      else
      {
        Debugger.assert(tile.type == Tiles.solid || tile.type == Tiles.solidWet,
            "Unexpected tile type.");
      }

      // Optimization: If it's already been cut off, we know it can be filled.
      if (!reachability.isReachable(pos)) continue;

      reachability.fill(pos);

      // See if we can still reach all the unfillable tiles.
      if (reachability.reachedOpenCount != openCount)
      {
        // Filling this tile would cause something to be unreachable, so it must
        // be a passage.
        _makePassage(unownedPassages, pos);
        reachability.undoFill();
      }

      // Yielding is slow, so don't do it often.
      if (count++ % 20 == 0) yield return $"{pos}";
    }
  }

  IEnumerator _addShortcuts(List<Vec> unownedPassages)
  {
    var possibleStarts = new List<_Path> { };
    foreach (var pos in stage.bounds.inflate(-1))
    {
      if (!_isOpenAt(pos)) continue;

      foreach (var dir in Direction.cardinal)
      {
        // Needs to be in an open area going into a solid area, like:
        //
        //     .#
        //     >#
        //     .#
        // TODO: Could loosen this somewhat. Should we let shortcuts start from
        // passages? Corners?
        if (!_isOpenAt(pos + dir.rotateLeft90)) continue;
        if (!_isSolidAt(pos + dir.rotateLeft45)) continue;
        if (!_isSolidAt(pos + dir)) continue;
        if (!_isSolidAt(pos + dir.rotateRight45)) continue;
        if (!_isOpenAt(pos + dir.rotateRight90)) continue;

        possibleStarts.Add(new _Path(pos, dir));
      }
    }

    Rng.rng.shuffle(possibleStarts);

    var shortcuts = 0;

    // TODO: Vary this?
    var maxShortcuts = Rng.rng.range(5, 40);

    foreach (var path in possibleStarts)
    {
      if (!_tryShortcut(unownedPassages, path.pos, path.dir)) continue;

      yield return "Shortcut";
      shortcuts++;
      if (shortcuts >= maxShortcuts) break;
    }
  }

  /// Tries to place a shortcut from [start] going towards [heading].
  ///
  /// The [start] position is the open tile next to the wall where the shortcut
  /// will begin.
  ///
  /// Returns `true` if a shortcut was added.
  bool _tryShortcut(List<Vec> unownedPassages, Vec start, Direction heading)
  {
    // A shortcut can start here, so try to walk it until it hits another open
    // area.
    var tiles = new List<Vec> { };
    var pos = start + heading;

    while (true)
    {
      tiles.Add(pos);

      var next = pos + heading;
      if (!stage.bounds.contains(next)) return false;

      if (_isOpenAt(next))
      {
        if (_isShortcut(start, next, tiles.Count))
        {
          foreach (var pos2 in tiles)
          {
            _makePassage(unownedPassages, pos2);
          }
          return true;
        }

        // We found a path, but it's not worth it.
        return false;
      }

      // If the passage runs into an opening on the side, it's weird, so don't
      // put a shortcut.
      if (!_isSolidAt(next + heading.rotateLeft90)) return false;
      if (!_isSolidAt(next + heading.rotateRight90)) return false;

      // Don't make shortcuts that are too long.
      if (Rng.rng.percent(tiles.Count * 10)) return false;

      // TODO: Consider having the path turn randomly.

      // Keep going.
      pos = next;
    }
  }

  /// Returns `true` if a passage with [passageLength] from [from] to [to] is
  /// significantly shorter than the current shortest path between those points.
  ///
  /// Used to avoid placing pointless shortcuts on the stage.
  bool _isShortcut(Vec from, Vec to, int passageLength)
  {
    // If the current path from [from] to [to] is this long or longer, then
    // the shortcut is worth adding.
    var longLength = passageLength * 2 + Rng.rng.range(8, 16);

    var pathfinder = new _LengthPathfinder(stage, from, to, longLength);

    // If there is an existing path that's short enough, this isn't a shortcut.
    return !pathfinder.search();
  }

  void _makePassage(List<Vec> unownedPassages, Vec pos)
  {
    var tile = stage[pos];

    // Filling this tile would cause something to be unreachable, so it must
    // be a passage.
    if (tile.type == Tiles.solid)
    {
      tile.type = Tiles.passage;
    }
    else if (tile.type == Tiles.solidWet)
    {
      tile.type = Tiles.passageWet;
    }
    else
    {
      Debugger.assert(false, "Unexpected tile type.");
    }

    var owner = _owners[pos];
    if (owner == null)
    {
      unownedPassages.Add(pos);
    }
    else
    {
      // The passage is within the edge of an architecture, so extend the
      // boundary around it too.
      _claimNeighbors(pos, owner);
    }
  }

  /// Find owners for all passage tiles that don't currently have one.
  ///
  /// This works by finding the passage tiles that have a neighboring owner and
  /// spreading that owner to this one. It does that repeatedly until all tiles
  /// are claimed.
  IEnumerator _claimPassages(List<Vec> unownedPassages)
  {
    while (true)
    {
      var stillUnowned = new List<Vec> { };
      foreach (var pos in unownedPassages)
      {
        var neighbors = new List<Architecture> { };
        foreach (var neighbor in pos.neighbors)
        {
          var owner = _owners[neighbor];
          if (owner != null) neighbors.Add(owner);
        }

        if (neighbors.isNotEmpty<Architecture>())
        {
          var owner = Rng.rng.item(neighbors);
          _owners[pos] = owner;
          _claimNeighbors(pos, owner);
        }
        else
        {
          stillUnowned.Add(pos);
        }
      }

      if (stillUnowned.isEmpty<Vec>()) break;
      unownedPassages = stillUnowned;

      yield return "Claim";
    }
  }

  /// Claims any neighboring tiles of [pos] for [owner] if they don't already
  /// have an owner.
  public void _claimNeighbors(Vec pos, Architecture owner)
  {
    foreach (var neighbor in pos.neighbors)
    {
      if (_owners[neighbor] == null) _owners[neighbor] = owner;
    }
  }

  public bool _isFormed(TileType type) =>
      type != Tiles.unformed && type != Tiles.unformedWet;

  public bool _isOpenAt(Vec pos)
  {
    var type = stage[pos].type;
    return type == Tiles.open ||
        type == Tiles.passage ||
        type == Tiles.passageWet;
  }

  bool _isSolidAt(Vec pos)
  {
    var type = stage[pos].type;
    return type == Tiles.solid || type == Tiles.solidWet;
  }
}

class _Path
{
  public Vec pos;
  public Direction dir;

  public _Path(Vec pos, Direction dir)
  {
    this.pos = pos;
    this.dir = dir;
  }
}

/// Each architecture is a separate algorithm and some tuning parameters for it
/// that generates part of a stage.
public abstract class Architecture
{
  public Architect _architect;
  public ArchitecturalStyle _style;
  public Region _region;

  public abstract IEnumerator build();

  public int depth => _architect.depth;

  public Rect bounds => _architect.stage.bounds;

  public int width => _architect.stage.width;

  public int height => _architect.stage.height;

  public Region region => _region;

  public virtual PaintStyle paintStyle => PaintStyle.rock;

  /// Gets the ratio of carved tiles to carvable tiles.
  ///
  /// This tells you how much of the stage has been opened up by architectures.
  public double carvedDensity
  {
    get
    {
      var possible = (width - 2) * (height - 2);
      return _architect._carvedTiles * 1f / possible;
    }
  }

  public ArchitecturalStyle style => _style;

  public void bind(ArchitecturalStyle style, Architect architect, Region region)
  {
    _architect = architect;
    _style = style;
    _region = region;
  }

  /// Override this if the architecture wants to handle spawning monsters in its
  /// tiles itself.
  public virtual bool spawnMonsters(Painter painter) => false;

  /// Sets the tile at [x], [y] to [tile] and owned by this architecture.
  ///
  /// If [tile] is omitted, uses [Tiles.open].
  public void carve(int x, int y, TileType tile = null) =>
      _architect._carve(this, x, y, tile);

  /// Whether this architecture can carve the tile at [pos].
  public bool canCarve(Vec pos) => _architect._canCarve(this, pos);

  public void placeWater(Vec pos)
  {
    _architect.stage[pos].type = Tiles.unformedWet;
    _architect._owners[pos] = this;

    // TODO: Should water own the walls that surround it (if not already owned)?
  }

  /// Marks the tile at [pos] as not allowing a passage to be dug through it.
  public void preventPassage(Vec pos)
  {
    Debugger.assert(_architect._owners[pos] == null ||
        _architect._owners[pos] == this ||
        _architect.stage[pos].type == Tiles.unformedWet);

    if (_architect.stage[pos].type == Tiles.unformed)
    {
      _architect.stage[pos].type = Tiles.solid;
    }
  }
}

/// Used to see if there is already a path between two points in the dungeon
/// before adding an extra passage between two open areas.
///
/// Returns `true` if it can find an existing path shorter or as short as the
/// given max length.
class _LengthPathfinder : Pathfinder<bool>
{
  public int _maxLength;

  public _LengthPathfinder(Stage stage, Vec start, Vec end, int _maxLength)
      : base(stage, start, end)
  {
    this._maxLength = _maxLength;
  }

  public override bool processStep(Path path, out bool result)
  {
    if (path.length >= _maxLength)
    {
      result = false;
      return true;
    }
    result = true;
    return false;
  }

  public override bool reachedGoal(Path path) => true;

  public override int? stepCost(Vec pos, Tile tile)
  {
    if (tile.canEnter(Motility.doorAndWalk)) return 1;

    return null;
  }

  public override bool unreachableGoal() => false;
}
