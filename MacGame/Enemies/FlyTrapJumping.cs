using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A fly trap that chomps in place and jumps straight up. It does two short hops followed by one
    /// big jump. Mac can't jump on it, but weapons will kill it.
    /// </summary>
    public class FlyTrapJumping : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private const float ShortHopHeightInTiles = 0.5f;
        private const float BigJumpHeightInTiles = 4f;
        private const int HopsPerCycle = 3;

        private const float PauseAfterHop = 0.4f;
        private const float PauseAfterBigJump = 1f;

        // Which jump in the cycle is next. The last one is the big jump.
        private int jumpIndex = 0;
        private float pauseTimer = PauseAfterHop;
        private bool wasOnGround = true;

        public FlyTrapJumping(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var chomp = new AnimationStrip(textures, Helpers.GetTileRect(2, 7), 2, "chomp");
            chomp.LoopAnimation = true;
            chomp.FrameLength = 0.2f;
            animations.Add(chomp);

            animations.Play("chomp");

            isEnemyTileColliding = true;
            IsAffectedByGravity = true;

            Attack = 1;
            Health = 1;
            CanBeJumpedOn = false;
            CanBeHitWithWeapons = true;

            SetWorldLocationCollisionRectangle(6, 7);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        /// <summary>
        /// The upward velocity needed to reach the given height with the current gravity.
        /// </summary>
        private float GetJumpVelocity(float heightInTiles)
        {
            var height = heightInTiles * TileMap.TileSize;
            return (float)Math.Sqrt(2f * Gravity.Y * height);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                velocity.X = 0;

                if (OnGround)
                {
                    if (!wasOnGround)
                    {
                        // Just landed. If that was the big jump, wait a bit longer.
                        pauseTimer = jumpIndex == 0 ? PauseAfterBigJump : PauseAfterHop;
                    }

                    pauseTimer -= elapsed;
                    if (pauseTimer <= 0)
                    {
                        var isBigJump = jumpIndex == HopsPerCycle - 1;
                        velocity.Y = -GetJumpVelocity(isBigJump ? BigJumpHeightInTiles : ShortHopHeightInTiles);
                        jumpIndex = (jumpIndex + 1) % HopsPerCycle;
                    }
                }

                wasOnGround = OnGround;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
