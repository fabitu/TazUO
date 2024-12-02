using ClassicUO.Configuration;
using ClassicUO.Game.Data.Preferences;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Resources;
using CUO_APINetPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Game.UI.Gumps
{
    internal class StaticFilterGump : ResizableGump
    {
        private readonly ushort? _graphic;
        uint _serial;

        private PreferenceManagerBase currentPreferenceMannager;
        #region Mannagers
        private readonly PreferenceManagerBase _wallMannager = new PreferenceWallManager();
        private readonly PreferenceManagerBase _doorsMannager = new PreferenceDoorMannager();
        #endregion

        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 20;
        #endregion

        #region private static vars
        public static int lastX = 100, lastY = 100, lastCorpseX = 100, lastCorpseY = 100;
        public static int gridItemSize { get { return (int)Math.Round(50 * (ProfileManager.CurrentProfile.GridContainerScale / 100f)); } }
        public static int borderWidth = 4;
        #endregion


        BaseGameObject _seletedObject;

        public StaticFilterGump(uint serial) : base(GetWidth(), GetHeight(), GetWidth(2), GetHeight(1), serial, 0)
        {
            _serial = serial;
            EnsureContextMenu();
            ShowContextMenu();
        }

        public StaticFilterGump(BaseGameObject seletedObject, uint serial) : base(GetWidth(), GetHeight(), GetWidth(2), GetHeight(1), serial, 0)
        {
            _serial = serial;
            _seletedObject = seletedObject;
            EnsureContextMenu();
            ShowContextMenu();
        }

        private void EnsureContextMenu()
        {
            ContextMenu = new ContextMenuControl();
            ContextMenu.Add("Open", Open);
            if (_graphic != null) { ContextMenu.Add("Add", AddItem); }
            if (_graphic != null) { ContextMenu.Add(ResGumps.Remove, RemoveItem); }
        }

      

        public void ShowContextMenu()
        {
            ContextMenu?.Show();
        }

        private void Open()
        {
            StaticFilterGump staticFilterGump = UIManager.GetGump<StaticFilterGump>();

            if (staticFilterGump == null)
            {
                staticFilterGump = new StaticFilterGump(_serial)
                {
                    X = Mouse.Position.X,
                    Y = Mouse.Position.Y
                };
                UIManager.Add(staticFilterGump);
                staticFilterGump.SetInScreen();
            }
            else
            {
                staticFilterGump.SetInScreen();
                staticFilterGump.BringOnTop();
            }
        }

        private void AddItem()
        {
            if (_graphic != null)
            {
                currentPreferenceMannager.RemoveGraphic(_graphic.Value);
                Client.LoadTileData();
                OptionsGump optionsGump = new();
                optionsGump.Apply();
            }
        }

        private void RemoveItem()
        {
            if (_graphic != null)
            {
                currentPreferenceMannager.RemoveGraphic(_graphic.Value);
                Client.LoadTileData();
                OptionsGump optionsGump = new();
                optionsGump.Apply();
            }
        }

        private static int GetWidth(int columns = -1)
        {
            if (columns < 0)
                columns = ProfileManager.CurrentProfile.Grid_DefaultColumns;
            return borderWidth * 2     //The borders around the container, one on the left and one on the right
            + 15                   //The width of the scroll bar
            + gridItemSize * columns //How many items to fit in left to right
            + X_SPACING * columns;      //Spacing between each grid item(x columns)
        }

        private static int GetHeight(int rows = -1)
        {
            if (rows < 0)
                rows = ProfileManager.CurrentProfile.Grid_DefaultRows;
            return TOP_BAR_HEIGHT + borderWidth * 2 + (gridItemSize + Y_SPACING) * rows;
        }
    }
}