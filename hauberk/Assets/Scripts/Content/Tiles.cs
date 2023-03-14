using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malison;

/// Static class containing all of the [TileType]s.
class Tiles {

  // Note: Not using lambdas for these because that prevents [Tiles.openDoor] and
  // [Tiles.closedDoor] from having their types inferred.
  public static Action _closeDoor(Vec pos) => new CloseDoorAction(pos, Tiles.closedDoor);

  public static Action _openDoor(Vec pos) => new OpenDoorAction(pos, Tiles.openDoor);

  public static Action _closeSquareDoor(Vec pos) =>
      new CloseDoorAction(pos, Tiles.closedSquareDoor);

  public static Action _openSquareDoor(Vec pos) => new OpenDoorAction(pos, Tiles.openSquareDoor);

  public static Action _closeBarredDoor(Vec pos) =>
      new CloseDoorAction(pos, Tiles.closedBarredDoor);

  public static Action _openBarredDoor(Vec pos) => new OpenDoorAction(pos, Tiles.openBarredDoor);

  // Temporary tile types used during stage generation.

  /// An unformed tile that can be turned into aquatic, passage, or solid.
  public static TileType unformed = tile("unformed", "?", Hues.coolGray).open();

  /// An unformed tile that can be turned into water of some kind when "filled"
  /// or a bridge when used as a passage.
  public static TileType unformedWet = tile("unformed wet", "≈", Hues.coolGray).open();

  /// An open floor tile generated by an architecture.
  public static TileType open = tile("open", "·", Hues.lightCoolGray).open();

  /// A solid tile that has been filled in the passage generator.
  public static TileType solid = tile("solid", "#", Hues.lightCoolGray).solid();

  /// An open tile that the passage generator knows must remain open.
  public static TileType passage = tile("passage", "-", Hues.lightCoolGray).open();

  /// The end of a passage.
  public static TileType doorway = tile("doorway", "○", Hues.lightCoolGray).open();

  /// An untraversable wet tile that has been filled in the passage generator.
  public static TileType solidWet = tile("solid wet", "≈", Hues.lightBlue).solid();

  /// A traversable wet tile that the passage generator knows must remain open.
  public static TileType passageWet = tile("wet passage", "-", Hues.lightBlue).open();

  // Real tiles.

  // Walls.
  public static TileType flagstoneWall =
      tile("flagstone wall", "▒", Hues.lightWarmGray, Hues.warmGray).solid();

  public static TileType graniteWall =
      tile("granite wall", "▒", Hues.coolGray, Hues.darkCoolGray).solid();

  public static TileType granite1 = tile("granite", "▓", Hues.coolGray, Hues.darkCoolGray)
      .blend(0.0, Hues.darkCoolGray, Hues.darkerCoolGray)
      .solid();
  public static TileType granite2 = tile("granite", "▓", Hues.coolGray, Hues.darkCoolGray)
      .blend(0.2, Hues.darkCoolGray, Hues.darkerCoolGray)
      .solid();
  public static TileType granite3 = tile("granite", "▓", Hues.coolGray, Hues.darkCoolGray)
      .blend(0.4, Hues.darkCoolGray, Hues.darkerCoolGray)
      .solid();

  // Floors.
  public static TileType flagstoneFloor = tile("flagstone floor", "·", Hues.warmGray).open();

  public static TileType graniteFloor = tile("granite floor", "·", Hues.coolGray).open();

  // Doors.
  public static TileType openDoor =
      tile("open door", "○", Hues.tan, Hues.darkBrown).onClose(_closeDoor).open();
  public static TileType closedDoor =
      tile("closed door", "◙", Hues.tan, Hues.darkBrown).onOpen(_openDoor).door();

  public static TileType openSquareDoor = tile("open square door", "♂", Hues.tan, Hues.darkBrown)
      .onClose(_closeSquareDoor)
      .open();

  public static TileType closedSquareDoor =
      tile("closed square door", "♀", Hues.tan, Hues.darkBrown)
          .onOpen(_openSquareDoor)
          .door();

  public static TileType openBarredDoor =
      tile("open barred door", "♂", Hues.lightWarmGray, Hues.coolGray)
          .onClose(_closeBarredDoor)
          .open();

  // TODO: Should be able to see through but not fly through.
  public static TileType closedBarredDoor =
      tile("closed barred door", "♪", Hues.lightWarmGray, Hues.coolGray)
          .onOpen(_openBarredDoor)
          .transparentDoor();

  // Unsorted.

