using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{

    public enum RotationDirection
    {
        Right = 0,
        DownRight = 1,
        Down = 2,
        DownLeft = 3,
        Left = 4,
        UpLeft = 5,
        Up = 6,
        UpRight = 7
    }

    public class Cannon : GameObject
    {
        protected Player _player;

        bool canAcceptPlayer = true;
        float cooldownTimer = 0.0f;

        /// <summary>
        /// True if the cannon rotates when the player is in it.
        /// </summary>
        public bool IsRotating { get; set; }

        float rotationTime = 0.35f;
        float rotateTimer = 0f;

        RotationDirection RotationDirection = RotationDirection.Up;

        private RotationDirection? _autoShootDirection;

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

        /// <summary>
        /// Set this if you want the cannon to automatically shoot in a direction with no player control.
        /// </summary>
        public RotationDirection? AutoShootDirection 
        {
            get
            {
                return _autoShootDirection;
            }
            set
            {
                _autoShootDirection = value;
                if (value.HasValue)
                {
                    // Speed it up if it's auto rotating.
                    rotationTime /= 2;
                }
            } 
        }

        public bool PlayerCanShootOut
        {
            get
            {
                return AutoShootDirection == null && inputDelayTimer <= 0f;
            }
        }

        Vector2 ShootDirection
        {
            get
            {
                switch (RotationDirection)
                {
                    case RotationDirection.Right:
                        return new Vector2(1, 0);
                    case RotationDirection.DownRight:
                        return new Vector2(1, 1);
                    case RotationDirection.Down:
                        return new Vector2(0, 1);
                    case RotationDirection.DownLeft:
                        return new Vector2(-1, 1);
                    case RotationDirection.Left:
                        return new Vector2(-1, 0);
                    case RotationDirection.UpLeft:
                        return new Vector2(-1, -1);
                    case RotationDirection.Up:
                        return new Vector2(0, -1);
                    case RotationDirection.UpRight:
                        return new Vector2(1, -1);
                    default:
                        return Vector2.Zero;
                }
            }
        }

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

            if (_player.CannonYouAreIn == null && canAcceptPlayer && this.CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                _player.EnterCannon(this);

                // Sounds weird, not sound for now.
                // SoundManager.PlaySound("EnterCannon", 0.3f);

                inputDelayTimer = 0.4f;

                if (AutoShootDirection == RotationDirection)
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
                    canAcceptPlayer = true;
                    cooldownTimer = 0f;
                }   
            }

            RotationDirection? rotateTarget = null;
            if (_player.CannonYouAreIn != this)
            {
                rotateTarget = RotationDirection.Up;
            }
            else if (AutoShootDirection != null)
            {
                rotateTarget = AutoShootDirection;
            }

            bool rotateWithPlayerInside = IsRotating && _player.CannonYouAreIn == this && !AutoShootDirection.HasValue;

            if (rotateWithPlayerInside || (rotateTarget.HasValue && rotateTarget != this.RotationDirection))
            {
                rotateTimer += elapsed;
                if (rotateTimer >= rotationTime)
                {
                    rotateTimer -= rotationTime;

                    // Rotate the fastest way, clockwise or counterclockwise.
                    var isRotationClockwise = true;

                    if (rotateTarget.HasValue)
                    {
                        var diff = (int)rotateTarget - (int)this.RotationDirection;
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
                        this.RotationDirection += 1;
                    }
                    else
                    {
                        this.RotationDirection -= 1;
                    }

                    // In case we've rotate too far in either direction, reset it back to keep
                    // the numbers 0 to 7
                    if((int)this.RotationDirection == 8)
                    {
                        this.RotationDirection = RotationDirection.Right;
                    }
                    else if ((int)this.RotationDirection == -1)
                    {
                        this.RotationDirection = RotationDirection.UpRight;
                    }
                }
            }

            if (_player.CannonYouAreIn == this && AutoShootDirection == this.RotationDirection && delayAutoShotTimer <= 0f)
            {
                Shoot();
            }

            var image = (StaticImageDisplay)this.DisplayComponent;
            image.Rotation = 0;
            image.Effect = SpriteEffects.None;

            switch (this.RotationDirection)
            {               
                case RotationDirection.Right:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    break;
                case RotationDirection.DownRight:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    break;
                case RotationDirection.Down:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Rotation = MathHelper.PiOver2;
                    image.Effect = SpriteEffects.FlipVertically;
                    break;
                case RotationDirection.DownLeft:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case RotationDirection.Left:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case RotationDirection.UpLeft:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.FlipHorizontally;
                    image.Rotation = MathHelper.PiOver2;
                    break;
                case RotationDirection.Up:
                    image.Source = Helpers.GetBigTileRect(3, 2);
                    image.Rotation = -MathHelper.PiOver2;
                    break;
                case RotationDirection.UpRight:
                    image.Source = Helpers.GetBigTileRect(4, 2);
                    image.Effect = SpriteEffects.None;
                    image.Rotation = MathHelper.Pi + MathHelper.PiOver2;
                    break;
            }

            base.Update(gameTime, elapsed);

            // The image is extra large compared to the hitbox so we need to offset it a bit.
            image.WorldLocation += new Vector2(0, Game1.TileSize / 2);
           
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public void Shoot()
        {
            // _player.CannonYouAreIn = null;

            var direction = ShootDirection;
            direction.Normalize();

            Vector2 velocity = direction * 600;

            _player.ShootOutOfCannon(this, velocity);

            // Don't allow the player to enter for a bit. This is so you don't just go right back in.
            cooldownTimer = 0.5f;
            canAcceptPlayer = false;

            SoundManager.PlaySound("ShootFromCannon", 0.5f);

        }

    }
}
