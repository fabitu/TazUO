using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Data.Preferences;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterFakeGump : Gump
    {
        private readonly ushort? _graphic;

        #region Mannagers
        private static PreferenceManagerBase currentPreferenceMannager; 
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


        GameObject _seletedObject;

        public StaticFilterFakeGump(GameObject seletedObject) : base(0, 0)
        {
            _seletedObject = seletedObject;
            if (SelectedObject.Object is GameObject gameObject && SelectedObject.Object is Static st)
            {
                _graphic = gameObject.Graphic;
                if (st.ItemData.IsWall)
                    currentPreferenceMannager = StaticFilters.PreferencesWallMannager;
                else if (st.ItemData.IsDoor)
                    currentPreferenceMannager = StaticFilters.PreferencesDoorMannager;

                if (st.ItemData.IsWall || st.ItemData.IsDoor)
                {
                    EnsureContextMenu();
                    ShowContextMenu();
                }
            }
        }
        private void EnsureContextMenu()
        {
            ContextMenu?.Dispose();
            ContextMenu = new ContextMenuControl();
            ContextMenu.Add("Open", Open);
            if (_graphic != null) { ContextMenu.Add(ResGumps.Add, AddItem); }
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
                staticFilterGump = new StaticFilterGump(_seletedObject)
                {
                    X = 400,
                    Y = 200
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
                currentPreferenceMannager.AddGraphic(null, _graphic.Value);
                currentPreferenceMannager.ReloadPreferences();
            }
        }
        private void RemoveItem()
        {
            if (_graphic != null)
            {
                currentPreferenceMannager.RemoveGraphic(_graphic.Value);
                currentPreferenceMannager.ReloadPreferences();
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