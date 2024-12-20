﻿using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Renderer.Gumps;
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
        private HitBox hit;
        public readonly Texture2D texture;
        public Rectangle rect;
        private Rectangle bounds;
        public bool ItemGridLocked = false;
        private readonly int _slot;
        private AlphaBlendControl background;
        public bool Hightlight = false;
        public bool SelectHighlight = false;
        public ushort Graphic;
        private Label lblGraphic;
        private readonly int _size;
        private readonly StaticFilterSlotManager _manager;
        public StaticFilterItem(ushort graphic, int slot, int size, StaticFilterSlotManager manager)
        {
            #region VARS
            _manager = manager;
            _size = size;
            _slot = slot;
            Graphic = graphic;

            ref readonly var text = ref Client.Game.Arts.GetArt(graphic);
            rect = Client.Game.Arts.GetRealArtBounds(graphic);
            texture = text.Texture;
            bounds = text.UV;

            #endregion

            Build();
        }
        private void Build()
        {
            background = new AlphaBlendControl(2.00f) { Width = _size, Height = _size, Hue = 0x0000 };
            Width = Height = _size;
            Add(background);

            hit = new HitBox(0, 0, _size, _size, null, 0f);
            Add(hit);
            //GraphicId
            lblGraphic?.Dispose();
            lblGraphic = new Label(Graphic.ToString(), true, 0x0481) { X = 1 };
            lblGraphic.Y = Height - lblGraphic.Height;
            hit.MouseUp += _hit_MouseUp;
        }
        private void _hit_MouseUp(object sender, MouseEventArgs e)
        {
            if (Keyboard.Ctrl)
            {
                _manager.RemoveItem(_slot);
            }
        }
        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            base.Draw(batcher, x, y);
            Vector3 hueVector;

            hueVector = ShaderHueTranslator.GetHueVector(ProfileManager.CurrentProfile.GridBorderHue, false, (float)ProfileManager.CurrentProfile.GridBorderAlpha / 100);

            //Borders
            batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.White), x, y, Width, Height, hueVector);

            //Selected Item
            if (hit.MouseIsOver)
            {
                hueVector.Z = 0.3f;
                //Over
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(x + 1, y, Width - 1, Height), hueVector);
            }

            if (texture != null & rect != null)
            {
                Point originalSize = new(hit.Width, hit.Height);
                Point point = new();
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
                lblGraphic?.Draw(batcher, x + lblGraphic.X, y + lblGraphic.Y);
            }
            return true;
        }
        public override void Dispose()
        {
            lblGraphic.Dispose();
            background.Dispose();
            hit.MouseUp -= _hit_MouseUp;
            hit.Dispose();
            base.Dispose();
        }
        public void Resize()
        {
            Width = _size;
            Height = _size;
            hit.Width = _size;
            hit.Height = _size;
            background.Width = _size;
            background.Height = _size;
        }
    }
}
