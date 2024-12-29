using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class YarnBall : Enemy
    {

        StaticImageDisplay image => (StaticImageDisplay)DisplayComponent;

        public bool IsBouncing { get; set; } = false;
        public YarnBall(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(textures);
            image.Source = Helpers.GetTileRect(5, 2);

            isTileColliding = true;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;

            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAbleToMoveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetCenteredCollisionRectangle(7, 7);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            var wasOnCeiling = OnCeiling;
            var wasOnGround = OnGround;
            var wasOnLeftWall = OnLeftWall;
            var wasOnRightWall = OnRightWall;
            var previousVelocity = this.velocity;

            base.Update(gameTime, elapsed);

            if (!IsBouncing && Alive && Enabled)
            {
                if (OnLeftWall || OnRightWall || OnCeiling || OnGround)
                {
                    this.Kill();
                }
            }

            if (Alive && Enabled && IsBouncing)
            {
                if (OnCeiling && !wasOnCeiling)
                {
                    velocity.Y = -previousVelocity.Y;
                    SoundManager.PlaySound("Bounce");
                }
                else if (OnGround && !wasOnGround)
                {
                    velocity.Y = -previousVelocity.Y;
                    SoundManager.PlaySound("Bounce");
                }
                if (OnLeftWall && !wasOnLeftWall)
                {
                    velocity.X = -previousVelocity.X;
                    SoundManager.PlaySound("Bounce");
                }
                else if (OnRightWall && !wasOnRightWall)
                {
                    velocity.X = -previousVelocity.X;
                    SoundManager.PlaySound("Bounce");
                }
            }
        }

        public override void Kill()
        {
            if (Enabled && Alive)
            {
                EffectsManager.EnemyPop(WorldCenter, 10, Color.Pink, 120f);
                SoundManager.PlaySound("Break");
                Enabled = false;
            }
            base.Kill();
        }
    }
}