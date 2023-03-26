using System.Collections;
using System.Collections.Generic;

using System.Linq;

class WhipMastery : MasteryDiscipline
{
  // TODO: Tune.
  static double _whipScale(int level) => MathUtils.lerpDouble(level, 1, 10, 1.0, 3.0);

  // TODO: Better name.
  public override string name => "Whip Mastery";
  public override string useName => "Whip Crack";
  public override string description =>
      "Whips and flails are difficult to use well, but deadly even at a " +
      "distance when mastered.";
  public override string weaponType => "whip";

  public override string levelDescription(int level)
  {
    var damage = (int)(_whipScale(level) * 100);
    return base.levelDescription(level) +
        " Ranged whip attacks inflict $damage% of the damage of a regular attack.";
  }

  int furyCost(HeroSave hero, int level) => 20;

  int getRange(Game game) => 3;

  Action onGetTargetAction(Game game, int level, Vec target)
  {
    var defender = game.stage.actorAt(target);

    // Find which hand has a whip. If both do, just pick the first.
    // TODO: Is this the best way to handle dual-wielded whips?
    var weapons = game.hero.equipment.weapons.ToList();
    var hits = game.hero.createMeleeHits(defender);
    DartUtils.assert(weapons.Count == hits.Count);

    // Should have at least one whip wielded.
    Hit hit = null;
    for (var i = 0; i < weapons.Count; i++)
    {
      if (weapons[i].type.weaponType != "whip") continue;

      hit = hits[i];
      break;
    }

    hit.scaleDamage(WhipMastery._whipScale(level));

    // TODO: Better effect.
    return new BoltAction(target, hit, range: getRange(game), canMiss: true);
  }
}
