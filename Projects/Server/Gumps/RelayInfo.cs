/***************************************************************************
 *                               RelayInfo.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

namespace Server.Gumps
{
  public class TextRelay
  {
    public TextRelay(int entryID, string text)
    {
      EntryID = entryID;
      Text = text;
    }

    public int EntryID { get; }

    public string Text { get; }
  }

  public class RelayInfo
  {
    public RelayInfo(int buttonID, int[] switches, TextRelay[] textEntries)
    {
      ButtonID = buttonID;
      Switches = switches;
      TextEntries = textEntries;
    }

    public int ButtonID { get; }

    public int[] Switches { get; }

    public TextRelay[] TextEntries { get; }

    public bool IsSwitched(int switchID)
    {
      for (var i = 0; i < Switches.Length; ++i)
        if (Switches[i] == switchID)
          return true;

      return false;
    }

    public TextRelay GetTextEntry(int entryID)
    {
      for (var i = 0; i < TextEntries.Length; ++i)
        if (TextEntries[i].EntryID == entryID)
          return TextEntries[i];

      return null;
    }
  }
}
