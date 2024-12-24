using System;
using System.Net.Http.Headers;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Clam : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        Pearl pearl;

        private ClamState State;

        // Set this so the clam doesn't just machine gun shots at the player.
        float cooldownTimer = 0;

        float shotSpeed = 80f;

        public enum ClamState
        {
            Idle,
            Opening,
            Closing
        }

        public Clam(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(12, 8), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.3f;
            animations.Add(idle);

            animations.Play("idle");

            var open = new AnimationStrip(textures, Helpers.GetTileRect(12, 8), 3, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.3f;
            animations.Add(open);

            var close = (AnimationStrip)open.Clone();
            close.Reverse = true;
            close.Name = "close";
            animations.Add(close);

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 5);

            // Shift it up a bit.
            this.collisionRectangle.Y -= 4;

            // Shift the image down slightly
            this.WorldLocation = this.WorldLocation - new Vector2(0, -4);

            pearl = new Pearl(content, cellX, cellY, player, camera);
            pearl.Enabled = false;
            Level.AddEnemy(pearl);

            State = ClamState.Idle;

            cooldownTimer = 5f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer -= elapsed;
            }

            switch (State)
            {
                case ClamState.Idle:
                    if (cooldownTimer <= 0 && !pearl.Enabled && Player.IsInWater)
                    {
                        State = ClamState.Opening;
                        animations.Play("open");
                    }
                    break;
                case ClamState.Opening:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        pearl.WorldLocation = WorldCenter;
                        pearl.WorldLocation += new Vector2(0, 8);
                        pearl.Enabled = true;
                        pearl.Alive = true;
                        State = ClamState.Closing;
                        cooldownTimer = 4f;
                        animations.Play("close");
                        
                        var direction = Helpers.GetEightWayDirectionTowardsTarget(WorldCenter, Player.WorldCenter);
                        pearl.Velocity = direction * shotSpeed;
                        pearl.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);

                    }
                    break;
                case ClamState.Closing:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        State = ClamState.Idle;
                        animations.Play("idle");
                    }
                    break;
            }

            base.Update(gameTime, elapsed);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);
            Enabled = false;
            base.Kill();
        }
    }
}