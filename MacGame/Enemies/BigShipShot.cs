using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigShipShot : Enemy
    {
        private const float Speed = 500f;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        public BigShipShot(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new StaticImageDisplay(Game1.ReallyBigTileTextures, Helpers.GetReallyBigTileRect(6, 6));

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 1000;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetCenteredCollisionRectangle(24, 24, 20, 6);

            Enabled = false;
        }

        public void Launch(Vector2 position, bool goLeft)
        {
            WorldLocation = position;
            Velocity = new Vector2(goLeft ? -Speed : Speed, 0);
            display.Effect = goLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Enabled = true;
            Alive = true;
        }
    }
}
