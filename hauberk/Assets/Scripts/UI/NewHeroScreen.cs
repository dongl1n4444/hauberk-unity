using System;
using System.Collections.Generic;
using UnityTerminal;
using UnityEngine;

class _Field {
  public const int name = 0;
  public const int race = 1;
  public const int heroClass = 2;
  public const int count = 3;
}

// TODO: Update to handle resizable UI.
class NewHeroScreen : UnityTerminal.Screen {

  // From: http://medieval.stormthecastle.com/medieval-names.htm.
  private static string[] _defaultNames = new string[]{
    "Merek",
    "Carac",
    "Ulric",
    "Tybalt",
    "Borin",
    "Sadon",
    "Terrowin",
    "Rowan",
    "Forthwind",
    "Althalos",
    "Fendrel",
    "Brom",
    "Hadrian",
    "Crewe",
    "Bolbec",
    "Fenwick",
    "Mowbray",
    "Drake",
    "Bryce",
    "Leofrick",
    "Letholdus",
    "Lief",
    "Barda",
    "Rulf",
    "Robin",
    "Gavin",
    "Terrin",
    "Jarin",
    "Cedric",
    "Gavin",
    "Josef",
    "Janshai",
    "Doran",
    "Asher",
    "Quinn",
    "Xalvador",
    "Favian",
    "Destrian",
    "Dain",
    "Millicent",
    "Alys",
    "Ayleth",
    "Anastas",
    "Alianor",
    "Cedany",
    "Ellyn",
    "Helewys",
    "Malkyn",
    "Peronell",
    "Thea",
    "Gloriana",
    "Arabella",
    "Hildegard",
    "Brunhild",
    "Adelaide",
    "Beatrix",
    "Emeline",
    "Mirabelle",
    "Helena",
    "Guinevere",
    "Isolde",
    "Maerwynn",
    "Catrain",
    "Gussalen",
    "Enndolynn",
    "Krea",
    "Dimia",
    "Aleida"
  };

  public const int _maxNameLength = 20;

  public Content content;
  public Storage storage;

  int _field = _Field.name;
  string _name = "";
  string _defaultName;
  int _race;
  int _class;

  public NewHeroScreen(Content content, Storage storage)
  {
    this.content = content;
    this.storage = storage;

    _defaultName = Rng.rng.item<string>(_defaultNames);
    _race = Rng.rng.range(content.races.Count);
    _class = Rng.rng.range(content.classes.Count);
  }

  public override void Render()
  {
    terminal.Clear();

    _renderName(terminal);
    _renderRace(terminal);
    _renderClass(terminal);
    _renderMenu(terminal);

    var help = new List<string>(){"[Tab] Next field"};
    switch (_field) {
      case _Field.name:
        help.Add("[A-Z Del] Edit name");
        break;
      case _Field.race:
        help.Add("[↕] Select race");
        break;
      case _Field.heroClass:
        help.Add("[↕] Select class");
        break;
    }

    help.Add("[Enter] Create hero");
    help.Add("[Esc] Cancel");
    terminal.WriteAt(0, terminal.height - 1, string.Join(", ", help), UIHue.helpText);
  }

  void _renderName(Terminal terminal) {
    var p = new Panel(0, 0, 40, 10);

    TerminalUtils.DrawFrame(terminal, 0, 0, terminal.width, terminal.height,
        _field == _Field.name ? UIHue.selection : Hues.darkCoolGray);

    terminal.WriteAt(p,
        1, 0, "Name", _field == _Field.name ? UIHue.selection : UIHue.text);
    terminal.WriteAt(p, 1, 2, "Out of the mists of history, a hero", UIHue.text);
    terminal.WriteAt(p, 1, 3, "appears named...", UIHue.text);

    TerminalUtils.DrawBox(terminal, p, 2, 5, 23, 3,
        _field == _Field.name ? UIHue.selection : UIHue.disabled);

    if (string.IsNullOrEmpty(_name) == false)
    {
      terminal.WriteAt(p, 3, 6, _name, UIHue.primary);
      if (_field == _Field.name) {
        terminal.WriteAt(p,
            3 + _name.Length, 6, " ", Color.black, UIHue.selection);
      }
    }
    else
    {
      if (_field == _Field.name) {
        terminal.WriteAt(p, 3, 6, _defaultName, Color.black, UIHue.selection);
      } else {
        terminal.WriteAt(p, 3, 6, _defaultName, UIHue.primary);
      }
    }
  }

  void _renderRace(Terminal terminal) {
    // terminal = terminal.rect(0, 10, 40, 29);

    // Draw.frame(terminal, 0, 0, terminal.width, terminal.height,
    //     _field == _Field.race ? UIHue.selection : darkCoolGray);
    // terminal.writeAt(
    //     1, 0, "Race", _field == _Field.race ? UIHue.selection : UIHue.text);

    // var race = content.races[_race];
    // terminal.writeAt(1, 2, race.name, UIHue.primary);

    // var y = 4;
    // for (var line in Log.wordWrap(38, race.description)) {
    //   terminal.writeAt(1, y, line, UIHue.text);
    //   y++;
    // }

    // y = 18;
    // for (var stat in Stat.all) {
    //   terminal.writeAt(2, y, stat.name, UIHue.secondary);
    //   var width = 25 * race.stats[stat]! ~/ 45;
    //   terminal.writeAt(12, y, " " * width, ash, red);
    //   terminal.writeAt(12 + width, y, " " * (25 - width), ash, maroon);
    //   y += 2;
    // }
  }

