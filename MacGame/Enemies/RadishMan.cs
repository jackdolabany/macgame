using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A radish buried in the ground. Every so often he pops up for a quick look around and then ducks
    /// back down. He's harmless and invulnerable while buried, but while he's up Mac can jump on him or hit him.
    /// </summary>
    public class RadishMan : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private enum RadishState
        {
            Buried,
            PoppingUp,
            Up,
            GoingDown
        }

        private RadishState state = RadishState.Buried;

        private float buriedTimer;
        private float upTimer;

        private const float UpTime = 1.5f;

        public RadishMan(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var buried = new AnimationStrip(textures, Helpers.GetTileRect(7, 6), 1, "buried");
            buried.LoopAnimation = true;
            animations.Add(buried);

            var popUp = new AnimationStrip(textures, Helpers.GetTileRect(7, 6), 3, "popUp");
            popUp.LoopAnimation = false;
            popUp.FrameLength = 0.1f;
            animations.Add(popUp);

            var up = new AnimationStrip(textures, Helpers.GetTileRect(9, 6), 1, "up");
            up.LoopAnimation = true;
            animations.Add(up);

            var goDown = (AnimationStrip)popUp.Clone();
            goDown.Reverse = true;
            goDown.Name = "goDown";
            animations.Add(goDown);

            animations.Play("buried");

            isEnemyTileColliding = false;
            IsAffectedByGravity = false;

            Attack = 1;
            Health = 1;

            SetWorldLocationCollisionRectangle(6, 6);

            SetVulnerable(false);
            ResetBuriedTimer();
        }

        private void ResetBuriedTimer()
        {
            buriedTimer = 1.5f + (float)Game1.Randy.NextDouble() * 2.5f;
        }

        /// <summary>
        /// He can only touch Mac, or be hurt by Mac, while he's popped up out of the ground.
        /// </summary>
        private void SetVulnerable(bool isVulnerable)
        {
            IsPlayerColliding = isVulnerable;
            CanBeJumpedOn = isVulnerable;
            CanBeHitWithWeapons = isVulnerable;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            switch (state)
            {
                case RadishState.Buried:
                    buriedTimer -= elapsed;
                    if (buriedTimer <= 0)
                    {
                        state = RadishState.PoppingUp;
                        SetVulnerable(true);
                        animations.Play("popUp");
                    }
                    break;

                case RadishState.PoppingUp:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        state = RadishState.Up;
                        upTimer = UpTime;
                        animations.Play("up");
                    }
                    break;

                case RadishState.Up:
                    upTimer -= elapsed;
                    if (upTimer <= 0)
                    {
                        state = RadishState.GoingDown;
                        animations.Play("goDown");
                    }
                    break;

                case RadishState.GoingDown:
                    if (animations.CurrentAnimation!.FinishedPlaying)
                    {
                        state = RadishState.Buried;
                        ResetBuriedTimer();
                        animations.Play("buried");
                        SetVulnerable(false);
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
