using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using static ClassicUO.Game.Data.StaticFilters;

namespace ClassicUO.Game.Data.Preferences
{
    internal class PreferenceWallManager : PreferenceManagerBase
    {
        private const string FILENAME = "wall.json";
        private List<ushort> _defaultReplaceGraphicList = [0x01FF, 0x0200, 0x0201, 0x0202, 0x0203, 0x0204, 0x0205, 0x0206];

        public PreferenceWallManager() : base(FILENAME)
        {
            //defaultReplaceGraphic = ProfileManager.CurrentProfile.DefaultWallGraphic;
            //defaultReplaceGraphicList = _defaultReplaceGraphicList;
        }
    }
}
