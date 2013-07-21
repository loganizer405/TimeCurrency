using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace TimeCurrency
{
    public class Player
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
        public int lasttileX { get; set; }
        public int lasttileY { get; set; }
        public bool afk { get; set; }
        public int timeplayed { get; set; }
        public bool deadlock { get; set; }
        public bool lowbal { get; set; }

        public Player(int index)
        {
            Index = index;
            lasttileX = TShock.Players[Index].TileX;
            lasttileY = TShock.Players[Index].TileY;
            afk = false;
            timeplayed = 0;
        }
    }
}
