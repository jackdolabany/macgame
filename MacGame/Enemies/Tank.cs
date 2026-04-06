using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Tank : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private const float MIN_STATE_TIME = 1f;
        private const float MAX_STATE_TIME = 3f;
        private const float MIN_SHOOT_TIME = 2f;
        private const float MAX_SHOOT_TIME = 3f;

        private float speed = 40;
        private float stateTimer = 0f;
        private float shootTimer = 0f;
        private TankState currentState = TankState.MovingRight;

        private enum TankState
        {
            MovingLeft,
            MovingRight,
            Stopped
        }

        public Tank(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");


            var idle = new AnimationStrip(textures, Helpers.GetTileRect(5, 5), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.12f;
            animations.Add(idle);

            var moveRight = new AnimationStrip(textures, Helpers.GetTileRect(5, 5), 4, "moveRight");
            moveRight.LoopAnimation = true;
            moveRight.FrameLength = 0.12f;
            animations.Add(moveRight);

            var moveLeft = (AnimationStrip)moveRight.Clone();
            moveLeft.Name = "moveLeft";
            moveLeft.Reverse = true;
            animations.Add(moveLeft);

            animations.Play("moveRight");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetWorldLocationCollisionRectangle(6, 7);

            // Set initial random state
            SetRandomState();
            ResetShootTimer();
        }

        private void SetRandomState()
        {
            // Randomly choose a state
            var stateChoice = Game1.Randy.Next(0, 3);
            currentState = (TankState)stateChoice;

            // Set duration between MIN_STATE_TIME and MAX_STATE_TIME seconds
            stateTimer = MIN_STATE_TIME + (Game1.Randy.NextFloat() * (MAX_STATE_TIME - MIN_STATE_TIME));

            // Update animation based on state
            if (currentState == TankState.MovingLeft)
            {
                animations.Play("moveLeft");
                Flipped = true;
            }
            else if (currentState == TankState.MovingRight)
            {
                animations.Play("moveRight");
                Flipped = false;
            }
            else
            {
                animations.Play("idle");
            }
        }

        private void ResetShootTimer()
        {
            shootTimer = MIN_SHOOT_TIME + (Game1.Randy.NextFloat() * (MAX_SHOOT_TIME - MIN_SHOOT_TIME));
        }

        private void Shoot()
        {
            // Calculate direction to player
            var directionToPlayer = Player.CollisionCenter - CollisionCenter;
            directionToPlayer.Normalize();

            var shootSpeed = 150f;
            var velocity = directionToPlayer * shootSpeed;

            ShotManager.FireSmallShot(CollisionCenter, velocity);

            PlaySoundIfOnScreen("Fire", 0.5f);

            ResetShootTimer();
        }

        public override void Kill()
        {
            // Explode without screen shake
            EffectsManager.AddExplosion(WorldCenter, false);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                // Update state timer
                stateTimer -= elapsed;

                // Check if it's time to change state
                if (stateTimer <= 0)
                {
                    SetRandomState();
                }

                // Update shoot timer
                shootTimer -= elapsed;

                // Check if it's time to shoot
                if (shootTimer <= 0)
                {
                    Shoot();
                }

                // Handle movement based on current state
                switch (currentState)
                {
                    case TankState.MovingLeft:
                        velocity.X = -speed;

                        // Stop if hitting a wall
                        if (OnLeftWall)
                        {
                            SetRandomState();
                        }
                        break;

                    case TankState.MovingRight:
                        velocity.X = speed;

                        // Stop if hitting a wall
                        if (OnRightWall)
                        {
                            SetRandomState();
                        }
                        break;

                    case TankState.Stopped:
                        velocity.X = 0;
                        break;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