  void _renderClass(Terminal terminal) {
    // terminal = terminal.rect(40, 10, 40, 29);

    // Draw.frame(terminal, 0, 0, terminal.width, terminal.height,
    //     _field == _Field.heroClass ? UIHue.selection : darkCoolGray);
    // terminal.writeAt(1, 0, "Class",
    //     _field == _Field.heroClass ? UIHue.selection : UIHue.text);

    // var heroClass = content.classes[_class];
    // terminal.writeAt(1, 2, heroClass.name, UIHue.primary);

    // var y = 4;
    // for (var line in Log.wordWrap(38, heroClass.description)) {
    //   terminal.writeAt(1, y, line, UIHue.text);
    //   y++;
    // }
  }

  void _renderMenu(Terminal terminal) {
    // terminal = terminal.rect(40, 0, 40, 10);

    // Draw.frame(terminal, 0, 0, terminal.width, terminal.height);

    // if (_field == _Field.name) return;

    // string label;
    // var items = <string>[];
    // int selected;
    // if (_field == _Field.race) {
    //   label = "race";
    //   items.addAll(content.races.map((race) => race.name));
    //   selected = _race;
    // } else {
    //   label = "class";
    //   items.addAll(content.classes.map((c) => c.name));
    //   selected = _class;
    // }

    // terminal.writeAt(1, 0, "Choose a $label:", UIHue.selection);

    // var y = 2;
    // for (var i = 0; i < items.length; i++) {
    //   var item = items[i];
    //   var isSelected = i == selected;
    //   terminal.writeAt(
    //       2, y, item, isSelected ? UIHue.selection : UIHue.primary);
    //   if (isSelected) {
    //     terminal.writeAt(1, y, "►", UIHue.selection);
    //   }
    //   y++;
    // }
  }

  // bool handleInput(Input input) {
  //   if (input == Input.cancel) {
  //     ui.pop();
  //     return true;
  //   }

  //   if (_field == _Field.race) {
  //     switch (input) {
  //       case Input.n:
  //         _changeRace(-1);
  //         return true;

  //       case Input.s:
  //         _changeRace(1);
  //         return true;
  //     }
  //   } else if (_field == _Field.heroClass) {
  //     switch (input) {
  //       case Input.n:
  //         _changeClass(-1);
  //         return true;

  //       case Input.s:
  //         _changeClass(1);
  //         return true;
  //     }
  //   }

  //   return false;
  // }

  // bool keyDown(int keyCode, {required bool shift, required bool alt}) {
  //   // TODO: Figuring out the char code manually here is lame. Pass it in from
  //   // the KeyEvent?

  //   switch (keyCode) {
  //     case KeyCode.enter:
  //       var hero = content.createHero(_name.isNotEmpty ? _name : _defaultName,
  //           content.races[_race], content.classes[_class]);
  //       storage.heroes.add(hero);
  //       storage.save();
  //       ui.goTo(GameScreen.town(storage, content, hero));
  //       return true;

  //     case KeyCode.tab:
  //       if (shift) {
  //         _changeField(-1);
  //       } else {
  //         _changeField(1);
  //       }
  //       return true;

  //     case KeyCode.delete:
  //       if (_field == _Field.name) {
  //         if (_name.isNotEmpty) {
  //           _name = _name.substring(0, _name.length - 1);

  //           // Pick a new default name.
  //           if (_name.isEmpty) {
  //             _defaultName = rng.item(_defaultNames);
  //           }

  //           dirty();
  //         }
  //       }
  //       return true;

  //     case KeyCode.space:
  //       if (_field == _Field.name) {
  //         // TODO: Handle modifiers.
  //         _appendToName(" ");
  //       }
  //       return true;

  //     default:
  //       if (_field == _Field.name && !alt) {
  //         var key = keyCode;

  //         if (key >= KeyCode.a && key <= KeyCode.z) {
  //           var charCode = key;
  //           // TODO: Handle other modifiers.
  //           if (!shift) {
  //             charCode = 'a'.codeUnits[0] - 'A'.codeUnits[0] + charCode;
  //           }

  //           _appendToName(string.fromCharCodes([charCode]));
  //           return true;
  //         } else if (key >= KeyCode.zero && key <= KeyCode.nine) {
  //           _appendToName(string.fromCharCodes([key]));
  //           return true;
  //         }
  //       }
  //       break;
  //   }

  //   return false;
  // }

  void _changeField(int offset) {
    // _field = (_field + offset + _Field.count) % _Field.count;
    // dirty();
  }

  void _appendToName(string text) {
    // _name += text;
    // if (_name.length > _maxNameLength) {
    //   _name = _name.substring(0, _maxNameLength);
    // }

    // dirty();
  }

  void _changeRace(int offset) {
    // var race = (_race + offset).clamp(0, content.races.length - 1);
    // if (race != _race) {
    //   _race = race;
    //   dirty();
    // }
  }

  void _changeClass(int offset) {
    // var heroClass = (_class + offset).clamp(0, content.classes.length - 1);
    // if (heroClass != _class) {
    //   _class = heroClass;
    //   dirty();
    // }
  }
}