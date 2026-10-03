using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A ghost that drifts back and forth hidden in the ground. Every so often he rises up, looks around,
    /// and sinks back down. He can only hurt Mac while he's up, and he can't be killed.
    /// </summary>
    public class HiddenGhost : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private enum GhostState
        {
            Wandering,
            Rising,
            LookingAround,
            Sinking
        }

        private GhostState state = GhostState.Wandering;

        private float speed = 100;
        private float minX;
        private float maxX;

        private float riseTimer;
        private float lookTimer;
        private int lookFlipsRemaining;

        private const int WanderRangeInTiles = 3;
        private const int LookFlips = 4;
        private const float LookFlipTime = 0.4f;

        public HiddenGhost(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var hidden = new AnimationStrip(textures, Helpers.GetTileRect(4, 6), 1, "hidden");
            hidden.LoopAnimation = true;
            animations.Add(hidden);

            var rise = new AnimationStrip(textures, Helpers.GetTileRect(4, 6), 3, "rise");
            rise.LoopAnimation = false;
            rise.FrameLength = 0.15f;
            animations.Add(rise);

            var up = new AnimationStrip(textures, Helpers.GetTileRect(6, 6), 1, "up");
            up.LoopAnimation = true;
            animations.Add(up);

            var sink = new AnimationStrip(textures, Helpers.GetTileRect(4, 6), 3, "sink");
            sink.LoopAnimation = false;
            sink.Reverse = true;
            sink.FrameLength = 0.15f;
            animations.Add(sink);

            animations.Play("hidden");

            // He's a ghost, he floats along a fixed line and ignores walls.
            isEnemyTileColliding = false;
            IsAffectedByGravity = false;

            Attack = 1;
            Health = 1;
            CanBeJumpedOn = false;
            CanBeHitWithWeapons = false;
            IsPlayerColliding = false;

            SetWorldLocationCollisionRectangle(8, 4);

            minX = WorldLocation.X - WanderRangeInTiles * TileMap.TileSize;
            maxX = WorldLocation.X + WanderRangeInTiles * TileMap.TileSize;

            ResetRiseTimer();
        }

        private void ResetRiseTimer()
        {
            riseTimer = 2f + (float)Game1.Randy.NextDouble() * 3f;
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            // Nothing can hurt him.
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            switch (state)
            {
                case GhostState.Wandering:
                    if (!Flipped && WorldLocation.X >= maxX)
                    {
                        Flipped = true;
                    }
                    else if (Flipped && WorldLocation.X <= minX)
                    {
                        Flipped = false;
                    }

                    velocity.X = Flipped ? -speed : speed;

                    riseTimer -= elapsed;
                    if (riseTimer <= 0)
                    {
                        state = GhostState.Rising;
                        velocity.X = 0;
                        animations.Play("rise");
                    }
                    break;

                case GhostState.Rising:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        state = GhostState.LookingAround;
                        IsPlayerColliding = true;
                        lookFlipsRemaining = LookFlips;
                        lookTimer = LookFlipTime;
                        animations.Play("up");
                    }
                    break;

                case GhostState.LookingAround:
                    lookTimer -= elapsed;
                    if (lookTimer <= 0)
                    {
                        if (lookFlipsRemaining > 0)
                        {
                            Flipped = !Flipped;
                            lookFlipsRemaining--;
                            lookTimer = LookFlipTime;
                        }
                        else
                        {
                            state = GhostState.Sinking;
                            IsPlayerColliding = false;
                            animations.Play("sink");
                        }
                    }
                    break;

                case GhostState.Sinking:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        state = GhostState.Wandering;
                        ResetRiseTimer();
                        animations.Play("hidden");
                    }
                    break;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
