using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class DraculaBat : Enemy
    {

        private Player _player;

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public DraculaBat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(0, 8), 2, "fly");
            fly.LoopAnimation = true;
            fly.Oscillate = true;
            fly.FrameLength = 0.08f;
            animations.Add(fly);
            animations.Play("fly");


            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAbleToMoveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(7, 7);
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y, this.collisionRectangle.Width, this.collisionRectangle.Height - 8);

            this.Enabled = false;
            this.Dead = true;
          
            _player = player;
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("BatChirp");
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            var wasOnCeiling = OnCeiling;
            var wasOnGround = OnGround;
            var wasOnLeftWall = OnLeftWall;
            var wasOnRightWall = OnRightWall;
            var previousVelocity = this.velocity;

            base.Update(gameTime, elapsed);

            if (Alive && Enabled)
            {
                if (OnCeiling && !wasOnCeiling)
                {
                    velocity.Y = -previousVelocity.Y;
                    SoundManager.PlaySound("BatChirp");
                }
                else if (OnGround && !wasOnGround)
                {
                    velocity.Y = -previousVelocity.Y;
                    SoundManager.PlaySound("BatChirp");
                }
                if (OnLeftWall && !wasOnLeftWall)
                {
                    velocity.X = -previousVelocity.X;
                    SoundManager.PlaySound("BatChirp");
                }
                else if (OnRightWall && !wasOnRightWall)
                {
                    velocity.X = -previousVelocity.X;
                    SoundManager.PlaySound("BatChirp");
                }
            }

            Flipped = Velocity.X <= 0;
        }
    }
}