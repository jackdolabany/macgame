using System;
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
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = false;
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
            for (int i = 0; i < 8; i++)
            {
                // Make particles randomly between blue and white.
                var color = Color.Lerp(Pallette.White, Pallette.LightBlue, Game1.Randy.NextFloat());
                EffectsManager.EnemyPop(WorldCenter, 1, color, 80);
            }
            Enabled = false;
            base.Kill();
        }

        // When set, this shot orbits around the center point instead of flying freely.
        public Vector2? RotateCenter;
        public Vector2 RotateCenterVelocity;
        public float RotateRadius;
        public float RotateAngle;
        public float RotateSpeed;

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (RotateCenter.HasValue)
            {
                RotateCenter = RotateCenter.Value + RotateCenterVelocity * elapsed;
                RotateAngle += RotateSpeed * elapsed;
                WorldLocation = RotateCenter.Value + new Vector2((float)Math.Cos(RotateAngle), (float)Math.Sin(RotateAngle)) * RotateRadius;
                if (Game1.Camera.IsWayOffscreen(new Rectangle((int)RotateCenter.Value.X - 50, (int)RotateCenter.Value.Y - 50, 100, 100)))
                {
                    Enabled = false;
                    return;
                }
            }
            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Skip the camera visibility check since shots are managed centrally
            if (Enabled)
            {
                DisplayComponent.Draw(spriteBatch, WorldLocation, Flipped);

                // Draw collision rectangle in debug mode
                if (Game1.DrawAllCollisionRects && !CollisionRectangle.IsEmpty)
                {
                    Color color = Color.Red * 0.25f;
                    spriteBatch.Draw(Game1.TileTextures, CollisionRectangle, Game1.WhiteSourceRect, color);
                }
            }
        }

        public override void PlayDeathSound() 
        {
            // Do nothing.
        }

        public override void AfterHittingPlayer()
        {
        }
    }
}
