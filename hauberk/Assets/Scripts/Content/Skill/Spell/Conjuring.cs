using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Flee : Spell {
  public override string description => "Teleports the hero a short distance away.";
  public override string name => "Flee";
  public override int baseComplexity => 10;
  public override int baseFocusCost => 16;
  public override int range => 8;

  Action onGetAction(Game game, int level) => new TeleportAction(range);
}

class Escape : Spell {
  public override string description => "Teleports the hero away.";
  public override string name => "Escape";
  public override int baseComplexity => 15;
  public override int baseFocusCost => 25;
  public override int range => 16;

  Action onGetAction(Game game, int level) => new TeleportAction(range);
}

class Disappear : Spell {
  public override string description => "Moves the hero across the dungeon.";
  public override string name => "Disappear";
  public override int baseComplexity => 30;
  public override int baseFocusCost => 50;
  public override int range => 100;

  Action onGetAction(Game game, int level) => new TeleportAction(range);
}

// TODO: These spells are all kind of similar and boring. Might be good if they
// had some differences. Maybe some could try to teleport specifically far away
// from monsters, etc.