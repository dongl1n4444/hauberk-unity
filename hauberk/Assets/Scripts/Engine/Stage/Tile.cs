using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Bitmask-like class defining ways that actors can move over tiles.
///
/// Each [TileType] has a set of motilities that determine which kind of
/// movement is needed to enter the tile. Monsters and the hero have a set of
/// motilities that determine which ways they are able to move. In order to
/// move into a tile, the actor must have one of the tile's motilities.
class Motility {
    public static Motility none = new Motility(0);

    // TODO: Should these be in content, engine, or a mixture of both?
    public static Motility door = new Motility(1);
    public static Motility fly = new Motility(2);
    public static Motility swim = new Motility(4);
    public static Motility walk = new Motility(8);

    public static Motility doorAndFly = Motility.door | Motility.fly;
    public static Motility doorAndWalk = Motility.door | Motility.walk;
    public static Motility flyAndWalk = Motility.fly | Motility.walk;
    public static Motility all = door | fly | swim | walk;

    int _bitMask;

    public Motility(int _bitMask)
    {
        this._bitMask = _bitMask;
    }

    int hashCode => _bitMask;

    public static bool operator ==(Motility a, object other) {
        if (other is Motility)
        {
            Motility b = other as Motility;
            return a.hashCode == b.hashCode;
        }
        return false;
    }
    public static bool operator !=(Motility a, object other) {
        return !(a == other);
    }

    public override bool Equals(object o)  
    {  
        return this == o;  
    }  
    
    public override int GetHashCode()  
    {  
        return hashCode;  
    }  

    /// Creates a new MotilitySet containing all of the motilities of this and
    /// [other].
    public static Motility operator |(Motility a, Motility other) => new Motility(a._bitMask | other._bitMask);

    /// Creates a new MotilitySet containing all of the motilities of this
    /// except for the motilities in [other].
    public static Motility operator -(Motility a, Motility other) => new Motility(a._bitMask & ~other._bitMask);

    public bool overlaps(Motility other) => (_bitMask & other._bitMask) != 0;

    string toString() => _bitMask.ToString();
}

/// Enum-like class for tiles that transport the hero: dungeon entrance, exit,
/// shops, etc.
class TilePortal {
  public string  name;

  TilePortal(string name)
  {
    this.name = name;
  }

  string  toString() => name;
}

class TileType {
    /// The type of a tile when first constructed.
    public static TileType uninitialized = new TileType("uninitialized", null, Motility.none);

    public string  name;

    /// Where the tile takes the hero, or `null` if it's a regular tile.
    public TilePortal portal;

    public int emanation;
    public object appearance;

    bool canClose => onClose != null;

    bool canOpen => onOpen != null;

    public Motility motility;

    /// If the tile can be "opened", this is the function that produces an open
    /// action for it. Otherwise `null`.
    public Func<Action, Vec> onClose;

    /// If the tile can be "opened", this is the function that produces an open
    /// action for it. Otherwise `null`.
    public Func<Action, Vec> onOpen;

    public bool isTraversable => canEnter(Motility.doorAndWalk);

    public bool isWalkable => canEnter(Motility.walk);

    TileType(string name, object appearance, Motility motility,
        int emanation = 0, TilePortal portal = null, 
        Func<Action, Vec> onClose = null, Func<Action, Vec> onOpen = null)
    {
        this.name = name;
        this.appearance = appearance;
        this.motility = motility;
        this.emanation = emanation;
        this.portal = portal;
        this.onClose = onClose;
        this.onOpen = onOpen;
    }

    /// Whether an actor with [motility] is able to enter this tile.
    public bool canEnter(Motility motility) => this.motility.overlaps(motility);
}

public class Tile {
    /// The tile's basic type.
    ///
    /// If you change this during the game, make sure to call
    /// [Stage.tileOpacityChanged] if the tile's opacity changed.
    TileType type = TileType.uninitialized;

    /// Whether some other opaque tile is blocking the hero's view of this tile.
    ///
    /// This gets updated by [Fov] as the hero moves around.
    bool _isOccluded = false;

    public bool isOccluded => _isOccluded;

    /// How much visibility is reduced by distance fall-off.
    int fallOff => _fallOff;
    int _fallOff = 0;

    /// Whether the tile can be seen through or blocks the hero's view beyond it.
    ///
    /// We assume any tile that an actor can fly over is also "open" enough to
    /// be seen through. We don't use [isWalkable] because things like water
    /// cannot be walked over but can be seen through.
    public bool blocksView => !isFlyable;

    /// Whether the hero can currently see the tile.
    ///
    /// To be visible, a tile must not be occluded, in the dark, or too far away.
    public bool isVisible => !isOccluded && illumination > _fallOff;

    /// The total amount of light being cast onto this tile from various sources.
    ///
    /// This is a combination of the tile's [emanation], the propagated emanation
    /// from nearby tiles, light from actors, etc.
    int illumination => floorIllumination + actorIllumination;

    public int floorIllumination = 0;
    public int actorIllumination = 0;

    /// The amount of light the tile produces.
    ///
    /// Includes "native" emanation from the tile itself along with light that
    /// has been applied to it.
    public int emanation =>
        Mathf.Clamp(type.emanation + _appliedEmanation, 0, Lighting.floorMax);

    /// The extra emanation applied to this tile independent of its type from
    /// things like light spells.
    int _appliedEmanation = 0;

    /// If you call this, make sure to call [Stage.tileEmanationChanged()].
    void addEmanation(int offset) {
        _appliedEmanation =
            Mathf.Clamp(_appliedEmanation + offset, 0, Lighting.floorMax);
    }

    void maxEmanation(int amount) {
        _appliedEmanation = Mathf.Max(_appliedEmanation, amount);
    }

    bool _isExplored = false;

    bool isExplored => _isExplored;

    /// Marks this tile as explored if the hero can see it and hasn't previously
    /// explored it.
    ///
    /// This should not be called directly. Instead, call [Stage.explore()].
    ///
    /// Returns true if this tile was explored just now.
    public bool updateExplored(bool force = false) {
        if ((force || isVisible) && !_isExplored) {
            _isExplored = true;
            return true;
        }
        return false;
    }

    public void updateVisibility(bool isOccluded, int fallOff) {
        _isOccluded = isOccluded;
        _fallOff = fallOff;
    }

    /// The element of the substance occupying this file: fire, water, poisonous
    /// gas, etc.
    public Element element = Element.none;

    /// How much of [element] is occupying the tile.
    public int substance = 0;

    public bool isWalkable => type.isWalkable;

    bool isTraversable => type.isTraversable;

    public bool isFlyable => canEnter(Motility.fly);

    public bool isClosedDoor => type.motility == Motility.door;

    TilePortal? portal => type.portal;

    public bool canEnter(Motility motility) => type.canEnter(motility);
}
