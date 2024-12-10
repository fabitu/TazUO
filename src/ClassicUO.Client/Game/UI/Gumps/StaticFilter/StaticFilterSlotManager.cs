using ClassicUO.Game.UI.Controls;
using System.Collections.Generic;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterSlotManager(Control controlArea, StaticFilterGump parent, int gridItemSize)
    {
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        //private const int TOP_BAR_HEIGHT = 60;
        #endregion

        private readonly Control Area = controlArea;
        public int slots = 0;
        private readonly int GridItemSize = gridItemSize;
        private readonly StaticFilterGump parent = parent;
        public Dictionary<int, StaticFilterItem> GridSlots = [];

        public void AddItem(ushort graphic)
        {
            StaticFilterItem GI = new(graphic, slots, GridItemSize, this);
            if (GI.texture == null)
                return;
            GridSlots.Add(slots, GI);
            Area.Add(GI);
            slots++;
        }
        public void RemoveItem(int slot)
        {
            try
            {
                for (int i = 0; i < GridSlots.Count; i++)
                {
                    if (i == slot)
                    {
                        if (parent.Walls[parent.currentPos].ToReplaceGraphicArray.Contains(GridSlots[i].Graphic))
                            parent.Walls[parent.currentPos].ToReplaceGraphicArray.Remove(GridSlots[i].Graphic);
                    }
                    if (i > slot && i > 0)
                    {
                        GridSlots[i - 1] = GridSlots[i];
                    }
                    if (i == GridSlots.Count)
                    {
                        Area.Remove(GridSlots[i]);
                        GridSlots[i].Dispose();
                        GridSlots.Remove(i);
                        slots--;
                    }                   
                }
                parent.ClearItens();
                SetGridPositions();
            }
            catch
            {
            }
        }
        /// <summary>
        /// Set the visual grid items to the current GridSlots dict
        /// </summary>
        public void SetGridPositions()
        {
            int i = 0;
            int x = X_SPACING, y = 0;
            foreach (var slot in GridSlots)
            {
                if (x + GridItemSize >= Area.Width - 14) //14 is the scroll bar width
                {
                    x = X_SPACING;
                    y += GridItemSize + Y_SPACING;
                }
                slot.Value.X = x;
                slot.Value.Y = y;
                slot.Value.Resize();
                x += GridItemSize + X_SPACING;
                i++;
            }
        }
    }
}
