using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A sickle throwing by the murderer.
    /// </summary>
    public class Sickle : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public float speed = 200f;
        public float decelerationRate = 200f;
        public float verticalSpeed = 25f;

        // Sickle will move straight this distance and then start to move down and decelerate until it's at negative velocity and goes back.
        public float moveStraightDistance = Game1.TileSize * 6;

        Vector2 initialTossLocation;

        public bool IsBouncing { get; set; } = false;

        public bool isComingBack = false;

        GameObject _thrower;

        public Sickle(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var toss = new AnimationStrip(textures, Helpers.GetTileRect(7, 1), 4, "toss");
            toss.LoopAnimation = true;
            toss.FrameLength = 0.1f;
            animations.Add(toss);

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;

            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAbleToMoveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(6, 6);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (Enabled)
            {
                var distance = Math.Abs(initialTossLocation.X - this.worldLocation.X);
                var isMovingRight = !Flipped;

                if (distance > moveStraightDistance)
                {

                    isComingBack = true;

                    // Start moving down and decelerating.
                    Velocity = new Vector2(Velocity.X, verticalSpeed);

                    if (isMovingRight)
                    {
                        Velocity = new Vector2(Velocity.X - decelerationRate * elapsed, Velocity.Y);
                    }
                    else
                    {
                        Velocity = new Vector2(Velocity.X + decelerationRate * elapsed, Velocity.Y);
                    }
                }
                else
                {
                    velocity.Y = 0f;
                }

                // If it went it's initial distance, it dies when it's off camera or hits the thrower again
                if (isComingBack)
                {
                    if (_thrower.CollisionRectangle.Contains(this.CollisionCenter))
                    {
                        this.Enabled = false;
                    }
                    if (camera.IsWayOffscreen(this.CollisionRectangle))
                    {
                        this.Enabled = false;
                    }
                }
            }
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("Break");
        }

        public override void Kill()
        {
            if (Enabled && Alive)
            {
                EffectsManager.EnemyPop(WorldCenter, 10, Color.Pink, 120f);
                Enabled = false;
            }
            base.Kill();
        }

        public void Toss(GameObject thrower, bool isToTheRight)
        {
            Enabled = true;
            initialTossLocation = thrower.CollisionCenter;
            animations.Play("toss");
            this.WorldLocation = thrower.CollisionCenter;
            if (isToTheRight)
            {
                Velocity = new Vector2(speed, 0);
                Flipped = false;
            }
            else
            {
                Velocity = new Vector2(-speed, 0);
                Flipped = true;
            }
            isComingBack = false;
            _thrower = thrower;
        }
    }
}