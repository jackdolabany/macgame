using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Bomb : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const float WickTime = 3f;
        private float TimeRemaining = 0.0f;

        private Player _player;

        public Bomb(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;
            
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var bomb = new AnimationStrip(textures, Helpers.GetTileRect(3, 21), 2, "bomb");
            bomb.LoopAnimation = true;
            bomb.FrameLength = 1f;
            animations.Add(bomb);

            animations.Play("bomb");

            isEnemyTileColliding = false;
            Attack = 0;
            Health = 1;
            IsAffectedByGravity = true;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            SetCenteredCollisionRectangle(6, 6);

            TimeRemaining = WickTime;
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Enabled && Alive)
            {
                if (OnGround)
                {
                    const float friction = 3.5f;
                    this.velocity.X -= (this.velocity.X * friction * elapsed);
                }
                var percentTimeRemaining = (WickTime - TimeRemaining) / WickTime;
                this.animations.CurrentAnimation.FrameLength = MathHelper.Lerp(0.5f, 1f/60f, percentTimeRemaining);

                TimeRemaining -= elapsed;
                if (TimeRemaining <= 0)
                {
                    // Explode!
                    var explosionRectangle = new Rectangle((int)WorldCenter.X - 32, (int)WorldCenter.Y - 32, 64, 64);
                    EffectsManager.AddExplosion(this.WorldCenter);
                    this.Attack = 1;
                    if (_player.CollisionRectangle.Intersects(explosionRectangle))
                    {
                        _player.TakeHit(this);
                    }
                    this.Attack = 0;
                    this.Enabled = false;
                }
            }

            base.Update(gameTime, elapsed);

        }

        public void Reset()
        {
            this.TimeRemaining = WickTime;
            this.Enabled = true;
        }
    }
}