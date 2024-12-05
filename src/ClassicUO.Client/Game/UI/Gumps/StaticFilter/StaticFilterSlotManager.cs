using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using System.Collections.Generic;
using System.Linq;
using static ClassicUO.Game.UI.Gumps.GridContainer;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterSlotManager
    {
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 20;
        #endregion

        private Dictionary<int, StaticFilterItem> gridSlots = new Dictionary<int, StaticFilterItem>();
        private GameObjectInfo item;
        private List<GameObjectInfo> gridContents;
        private int amount = 125;
        private Control area;
        private Dictionary<int, uint> itemPositions = new Dictionary<int, uint>();
        private List<uint> itemLocks = new List<uint>();

        public Dictionary<int, StaticFilterItem> GridSlots { get { return gridSlots; } }
        public List<GameObjectInfo> ContainerContents { get { return gridContents; } }
        public Dictionary<int, uint> ItemPositions { get { return itemPositions; } }

        public StaticFilterSlotManager(StaticFilterGump staticFilter, Control controlArea)
        {
            #region VARS
            area = controlArea;           
            LoadItens();
            if (gridContents.Count > 125)
                amount = gridContents.Count;
            #endregion

            for (int i = 0; i < amount; i++)
            {
                StaticFilterItem GI = new StaticFilterItem(0, gridItemSize, item, staticFilter, i);
                gridSlots.Add(i, GI);
                area.Add(GI);
            }
        }

       
        public StaticFilterItem FindItem(uint serial)
        {
            foreach (var slot in gridSlots)
                if (slot.Value.LocalSerial == serial)
                    return slot.Value;
            return null;
        }

        public void RebuildContainer(List<Item> filteredItems, string searchText = "", bool overrideSort = false)
        {
            foreach (var slot in gridSlots)
            {
                slot.Value.SetGridItem(null);
            }

            foreach (var spot in itemPositions)
            {
                Item i = World.Items.Get(spot.Value);
                if (i != null)
                    if (filteredItems.Contains(i) && (!overrideSort || itemLocks.Contains(spot.Value)))
                    {
                        if (spot.Key < gridSlots.Count)
                        {
                            gridSlots[spot.Key].SetGridItem(i);

                            if (itemLocks.Contains(spot.Value))
                                gridSlots[spot.Key].ItemGridLocked = true;

                            filteredItems.Remove(i);
                        }
                    }
            }

            foreach (Item i in filteredItems)
            {
                foreach (var slot in gridSlots)
                {
                    if (slot.Value.SlotItem != null)
                        continue;
                    slot.Value.SetGridItem(i);                   
                    break;
                }
            }

            foreach (var slot in gridSlots)
            {
                slot.Value.IsVisible = !(!string.IsNullOrWhiteSpace(searchText) && ProfileManager.CurrentProfile.GridContainerSearchMode == 0);
                if (slot.Value.SlotItem != null && !string.IsNullOrWhiteSpace(searchText))
                {
                    if (SearchItemNameAndProps(searchText, slot.Value.SlotItem))
                    {
                        slot.Value.Hightlight = ProfileManager.CurrentProfile.GridContainerSearchMode == 1;
                        slot.Value.IsVisible = true;
                    }
                }
            }
            SetGridPositions();            
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <returns>List of items matching the search result, or all items if search is blank/profile does has hide search mode disabled</returns>
        public List<GameObjectInfo> SearchResults(string search)
        {
            LoadItens(); 
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

        public void LoadItens()
        {
            gridContents = GetItemsInContainer();
        }

        public static List<GameObjectInfo> GetItemsInContainer()
        {
            List<GameObjectInfo> contents = [];

           
            return contents.OrderBy((x) => x.Graphic).ToList();
        }

        public int hcount = 0;        
    }
}
