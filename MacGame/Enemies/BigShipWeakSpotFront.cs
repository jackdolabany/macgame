using MacGame.DisplayComponents;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Enemies
{
    public class BigShipWeakSpotFront : BigShipWeakSpot
    {
        public BigShipWeakSpotFront(ContentManager content, int cellX, int cellY, Player player, Camera camera, BigShipBoss bigShip)
            : base(content, cellX, cellY, player, camera, bigShip)
        {
            var normal = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(6, 10), 1, "normal");
            normal.LoopAnimation = true;
            var orange = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(7, 10), 1, "orange");
            orange.LoopAnimation = true;
            var white = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(8, 10), 1, "white");
            white.LoopAnimation = true;

            animations.Add(normal);
            animations.Add(orange);
            animations.Add(white);
            animations.Play("normal");

            SetCenteredCollisionRectangle(16, 16, 6, 14);
        }
    }
}
