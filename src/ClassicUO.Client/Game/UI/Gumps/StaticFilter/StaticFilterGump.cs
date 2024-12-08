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
using static ClassicUO.Game.UI.Gumps.GridContainer;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterGump : ResizableGump
    {
        #region Mannagers
        private PreferenceManagerBase currentPreferenceMannager;
        private readonly PreferenceManagerBase _wallMannager = new PreferenceWallManager();
        private readonly PreferenceManagerBase _doorsMannager = new PreferenceDoorMannager();
        #endregion
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 60;
        private ushort hue = 39;
        #endregion

        #region private static vars
        public static int lastX = 100, lastY = 100, lastCorpseX = 100, lastCorpseY = 100;
        public static int gridItemSize { get { return (int)Math.Round(50 * (ProfileManager.CurrentProfile.GridContainerScale / 100f)); } }
        public static int borderWidth = 4;
        #endregion

        #region private readonly vars
        private AlphaBlendControl background;
        private StbTextBox graphicTextBox;
        private StbTextBox preferenceNameTextBox;
        private StbTextBox replaceGraphicTextBox;
        private GumpPicTiled backgroundTexture;
        #endregion
        #region private vars
        public Item container { get { return World.Items.Get(LocalSerial); } }
        private float lastGridItemScale = ProfileManager.CurrentProfile.GridContainerScale / 100f;
        private int lastWidth = GetWidth(), lastHeight = GetHeight();

        private GridScrollArea scrollArea;
        public StaticFilterSlotManager filterSlotManager;
        private ushort? Graphic;
        List<StaticCustomItens> Walls;
        private int currentPos = 0;
        private string Type;
        #endregion

        public StaticFilterGump(GameObject selectedObject) : base(GetWidth(), GetHeight(), 500, 500, 0, 0)
        {
            CanCloseWithEsc = true;
            AcceptMouseInput = true;
            EnsureSelectedObject(selectedObject);
            LoadFiles();
            GumpBuild();
        }

        private void LoadFiles()
        {
            Walls = _wallMannager.LoadFile();
            for (int i = 0; i < Walls.Count; i++)
            {
                if (Walls[i].ToReplaceGraphicArray.Contains(Graphic.Value))
                    currentPos = i;
            }
        }
        public void EnsureSelectedObject(GameObject selectedObject)
        {
            Graphic = selectedObject.Graphic;
            if (selectedObject is Static stat)
            {
                if (stat.ItemData.IsWall)
                    Type = "Wall";
                if (stat.ItemData.IsDoor)
                    Type = "Door";
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

            BuildGraphicText();
            BuildAddStatic();
            BuildSaveStatic();
            #endregion

            #region Scroll Area
            BuildScroolArea();
            #endregion      

            #region Add controlse
            AddControls();
            #endregion

            filterSlotManager = new StaticFilterSlotManager(Graphic.Value, scrollArea); //Must come after scroll area         
            BuildBorder();
            ResizeWindow(new Microsoft.Xna.Framework.Point(Width, Height));
        }
        private void BuildPreferences()
        {
            NiceButton previous;
            Add(previous = new NiceButton(20, 18, 20, 20, ButtonAction.Default, "<", align: Assets.TEXT_ALIGN_TYPE.TS_LEFT, hue: hue));
            previous.SetTooltip("Previous");
            previous.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left && currentPos > 0)
                {
                    currentPos--;
                }
            };
            preferenceNameTextBox = new StbTextBox(1, 50, 50, true, FontStyle.None, 0x0481)
            {
                X = previous.X + 30,
                Y = previous.Y,
                Multiline = false,
                Width = 50,
                Height = 20
            };
            preferenceNameTextBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = preferenceNameTextBox.Width,
                Height = preferenceNameTextBox.Height
            });

            NiceButton next;
            Add(next = new NiceButton(preferenceNameTextBox.X + 30, 18, 20, 20, ButtonAction.Default, ">", align: Assets.TEXT_ALIGN_TYPE.TS_LEFT, hue: hue));
            next.SetTooltip("Next");
            next.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left && currentPos < Walls.Count)
                {
                    currentPos++;
                }
            };

            replaceGraphicTextBox = new StbTextBox(1, 50, 50, true, FontStyle.None, 0x0481)
            {
                X = 10,
                Y = previous.Y + 25,
                Multiline = false,
                Width = 50,
                Height = 20
            };
            replaceGraphicTextBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = replaceGraphicTextBox.Width,
                Height = replaceGraphicTextBox.Height
            });
        }
        private void BuildAddStatic()
        {
            NiceButton add;
            Add(add = new NiceButton(410, 18, 25, 20, ButtonAction.Default, "Add", align: Assets.TEXT_ALIGN_TYPE.TS_CENTER, hue: hue));
            add.SetTooltip("Add iten to static filter.");
            add.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left)
                {
                    if (!Walls.Any(X => X.Description == preferenceNameTextBox.Text))
                    {
                        Walls.Add(new StaticCustomItens() { Type = "Wall", Description = preferenceNameTextBox.Text });
                        currentPos = Walls.Count;
                        UpdateFields();
                    }
                }
            };
        }
        private void BuildSaveStatic()
        {
            NiceButton add;
            Add(add = new NiceButton(440, 18, 50, 20, ButtonAction.Default, "Save", align: Assets.TEXT_ALIGN_TYPE.TS_CENTER, hue: hue));
            add.SetTooltip("Save static file.");
            add.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtonType.Left)
                {
                    _wallMannager.SavePreferences(Walls);
                    _wallMannager.ReloadPreferences();
                }
            };
        }
        private void BuildGraphicText()
        {
            graphicTextBox = new StbTextBox(1, 50, 100, true, FontStyle.None, 0x0481)
            {
                X = borderWidth,
                Y = borderWidth,
                Multiline = false,
                Width = 100,
                Height = 20
            };
            graphicTextBox.KeyUp += (sender, e) =>
            {
                if (e.Key == SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER)
                {
                    if (!string.IsNullOrEmpty(graphicTextBox.Text))
                    {
                        var graphic = (ushort)Convert.ToInt32(graphicTextBox.Text);
                        filterSlotManager.AddItem(graphic);
                        Walls[currentPos].ToReplaceGraphicArray.Add(graphic);
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
        }
        protected override void UpdateContents()
        {
            if (InvalidateContents && !IsDisposed && IsVisible)
            {
                UpdateFields();
                GridContainerManager.UpdateAllContainers();
            }
        }
        private void AddControls()
        {
            Add(background);
            Add(backgroundTexture);
            Add(preferenceNameTextBox);
            //Add(replaceGraphicTextBox);
            Add(graphicTextBox);
            Add(scrollArea);
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
            background.X = borderWidth;
            background.Y = borderWidth;
            scrollArea.X = background.X;
            scrollArea.Y = TOP_BAR_HEIGHT + background.Y;
            graphicTextBox.Y = borderWidth;
            graphicTextBox.X = borderWidth;
            backgroundTexture.X = background.X;
            backgroundTexture.Y = background.Y;
            backgroundTexture.Width = Width - borderWidth * 2;
            backgroundTexture.Height = Height - borderWidth * 2;
            background.Width = Width - borderWidth * 2;
            background.Height = Height - borderWidth * 2;
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
        }
        private void BuildBackGround()
        {
            background = new AlphaBlendControl()
            {
                Width = Width - borderWidth * 2,
                Height = Height - borderWidth * 2,
                X = borderWidth,
                Y = borderWidth,
                Alpha = (float)ProfileManager.CurrentProfile.ContainerOpacity / 100,
                Hue = ProfileManager.CurrentProfile.AltGridContainerBackgroundHue
            };

            backgroundTexture = new GumpPicTiled(0);
        }

        #endregion GumpBuild
        public override void Update()
        {
            base.Update();

            if (IsDisposed)
                return;

            if (lastWidth != Width || lastHeight != Height || lastGridItemScale != gridItemSize)
            {
                lastGridItemScale = gridItemSize;
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
        public void UpdateFields()
        {
            if (Walls.Count <= currentPos)
            {
                filterSlotManager.gridSlots.Clear();
                var wall = Walls[currentPos];
                preferenceNameTextBox.Text = wall.Description;

                foreach (var graphic in wall.ToReplaceGraphicArray)
                {
                    filterSlotManager.AddItem(graphic);
                }
            }
            filterSlotManager.SetGridPositions();
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
        public override void Dispose()
        {
            _wallMannager.ReloadPreferences();
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
