using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class EnemyShot : Enemy
    {
        public EnemyShot(ContentManager content, Rectangle sourceRect, int collisionWidth, int collisionHeight)
            : base(content, 0, 0, null, null)
        {
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = false;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");

            DisplayComponent = new StaticImageDisplay(textures, sourceRect);

            Attack = 1;
            Health = 1;

            SetCenteredCollisionRectangle(8, 8, collisionWidth, collisionHeight);

            InvincibleTimeAfterBeingHit = 0f;
        }

        public override void Kill()
        {
            EffectsManager.AddSparksEffect(WorldCenter, velocity, Color.Orange, 0.5f);
            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Skip the camera visibility check since shots are managed centrally
            if (Enabled)
            {
                DisplayComponent.Draw(spriteBatch, WorldLocation, Flipped);

                // Draw collision rectangle in debug mode
                if ((DrawCollisionRect || Game1.DrawAllCollisionRects) && !CollisionRectangle.IsEmpty)
                {
                    Color color = Color.Red * 0.25f;
                    spriteBatch.Draw(Game1.TileTextures, CollisionRectangle, Game1.WhiteSourceRect, color);
                }
            }
        }

        public override void AfterHittingPlayer()
        {
            Kill();
        }
    }
}
