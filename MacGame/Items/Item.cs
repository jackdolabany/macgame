using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    public abstract class Item : GameObject
    {
        /// <summary>
        /// This is always expected to be a static image.
        /// </summary>
        public StaticImageDisplay ItemIcon
        {
            get
            {
                return (StaticImageDisplay)this.DisplayComponent;
            }
        }

        protected Player _player;

        private bool _isInitailized = false;

        public Item(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            _player = player;
        }

        public virtual void Collect(Player player)
        {
            PlayCollectedSound();
        }

        public virtual void PlayCollectedSound()
        {
            SoundManager.PlaySound("PowerUp");
        }


        protected virtual void Initialize() { }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitailized)
            {
                Initialize();
                _isInitailized = true;
            }

            if (Enabled)
            {
                // Check for player/item collision.
                if (_player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    this.Collect(_player);
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void SetDrawDepth(float depth)
        {
            this.DisplayComponent.DrawDepth = depth;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;

            base.Draw(spriteBatch);
        }
    }
}
