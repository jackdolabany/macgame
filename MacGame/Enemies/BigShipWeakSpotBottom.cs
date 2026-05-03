using MacGame.DisplayComponents;
using Microsoft.Xna.Framework.Content;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigShipWeakSpotBottom : BigShipWeakSpot
    {
        public BigShipWeakSpotBottom(ContentManager content, int cellX, int cellY, Player player, Camera camera, BigShipBoss bigShip)
            : base(content, cellX, cellY, player, camera, bigShip)
        {
            var normal = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(6, 13), 1, "normal");
            normal.LoopAnimation = true;
            var orange = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(7, 13), 1, "orange");
            orange.LoopAnimation = true;
            var white = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(8, 13), 1, "white");
            white.LoopAnimation = true;

            animations.Add(normal);
            animations.Add(orange);
            animations.Add(white);
            animations.Play("normal");

            SetCenteredCollisionRectangle(16, 16, 10, 10);
        }
    }
}
