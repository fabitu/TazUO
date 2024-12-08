using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Renderer.Lights;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static ClassicUO.Game.UI.Gumps.GridContainer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterSlotManager
    {
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 60;
        #endregion

        public Dictionary<int, StaticFilterItem> gridSlots = [];    
        private List<GameObjectInfo> gridContents;
        private int amount = 125;
        private Control area;
        private Dictionary<int, uint> itemPositions = new();
        private List<uint> itemLocks = new();
        private static int slots = 0;

        public Dictionary<int, StaticFilterItem> GridSlots { get { return gridSlots; } }
        public List<GameObjectInfo> ContainerContents { get { return gridContents; } }
        public Dictionary<int, uint> ItemPositions { get { return itemPositions; } }
        public StaticFilterSlotManager(ushort? graphic, Control controlArea)
        {
            area = controlArea;
            if (graphic != null)
                AddItem(graphic.Value);           
        }        
        public void AddItem(ushort graphic)
        {
            StaticFilterItem GI = new(graphic, slots, gridItemSize);
            if (GI.texture == null)
                return;
            gridSlots.Add(slots, GI);
            area.Add(GI);
            slots++;
            SetGridPositions();
        }
        
        public StaticFilterItem FindItem(uint serial)
        {
            foreach (var slot in gridSlots)
                if (slot.Value.LocalSerial == serial)
                    return slot.Value;
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <returns>List of items matching the search result, or all items if search is blank/profile does has hide search mode disabled</returns>
        public List<GameObjectInfo> SearchResults(string search)
        {
            if (search != "")
            {
                if (ProfileManager.CurrentProfile.GridContainerSearchMode == 0) //Hide search mode
                {
                    List<GameObjectInfo> filteredContents = new();
                    foreach (GameObjectInfo i in gridContents)
                    {
                        if (SearchItemNameAndProps(search, i))
                            filteredContents.Add(i);
                    }
                    return filteredContents;
                }
            }
            return gridContents;
        }

        private bool SearchItemNameAndProps(string search, GameObjectInfo item)
        {
            if (item == null)
                return false;

            if (World.OPL.TryGetNameAndData(item.Graphic, out string name, out string data))
            {
                if (name != null && name.ToLower().Contains(search.ToLower()))
                    return true;
                if (data != null)
                    if (data.ToLower().Contains(search.ToLower()))
                        return true;
            }
            else
            {
                if (item.Name != null && item.Name.ToLower().Contains(search.ToLower()))
                    return true;

                //if (item.ItemData.Name.ToLower().Contains(search.ToLower()))
                //    return true;
            }

            return false;
        }
        /// <summary>
        /// Set the visual grid items to the current GridSlots dict
        /// </summary>
        public void SetGridPositions()
        {
            int x = X_SPACING, y = 0;
            foreach (var slot in gridSlots)
            {
                if (!slot.Value.IsVisible)
                {
                    continue;
                }
                if (x + gridItemSize >= area.Width - 14) //14 is the scroll bar width
                {
                    x = X_SPACING;
                    y += gridItemSize + Y_SPACING;
                }
                slot.Value.X = x;
                slot.Value.Y = y;
                slot.Value.Resize();
                x += gridItemSize + X_SPACING;
            }
        }      

        public int hcount = 0;
    }
}
