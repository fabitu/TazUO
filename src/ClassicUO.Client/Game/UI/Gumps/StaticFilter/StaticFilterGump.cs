using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Data.Preferences;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static ClassicUO.Game.UI.Gumps.CounterBarGump;
using static ClassicUO.Game.UI.Gumps.GridContainer;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterGump : ResizableGump
    {
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 60;
        private const ushort LABELHUE = 0x0481;
        private const ushort TEXTHUE = 0x0000;
        #endregion

        #region private static vars
        public static int lastX = 100, lastY = 100, lastCorpseX = 100, lastCorpseY = 100;
        private static int _gridItemSize { get { return (int)Math.Round(50 * (ProfileManager.CurrentProfile.GridContainerScale / 100f)); } }
        public static int borderWidth = 4;
        public static int padding = 18;
        #endregion

        #region private readonly vars
        private AlphaBlendControl background;
        private StbTextBox graphicTextBox;
        private StbTextBox preferenceNameTextBox;
        private StbTextBox replaceToGraphicTextBox;
        private GumpPicTiled backgroundTexture;
        private StaticPic _replaceImage;
        #endregion
        #region private vars
        public Item container { get { return World.Items.Get(LocalSerial); } }
        private float lastGridItemScale = ProfileManager.CurrentProfile.GridContainerScale / 100f;
        private int lastWidth = GetWidth(), lastHeight = GetHeight();

        private GridScrollArea scrollArea;
        public StaticFilterSlotManager filterSlotManager;
        private ushort? Graphic;
        public List<StaticCustomItens> currentCustomItens;
        public PreferenceManagerBase preferenceManager;
        public int currentPos = 0;
        private static readonly int defaultColumns = 12;
        private static readonly int defaultRows = 3;
        private string Type;
        #endregion

        public StaticFilterGump(GameObject selectedObject) : base(GetWidth(), GetHeight(), 800, 500, 0, 0)
        {
            try
            {
                CanCloseWithEsc = true;
                AcceptMouseInput = true;
                EnsureSelectedObject(selectedObject);
                LoadFiles();
                GumpBuild();
            }
            catch (Exception ex)
            {
                GameActions.LogError(ex.Message);                
            }            
        }
        private void LoadFiles()
        {
            currentCustomItens = preferenceManager.LoadFile();
            for (int i = 0; i < currentCustomItens.Count; i++)
            {
                if (currentCustomItens[i].ToReplaceGraphicArray.Contains(Graphic.Value))
                    currentPos = i;
            }
        }
        public void ClearItens()
        {
            StaticFilterItem[] items = GetControls<StaticFilterItem>(scrollArea);
            foreach (var item in items)
            {
                item.Parent = null;
                item.Dispose();
            }
        }
        public void EnsureSelectedObject(GameObject selectedObject)
        {
            Graphic = selectedObject.Graphic;
            StaticTiles _itemData;
            if (selectedObject is Static stat)
            {
                _itemData = stat.ItemData;
            }
            else if (selectedObject is Item item)
            {
                _itemData = item.ItemData;
            }
            else
            {
                return;
            }

            if (_itemData.IsWall && !_itemData.IsDoor)
            {
                Type = "Wall";
                preferenceManager = StaticFilters.PreferencesWallMannager;
            }
            else if (_itemData.IsDoor)
            {
                Type = "Door";
                preferenceManager = StaticFilters.PreferencesDoorMannager;
            }

        }
        #region GumpBuild             
        private void GumpBuild()
        {
            CanMove = true;
            AcceptMouseInput = true;

            #region background
            BuildBackGround();
            #endregion

            #region TOP BAR AREA
            BuildPreferences();
            #endregion

            #region Scroll Area
            BuildScroolArea();
            #endregion              

            filterSlotManager = new StaticFilterSlotManager(scrollArea, this, _gridItemSize); //Must come after scroll area         
            BuildBorder();
            ResizeWindow(new Microsoft.Xna.Framework.Point(Width, Height));
            UpdateFields();
        }
        private void BuildPreferences()
        {
            var lblPreferenceName = new Label("Preference:", true, LABELHUE)
            {
                X = padding,
                Y = padding,
            };
            Add(lblPreferenceName);

            preferenceNameTextBox = new StbTextBox(1, 50, 80, true, FontStyle.None, TEXTHUE)
            {
                X = lblPreferenceName.EndXPos + 10,
                Y = padding,
                Multiline = false,
                Width = 80,
                Height = 20
            };
            preferenceNameTextBox.KeyUp += (sender, e) =>
            {
                if (e.Key == SDL2.SDL.SDL_Keycode.SDLK_RETURN)
                {
                    if (!string.IsNullOrEmpty(preferenceNameTextBox.Text))
                    {
                        currentCustomItens[currentPos].Description = preferenceNameTextBox.Text;
                        UpdateFields();
                    }
                }
            };
            preferenceNameTextBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = preferenceNameTextBox.Width,
                Height = preferenceNameTextBox.Height
            });
            Add(preferenceNameTextBox);

            NiceButton previous;
            Add(previous = new NiceButton(preferenceNameTextBox.EndXPos + 10, 18, 20, 20, ButtonAction.Default, "<", align: Assets.TEXT_ALIGN_TYPE.TS_LEFT, hue: LABELHUE));
            previous.SetTooltip("Previous");
            previous.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left && currentPos > 0)
                {
                    currentPos--;
                    ClearItens();
                    UpdateFields();
                }
            };

            NiceButton next;
            Add(next = new NiceButton(previous.EndXPos + 10, 18, 20, 20, ButtonAction.Default, ">", align: Assets.TEXT_ALIGN_TYPE.TS_LEFT, hue: LABELHUE));
            next.SetTooltip("Next");
            next.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left && currentPos < currentCustomItens.Count)
                {
                    currentPos++;
                    ClearItens();
                    UpdateFields();
                }
            };

            NiceButton add;
            Add(add = new NiceButton(X = next.EndXPos + 20, 18, 25, 20, ButtonAction.Default, "New", align: Assets.TEXT_ALIGN_TYPE.TS_CENTER, hue: LABELHUE));
            add.SetTooltip("Add iten to static filter.");
            add.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left)
                {
                    if (!currentCustomItens.Any(X => X.Description == preferenceNameTextBox.Text))
                    {
                        currentCustomItens.Add(new StaticCustomItens() { Description = preferenceNameTextBox.Text });
                        currentPos = currentCustomItens.Count - 1;
                        ClearItens();
                        UpdateFields();
                    }
                }
            };

            var lblReplaceTo = new Label("ReplaceTo:", true, LABELHUE)
            {
                X = add.EndXPos + 100,
                Y = padding,
            };
            Add(lblReplaceTo);

            replaceToGraphicTextBox = new StbTextBox(1, 50, 80, true, FontStyle.None, TEXTHUE)
            {
                X = lblReplaceTo.EndXPos + 10,
                Y = padding,
                Multiline = false,
                Width = 80,
                Height = 20
            };
            replaceToGraphicTextBox.KeyUp += (sender, e) =>
            {
                if (e.Key == SDL2.SDL.SDL_Keycode.SDLK_RETURN)
                {
                    if (!string.IsNullOrEmpty(replaceToGraphicTextBox.Text))
                    {
                        var graphic = (ushort)Convert.ToInt32(replaceToGraphicTextBox.Text);
                        currentCustomItens[currentPos].ReplaceToGraphic = graphic;
                        UpdateFields();
                    }
                }
            };
            replaceToGraphicTextBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = replaceToGraphicTextBox.Width,
                Height = replaceToGraphicTextBox.Height
            });
            Add(replaceToGraphicTextBox);

            NiceButton save;
            Add(save = new NiceButton(replaceToGraphicTextBox.EndXPos + 100, 18, 50, 20, ButtonAction.Default, "Save", align: Assets.TEXT_ALIGN_TYPE.TS_CENTER, hue: LABELHUE));
            save.SetTooltip("Save static file.");
            save.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left)
                {
                    preferenceManager.SavePreferences(currentCustomItens);
                    preferenceManager.ReloadPreferences();
                }
            };

            var lblToReplace = new Label("ToReplace:", true, LABELHUE)
            {
                X = padding,
                Y = 44,
            };
            Add(lblToReplace);

            graphicTextBox = new StbTextBox(1, 50, 80, true, FontStyle.None, TEXTHUE)
            {
                X = lblToReplace.EndXPos + 10,
                Y = 44,
                Multiline = false,
                Width = 80,
                Height = 20
            };
            graphicTextBox.KeyUp += (sender, e) =>
            {
                if (e.Key == SDL2.SDL.SDL_Keycode.SDLK_RETURN)
                {
                    if (!string.IsNullOrEmpty(graphicTextBox.Text))
                    {
                        var graphic = (ushort)Convert.ToInt32(graphicTextBox.Text);
                        if (!currentCustomItens[currentPos].ToReplaceGraphicArray.Contains(graphic))
                            currentCustomItens[currentPos].ToReplaceGraphicArray.Add(graphic);
                        UpdateFields();
                    }
                }
            };
            graphicTextBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = graphicTextBox.Width,
                Height = graphicTextBox.Height
            });
            Add(graphicTextBox);
        }
        protected override void UpdateContents()
        {
            if (InvalidateContents && !IsDisposed && IsVisible)
            {
                UpdateFields();
            }
        }
        public void BuildBorder()
        {
            int graphic = 9260, borderSize = 17;

            if ((BorderStyle)ProfileManager.CurrentProfile.Grid_BorderStyle != BorderStyle.Default)
            {
                BorderControl.T_Left = (ushort)graphic;
                BorderControl.H_Border = (ushort)(graphic + 1);
                BorderControl.T_Right = (ushort)(graphic + 2);
                BorderControl.V_Border = (ushort)(graphic + 3);

                backgroundTexture.Graphic = (ushort)(graphic + 4);
                backgroundTexture.IsVisible = true;
                backgroundTexture.Hue = background.Hue;
                BorderControl.Hue = background.Hue;
                BorderControl.Alpha = background.Alpha;
                background.IsVisible = false;

                BorderControl.V_Right_Border = (ushort)(graphic + 5);
                BorderControl.B_Left = (ushort)(graphic + 6);
                BorderControl.H_Bottom_Border = (ushort)(graphic + 7);
                BorderControl.B_Right = (ushort)(graphic + 8);
                BorderControl.BorderSize = borderSize;
                borderWidth = borderSize;
            }
            UpdateUIPositions();
            OnResize();

            BorderControl.IsVisible = !ProfileManager.CurrentProfile.Grid_HideBorder;
        }
        private void UpdateUIPositions()
        {
            background.X = padding;
            background.Y = padding;
            scrollArea.X = background.X;
            scrollArea.Y = TOP_BAR_HEIGHT + background.Y;
            backgroundTexture.X = background.X;
            backgroundTexture.Y = background.Y;
            backgroundTexture.Width = Width - padding * 2;
            backgroundTexture.Height = Height - padding * 2;
            background.Width = Width - padding * 2;
            background.Height = Height - padding * 2;
            scrollArea.Width = background.Width;
            scrollArea.Height = background.Height - TOP_BAR_HEIGHT;
        }
        private void BuildScroolArea()
        {
            scrollArea = new GridScrollArea(
                            background.X,
                            TOP_BAR_HEIGHT + background.Y,
                            background.Width,
                            background.Height - 50
                            );

            scrollArea.MouseUp += ScrollArea_MouseUp;
            Add(scrollArea);
        }
        private void BuildBackGround()
        {
            background = new AlphaBlendControl()
            {
                Width = Width - padding * 2,
                Height = Height - padding * 2,
                X = padding,
                Y = padding,
                Alpha = (float)ProfileManager.CurrentProfile.ContainerOpacity / 100,
                Hue = ProfileManager.CurrentProfile.AltGridContainerBackgroundHue
            };
            Add(background);

            backgroundTexture = new GumpPicTiled(0);
            Add(backgroundTexture);
        }
        #endregion GumpBuild       
        private static int GetWidth(int columns = -1)
        {
            if (columns < 0)
                columns = defaultColumns;
            return borderWidth * 2     //The borders around the container, one on the left and one on the right
            + 15                   //The width of the scroll bar
            + _gridItemSize * columns //How many items to fit in left to right
            + X_SPACING * columns;      //Spacing between each grid item(x columns)
        }
        private static int GetHeight(int rows = -1)
        {
            if (rows < 0)
                rows = defaultRows;
            return TOP_BAR_HEIGHT + borderWidth * 2 + (_gridItemSize + Y_SPACING) * rows;
        }

        public void UpdateFields()
        {
            if (currentPos <= currentCustomItens.Count)
            {
                filterSlotManager.GridSlots.Clear();
                filterSlotManager.slots = 0;
                EnsureItens();
            }
            filterSlotManager.SetGridPositions();
        }

        private void EnsureItens()
        {
            var wall = currentCustomItens[currentPos];
            preferenceNameTextBox.Text = wall.Description;
            replaceToGraphicTextBox.Text = Convert.ToString(wall.ReplaceToGraphic);

            _replaceImage?.Dispose();
            if (!string.IsNullOrEmpty(replaceToGraphicTextBox.Text))
            {
                _replaceImage = new StaticPic((ushort)Convert.ToInt32(replaceToGraphicTextBox.Text), 0)
                {
                    UseBorder = true,
                    X = replaceToGraphicTextBox.EndXPos + 10,
                    Y = replaceToGraphicTextBox.Y
                };
                Add(_replaceImage);
            }

            foreach (var graphic in wall.ToReplaceGraphicArray)
            {
                filterSlotManager.AddItem(graphic);
            }
        }

        private void ScrollArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtonType.Left && scrollArea.MouseIsOver)
            {
                if (Client.Game.GameCursor.ItemHold.Enabled)
                {
                    GameActions.DropItem(Client.Game.GameCursor.ItemHold.Serial, 0xFFFF, 0xFFFF, 0, LocalSerial);
                }
                else if (TargetManager.IsTargeting)
                {
                    TargetManager.Target(LocalSerial);
                }
            }
            else if (e.Button == MouseButtonType.Right)
            {
                InvokeMouseCloseGumpWithRClick();
            }
        }
        public override void Update()
        {
            base.Update();

            if (IsDisposed)
                return;

            if (lastWidth != Width || lastHeight != Height || lastGridItemScale != _gridItemSize)
            {
                lastGridItemScale = _gridItemSize;
                background.Width = Width - borderWidth * 2;
                background.Height = Height - borderWidth * 2;
                scrollArea.Width = background.Width;
                scrollArea.Height = background.Height - TOP_BAR_HEIGHT;
                lastHeight = Height;
                lastWidth = Width;
                graphicTextBox.Width = Math.Min(Width - borderWidth * 2, 150);
                backgroundTexture.Width = background.Width;
                backgroundTexture.Height = background.Height;
                backgroundTexture.Alpha = background.Alpha;
                backgroundTexture.Hue = background.Hue;

                RequestUpdateContents();
            }
        }
        public override void Dispose()
        {
            preferenceManager.ReloadPreferences();
            base.Dispose();
        }
    }
    public class GameObjectInfo()
    {
        public string Name { get; set; }
        public ushort Graphic { get; set; }
        public TileFlag Flags;
    }
}
