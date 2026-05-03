using MacGame.DisplayComponents;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Enemies
{
    public class BigShipWeakSpotBack : BigShipWeakSpot
    {
        public BigShipWeakSpotBack(ContentManager content, int cellX, int cellY, Player player, Camera camera, BigShipBoss bigShip)
            : base(content, cellX, cellY, player, camera, bigShip)
        {
            var normal = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(6, 11), 1, "normal");
            normal.LoopAnimation = true;
            var orange = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(7, 11), 1, "orange");
            orange.LoopAnimation = true;
            var white = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(8, 11), 1, "white");
            white.LoopAnimation = true;

            animations.Add(normal);
            animations.Add(orange);
            animations.Add(white);
            animations.Play("normal");

            SetCenteredCollisionRectangle(16, 16, 8, 6);
        }
    }
}