  // TODO: Organize these.
  public static TileType burntFloor = tile("burnt floor", "φ", Hues.darkCoolGray).open();
  public static TileType burntFloor2 = tile("burnt floor", "ε", Hues.darkCoolGray).open();
  public static TileType lowWall = tile("low wall", "%", Hues.lightWarmGray).obstacle();

  // TODO: Different character that doesn't look like bridge?
  public static TileType stairs =
      tile("stairs", "≡", Hues.lightWarmGray, Hues.coolGray).to(TilePortals.exit).open();
  public static TileType bridge = tile("bridge", "≡", Hues.tan, Hues.darkBrown).open();

  // TODO: Stop glowing when stepped on?
  public static TileType glowingMoss = Tiles.tile("moss", "░", Hues.aqua).emanate(128).open();

  public static TileType water = tile("water", "≈", Hues.blue, Hues.darkBlue)
      .animate(10, 0.5, Hues.darkBlue, Hues.darkerCoolGray)
      .water();
  public static TileType steppingStone =
      tile("stepping stone", "•", Hues.lightCoolGray, Hues.darkBlue).open();

  public static TileType dirt = tile("dirt", "·", Hues.brown).open();
  public static TileType dirt2 = tile("dirt2", "φ", Hues.brown).open();
  public static TileType grass = tile("grass", "░", Hues.peaGreen).open();
  public static TileType tallGrass = tile("tall grass", "√", Hues.peaGreen).open();
  public static TileType tree = tile("tree", "▲", Hues.peaGreen, Hues.sherwood).solid();
  public static TileType treeAlt1 = tile("tree", "♠", Hues.peaGreen, Hues.sherwood).solid();
  public static TileType treeAlt2 = tile("tree", "♣", Hues.peaGreen, Hues.sherwood).solid();

  // Decor.

  public static TileType openChest = tile("open chest", "⌠", Hues.tan).obstacle();
  public static TileType closedChest = tile("closed chest", "⌡", Hues.tan)
      .onOpen((pos) => new OpenChestAction(pos))
      .obstacle();
  public static TileType closedBarrel = tile("closed barrel", "°", Hues.tan)
      .onOpen((pos) => new OpenBarrelAction(pos))
      .obstacle();
  public static TileType openBarrel = tile("open barrel", "∙", Hues.tan).obstacle();

  public static TileType tableTopLeft = tile("table", "┌", Hues.tan).obstacle();
  public static TileType tableTop = tile("table", "─", Hues.tan).obstacle();
  public static TileType tableTopRight = tile("table", "┐", Hues.tan).obstacle();
  public static TileType tableSide = tile("table", "│", Hues.tan).obstacle();
  public static TileType tableCenter = tile("table", " ", Hues.tan).obstacle();
  public static TileType tableBottomLeft = tile("table", "╘", Hues.tan).obstacle();
  public static TileType tableBottom = tile("table", "═", Hues.tan).obstacle();
  public static TileType tableBottomRight = tile("table", "╛", Hues.tan).obstacle();

  public static TileType tableLegLeft = tile("table", "╞", Hues.tan).obstacle();
  public static TileType tableLeg = tile("table", "╤", Hues.tan).obstacle();
  public static TileType tableLegRight = tile("table", "╡", Hues.tan).obstacle();

  // TODO: Animate.
  public static TileType candle = tile("candle", "≥", Hues.sandal).emanate(128).obstacle();

  // TODO: Animate.
  public static TileType wallTorch =
      tile("wall torch", "≤", Hues.gold, Hues.coolGray).emanate(192).solid();

  // TODO: Different glyph.
  // TODO: Animate.
  public static List<TileType> braziers = multi("brazier", "≤", Hues.tan, null, 5,
      (tile, n) => tile.emanate(192 - n * 12).obstacle());

  public static TileType statue = tile("statue", "P", Hues.ash, Hues.coolGray).obstacle();

  // Make these "monsters" that can be pushed around.
  public static TileType chair = tile("chair", "π", Hues.tan).open();

  // Stains.

  // TODO: Not used right now.
  public static TileType brownJellyStain = tile("brown jelly stain", "·", Hues.tan).open();

  public static TileType grayJellyStain =
      tile("gray jelly stain", "·", Hues.darkCoolGray).open();

  public static TileType greenJellyStain = tile("green jelly stain", "·", Hues.lima).open();

  public static TileType redJellyStain = tile("red jelly stain", "·", Hues.red).open();

  public static TileType violetJellyStain =
      tile("violet jelly stain", "·", Hues.purple).open();

