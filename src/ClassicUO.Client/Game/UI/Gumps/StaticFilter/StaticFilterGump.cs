using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using Microsoft.Xna.Framework;
using System;
using static ClassicUO.Game.UI.Gumps.GridContainer;

namespace ClassicUO.Game.UI.Gumps.StaticFilter
{
    internal class StaticFilterGump : ResizableGump
    {
        #region CONSTANTS
        private const int X_SPACING = 1, Y_SPACING = 1;
        private const int TOP_BAR_HEIGHT = 20;
        #endregion

        #region private static vars
        public static int lastX = 100, lastY = 100, lastCorpseX = 100, lastCorpseY = 100;
        public static int gridItemSize { get { return (int)Math.Round(50 * (ProfileManager.CurrentProfile.GridContainerScale / 100f)); } }
        public static int borderWidth = 4;
        #endregion

        #region private readonly vars
        private AlphaBlendControl background;
        private StbTextBox searchBox;
        private GumpPicTiled backgroundTexture;
        #endregion
        #region private vars
        public Item container { get { return World.Items.Get(LocalSerial); } }
        private float lastGridItemScale = ProfileManager.CurrentProfile.GridContainerScale / 100f;
        private int lastWidth = GetWidth(), lastHeight = GetHeight();

        private GridScrollArea scrollArea;
        public StaticFilterSlotManager filterSlotManager;
        private BaseGameObject _seletedObject;
        private GameObjectInfo _gameObjectInfo;
        #endregion

        public StaticFilterGump(BaseGameObject seletedObject) : base(GetWidth(), GetHeight(), 500, 500, 0, 0)
        {
            CanMove = true;
            AcceptMouseInput = true;
            _seletedObject = seletedObject;
            EnsureSelectedObject();
            GumpBuild();
        }
        private void EnsureSelectedObject()
        {
            if (SelectedObject.Object != null)
            {
                var gameObjectInfo = new GameObjectInfo
                {
                    Name = SelectedObject.Object?.GetType().Name
                };

                if (SelectedObject.Object is Land land)
                {
                    gameObjectInfo.Flags = land.TileData.Flags;
                    gameObjectInfo.Graphic = land.Graphic;
                }
                else if (SelectedObject.Object is Static stat)
                {
                    gameObjectInfo.Flags = stat.ItemData.Flags;
                    gameObjectInfo.Graphic = stat.Graphic;
                }
                else if (SelectedObject.Object is Item item)
                {
                    gameObjectInfo.Flags = item.ItemData.Flags;
                    gameObjectInfo.Graphic = item.Graphic;
                }
                _gameObjectInfo = gameObjectInfo;
            }
            else
            {
                _gameObjectInfo = null;
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
            BuildSearchBox();
            #endregion

            #region Scroll Area
            BuildScroolArea();
            #endregion      

            #region Add controlse
            AddControls();
            #endregion

            filterSlotManager = new StaticFilterSlotManager(this, scrollArea); //Must come after scroll area

            BuildBorder();
            ResizeWindow(new Point(Width, Height));
        }
        private void AddControls()
        {
            Add(background);
            Add(backgroundTexture);
            Add(searchBox);
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
            searchBox.Y = borderWidth;
            searchBox.X = borderWidth;
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
                            background.Height - 21
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
        private void BuildSearchBox()
        {
            searchBox = new StbTextBox(0xFF, 20, 150, true, FontStyle.None, 0x0481)
            {
                X = borderWidth,
                Y = borderWidth,
                Multiline = false,
                Width = 150,
                Height = 20
            };
            searchBox.TextChanged += (sender, e) => { UpdateItems(); };

            searchBox.Add(new AlphaBlendControl(0.5f)
            {
                Hue = 0x0481,
                Width = searchBox.Width,
                Height = searchBox.Height
            });
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
                searchBox.Width = Math.Min(Width - borderWidth * 2, 150);
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
        public void UpdateItems(bool overrideSort = false)
        {
            //List<Item> sortedContents = ProfileManager.CurrentProfile is null || ProfileManager.CurrentProfile.GridContainerSearchMode == 0 ?
            //    gridSlotManager.SearchResults(searchBox.Text) :
            //    GridSlotManager.GetItemsInContainer(container);
            //gridSlotManager.RebuildContainer(sortedContents, searchBox.Text, overrideSort);
            //InvalidateContents = false;
        }      
       
    }
    public class GameObjectInfo()
    {
        public string Name { get; set; }
        public ushort Graphic { get; set; }
        public TileFlag Flags;
    }
}