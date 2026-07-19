using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Saucer : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private readonly Player _player;
        private float speed = 40;

        private float _fireTimer;
        private const float ShotSpeed = 100f;

        public Saucer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(2, 1), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetWorldLocationCollisionRectangle(8, 5);

            Flipped = true;

            InvincibleTimeAfterBeingHit = 0.1f;

            _fireTimer = (float)Game1.Randy.NextDouble() * 8f;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!camera.IsWayOffscreen(this.CollisionRectangle) && Alive)
            {
                velocity.X = -speed;

                _fireTimer -= elapsed;
                if (_fireTimer <= 0f)
                {
                    _fireTimer = 8f;
                    var direction = Vector2.Normalize(_player.WorldCenter - WorldCenter);
                    ShotManager.FireSmallShot(WorldCenter, direction * ShotSpeed, this);
                    SoundManager.PlaySound("Shoot");
                }
            }
            else
            {
                velocity = Vector2.Zero;
            }

            base.Update(gameTime, elapsed);

        }
    }
}