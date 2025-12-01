using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Items
{
    public class HeartSpinner : Item
    {

        private float _spinTimer;

        private enum HeartState
        {
            Idle,
            FillingHealth,
            SlowingDown
        }

        HeartState _heartState = HeartState.Idle;

        float _maxSpinAnimationSpeed;

        float _slowDownTimer = 0f;

        AnimationDisplay Animations => (AnimationDisplay)this.DisplayComponent;
        public HeartSpinner(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(1, 4), 1, "idle");
            idle.LoopAnimation = false;
            idle.Oscillate = false;
            idle.FrameLength = 0.33f;
            animations.Add(idle);

            var spin = new AnimationStrip(textures, Helpers.GetTileRect(1, 4), 3, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = true;
            spin.FrameLength = 0.07f;
            animations.Add(spin);

            _maxSpinAnimationSpeed = spin.FrameLength;

            animations.Play("idle");

            SetWorldLocationCollisionRectangle(8, 8);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (_heartState == HeartState.FillingHealth)
            {
                _spinTimer += elapsed;
                if (_spinTimer > 0.7f)
                {
                    if (_player.Health < Player.MaxHealth)
                    {
                        SoundManager.PlaySound("PowerUp");
                        _spinTimer = 0f;
                        _player.Health += 1;
                    }
                    else
                    {
                        _heartState = HeartState.SlowingDown;
                        _slowDownTimer = 0f;
                    }
                }
            }
            else if (_heartState == HeartState.SlowingDown)
            {
                // Slow the animation down over a few seconds.
                _slowDownTimer += elapsed;
                const float slowDownTime = 3f;
                if (_slowDownTimer < slowDownTime)
                {
                    Animations.CurrentAnimation.FrameLength = MathHelper.Lerp(_maxSpinAnimationSpeed, 3 * _maxSpinAnimationSpeed, _slowDownTimer / slowDownTime);
                }
                else
                {
                    // Wait until we get back to the first frame to stick on idle.
                    if (Animations.CurrentAnimation.currentFrameIndex == 0 && !Animations.CurrentAnimation.Reverse)
                    { 
                        Animations.CurrentAnimation.FrameLength = _maxSpinAnimationSpeed;
                        Animations.Play("idle");
                        _heartState = HeartState.Idle;
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Collect(Player player)
        {
            if (_heartState != HeartState.FillingHealth && _player.Health < Player.MaxHealth)
            {
                _heartState = HeartState.FillingHealth;
                _spinTimer = 0f;
                _player.Health += 1;
                Animations.Play("spin");
                Animations.CurrentAnimation.FrameLength = _maxSpinAnimationSpeed;
                base.Collect(player);
            }
        }
    }
}
