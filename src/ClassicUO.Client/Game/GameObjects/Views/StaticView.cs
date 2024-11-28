#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ClassicUO.Game.GameObjects
{
    internal sealed partial class Static
    {
        private int _canBeTransparent;

        public override bool TransparentTest(int z)
        {
            bool r = true;

            if (Z <= z - ItemData.Height)
            {
                r = false;
            }
            else if (z < Z && (_canBeTransparent & 0xFF) == 0)
            {
                r = false;
            }

            return r;
        }

        public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
        {
            if (!AllowedToDraw || IsDestroyed)
            {
                return false;
            }

            ushort graphic = Graphic;
            ushort hue = Hue;
            bool partial = ItemData.IsPartialHue;

            if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.Object == this)
            {
                hue = Constants.HIGHLIGHT_CURRENT_OBJECT_HUE;
                partial = false;
            }
            else if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && Distance > World.ClientViewRange)
            {
                hue = Constants.OUT_RANGE_COLOR;
                partial = false;
            }
            else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
            {
                hue = Constants.DEAD_RANGE_COLOR;
                partial = false;
            }

            Vector3 hueVec = ShaderHueTranslator.GetHueVector(hue, partial, AlphaHue / 255f);
            bool isTree = ReplaceTree(ref graphic);
            ReplaceWall(ref graphic);
            //ReplaceDoor(ref graphic);

            DrawStaticAnimated(
                batcher,
                graphic,
                posX,
                posY,
                hueVec,
                ProfileManager.CurrentProfile.ShadowsEnabled && ProfileManager.CurrentProfile.ShadowsStatics && (isTree || ItemData.IsFoliage || StaticFilters.IsRock(graphic)),
                depth,
                ProfileManager.CurrentProfile.AnimatedWaterEffect && ItemData.IsWet
            );

            if (ItemData.IsLight)
            {
                Client.Game.GetScene<GameScene>().AddLight(this, this, posX + 22, posY + 22);
            }

            return true;
        }

        //EP: ReplaceDoor
        private static void ReplaceDoor(ref ushort graphic)
        {
            if (ProfileManager.CurrentProfile.ChangeWallAndDoors && ProfileManager.CurrentProfile.EnableStaticFilter)
            {
                var _graphic = graphic;
                var replaceGraphic = Constants.WALL_REPLACE_GRAPHIC;
                var customReplaceWall = StaticFilters.WallTiles.FirstOrDefault(x => x.ToReplaceGraphicArray.Contains(_graphic));
                if (customReplaceWall != null)
                    replaceGraphic = customReplaceWall.ReplaceToGraphic;

                graphic = replaceGraphic;
            }
        }

        //EP: ReplaceWall
        private static void ReplaceWall(ref ushort graphic)
        {
            if (ProfileManager.CurrentProfile.ChangeWallAndDoors && ProfileManager.CurrentProfile.EnableStaticFilter)
            {
                var _graphic = graphic;
                var customReplaceWall = StaticFilters.WallTiles.FirstOrDefault(x => x.ToReplaceGraphicArray.Contains(_graphic));
                if (customReplaceWall != null)
                    graphic = customReplaceWall.ReplaceToGraphic;
            }
        }

        //EP: ReplaceTree
        private static bool ReplaceTree(ref ushort graphic)
        {
            bool isTree = StaticFilters.IsTree(graphic, out int treeType);

            if (isTree && ProfileManager.CurrentProfile.TreeToStumps && ProfileManager.CurrentProfile.EnableStaticFilter)
            {
                if (treeType == 0)
                    graphic = Constants.TREE_STUMPED_REPLACE_GRAPHIC;
                else
                    graphic = Constants.TREE_REPLACE_GRAPHIC;
            }

            return isTree;
        }

        public override bool CheckMouseSelection()
        {
            if (
                !(
                    SelectedObject.Object == this
                    || FoliageIndex != -1
                        && Client.Game.GetScene<GameScene>().FoliageIndex == FoliageIndex
                )
            )
            {
                ushort graphic = Graphic;

                bool isTree = StaticFilters.IsTree(graphic, out _);

                if (isTree && ProfileManager.CurrentProfile.TreeToStumps)
                {
                    graphic = Constants.TREE_REPLACE_GRAPHIC;
                }

                ref var index = ref ArtLoader.Instance.GetValidRefEntry(graphic + 0x4000);

                Point position = RealScreenPosition;
                position.X -= index.Width;
                position.Y -= index.Height;

                return Client.Game.Arts.PixelCheck(
                    graphic,
                    SelectedObject.TranslatedMousePositionByViewport.X - position.X,
                    SelectedObject.TranslatedMousePositionByViewport.Y - position.Y
                );
            }

            return false;
        }
    }
}
