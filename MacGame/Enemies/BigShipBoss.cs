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
        private static readonly Rectangle ShipSourceRect = new Rectangle(22 * Game1.TileScale, 10 * Game1.TileScale, 176 * Game1.TileScale, 84 * Game1.TileScale);

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

        private ShootEverywhereCannon _shootEverywhereFrontTop;
        private ShootEverywhereCannon _shootEverywhereFrontBottom;

        private MiniSpaceCannon _miniCannonFrontTop;
        private MiniSpaceCannon _miniCannonFrontBottom;

        private BomberCarriage _bomberCarriage;
        private BigShipSatellite _satellite;

        private MiniSpaceCannon _miniSpaceCannonTopFinOne;
        private MiniSpaceCannon _miniSpaceCannonTopFinTwo;
        private MiniSpaceCannon _miniSpaceCannonTopFinThree;
        private MiniSpaceCannon _miniSpaceCannonBottomFinTwo;
        private MiniSpaceCannon _miniSpaceCannonBottomFinThree;

        private ShootEverywhereCannon _shootEverywhereMiddleTop;
        
        private BigShipHomingLauncher _topBigShipHomingLauncher;
        private BigShipHomingLauncher _bottomBigShipHomingLauncher;

        private MiniSpaceCannon _miniCannonUnderCarriageOne;
        private MiniSpaceCannon _miniCannonUnderCarriageTwo;

        // Offsets from WorldLocation — tune these to match the ship art.
        private Vector2 WeakSpotFrontOffset = new Vector2(-330, -136);
        private Vector2 WeakSpotBackOffset = new Vector2(22, -136);
        private Vector2 WeakSpotTopOffset = new Vector2(-22, -276);
        private Vector2 WeakSpotBottomOffset = new Vector2(-22, 4);

        private Vector2 BomberCarriageOffset = new Vector2(-40, 20);

        private float _explosionTimer = 0f;
        private const float ExplosionInterval = 0.1f;

        public BigShipBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var texture = content.Load<Texture2D>(@"Textures\BigShip");

            DisplayComponent = new StaticImageDisplay(texture, ShipSourceRect);
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0f;
            Attack = 1;
            Health = 100;
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

            _shootEverywhereFrontTop    = new ShootEverywhereCannon(content, cellX, cellY, player, camera);
            _shootEverywhereFrontBottom = new ShootEverywhereCannon(content, cellX, cellY, player, camera);
            _shootEverywhereFrontBottom.UpsideDown = true;
            ExtraEnemiesToAddAfterConstructor.Add(_shootEverywhereFrontTop);
            ExtraEnemiesToAddAfterConstructor.Add(_shootEverywhereFrontBottom);

            _miniCannonFrontTop    = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniCannonFrontBottom = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniCannonFrontBottom.UpsideDown = true;
            ExtraEnemiesToAddAfterConstructor.Add(_miniCannonFrontTop);
            ExtraEnemiesToAddAfterConstructor.Add(_miniCannonFrontBottom);

            _bomberCarriage = new BomberCarriage(content, cellX, cellY, player, camera, this);
            _satellite = new BigShipSatellite(content, cellX, cellY, player, camera, this);

            // To be enabled when seen.
            _bomberCarriage.Enabled = false;
            _satellite.Enabled = false;

            ExtraEnemiesToAddAfterConstructor.Add(_bomberCarriage);
            ExtraEnemiesToAddAfterConstructor.Add(_satellite);

            // Add three minispace cannons along the bottom and top back fins
            _miniSpaceCannonTopFinOne = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniSpaceCannonTopFinTwo = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniSpaceCannonTopFinThree = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniSpaceCannonBottomFinTwo = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniSpaceCannonBottomFinTwo.UpsideDown = true;
            _miniSpaceCannonBottomFinThree = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniSpaceCannonBottomFinThree.UpsideDown = true;
            ExtraEnemiesToAddAfterConstructor.Add(_miniSpaceCannonTopFinOne);
            ExtraEnemiesToAddAfterConstructor.Add(_miniSpaceCannonTopFinTwo);
            ExtraEnemiesToAddAfterConstructor.Add(_miniSpaceCannonTopFinThree);
            ExtraEnemiesToAddAfterConstructor.Add(_miniSpaceCannonBottomFinTwo);
            ExtraEnemiesToAddAfterConstructor.Add(_miniSpaceCannonBottomFinThree);

            _shootEverywhereMiddleTop = new ShootEverywhereCannon(content, cellX, cellY, player, camera);
            ExtraEnemiesToAddAfterConstructor.Add(_shootEverywhereMiddleTop);

            _topBigShipHomingLauncher = new BigShipHomingLauncher(content, cellX, cellY, player, camera);
            _bottomBigShipHomingLauncher = new BigShipHomingLauncher(content, cellX, cellY, player, camera);
            _bottomBigShipHomingLauncher.FlipUpsideDown();
            ExtraEnemiesToAddAfterConstructor.Add(_topBigShipHomingLauncher);
            ExtraEnemiesToAddAfterConstructor.Add(_bottomBigShipHomingLauncher);


            _miniCannonUnderCarriageOne = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniCannonUnderCarriageOne.UpsideDown = true;
            ExtraEnemiesToAddAfterConstructor.Add(_miniCannonUnderCarriageOne);
            _miniCannonUnderCarriageTwo = new MiniSpaceCannon(content, cellX, cellY, player, camera);
            _miniCannonUnderCarriageTwo.UpsideDown = true;
            ExtraEnemiesToAddAfterConstructor.Add(_miniCannonUnderCarriageTwo);

            // Disable these guys, they'll be re-enabled when you destroy the carriage
            _miniCannonUnderCarriageOne.Enabled = false;
            _miniCannonUnderCarriageTwo.Enabled = false;
            _bottomBigShipHomingLauncher.Enabled = false;

            // Disable these until you destroy the Satelitte.
            _miniSpaceCannonTopFinOne.Enabled = false;
            _miniSpaceCannonTopFinTwo.Enabled = false;
            _topBigShipHomingLauncher.Active = false;

        }

        /// <summary>
        /// Takes a point on the original unscaled ship image and adjusts it based on the 
        /// Ship's current location, WorldLocation offset, and scale.
        /// </summary>
        private Vector2 GetShipAdjustedPosition(int x, int y)
        {
            // Adjust for WorldLocation being the bottom middle point and the image being a subset of the original
            // image.
            var startingX = this.WorldLocation.X - (ShipSourceRect.Width / 2) - ShipSourceRect.X;
            var startingY = this.WorldLocation.Y - (ShipSourceRect.Height) - ShipSourceRect.Y;
            return new Vector2(startingX, startingY) + new Vector2(x, y) * Game1.TileScale;
        }

        /// <summary>
        /// The additional rectangles are drawn on the original unscaled image.
        /// This will update them to account for the ship's current world location and 
        /// scale.
        /// </summary>
        private Rectangle GetShipAdjustedRectangle(Rectangle rect)
        {
            var position = GetShipAdjustedPosition(rect.X, rect.Y);

            return new Rectangle(position.X.ToInt(), 
                position.Y.ToInt(), 
                rect.Width * Game1.TileScale, 
                rect.Height * Game1.TileScale);
        }

        private void Initialize()
        {
            // weak spots in front of the main ship
            float weakSpotDepth = DrawDepth - Game1.MIN_DRAW_INCREMENT;
            _weakSpotFront.SetDrawDepth(weakSpotDepth);
            _weakSpotBack.SetDrawDepth(weakSpotDepth);
            _weakSpotTop.SetDrawDepth(weakSpotDepth);
            _weakSpotBottom.SetDrawDepth(weakSpotDepth);

            // Shots are behind so it seems like they are coming out of the weak spots.
            float shotDepth = DrawDepth + Game1.MIN_DRAW_INCREMENT;
            _shotLeft.SetDrawDepth(shotDepth);
            _shotRight.SetDrawDepth(shotDepth);

            // Cannons are in front for now.
            float gunDepth = DrawDepth - Game1.MIN_DRAW_INCREMENT;
            _shootEverywhereFrontTop.SetDrawDepth(gunDepth);
            _shootEverywhereFrontBottom.SetDrawDepth(gunDepth);
            _miniCannonFrontTop.SetDrawDepth(gunDepth);
            _miniCannonFrontBottom.SetDrawDepth(gunDepth);
            _miniSpaceCannonBottomFinTwo.SetDrawDepth(gunDepth);
            _miniSpaceCannonBottomFinThree.SetDrawDepth(gunDepth);
            _shootEverywhereMiddleTop.SetDrawDepth(gunDepth);
            _topBigShipHomingLauncher.SetDrawDepth(gunDepth);
            _miniSpaceCannonTopFinOne.SetDrawDepth(gunDepth);
            _miniSpaceCannonTopFinTwo.SetDrawDepth(gunDepth);
            _miniSpaceCannonTopFinThree.SetDrawDepth(gunDepth);
            _bottomBigShipHomingLauncher.SetDrawDepth(gunDepth);

            // carriage and satellite should be behind the main ship
            _bomberCarriage.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            _satellite.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
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

                    _bomberCarriage.Enabled = true;
                    _satellite.Enabled = true;

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
                const float maxOffset = 150;

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

                _shotTimer += elapsed;
                if (_shotTimer >= ShotInterval)
                {
                    _shotTimer = 0f;
                    var shotLocation = CollisionCenter + new Vector2(0, 12 * Game1.TileScale);
                    _shotLeft.Launch(shotLocation + new Vector2(-260, 0), goLeft: true);
                    _shotRight.Launch(shotLocation, goLeft: false);
                    SoundManager.PlaySound("BigShipShot");
                }
            }
            else
            {
                Game1.DrawBossHealth = false;
            }

            // The extra collision rectangles will break shots and bombs, but the enemy won't take damange.
            if (Alive)
            {
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
            }

            base.Update(gameTime, elapsed);

            _weakSpotFront.WorldLocation = worldLocation + WeakSpotFrontOffset;
            _weakSpotBack.WorldLocation = worldLocation + WeakSpotBackOffset;
            _weakSpotTop.WorldLocation = worldLocation + WeakSpotTopOffset;
            _weakSpotBottom.WorldLocation = worldLocation + WeakSpotBottomOffset;

            _shootEverywhereFrontTop.WorldLocation = GetShipAdjustedPosition(34, 42);
            _shootEverywhereFrontBottom.WorldLocation = GetShipAdjustedPosition(34, 70);
            _miniCannonFrontTop.WorldLocation = GetShipAdjustedPosition(48, 34);
            _miniCannonFrontBottom.WorldLocation = GetShipAdjustedPosition(48, 78);

            _miniSpaceCannonTopFinOne.WorldLocation = GetShipAdjustedPosition(139, 37);
            _miniSpaceCannonTopFinTwo.WorldLocation = GetShipAdjustedPosition(158, 36);
            _miniSpaceCannonTopFinThree.WorldLocation = GetShipAdjustedPosition(178, 35);
            //_miniSpaceCannonBottomFinOne.WorldLocation = GetShipAdjustedPosition(139, 75);
            _miniSpaceCannonBottomFinTwo.WorldLocation = GetShipAdjustedPosition(166, 76);
            _miniSpaceCannonBottomFinThree.WorldLocation = GetShipAdjustedPosition(178, 77);

            _shootEverywhereMiddleTop.WorldLocation = GetShipAdjustedPosition(91, 19);

            // Homing missile launchers
            _topBigShipHomingLauncher.WorldLocation = GetShipAdjustedPosition(128, 22);
            _bottomBigShipHomingLauncher.WorldLocation = GetShipAdjustedPosition(91, 95);

            // Cannons hidden behind the carraige thing
            _miniCannonUnderCarriageOne.WorldLocation = GetShipAdjustedPosition(75, 87);
            _miniCannonUnderCarriageTwo.WorldLocation = GetShipAdjustedPosition(110, 101);

            if (_bomberCarriage.Alive)
            {
                _bomberCarriage.WorldLocation = worldLocation + BomberCarriageOffset;
            }

            if (_satellite.Enabled)
            {
                _satellite.WorldLocation = GetShipAdjustedPosition(147, 37) + new Vector2(0, 3);
            }

            if (Dead)
            {
                this.Velocity = new Vector2(-50, 70);

                if (IsOnScreen())
                {
                    _explosionTimer -= elapsed;
                    if (_explosionTimer <= 0f)
                    {
                        _explosionTimer = ExplosionInterval;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), true);
                    }
                }
            }

        }

        public void HandleSatelliteDestroyed()
        {
            if (Alive)
            {
                _miniSpaceCannonTopFinOne.Enabled = true;
                _miniSpaceCannonTopFinTwo.Enabled = true;
                _topBigShipHomingLauncher.Active = true;
            }
        }

        public void HandleCarriageDestroyed()
        {
            if (Alive)
            {
                _miniCannonUnderCarriageOne.Enabled = true;
                _miniCannonUnderCarriageTwo.Enabled = true;
                _bottomBigShipHomingLauncher.Enabled = true;
            }
        }

        public override void Kill()
        {
            Game1.DrawBossHealth = false;
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();
            if (_player.IsShipFlipped)
            {
                _player.FlipShip();
            }

            foreach (var enemy in ExtraEnemiesToAddAfterConstructor)
            {
                if (enemy.Alive && enemy.Enabled)
                {
                    var blowUpTimer = Game1.Randy.NextFloat();
                    TimerManager.AddNewTimer(blowUpTimer, () => enemy.Kill());
                }
            }
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
