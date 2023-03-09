using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Icicle : Spell {
  public override string name => "Icicle";
  public override string description => "Launches a spear-like icicle.";
  int baseComplexity => 10;
  int baseFocusCost => 12;
  int damage => 8;
  int range => 8;

  Action onGetTargetAction(Game game, int level, Vec target) {
    var attack =
        new Attack(new Noun("the icicle"), "pierce", damage, range, Elements.cold);
    return new BoltAction(target, attack.createHit());
  }
}

class BrilliantBeam : Spell {
  public override string name => "Brilliant Beam";
  public override string description => "Emits a blinding beam of radiance.";
  int baseComplexity => 14;
  int baseFocusCost => 24;
  int damage => 10;
  int range => 12;

  Action onGetTargetAction(Game game, int level, Vec target) {
    var attack =
        new Attack(new Noun("the light"), "sear", damage, range, Elements.light);
    return RayAction.cone(game.hero.pos, target, attack.createHit());
  }
}

class Windstorm : Spell {
  public override string name => "Windstorm";
  public override string description =>
      "Summons a blast of air, spreading out from the sorceror.";
  int baseComplexity => 18;
  int baseFocusCost => 36;
  int damage => 10;
  int range => 6;

  Action onGetAction(Game game, int level) {
    var attack = new Attack(new Noun("the wind"), "blast", damage, range, Elements.air);
    return new FlowAction(game.hero.pos, attack.createHit(), Motility.flyAndWalk);
  }
}

class FireBarrier : Spell {
  public override string name => "Fire Barrier";
  public override string description => "Creates a wall of fire.";
  int baseComplexity => 30;
  int baseFocusCost => 45;
  int damage => 10;
  int range => 8;

  Action onGetTargetAction(Game game, int level, Vec target) {
    var attack = new Attack(new Noun("the fire"), "burn", damage, range, Elements.fire);
    return new BarrierAction(game.hero.pos, target, attack.createHit());
  }
}

class TidalWave : Spell{
  public override string name => "Tidal Wave";
  public override string description => "Summons a giant tidal wave.";
  int baseComplexity => 40;
  int baseFocusCost => 70;
  int damage => 50;
  int range => 15;

  Action onGetAction(Game game, int level) {
    var attack =
        new Attack(new Noun("the wave"), "inundate", damage, range, Elements.water);
    return new FlowAction(game.hero.pos, attack.createHit(),
        Motility.walk | Motility.door | Motility.swim,
        slowness: 2);
  }
}