  public static TileType whiteJellyStain = tile("white jelly stain", "·", Hues.ash).open();

  // TODO: Make this do stuff when walked through.
  public static TileType spiderweb = tile("spiderweb", "÷", Hues.coolGray).open();

  // Town tiles.

  public static TileType dungeonEntrance =
      tile("dungeon entrance", "≡", Hues.lightWarmGray, Hues.coolGray)
          .to(TilePortals.dungeon)
          .open();

  public static TileType home =
      tile("home entrance", "○", Hues.sandal).to(TilePortals.home).open();

  public static TileType shop1 =
      tile("shop entrance", "○", Hues.carrot).to(TilePortals.shop1).open();

  public static TileType shop2 =
      tile("shop entrance", "○", Hues.gold).to(TilePortals.shop2).open();

  public static TileType shop3 =
      tile("shop entrance", "○", Hues.lima).to(TilePortals.shop3).open();

  public static TileType shop4 =
      tile("shop entrance", "○", Hues.peaGreen).to(TilePortals.shop4).open();

  public static TileType shop5 =
      tile("shop entrance", "○", Hues.aqua).to(TilePortals.shop5).open();

  public static TileType shop6 =
      tile("shop entrance", "○", Hues.lightAqua).to(TilePortals.shop6).open();

  public static TileType shop7 =
      tile("shop entrance", "○", Hues.blue).to(TilePortals.shop7).open();

  public static TileType shop8 =
      tile("shop entrance", "○", Hues.purple).to(TilePortals.shop8).open();

  public static TileType shop9 =
      tile("shop entrance", "○", Hues.red).to(TilePortals.shop9).open();

  public static _TileBuilder tile(string name, object ch, Malison.Color fore,
          Malison.Color back = null) =>
      new _TileBuilder(name, ch, fore, back);

  public static List<TileType> multi(string name, object ch, Malison.Color fore, Malison.Color back,
      int count, System.Func<_TileBuilder, int, TileType> generate) {
    var result = new List<TileType>();
    for (var i = 0; i < count; i++) {
      var builder = tile(name, ch, fore, back);
      result.Add(generate(builder, i));
    }

    return result;
  }

  /// The amount of heat required for [tile] to catch fire or 0 if the tile
  /// cannot be ignited.
  public static int ignition(TileType tile) => _ignition.ContainsKey(tile) ? _ignition[tile] : 0;

  public static Dictionary<TileType, int> _ignition = new Dictionary<TileType, int>(){
    {openDoor, 30},
    {closedDoor, 30},
    {bridge, 50},
    {glowingMoss, 10},
    {grass, 3},
    {tallGrass, 3},
    {tree, 40},
    {treeAlt1, 40},
    {treeAlt2, 40},
    {tableTopLeft, 20},
    {tableTop, 20},
    {tableTopRight, 20},
    {tableSide, 20},
    {tableCenter, 20},
    {tableBottomLeft, 20},
    {tableBottom, 20},
    {tableBottomRight, 20},
    {tableLegLeft, 20},
    {tableLeg, 20},
    {tableLegRight, 20},
    {openChest, 40},
    {closedChest, 80},
    {openBarrel, 15},
    {closedBarrel, 40},
    {candle, 1},
    {chair, 10},
    {spiderweb, 1},
  };

  /// How long [tile] burns before going out.
  public static int fuel(TileType tile) => _fuel.ContainsKey(tile) ? _fuel[tile] : 0;

  public static Dictionary<TileType, int> _fuel = new Dictionary<TileType, int>() {
    {openDoor, 70},
    {closedDoor, 70},
    {bridge, 50},
    {glowingMoss, 20},
    {grass, 30},
    {tallGrass, 50},
    {tree, 100},
    {treeAlt1, 100},
    {treeAlt2, 100},
    {tableTopLeft, 60},
    {tableTop, 60},
    {tableTopRight, 60},
    {tableSide, 60},
    {tableCenter, 60},
    {tableBottomLeft, 60},
    {tableBottom, 60},
    {tableBottomRight, 60},
    {tableLegLeft, 60},
    {tableLeg, 60},
    {tableLegRight, 60},
    {openChest, 70},
    {closedChest, 80},
    {openBarrel, 30},
    {closedBarrel, 40},
    {candle, 60},
    {chair, 40},
    {spiderweb, 20}
  };

  /// What types [tile] can turn into when it finishes burning.
  public static List<TileType> burnResult(TileType tile) {
    if (_burnTypes.ContainsKey(tile)) return _burnTypes[tile]!;

    return new List<TileType>(){burntFloor, burntFloor2};
  }

