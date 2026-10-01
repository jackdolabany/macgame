using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// The boomerang Mac throws. Not the item to get the boomerang (Boomerang.cs).
    /// Works like the cross in Castlevania. It flies about 2/3 of the way across the screen, or bounces off the
    /// edge of the screen if it gets there first, and comes back. It only turns around once, so if Mac doesn't
    /// catch it, it flies off the other side. It passes through walls and enemies, hurting each enemy once per pass.
    /// </summary>
    public class MacBoomerang : GameObject
    {
        private Player _player;

        private const float throwSpeed = 250f;

        /// <summary>
        /// How far the boomerang goes before turning around.
        /// </summary>
        private const int reachInTiles = 8;

        /// <summary>
        /// How quickly it swings around at the end of its reach. Takes about a block to stop.
        /// </summary>
        private const float turnDeceleration = (throwSpeed * throwSpeed) / (2f * TileMap.TileSize);

        private enum BoomerangState
        {
            Outgoing,
            Turning,
            Returning
        }

        private BoomerangState _state;

        private float _startX;
        private float _reach;

        // 1 if thrown to the right, -1 if to the left.
        private float facing;

        /// <summary>
        /// Enemies the boomerang is currently passing through. They can't be hit again until it leaves them.
        /// </summary>
        private HashSet<Enemy> _enemiesBeingHit = new HashSet<Enemy>();

        private bool IsComingBack => Math.Sign(velocity.X) == -facing;

        public MacBoomerang(Player player, Texture2D textures2)
        {
            _player = player;

            var animations = new AnimationDisplay();
            DisplayComponent = animations;

            var spin = new AnimationStrip(textures2, Helpers.GetTileRect(10, 14), 4, "spin");
            spin.LoopAnimation = true;
            spin.FrameLength = 0.06f;
            animations.Add(spin);
            animations.Play("spin");

            SetCenteredCollisionRectangle(8, 8, 6, 6);
            IsAffectedByGravity = false;
            isTileColliding = false;
            Enabled = false;
        }

        public void Throw(bool isFacingLeft)
        {
            facing = isFacingLeft ? -1f : 1f;
            Flipped = isFacingLeft;
            WorldLocation = _player.WorldLocation;
            Velocity = new Vector2(facing * throwSpeed, 0);
            _state = BoomerangState.Outgoing;
            _startX = WorldLocation.X;
            _reach = reachInTiles * TileMap.TileSize;
            _enemiesBeingHit.Clear();
            Enabled = true;

            SoundManager.PlaySound("Kick");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled) return;

            if (_state == BoomerangState.Outgoing && Math.Abs(WorldLocation.X - _startX) >= _reach)
            {
                _state = BoomerangState.Turning;
            }

            if (_state == BoomerangState.Turning)
            {
                velocity.X -= facing * turnDeceleration * elapsed;
                if (IsComingBack && Math.Abs(velocity.X) >= throwSpeed)
                {
                    _state = BoomerangState.Returning;
                }
            }

            if (_state != BoomerangState.Returning && IsAtScreenEdge())
            {
                // Bounce straight back.
                _state = BoomerangState.Returning;
            }

            if (_state == BoomerangState.Returning)
            {
                velocity.X = -facing * throwSpeed;
            }

            base.Update(gameTime, elapsed);

            if (IsComingBack && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                ReturnBoomerang();
                return;
            }

            if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
            {
                ReturnBoomerang();
                return;
            }

            HitEnemies();
        }

        private bool IsAtScreenEdge()
        {
            var viewPort = Game1.Camera.ViewPort;
            if (facing > 0)
            {
                return CollisionRectangle.Right >= viewPort.Right;
            }
            return CollisionRectangle.Left <= viewPort.Left;
        }

        private void HitEnemies()
        {
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                var isTouching = enemy.Enabled && enemy.Alive && enemy.CanBeHitWithWeapons && enemy.CollisionRectangle.Intersects(this.CollisionRectangle);
                if (isTouching)
                {
                    // Only hurt enemies the first frame the boomerang touches them.
                    if (_enemiesBeingHit.Add(enemy))
                    {
                        enemy.TakeHit(this, 1);
                    }
                }
                else
                {
                    _enemiesBeingHit.Remove(enemy);
                }
            }
        }

        private void ReturnBoomerang()
        {
            _player.Boomerangs.ReturnObject(this);
        }
    }
}
