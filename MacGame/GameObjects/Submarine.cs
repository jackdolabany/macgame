using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Submarine : GameObject
    {
        private Player _player;

        /// <summary>
        /// Used to temporarily block the player from entering after he leaves the sub, until he stops
        /// colliding with it.
        /// </summary>
        bool allowPlayerIn = false;
        
        public Rectangle RelativeCollisionRectangle
        {
            get
            {
                return collisionRectangle;
            }
        }
      
        public Submarine(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var image = new StaticImageDisplay(textures, Helpers.GetBigTileRect(7, 3));
            this.DisplayComponent = image;

            this.CollisionRectangle = new Rectangle((-5 * Game1.TileScale), -11 * Game1.TileScale, 10 * Game1.TileScale, 10 * Game1.TileScale);

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            _player = player;

            IsAffectedByGravity = true;
            IsAbleToSurviveOutsideOfWorld = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled && allowPlayerIn && !_player.IsInSub)
            {
                if (this.CollisionRectangle.Contains(_player.WorldCenter))
                {
                    PlayerEnter();
                    _player.EnterSub(this);
                }
            }

            // After leaving the sub you don't allow the player in until he stops colliding with the sub.
            if (!allowPlayerIn && !this.CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                allowPlayerIn = true;
            }

            base.Update(gameTime, elapsed);
        }

        public override Vector2 Gravity
        {
            get
            {
                var centerSquare = Game1.CurrentLevel.Map.GetMapSquareAtPixel(this.CollisionCenter);
                var isInWater = centerSquare != null && centerSquare.IsWater;

                if (isInWater)
                {
                    return Game1.WaterGravity;
                }
                else
                {
                    return Game1.EarthGravity;
                }
            }
        }

        public void PlayerEnter()
        {
            this.Enabled = false;
            SoundManager.PlaySound("PowerUp");
        }

        public void PlayerExit()
        {
            this.Enabled = true;
            allowPlayerIn = false;
            this.WorldLocation = _player.WorldLocation;
        }
    }
}
