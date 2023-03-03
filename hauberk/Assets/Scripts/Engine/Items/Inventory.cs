using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// An [Item] in the game can be either on the ground in the level, or held by
/// the [Hero] in their [Inventory] or [Equipment]. This enum describes which
/// of those is the case.
public class ItemLocation 
{
    public static ItemLocation onGround =
        new ItemLocation("On Ground", "There is nothing on the ground.");
    public static ItemLocation inventory =
        new ItemLocation("Inventory", "Your backpack is empty.");
    public static ItemLocation equipment = new ItemLocation("Equipment", "<not used>");
    public static ItemLocation home = new ItemLocation("Home", "There is nothing in your home.");
    public static ItemLocation crucible =
        new ItemLocation("Crucible", "The crucible is waiting.");

    public string name;
    public string emptyDescription;

    ItemLocation(string name, string emptyDescription)
    {
        this.name = name;
        this.emptyDescription = emptyDescription;
    }

    public static ItemLocation shop(string name) => new ItemLocation(name, "All sold out!");
}

// TODO: Move tryAdd() out of ItemCollection and Equipment? I think it's only
// needed for the home and crucible?
mixin ItemCollection implements Iterable<Item> {
  ItemLocation get location;

  string get name => location.name;

  int get length;

  Item operator [](int index);

  /// If the item collection has named slots, returns their names.
  ///
  /// Otherwise returns `null`. It's only valid to access this if [slots]
  /// returns `null` for some index.
  List<string> get slotTypes => const [];

  /// If the item collection may have empty slots in it (equipment) this returns
  /// the sequence of items and slots.
  Iterable<Item?> get slots => this;

  void remove(Item item);

  Item removeAt(int index);

  /// Returns `true` if the entire stack of [item] will fit in this collection.
  bool canAdd(Item item);

  AddItemResult tryAdd(Item item);

  /// Called when the count of an item in the collection has changed.
  void countChanged();
}

/// The collection of [Item]s held by an [Actor].
class Inventory : IterableMixin<Item> with ItemCollection {
  public ItemLocation location;

  public List<Item> _items;
  public int? _capacity;

  /// If the [Hero] had to unequip an item in order to equip another one, this
  /// will refer to the item that was unequipped.
  ///
  /// If the hero isn't holding an unequipped item, returns `null`.
  Item? get lastUnequipped => _lastUnequipped;
  Item? _lastUnequipped;

  int get length => _items.length;

  Item operator [](int index) => _items[index];

  Inventory(this.location, [this._capacity, Iterable<Item>? items])
      : _items = [...?items];

  /// Creates a new copy of this Inventory. This is done when the [Hero] enters
  /// a stage so that any inventory changes that happen in the stage are
  /// discarded if the hero dies.
  Inventory clone() {
    var items = _items.map((item) => item.clone());
    return Inventory(location, _capacity, items);
  }

  /// Removes all items from the inventory.
  void clear() {
    _items.clear();
    _lastUnequipped = null;
  }

  void remove(Item item) {
    _items.remove(item);
  }

  Item removeAt(int index) {
    var item = _items[index];
    _items.removeAt(index);
    if (_lastUnequipped == item) _lastUnequipped = null;
    return item;
  }

  bool canAdd(Item item) {
    // If there's an empty slot, can always add it.
    if (_capacity == null || _items.length < _capacity!) return true;

    // See if we can merge it with other stacks.
    var remaining = item.count;
    for (var existing in _items) {
      if (existing.canStack(item)) {
        remaining -= existing.type.maxStack - existing.count;
        if (remaining <= 0) return true;
      }
    }

    // If we get here, there are no open slots and not enough stacks that can
    // take the item.
    return false;
  }

  AddItemResult tryAdd(Item item, {bool wasUnequipped = false}) {
    var adding = item.count;

    // Try to add it to existing stacks.
    for (var existing in _items) {
      existing.stack(item);

      // If we completely stacked it, we're done.
      if (item.count == 0) {
        return AddItemResult(adding, 0);
      }
    }

    // See if there is room to start a new stack with the rest.
    if (_capacity != null && _items.length >= _capacity!) {
      // There isn't room to pick up everything.
      return AddItemResult(adding - item.count, item.count);
    }

    // Add a new stack.
    _items.add(item);
    _items.sort();

    if (wasUnequipped) _lastUnequipped = item;
    return AddItemResult(adding, 0);
  }

  /// Re-sorts multiple stacks of the same item to pack them into the minimum
  /// number of stacks.
  ///
  /// This should be called any time the count of an item stack in the hero's
  /// inventory is changed.
  void countChanged() {
    // Hacky. Just re-add everything from scratch.
    var items = _items.toList();
    _items.clear();

    for (var item in items) {
      var result = tryAdd(item);
      assert(result.remaining == 0);
    }
  }

  Iterator<Item> get iterator => _items.iterator;
}

/// Describes the result of attempting to add an item stack to the inventory.
class AddItemResult 
{
  /// The count of items in the stack that were successfully added.
  public int added;

  /// The count of items that could not be fit into the inventory.
  public int remaining;

  AddItemResult(int added, int remaining)
  {
    this.added = added;
    this.remaining = remaining;
  }
}