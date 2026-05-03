using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigShipBoss : Enemy
    {
        private static readonly Rectangle ShipSourceRect = new Rectangle(22 * Game1.TileScale, 10 * Game1.TileScale, 175 * Game1.TileScale, 84 * Game1.TileScale);

        private int _maxHealth;
        private bool _hasBeenSeen = false;
        public bool HasBeenSeen => _hasBeenSeen;
        private float _normalY;
        private bool _isInitialized = false;

        private List<Rectangle> collisionRectangles = new List<Rectangle>();
        private Player _player;

        private BigShipWeakSpotFront _weakSpotFront;
        private BigShipWeakSpotBack _weakSpotBack;
        private BigShipWeakSpotTop _weakSpotTop;
        private BigShipWeakSpotBottom _weakSpotBottom;

        private BigShipShot _shotLeft;
        private BigShipShot _shotRight;
        private float _shotTimer = 0f;
        private const float ShotInterval = 4f;

        // Offsets from WorldLocation — tune these to match the ship art.
        private  Vector2 WeakSpotFrontOffset = new Vector2(-330, -136);
        private Vector2 WeakSpotBackOffset = new Vector2(22, -136);
        private Vector2 WeakSpotTopOffset = new Vector2(-22, -276);
        private Vector2 WeakSpotBottomOffset = new Vector2(-22, 4);

        public BigShipBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var texture = content.Load<Texture2D>(@"Textures\BigShip");

            DisplayComponent = new StaticImageDisplay(texture, ShipSourceRect);
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0f;
            Attack = 1;
            Health = 200;
            _maxHealth = Health;
            _player = player;

            // The ship won't directly hit the player, but the extra collision rectangles and the 
            // enemies it spawns will.
            IsPlayerColliding = false;
            CanBeHitWithWeapons = false;

            SetCenteredCollisionRectangle(175, 84, 170, 80);

            WorldLocation += new Vector2(CollisionRectangle.Width / 2, CollisionRectangle.Height / 2);

            // Front and body
            collisionRectangles.Add(new Rectangle(30, 40, 6, 24));
            collisionRectangles.Add(new Rectangle(35, 33, 21, 38));
            collisionRectangles.Add(new Rectangle(56, 47, 15, 10));
            collisionRectangles.Add(new Rectangle(67, 34, 46, 35));
            
            // Back leg things
            collisionRectangles.Add(new Rectangle(113, 37, 74, 11));
            collisionRectangles.Add(new Rectangle(113, 56, 74, 11));

            // Top fins
            collisionRectangles.Add(new Rectangle(74, 24, 55, 9));
            collisionRectangles.Add(new Rectangle(88, 22, 48, 2));
            collisionRectangles.Add(new Rectangle(87, 17, 12, 5));
            collisionRectangles.Add(new Rectangle(107, 11, 5, 9));
            collisionRectangles.Add(new Rectangle(114, 17, 5, 3));

            // Bottom fins
            collisionRectangles.Add(new Rectangle(75, 72, 53, 7));
            collisionRectangles.Add(new Rectangle(92, 77, 42, 5));
            collisionRectangles.Add(new Rectangle(87, 82, 12, 5));
            collisionRectangles.Add(new Rectangle(105, 82, 15, 4));
            collisionRectangles.Add(new Rectangle(107, 86, 5, 7));

            _weakSpotFront  = new BigShipWeakSpotFront (content, cellX, cellY, player, camera, this);
            _weakSpotBack   = new BigShipWeakSpotBack  (content, cellX, cellY, player, camera, this);
            _weakSpotTop    = new BigShipWeakSpotTop   (content, cellX, cellY, player, camera, this);
            _weakSpotBottom = new BigShipWeakSpotBottom(content, cellX, cellY, player, camera, this);

            ExtraEnemiesToAddAfterConstructor.Add(_weakSpotFront);
            ExtraEnemiesToAddAfterConstructor.Add(_weakSpotBack);
            ExtraEnemiesToAddAfterConstructor.Add(_weakSpotTop);
            ExtraEnemiesToAddAfterConstructor.Add(_weakSpotBottom);

            _shotLeft  = new BigShipShot(content, cellX, cellY, player, camera);
            _shotRight = new BigShipShot(content, cellX, cellY, player, camera);
            ExtraEnemiesToAddAfterConstructor.Add(_shotLeft);
            ExtraEnemiesToAddAfterConstructor.Add(_shotRight);
        }

        /// <summary>
        /// The additional rectangles are drawn on the original unscaled image.
        /// This will update them to account for the ship's current world location and 
        /// scale.
        /// </summary>
        private Rectangle GetShipAdjustedRectangle(Rectangle rect)
        {
            // Adjust for WorldLocation being the bottom middle point and the image being a subset of the original
            // image.
            var startingX = this.WorldLocation.X.ToInt() - (ShipSourceRect.Width / 2) - ShipSourceRect.X;
            var startingY = this.WorldLocation.Y.ToInt() - (ShipSourceRect.Height) - ShipSourceRect.Y;

            return new Rectangle(startingX + (rect.X * Game1.TileScale), 
                startingY + (rect.Y * Game1.TileScale), 
                rect.Width * Game1.TileScale, 
                rect.Height * Game1.TileScale);
        }

        private void Initialize()
        {
            float weakSpotDepth = DrawDepth - Game1.MIN_DRAW_INCREMENT;
            _weakSpotFront.SetDrawDepth(weakSpotDepth);
            _weakSpotBack.SetDrawDepth(weakSpotDepth);
            _weakSpotTop.SetDrawDepth(weakSpotDepth);
            _weakSpotBottom.SetDrawDepth(weakSpotDepth);

            float shotDepth = DrawDepth + Game1.MIN_DRAW_INCREMENT;
            _shotLeft.SetDrawDepth(shotDepth);
            _shotRight.SetDrawDepth(shotDepth);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }

            if (Alive)
            {
                if (!_hasBeenSeen)
                {
                    if (!IsOnScreen())
                    {
                        base.Update(gameTime, elapsed);
                        return;
                    }
                    _hasBeenSeen = true;
                    _normalY = worldLocation.Y;
                }

                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = _maxHealth;
                Game1.BossHealth = Health;
                Game1.BossName = "Big Ship";

                const int flipShipDistance = 264;
                if (!_player.IsShipFlipped && _player.WorldLocation.X > CollisionRectangle.Right + flipShipDistance)
                {
                    _player.FlipShip();
                }

                else if (_player.IsShipFlipped && _player.WorldLocation.X < CollisionRectangle.Left - flipShipDistance)
                {
                    _player.FlipShip();
                }

                // Slide the ship above/below based on camera center X relative to the ship.
                // Triangle: 0 at shipLeft, peak at shipCenterX, back to 0 at shipRight.
                var camCenterX = (Game1.Camera.ViewPort.Left + Game1.Camera.ViewPort.Right) / 2f;
                var shipLeft = (float)CollisionRectangle.Left;
                var shipRight = (float)CollisionRectangle.Right;
                var shipCenterX = (shipLeft + shipRight) / 2f;
                const float maxOffset = 80;

                float t;
                if (camCenterX <= shipLeft || camCenterX >= shipRight)
                {
                    t = 0f;
                }
                else if (camCenterX <= shipCenterX)
                {
                    t = (camCenterX - shipLeft) / (shipCenterX - shipLeft);
                }
                else
                {
                    t = 1f - (camCenterX - shipCenterX) / (shipRight - shipCenterX);
                }

                float bob = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.2f) * 16f;
                worldLocation.Y = _normalY + (_player.IsShipFlipped ? maxOffset : -maxOffset) * t + bob;

                _weakSpotFront.WorldLocation  = worldLocation + WeakSpotFrontOffset;
                _weakSpotBack.WorldLocation   = worldLocation + WeakSpotBackOffset;
                _weakSpotTop.WorldLocation    = worldLocation + WeakSpotTopOffset;
                _weakSpotBottom.WorldLocation = worldLocation + WeakSpotBottomOffset;

                _shotTimer += elapsed;
                if (_shotTimer >= ShotInterval)
                {
                    _shotTimer = 0f;
                    var shotLocation = CollisionCenter + new Vector2(0, 12 * Game1.TileScale);
                    _shotLeft.Launch(shotLocation + new Vector2(-260, 0), goLeft: true);
                    _shotRight.Launch(shotLocation, goLeft: false);
                }
            }
            else
            {
                Game1.DrawBossHealth = false;
            }

            // The extra collision rectangles will break shots and bombs, but the enemy won't take damange.
            foreach (var rawRect in collisionRectangles)
            {
                var rect = GetShipAdjustedRectangle(rawRect);
                if (rect.Intersects(_player.CollisionRectangle))
                {
                    _player.TakeHit(this);
                }

                foreach (var shot in _player.Shots.RawList)
                {
                    if (shot.Enabled && shot.CollisionRectangle.Intersects(rect))
                    {
                        shot.Break();
                    }
                }

                foreach (var bomb in _player.Bombs.RawList)
                {
                    if (bomb.Enabled && bomb.CollisionRectangle.Intersects(rect))
                    {
                        bomb.Break();
                    }
                }
            }

            

            base.Update(gameTime, elapsed);
        }

        public override void Kill()
        {
            Game1.DrawBossHealth = false;
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw Collision Rectangle in reddish
            if (DrawCollisionRect || Game1.DrawAllCollisionRects)
            {
                Color color = Color.Red * 0.25f;

                foreach (var rawRect in collisionRectangles)
                {
                    var rect = GetShipAdjustedRectangle(rawRect);
                    spriteBatch.Draw(Game1.TileTextures, rect, Game1.WhiteSourceRect, color);
                }

            }

            base.Draw(spriteBatch);
        }
    }
}
