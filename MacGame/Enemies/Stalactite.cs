using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A stone spike on the ceiling that will shake and fall if Mac goes under it.
    /// 
    /// use an object modifier to change the fall time.
    /// 
    /// Falltime: 0.2f
    /// 
    /// </summary>
    public class Stalactite : Enemy
    {

        /// <summary>
        /// The rectangle we will check for collisions with the player before giving a shake and falling.
        /// </summary>
        Rectangle DangerZone;

        /// <summary>
        /// The spike will shake for a bit before dropping.
        /// </summary>
        float shakeTimer = 0f;

        /// <summary>
        /// The amount of time between this object detecting the player and it falling.
        /// </summary>
        public float TimeBeforeFall { get; set; } = 0.6f;

        private bool _isInitialized = false;

        private int _cellX;
        private int _cellY;

        public enum StalactiteState
        {
            Idle,
            Shaking,
            Falling,
            Dead
        }

        private StalactiteState _state = StalactiteState.Idle;

        public Stalactite(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _cellX = cellX;
            _cellY = cellY;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(10, 4));

            isTileColliding = true;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(6, 8);
        }

        public override void SetProps(Dictionary<string, string> props)
        {
            base.SetProps(props);
            if (props.ContainsKey("Falltime"))
            {
                if (float.TryParse(props["Falltime"], out float fallTime))
                {
                    TimeBeforeFall = fallTime;
                }
            }
        }

        private void Initialize()
        {
            // Scan down up to 8 blocks below until you hit a solid tile. That will be the Danger zone.
            var dangerZoneCellHeight = 0;

            for (int i = 1; i < 9; i++)
            {
                dangerZoneCellHeight++;
                var tile = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY + i);
                if (tile == null || !tile.Passable)
                {
                    break;
                }
            }

            DangerZone = new Rectangle(_cellX * TileMap.TileSize, _cellY * TileMap.TileSize, TileMap.TileSize, dangerZoneCellHeight * TileMap.TileSize);

            _isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            if (_state == StalactiteState.Idle)
            {
                if (Player.CollisionRectangle.Intersects(DangerZone))
                {
                    shakeTimer = TimeBeforeFall;
                    _state = StalactiteState.Shaking;
                }
            }

            if (_state == StalactiteState.Shaking)
            {
                shakeTimer -= elapsed;
                if (shakeTimer <= 0)
                {
                    _state = StalactiteState.Falling;
                    IsAffectedByGravity = true;
                    this.velocity.X = 0f;
                    this.worldLocation.X = (_cellX * TileMap.TileSize) + (TileMap.TileSize / 2);
                    PlaySoundIfOnScreen("Fall");
                }
                else
                {
                    const float shakeSpeed = 13;

                    if (velocity.X == 0)
                    {
                        velocity.X = shakeSpeed;
                    }

                    var originalX = (_cellX * TileMap.TileSize) + (TileMap.TileSize / 2);

                    // Move the other way if it's gone past 1 pixel
                    if (this.worldLocation.X > originalX + 1)
                    {
                        velocity.X = -shakeSpeed;
                        PlaySoundIfOnScreen("Crackle", 0.5f);
                    }
                    else if (this.worldLocation.X < originalX - 1)
                    {
                        velocity.X = shakeSpeed;
                        PlaySoundIfOnScreen("Crackle", 0.5f);
                    }
                }
            }

            if (_state == StalactiteState.Falling)
            {
                if (onGround)
                {
                    Kill();
                    Enabled = false;
                    EffectsManager.SmallEnemyPop(this.WorldLocation);
                    _state = StalactiteState.Dead;
                }

                // Why not, attack the enemies too
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.Alive && enemy.Enabled && enemy.CanBeHitWithWeapons)
                    {
                        if (enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            enemy.TakeHit(this, 1, this.velocity);
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            // Draw the danger zone
            if (Game1.DrawAllCollisionRects)
            {
                Color color = Color.Red * 0.25f;
                spriteBatch.Draw(Game1.TileTextures, DangerZone, Game1.WhiteSourceRect, color);
            }

            base.Draw(spriteBatch);
        }

        public override void PlayDeathSound()
        {
            PlaySoundIfOnScreen("Break");
        }
    }
}