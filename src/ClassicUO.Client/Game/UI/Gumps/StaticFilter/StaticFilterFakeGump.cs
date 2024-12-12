using ClassicUO.Assets;
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
        #region Mannagers
        private static PreferenceManagerBase currentPreferenceMannager;
        private static List<StaticCustomItens> currentPreferenceData;
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

        private readonly GameObject _seletedObject;
        public StaticFilterFakeGump(GameObject seletedObject) : base(0, 0)
        {
            _seletedObject = seletedObject;
            Init();
        }
        private void Init()
        {
            StaticTiles _itemData;
            if (SelectedObject.Object is Static @static)
            {
                _itemData = @static.ItemData;
            }
            else if (SelectedObject.Object is Item item)
            {
                _itemData = item.ItemData;
            }
            else if (SelectedObject.Object is Multi multi)
            {
                _itemData = multi.ItemData;
            }
            else
            {
                GameActions.Log($"{SelectedObject.Object.GetType()}");
                return;
            }

            if (_itemData.IsWall && !_itemData.IsDoor)
            {
                currentPreferenceMannager = StaticFilters.PreferencesWallMannager;
                currentPreferenceData = StaticFilters.CustomWalls;
            }
            else if (_itemData.IsDoor)
            {
                currentPreferenceMannager = StaticFilters.PreferencesDoorMannager;
                currentPreferenceData = StaticFilters.CustomDoors;
            }

            if (_itemData.IsWall || _itemData.IsDoor)
            {
                EnsureContext();
                ShowContextMenu();
            }
        }
        private void EnsureContext()
        {
            ContextMenu?.Dispose();
            ContextMenu = new ContextMenuControl();
            ContextMenu.Add("Open", Open);
            if (currentPreferenceData.Count > 0)
            {
                var item = currentPreferenceData.FirstOrDefault(x => x.ToReplaceGraphicArray.Contains(_seletedObject.Graphic));
                if (item == null)
                {
                    List<ContextMenuItemEntry> entries = [];
                    foreach (var preferences in currentPreferenceData)
                    {
                        if (!preferences.ToReplaceGraphicArray.Contains(_seletedObject.Graphic))
                            entries.Add(new ContextMenuItemEntry(preferences.Description, () => { AddItem(preferences); }));
                    }
                    ContextMenu.Add(ResGumps.Add, entries);
                }
                else
                {
                    ContextMenu.Add($"{ResGumps.Remove} - {item.Description}", RemoveItem);
                }
            }
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
        private void AddItem(StaticCustomItens customItem)
        {
            currentPreferenceMannager.AddGraphic(customItem, _seletedObject.Graphic);
            currentPreferenceMannager.ReloadPreferences();
        }
        private void RemoveItem()
        {
            currentPreferenceMannager.RemoveGraphic(_seletedObject.Graphic);
            currentPreferenceMannager.ReloadPreferences();
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