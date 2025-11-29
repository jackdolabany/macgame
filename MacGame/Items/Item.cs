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

        public Item(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            _player = player;
        }

        protected virtual void Collect(Player player)
        {
            WhenCollected(player);
            PlayCollectedSound();
        }

        public abstract void WhenCollected(Player player);

        public virtual void PlayCollectedSound()
        {
            SoundManager.PlaySound("PowerUp");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
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
