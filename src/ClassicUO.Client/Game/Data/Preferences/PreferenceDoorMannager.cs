using ClassicUO.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Game.Data.Preferences
{
    internal class PreferenceDoorMannager : PreferenceManagerBase
    {
        private const string FILENAME = "door.json";
        private List<ushort> _defaultReplaceGraphicList = [0x01FF, 0x0200, 0x0201, 0x0202, 0x0203, 0x0204, 0x0205, 0x0206];

        public PreferenceDoorMannager() : base(FILENAME)
        {
            //defaultReplaceGraphic = ProfileManager.CurrentProfile.DefaultDoorGraphic;
            //defaultReplaceGraphicList = _defaultReplaceGraphicList;
        }
    }
}
