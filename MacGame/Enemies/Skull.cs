using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A skull that floats around.
    /// </summary>
    public class Skull : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 90;
        Vector2 startLocation;
        
        private SkullState state = SkullState.Idle;

        private float _changeStateTimer = 0;

        Vector2 moveDirection;

        private enum SkullState
        {
            Idle,
            MovingTowardsPlayer
        }

        public Skull(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(6, 0), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            isEnemyTileColliding = false;

            SetCenteredCollisionRectangle(10, 14);

            startLocation = WorldLocation; 

            // Idle for a bit to start.
            state = SkullState.Idle;
            _changeStateTimer = 1.5f;
            
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;
            if (!Alive) return;

            if (state == SkullState.Idle)
            {
                velocity = Vector2.Zero;
                if (_changeStateTimer > 0)
                {
                    _changeStateTimer -= elapsed;
                }

                if (_changeStateTimer <= 0)
                {
                    _changeStateTimer = 2.5f;
                    state = SkullState.MovingTowardsPlayer;

                    moveDirection = Player.WorldCenter - WorldCenter;
                    moveDirection.Normalize();
                }
 
            }
            else if (state == SkullState.MovingTowardsPlayer)
            {
                if (_changeStateTimer > 0)
                {
                    _changeStateTimer -= elapsed;
                }

           
                Velocity = moveDirection * speed;

                if (_changeStateTimer <= 0)
                {
                    this.state = SkullState.Idle;
                    _changeStateTimer = 1.5f;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}