using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class CanadaGooseBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float startLocationX;
        private float maxTravelDistance = 8 * Game1.TileScale;

        public enum GooseState
        {
            Idle,
            Attacking
        }
        private GooseState state = GooseState.Idle;

        float idleTimer = 0;
        float explosionTimer = 0;

        public CanadaGooseBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            var honk = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 1), 2, "honk");
            honk.LoopAnimation = false;
            honk.FrameLength = 0.14f;
            animations.Add(honk);

            var attack = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 1), 3, "attack");
            attack.LoopAnimation = false;
            attack.FrameLength = 0.14f;
            attack.Oscillate = true;
            animations.Add(attack);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 3;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(60, 60);

            startLocationX = WorldLocation.X;

            state = GooseState.Idle;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (state == GooseState.Idle)
            {
                idleTimer += elapsed;

                if (idleTimer >= 1.5f)
                {
                    state = GooseState.Attacking;
                    idleTimer = 0;
                }
            }
            if (state == GooseState.Attacking)
            {
                if (animations.CurrentAnimationName == "idle")
                {
                    if (Game1.Randy.NextBool())
                    {
                        animations.Play("honk");
                    }
                    else
                    {
                        animations.Play("attack");
                    }
                }
                else
                {
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        animations.Play("idle");
                        state = GooseState.Idle;
                    }
                }
            }


            // random explosions
            explosionTimer += elapsed;
            if (explosionTimer >= 0.05f)
            {
                explosionTimer = 0f;
                // Get a random location over this collision rectangle
                var randomX = Game1.Randy.Next(CollisionRectangle.Width);
                var randomY = Game1.Randy.Next(CollisionRectangle.Height);

                var randomLocation = new Vector2(CollisionRectangle.X + randomX, CollisionRectangle.Y + randomY);
                EffectsManager.AddExplosion(randomLocation);
            }

            base.Update(gameTime, elapsed);

        }
    }
}