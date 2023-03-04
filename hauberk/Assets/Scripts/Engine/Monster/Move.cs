using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using num = System.Double;

/// A [Move] is an action that a [Monster] can perform aside from the basic
/// walking and melee attack actions. Moves include things like spells, breaths,
/// and missiles.
abstract class Move {
  /// The frequency at which the monster can perform this move (with some
  /// randomness added in).
  ///
  /// A rate of 1 means the monster can perform the move roughly every turn.
  /// A rate of 10 means it can perform it about one in ten turns. Fractional
  /// rates are allowed.
  public num rate;

  /// The range of this move if it's a ranged one, or `0` otherwise.
  int range => 0;

  /// The experience gained by killing a [Monster] with this move.
  ///
  /// This should take the power of the move into account, but not its rate.
  num experience;

  public Move(num rate)
  {
    this.rate = rate;
  }

  /// Returns `true` if the monster would reasonably perform this move right
  /// now during its turn.
  bool shouldUse(Monster monster) => true;

  /// Returns `true` if the monster would reasonably perform this move in
  /// response to taking [damage].
  bool shouldUseOnDamage(Monster monster, int damage) => false;

  /// Called when the [Monster] has selected this move. Returns an [Action] that
  /// performs the move.
  Action getAction(Monster monster) {
    monster.useMove(this);
    return onGetAction(monster);
  }

  /// Create the [Action] to perform this move.
  public abstract Action onGetAction(Monster monster);
}

/// Base class for a Move that performs a ranged attack in some way.
///
/// The monster AI looks for this to determine whether it should go for melee
/// or ranged behavior.
abstract class RangedMove : Move {
  public Attack attack;

  int range => attack.range;

  RangedMove(num rate, Attack attack) 
    : base(rate)
  {
    this.attack = attack;
  }
}
