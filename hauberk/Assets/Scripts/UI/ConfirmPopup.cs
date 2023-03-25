using System;
using System.Collections.Generic;
using System.Linq;
using Color = UnityEngine.Color;
using Input = UnityEngine.Input;
using UnityTerminal;
using UnityEngine;

/// Modal dialog for letting the user confirm an action.
class ConfirmPopup : Popup 
{
  public string _message;
  public object _result;

  public ConfirmPopup(string _message, object _result)
  {
    this._message = _message;
    this._result = _result;
  }

  List<string> message => new List<string>(){_message};

  Dictionary<string, string> helpKeys =>
      new Dictionary<string, string>() {
        {"Y", "Yes"}, 
        {"N", "No"}, 
        {"Esc", "No"}
      };

  public override bool KeyDown(KeyCode keyCode, bool shift, bool alt)
  {
    if (keyCode == InputX.cancel) {
      terminal.Pop();
      return true;
    }

    if (shift || alt) return false;
    
    if (keyCode == KeyCode.N)
    {
      terminal.Pop();
      return true;
    }
    else if (keyCode == KeyCode.Y)
    {
      terminal.Pop(_result);
      return true;
    }
    return false;
  }
}