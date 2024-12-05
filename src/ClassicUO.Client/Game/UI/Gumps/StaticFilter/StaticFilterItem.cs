using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static ClassicUO.Game.UI.Gumps.GridContainer;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterItem : Control
    {
        private readonly HitBox hit;
        private bool mousePressedWhenEntered = false;
        private readonly GameObjectInfo objectInfo;
        private Item _item;
        //public readonly StaticFilterGump _gump;
        public bool ItemGridLocked = false;
        private readonly int slot;
        Label count;
        Label weight;
        AlphaBlendControl background;
        private CustomToolTip toolTipThis, toolTipitem1, toolTipitem2;

        private bool IsHighlight = false;
        private ushort borderHighlightHue = 0;

        public bool Hightlight = false;
        public bool SelectHighlight = false;   
        public GameObjectInfo SlotItem { get; set ; }
        private readonly int[] spellbooks = { 0x0EFA, 0x2253, 0x2252, 0x238C, 0x23A0, 0x2D50, 0x2D9D, 0x225A };
        public StaticFilterItem(uint serial, int size, GameObjectInfo objectInfo, StaticFilterGump staticFilter, int slot)
        {
            #region VARS
            this.slot = slot;
            this.objectInfo = objectInfo;
            //_gump = staticFilter;
            LocalSerial = serial;
            _item = World.Items.Get(serial);
            
            if (_item != null)
            {
                ref readonly var text = ref Client.Game.Arts.GetArt(_item.DisplayedGraphic);
                texture = text.Texture;
                bounds = text.UV;

                rect = Client.Game.Arts.GetRealArtBounds(_item.DisplayedGraphic);
            }
            #endregion

            background = new AlphaBlendControl(0.25f);
            background.Width = size;
            background.Height = size;
            Width = Height = size;
            Add(background);

            hit = new HitBox(0, 0, size, size, null, 0f);
            Add(hit);

            SetGridItem(_item);

            hit.MouseEnter += _hit_MouseEnter;
            hit.MouseExit += _hit_MouseExit;
            hit.MouseUp += _hit_MouseUp;
            hit.MouseDoubleClick += _hit_MouseDoubleClick;
        }
        public void SetHighLightBorder(ushort hue)
        {
            IsHighlight = hue == 0 ? false : true;
            borderHighlightHue = hue;
        }
        public void Resize()
        {
            Width = gridItemSize;
            Height = gridItemSize;
            hit.Width = gridItemSize;
            hit.Height = gridItemSize;
            background.Width = gridItemSize;
            background.Height = gridItemSize;
        }
        public void SetGridItem(Item item)
        {
            if (item == null)
            {
                _item = null;
                LocalSerial = 0;
                hit.ClearTooltip();
                Hightlight = false;
                count?.Dispose();
                count = null;
                ItemGridLocked = false;
            }
            else
            {
                _item = item;
                ref readonly var text = ref Client.Game.Arts.GetArt(_item.DisplayedGraphic);
                texture = text.Texture;
                bounds = text.UV;

                rect = Client.Game.Arts.GetRealArtBounds(_item.DisplayedGraphic);

                LocalSerial = item.Serial;
                //EP: Set Stackable count
                int itemAmt = 0;
                double stones = 0;

                if (_item.ItemData.IsStackable)
                {
                    itemAmt = item.Amount;
                }
                //EP: Set Container inside count

                string rawProp = ReadProperties(_item.Serial, out string htmlText);
                GameActions.Log($"Name {_item.Name} {rawProp}");
                var prop = rawProp.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (prop.Length > 0)
                {
                    foreach (var propLine in prop)
                    {
                        var tItens = MatchCountAndStones(propLine);
                        if (!_item.ItemData.IsStackable)
                            itemAmt = tItens.Item1;
                        stones = tItens.Item2;
                    }
                }
                if (_item.ItemData.IsStackable && itemAmt > 1 || _item.ItemData.IsContainer && itemAmt > 0)
                {
                    count?.Dispose();
                    count = new Label(itemAmt.ToString(), true, 0x0481)
                    {
                        X = 1
                    };
                    count.Y = Height - count.Height;
                }
                if ((_item.ItemData.IsStackable || _item.ItemData.IsContainer) && stones != 0.00)
                {
                    var hue = GetHue(stones);
                    weight?.Dispose();
                    weight = new Label(stones.ToString(), true, hue)
                    {
                        Y = 1,
                        X = 1
                    };
                }

                if (MultiItemMoveGump.MoveItems.Contains(_item))
                    Hightlight = true;
                hit.SetTooltip(_item);
            }
        }
        private ushort GetHue(double stones)
        {
            var percent = ((World.Player.Weight + stones) / World.Player.WeightMax) * 100;
            if (percent < 50) //green
                return 68;
            else if (percent < 70) //yelow
                return 54;
            else if (percent < 99) //orange
                return 44;
            return 38; //red
        }
        private Tuple<int, double> MatchCountAndStones(string text)
        {
            // Regex pattern to extract the numbers
            string pattern = @"(\d+)\s?(Items?|Item),\s?(\d+\.\d+|\d+)\s?(Stones?|Stone)";

            // Create the regex object
            Regex regex = new Regex(pattern);

            // Match the text using the regex pattern
            Match match = regex.Match(text);

            // If a match is found, extract the numbers
            if (match.Success)
            {
                var itemsNumber = Convert.ToInt32(match.Groups[1].Value);   // Number for Items
                float.TryParse(match.Groups[3].Value, out float stonesNumber);  // Number for Stones
                                                                                // 
                return new Tuple<int, double>(itemsNumber, Math.Round(stonesNumber, 1));
            }
            return new Tuple<int, double>(0, 0.00);
        }

        private bool MatchText(string text, string pattern)
        {

            // Create the regex object
            Regex regex = new(pattern);

            // Match the text using the regex pattern
            Match match = regex.Match(text);

            // If a match is found, extract the numbers
            return match.Success;
        }

        private string ReadProperties(uint serial, out string htmltext)
        {
            bool hasStartColor = false;

            string result = null;
            htmltext = string.Empty;

            if (SerialHelper.IsValid(serial) && World.OPL.TryGetNameAndData(serial, out string name, out string data))
            {
                ValueStringBuilder sbHTML = new ValueStringBuilder();
                {
                    ValueStringBuilder sb = new ValueStringBuilder();
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (SerialHelper.IsItem(serial))
                            {
                                sbHTML.Append("<basefont color=\"yellow\">");
                                hasStartColor = true;
                            }
                            else
                            {
                                Mobile mob = World.Mobiles.Get(serial);

                                if (mob != null)
                                {
                                    sbHTML.Append(Notoriety.GetHTMLHue(mob.NotorietyFlag));
                                    hasStartColor = true;
                                }
                            }

                            sb.Append(name);
                            sbHTML.Append(name);

                            if (hasStartColor)
                            {
                                sbHTML.Append("<basefont color=\"#FFFFFFFF\">");
                            }
                        }

                        if (!string.IsNullOrEmpty(data))
                        {
                            sb.Append('\n');
                            sb.Append(data);
                            sbHTML.Append('\n');
                            sbHTML.Append(data);
                        }

                        htmltext = sbHTML.ToString();
                        result = sb.ToString();

                        sb.Dispose();
                        sbHTML.Dispose();
                    }
                }
            }
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }
        private void _hit_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
        {
           
        }
        private void _hit_MouseUp(object sender, MouseEventArgs e)
        {
          
        }
        private void _hit_MouseExit(object sender, MouseEventArgs e)
        {
            
        }
        private void _hit_MouseEnter(object sender, MouseEventArgs e)
        {
           
        }

        private Texture2D texture;
        private Rectangle rect;
        private Rectangle bounds;

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (_item != null && _item.ItemData.Layer > 0 && hit.MouseIsOver && Keyboard.Ctrl && (toolTipThis == null || toolTipThis.IsDisposed) && (toolTipitem1 == null || toolTipitem1.IsDisposed) && (toolTipitem2 == null || toolTipitem2.IsDisposed))
            {
                Item compItem = World.Player.FindItemByLayer((Layer)_item.ItemData.Layer);
                if (compItem != null && (Layer)_item.ItemData.Layer != Layer.Backpack)
                {
                    hit.ClearTooltip();
                    List<CustomToolTip> toolTipList = new List<CustomToolTip>();
                    toolTipThis = new CustomToolTip(_item, Mouse.Position.X + 5, Mouse.Position.Y + 5, hit, compareTo: compItem);
                    toolTipList.Add(toolTipThis);
                    toolTipitem1 = new CustomToolTip(compItem, toolTipThis.X + toolTipThis.Width + 10, toolTipThis.Y, hit, "<basefont color=\"orange\">Equipped Item<br>");
                    toolTipList.Add(toolTipitem1);

                    if (CUOEnviroment.Debug)
                    {
                        ItemPropertiesData i1 = new ItemPropertiesData(_item);
                        ItemPropertiesData i2 = new ItemPropertiesData(compItem);

                        if (i1.GenerateComparisonTooltip(i2, out string compileToolTip))
                            GameActions.Print(compileToolTip);
                    }

                    if ((Layer)_item.ItemData.Layer == Layer.OneHanded)
                    {
                        Item compItem2 = World.Player.FindItemByLayer(Layer.TwoHanded);
                        if (compItem2 != null)
                        {
                            toolTipitem2 = new CustomToolTip(compItem2, toolTipitem1.X + toolTipitem1.Width + 10, toolTipitem1.Y, hit, "<basefont color=\"orange\">Equipped Item<br>");
                            //UIManager.Add(toolTipitem2);
                            toolTipList.Add(toolTipitem2);
                        }
                    }
                    else if ((Layer)_item.ItemData.Layer == Layer.TwoHanded)
                    {
                        Item compItem2 = World.Player.FindItemByLayer(Layer.OneHanded);
                        if (compItem2 != null)
                        {
                            toolTipitem2 = new CustomToolTip(compItem2, toolTipitem1.X + toolTipitem1.Width + 10, toolTipitem1.Y, hit, "<basefont color=\"orange\">Equipped Item<br>");
                            //UIManager.Add(toolTipitem2);
                            toolTipList.Add(toolTipitem2);
                        }
                    }

                    MultipleToolTipGump multipleToolTipGump = new MultipleToolTipGump(Mouse.Position.X + 10, Mouse.Position.Y + 10, toolTipList.ToArray(), hit);
                    UIManager.Add(multipleToolTipGump);
                }
            }

            if (SelectHighlight)
                if (!MultiItemMoveGump.MoveItems.Contains(_item))
                    SelectHighlight = false;

            base.Draw(batcher, x, y);

            Vector3 hueVector;

            hueVector = ShaderHueTranslator.GetHueVector(ProfileManager.CurrentProfile.GridBorderHue, false, (float)ProfileManager.CurrentProfile.GridBorderAlpha / 100);

            if (ItemGridLocked)
                hueVector = ShaderHueTranslator.GetHueVector(0x2, false, (float)ProfileManager.CurrentProfile.GridBorderAlpha / 100);
            if (Hightlight || SelectHighlight)
            {
                hueVector = ShaderHueTranslator.GetHueVector(0x34, false, 1);
            }

            //Borders
            batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.White), x, y, Width, Height, hueVector);

            if (IsHighlight)
            {
                int bsize = ProfileManager.CurrentProfile.GridHightlightSize;
                Texture2D borderTexture = SolidColorTextureCache.GetTexture(Color.White);
                Vector3 borderHueVec = ShaderHueTranslator.GetHueVector(borderHighlightHue, false, 2.0f);

                batcher.Draw(borderTexture, new Rectangle(x, y, Height, Width), borderHueVec);
            }

            //Selected Item
            if (hit.MouseIsOver && _item != null)
            {
                hueVector.Z = 0.3f;
                //Over
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(x + 1, y, Width - 1, Height), hueVector);
            }

            if (_item != null && texture != null & rect != null)
            {
                hueVector = ShaderHueTranslator.GetHueVector(_item.Hue, _item.ItemData.IsPartialHue, 1f);

                Point originalSize = new Point(hit.Width, hit.Height);
                Point point = new Point();
                var scale = ProfileManager.CurrentProfile.GridContainerScale / 100f;

                if (rect.Width < hit.Width)
                {
                    if (ProfileManager.CurrentProfile.GridContainerScaleItems)
                        originalSize.X = (ushort)(rect.Width * scale);
                    else
                        originalSize.X = rect.Width;

                    point.X = (hit.Width >> 1) - (originalSize.X >> 1);
                }
                else if (rect.Width > hit.Width)
                {
                    if (ProfileManager.CurrentProfile.GridContainerScaleItems)
                        originalSize.X = (ushort)(hit.Width * scale);
                    else
                        originalSize.X = hit.Width;
                    point.X = (hit.Width >> 1) - (originalSize.X >> 1);
                }

                if (rect.Height < hit.Height)
                {
                    if (ProfileManager.CurrentProfile.GridContainerScaleItems)
                        originalSize.Y = (ushort)(rect.Height * scale);
                    else
                        originalSize.Y = rect.Height;

                    point.Y = (hit.Height >> 1) - (originalSize.Y >> 1);
                }
                else if (rect.Height > hit.Height)
                {
                    if (ProfileManager.CurrentProfile.GridContainerScaleItems)
                        originalSize.Y = (ushort)(hit.Height * scale);
                    else
                        originalSize.Y = hit.Height;

                    point.Y = (hit.Height >> 1) - (originalSize.Y >> 1);
                }

                batcher.Draw
                (
                    texture,
                    new Rectangle(x + point.X, y + point.Y + hit.Y, originalSize.X, originalSize.Y),//texture
                    new Rectangle(bounds.X + rect.X, bounds.Y + rect.Y, rect.Width, rect.Height),
                    hueVector
                );
                count?.Draw(batcher, x + count.X, y + count.Y);
                weight?.Draw(batcher, x + weight.X, y + weight.Y);
            }
            return true;
        }
    }
}