  public static Dictionary<TileType, List<TileType>> _burnTypes = new Dictionary<TileType, List<TileType>>(){
    {bridge, new List<TileType>(){water}},
    {grass, new List<TileType>(){dirt, dirt2}},
    {tallGrass, new List<TileType>(){dirt, dirt2}},
    {tree, new List<TileType>(){dirt, dirt2}},
    {treeAlt1, new List<TileType>(){dirt, dirt2}},
    {treeAlt2, new List<TileType>(){dirt, dirt2}},
    {candle, new List<TileType>(){tableCenter}},
    // TODO: This doesn't handle spiderwebs on other floors.
    {spiderweb, new List<TileType>(){flagstoneFloor}}
  };
}

class _TileBuilder {
  public string name;
  public List<Glyph> glyphs;

  System.Func<Vec, Action>? _onClose;
  System.Func<Vec, Action>? _onOpen;
  TilePortal? _portal;
  int _emanation = 0;

  public _TileBuilder(string name, object ch, Malison.Color fore, Malison.Color? back) 
  {
    back ??= Hues.darkerCoolGray;
    var charCode = ch is int ? ch : (ch as string)[0];

    this.name = name;
    glyphs = new List<Glyph>(){Glyph.fromDynamic(charCode.ToString(), fore, back)};
  }

  public _TileBuilder(string name, Glyph glyph)
  {
    this.name = name;
    glyphs = new List<Glyph>(){glyph};
  }

  public _TileBuilder blend(double amount, Malison.Color fore, Malison.Color back) {
    for (var i = 0; i < glyphs.Count; i++) {
      var glyph = glyphs[i];
      glyphs[i] = new Glyph(glyph._char, glyph.fore.blend(fore, amount),
          glyph.back.blend(back, amount));
    }

    return this;
  }

  public _TileBuilder animate(int count, double maxMix, Malison.Color fore, Malison.Color back) {
    var glyph = glyphs[0];
    for (var i = 1; i < count; i++) {
      var mixedFore =
          glyph.fore.blend(fore, MathUtils.lerpDouble(i, 0, count, 0.0, maxMix));
      var mixedBack =
          glyph.back.blend(back, MathUtils.lerpDouble(i, 0, count, 0.0, maxMix));

      glyphs.Add(new Glyph(glyph._char, mixedFore, mixedBack));
    }

    return this;
  }

  public _TileBuilder emanate(int emanation) {
    _emanation = emanation;
    return this;
  }

  public _TileBuilder to(TilePortal portal) {
    _portal = portal;
    return this;
  }

  public _TileBuilder onClose(System.Func<Vec, Action> onClose) {
    _onClose = onClose;
    return this;
  }

  public _TileBuilder onOpen(System.Func<Vec, Action> onOpen) {
    _onOpen = onOpen;
    return this;
  }

  public TileType door() => _motility(Motility.door);

  public TileType transparentDoor() => _motility(Motility.fly | Motility.door);

  public TileType obstacle() => _motility(Motility.fly);

  public TileType open() => _motility(Motility.flyAndWalk);

  public TileType solid() => _motility(Motility.none);

  public TileType water() => _motility(Motility.fly | Motility.swim);

  public TileType _motility(Motility motility) {
    if (glyphs.Count == 1)
        return new TileType(name, glyphs[0], motility,
          portal: _portal,
          emanation: _emanation,
          onClose: _onClose,
          onOpen: _onOpen);
    else
        return new TileType(name, glyphs, motility,
            portal: _portal,
            emanation: _emanation,
            onClose: _onClose,
            onOpen: _onOpen);
  }
}

class TilePortals {
  /// Stairs to exit the dungeon.
  public static TilePortal exit = new TilePortal("exit");

  /// Stairs to enter the dungeon from the town.
  public static TilePortal dungeon = new TilePortal("dungeon");

  public static TilePortal home = new TilePortal("home");

  public static TilePortal shop1 = new TilePortal("shop 1");
  public static TilePortal shop2 = new TilePortal("shop 2");
  public static TilePortal shop3 = new TilePortal("shop 3");
  public static TilePortal shop4 = new TilePortal("shop 4");
  public static TilePortal shop5 = new TilePortal("shop 5");
  public static TilePortal shop6 = new TilePortal("shop 6");
  public static TilePortal shop7 = new TilePortal("shop 7");
  public static TilePortal shop8 = new TilePortal("shop 8");
  public static TilePortal shop9 = new TilePortal("shop 9");
}

