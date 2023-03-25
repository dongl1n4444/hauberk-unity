using System;
using System.Collections.Generic;
using System.Linq;
using Color = UnityEngine.Color;
using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;
using UnityTerminal;

// TODO: Update to handle resizable UI.
class GameOverScreen : Screen 
{
  public Log log;

  public GameOverScreen(Log log)
  {
    this.log = log;
  }

  public override bool KeyDown(KeyCode keyCode, bool shift, bool alt)
  {
    if (keyCode == InputX.cancel)
    {
      terminal.Pop();
      return true;
    }

    return false;
  }

  public override void Render() {
    terminal.Clear();

    terminal.WriteAt(0, 0, log.messages.Last().text);
    terminal.WriteAt(0, terminal.height - 1, "[Esc] Try again", UIHue.helpText);
  }
}