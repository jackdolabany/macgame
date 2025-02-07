using MacGame.DisplayComponents;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{

    public class Cannon : GameObject
    {

        public string Name { get; set; }

        protected Player _player;

        bool isCooledOff = true;
        float cooldownTimer = 0.0f;

        /// <summary>
        /// True if the cannon rotates when the player is in it.
        /// </summary>
        public bool IsRotating { get; set; }

        float rotationTime = 0.35f;
        float rotateTimer = 0f;

        EightWayRotation RotationDirection = new EightWayRotation(EightWayRotationDirection.Up);

        /// <summary>
        /// If a cannon shoots you automatically this will delay it a bit. especially if it's going to auto shoot you in the 
        /// same direction it's facing.
        /// </summary>
        float delayAutoShotTimer = 0f;

        /// <summary>
        /// Normally a cannon shoots you for a bit. If the cannon is a "Super Shot" it will shoot you until
        /// you hit a wall or another cannon. This is used for getting around the map or something.
        /// </summary>
        public bool IsSuperShot { get; set; }

        /// <summary>
        /// Once Mac enters a cannon delay his inputs so he can't insta shoot right out.
        /// </summary>
        float inputDelayTimer = 0.0f;

        private EightWayRotation? _autoShootDirection;
        /// <summary>
        /// Set this if you want the cannon to automatically shoot in a direction with no player control.
        /// </summary>
        public EightWayRotation? AutoShootDirection 
        {
            get
            {
                return _autoShootDirection;
            }
            set
            {
                _autoShootDirection = value;
                if (value != null)
                {
                    // Speed it up if it's auto rotating.
                    rotationTime /= 2;
                }
            } 
        }

        public Cannonball CannonballInside;

        public bool HasPlayerInside
        {
            get
            {
                return Game1.Player.CannonYouAreIn == this;
            }
        }

        public bool HasCannonballInside
        {
            get
            {
                return CannonballInside != null;
            }
        }

        public bool PlayerCanShootOut
        {
            get
            {
                return AutoShootDirection == null && inputDelayTimer <= 0f;
            }
        }

        /// <summary>
        /// The cannon will automatically shoot after some time if it has a cannon ball in it.
        /// </summary>
        private float _shootCannonballTimer = 0f;

        public Cannon(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetCenteredCollisionRectangle(8, 8);

            _player = player;

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            this.DisplayComponent = new StaticImageDisplay(textures, Helpers.GetBigTileRect(3, 2));

            // Temp, get this from the map.
            IsRotating = true;

            DisplayComponent.Offset += new Vector2(0, Game1.TileSize / 2);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (inputDelayTimer > 0)
            {
                inputDelayTimer -= elapsed;
            }

            if (delayAutoShotTimer > 0)
            {
                delayAutoShotTimer -= elapsed;
            }

            if (_player.CanEnterCannon && isCooledOff && !HasCannonballInside && this.CollisionRectangle.Contains(_player.SmallerCollisionRectangle))
            {
                _player.EnterCannon(this);

                // Sounds weird, not sound for now.
                // SoundManager.PlaySound("EnterCannon", 0.3f);

                inputDelayTimer = 0.4f;

                if (AutoShootDirection.HasValue && AutoShootDirection.Value.Direction == RotationDirection.Direction)
                {
                    delayAutoShotTimer = 0.3f;
                }
                else
                {
                    delayAutoShotTimer = 0.0f;
                }
            }

            if (cooldownTimer > 0)
            {
                cooldownTimer -= elapsed;
                if (cooldownTimer <= 0)
                {
                    isCooledOff = true;
                    cooldownTimer = 0f;
                }   
            }

            EightWayRotation? rotateTarget = null;
            if (!HasPlayerInside && !HasCannonballInside)
            {
                rotateTarget = new EightWayRotation(EightWayRotationDirection.Up);
            }
            else if (AutoShootDirection != null)
            {
                rotateTarget = AutoShootDirection;
            }

            bool rotate = IsRotating && (HasPlayerInside || HasCannonballInside) && !AutoShootDirection.HasValue;

            if (rotate || (rotateTarget.HasValue && rotateTarget.Value.Direction != this.RotationDirection.Direction))
            {
                rotateTimer += elapsed;
                if (rotateTimer >= rotationTime)
                {
                    rotateTimer -= rotationTime;

                    // Rotate the fastest way, clockwise or counterclockwise.
                    var isRotationClockwise = true;

                    if (rotateTarget.HasValue)
                    {
                        var diff = (int)rotateTarget.Value.Direction - (int)this.RotationDirection.Direction;
                        if (diff < 0)
                        {
                            diff += 8;
                        }

                        if (diff > 4)
                        {
                            isRotationClockwise = false;
                        }
                    }

                    if (isRotationClockwise)
                    {
                        this.RotationDirection.MoveClockwise();
                    }
                    else
                    {
                        this.RotationDirection.MoveCounterClockwise();
                    }
                }
            }

            if (_player.CannonYouAreIn == this && AutoShootDirection.HasValue && AutoShootDirection.Value.Direction == this.RotationDirection.Direction && delayAutoShotTimer <= 0f)
            {
                Shoot();
            }

            var image = (StaticImageDisplay)this.DisplayComponent;
            image.Rotation = 0;
            image.Effect = SpriteEffects.None;

            switch (this.RotationDirection.Direction)
            {               
                case EightWayRotationDirection.Right:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    break;
                case EightWayRotationDirection.DownRight:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    break;
                case EightWayRotationDirection.Down:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Rotation = MathHelper.PiOver2;
                    image.Effect = SpriteEffects.FlipVertically;
                    break;
                case EightWayRotationDirection.DownLeft:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case EightWayRotationDirection.Left:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case EightWayRotationDirection.UpLeft:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    image.Rotation = MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.Up:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Rotation = -MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.UpRight:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.None;
                    image.Rotation = MathHelper.Pi + MathHelper.PiOver2;
                    break;
            }

            // Shoot eventually if the player doesn't trigger a shot with a button or something.
            if (HasCannonballInside)
            {
                _shootCannonballTimer += elapsed;
                if (_shootCannonballTimer > 10f)
                {
                    Shoot();
                }
            }

            base.Update(gameTime, elapsed);

            // The image is extra large compared to the hitbox so we need to offset it a bit.
           
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;

            base.Draw(spriteBatch);
        }

        public void LoadCannonball(Cannonball ball)
        {
            CannonballInside = ball;
            SoundManager.PlaySound("Dig");
            _shootCannonballTimer = 0f;
        }

        public void Shoot()
        {
            var direction = RotationDirection.Vector2;

            Vector2 velocity = direction * 700;

            if (HasPlayerInside)
            {
                _player.ShootOutOfCannon(this, velocity);
            }
            else if (HasCannonballInside)
            {
                CannonballInside.ShootOutOfCannon(velocity);
            }
            else
            {
                return;
            }

            // Don't allow the player to enter for a bit. This is so you don't just go right back in.
            cooldownTimer = 0.5f;
            isCooledOff = false;

            SoundManager.PlaySound("ShootFromCannon", 0.5f);

        }

        public bool CanAcceptCannonball()
        {
            return isCooledOff && !HasPlayerInside && !HasCannonballInside;
        }

    }
}
