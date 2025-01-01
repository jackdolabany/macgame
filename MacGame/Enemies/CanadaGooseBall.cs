using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class CanadaGooseBall : Enemy
    {

        private Player _player;

        public CanadaGooseBall(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(7, 27));
            IsAffectedByGravity = true;
            CanBeJumpedOn = false;
            CanBeHitWithWeapons = false;
            IsAffectedByPlatforms = false;
            
            SetCenteredCollisionRectangle(7, 7);
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y, this.collisionRectangle.Width, this.collisionRectangle.Height - 8);

            this.Enabled = false;
            this.Dead = true;
          
            _player = player;
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("Break");
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);
            Enabled = false;
            base.Kill();
        }

        public override void AfterHittingPlayer()
        {
            this.Kill();
            base.AfterHittingPlayer();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled) return;

            base.Update(gameTime, elapsed);

            if (OnRightWall && Alive)
            {
                this.Kill();
            }

            if (OnGround)
            {
                this.velocity.Y -= 700f;
                SoundManager.PlaySound("GooseBallBounce");
            }
        }
    }
}