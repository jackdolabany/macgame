using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    // This is the spear Mac throws. Not the item to get the spear (Spear.cs).
    // It flies straight ahead and hits enemies. If it hits a wall it sticks in and becomes a platform
    // until Mac throws it again.
    public class MacSpear : GameObject
    {
        private enum SpearState
        {
            Idle,
            Flying,
            Stuck
        }

        private Player _player;
        private SpearState _state = SpearState.Idle;

        private const float speed = 500f;

        // 1 if thrown right, -1 if left.
        private float facing;

        // The sprite is 16x16 art pixels with the shaft on row 7 from x 2 to 12. The tip points right.
        // These are the world pixel offsets from WorldLocation (bottom center) when facing right.
        private const int tipOffsetX = 20;
        private const int shaftTopOffsetY = -36;
        private const int shaftLength = 44;
        private const int shaftHeight = 8;

        // How far the tip sinks into the wall when it sticks. 3 art pixels.
        private const int stickDepth = 12;

        /// <summary>
        /// Invisible platform that Mac can stand on while the spear is stuck in a wall.
        /// </summary>
        private Platform _platform;

        /// <summary>
        /// The platform list the platform was added to. Rooms each have their own list.
        /// </summary>
        private List<Platform>? _platformList;

        public bool CanThrow
        {
            get
            {
                return _state != SpearState.Flying;
            }
        }

        public MacSpear(Player player, ContentManager content, Texture2D bigTextures)
        {
            _player = player;

            var image = new StaticImageDisplay(bigTextures, Helpers.GetBigTileRect(4, 12));
            DisplayComponent = image;
            Enabled = false;
            IsAffectedByGravity = false;
            isTileColliding = false;

            _platform = new Platform(content, 0, 0);
            _platform.DisplayComponent = new NoDisplay();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == SpearState.Flying)
            {
                if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    Reset();
                    return;
                }

                // Check if the tip hit a wall.
                var tip = new Vector2(WorldLocation.X + facing * (tipOffsetX - 1), WorldLocation.Y + shaftTopOffsetY + 2);
                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(tip);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    StickIntoWall(tip);
                }
                else
                {
                    foreach (var enemy in Game1.CurrentLevel.Enemies)
                    {
                        if (enemy.Enabled && enemy.CanBeHitWithWeapons && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            enemy.TakeHit(this, 1);
                            EffectsManager.SmallEnemyPop(this.WorldCenter);
                            Reset();
                            return;
                        }
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                base.Draw(spriteBatch);
            }
        }

        public void Throw(bool isFacingLeft)
        {
            if (!CanThrow) return;

            // Throwing a new spear takes away the old platform.
            RemovePlatform();

            facing = isFacingLeft ? -1f : 1f;
            Flipped = isFacingLeft;
            SetShaftCollisionRectangle();

            // Line the shaft up with the middle of Mac.
            WorldLocation = _player.WorldLocation + new Vector2(0, -(TileMap.TileSize / 2) - shaftTopOffsetY - shaftHeight / 2);
            Velocity = new Vector2(facing * speed, 0);
            SetDrawDepth(_player.DrawDepth + Game1.MIN_DRAW_INCREMENT);
            Enabled = true;
            _state = SpearState.Flying;

            SoundManager.PlaySound("Kick");
        }

        /// <summary>
        /// Disables the spear and removes the platform if there is one.
        /// </summary>
        public void Reset()
        {
            RemovePlatform();
            Enabled = false;
            Velocity = Vector2.Zero;
            _state = SpearState.Idle;
        }

        private void StickIntoWall(Vector2 tip)
        {
            var cellX = Game1.CurrentMap.GetCellByPixelX((int)tip.X);
            var cellY = Game1.CurrentMap.GetCellByPixelY((int)tip.Y);

            // Snap the spear so the tip is sunk into the wall.
            if (facing > 0)
            {
                var wallLeft = cellX * TileMap.TileSize;
                WorldLocation = new Vector2(wallLeft + stickDepth - tipOffsetX, WorldLocation.Y);
            }
            else
            {
                var wallRight = (cellX + 1) * TileMap.TileSize;
                WorldLocation = new Vector2(wallRight - stickDepth + tipOffsetX, WorldLocation.Y);
            }

            Velocity = Vector2.Zero;
            _state = SpearState.Stuck;

            // Draw just behind the frontmost layer of the wall so the tip is hidden inside it.
            var mapSquare = Game1.CurrentMap.GetMapSquareAtCell(cellX, cellY);
            if (mapSquare != null)
            {
                for (int z = mapSquare.LayerTiles.Length - 1; z >= 0; z--)
                {
                    var tile = mapSquare.LayerTiles[z];
                    if (tile != null && tile.ShouldDraw && tile.Texture != null)
                    {
                        SetDrawDepth(Game1.CurrentMap.GetLayerDrawDepth(z) + Game1.MIN_DRAW_INCREMENT);
                        break;
                    }
                }
            }

            // The platform covers the part of the shaft sticking out of the wall.
            var exposedLength = shaftLength - stickDepth;
            var platformX = facing > 0 ? -tipOffsetX - 4 : -tipOffsetX + stickDepth;
            _platform.WorldLocation = WorldLocation;
            _platform.PreviousLocation = WorldLocation;
            _platform.CollisionRectangle = new Rectangle(platformX, shaftTopOffsetY, exposedLength, shaftHeight);
            _platformList = Game1.CurrentLevel.Platforms;
            _platformList.Add(_platform);

            SoundManager.PlaySound("Break");
        }

        private void RemovePlatform()
        {
            if (_platformList != null)
            {
                _platformList.Remove(_platform);
                _platformList = null;
            }

            if (_player.PlatformThatThisIsOn == _platform)
            {
                _player.PlatformThatThisIsOn = null;
            }
        }

        private void SetShaftCollisionRectangle()
        {
            // The sprite isn't perfectly centered so the shaft shifts a bit when flipped.
            var x = facing > 0 ? -tipOffsetX - 4 : -tipOffsetX;
            CollisionRectangle = new Rectangle(x, shaftTopOffsetY, shaftLength, shaftHeight);
        }
    }
}
