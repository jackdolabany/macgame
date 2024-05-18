using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Cannon : GameObject
    {
        protected Player _player;

        bool canAcceptPlayer = false;
        const float cooldownTimeLimit = 0.5f;
        float cooldownTimer = 0.0f;

        public Cannon(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetCenteredCollisionRectangle(8, 8);

            _player = player;

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            this.DisplayComponent = new StaticImageDisplay(textures, Helpers.GetBigTileRect(3, 2));
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (canAcceptPlayer && this.CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                _player.CannonYouAreIn = this;
            }

            if(!canAcceptPlayer)
            {
                cooldownTimer += elapsed;
                if (cooldownTimer >= cooldownTimeLimit)
                {
                    canAcceptPlayer = true;
                    cooldownTimer = 0f;
                }   
            }

            base.Update(gameTime, elapsed);
        }

        public void Shoot()
        {
            _player.CannonYouAreIn = null;
            _player.Velocity = new Vector2(600, 0);

            // Don't allow the player to enter for a bit. This is so you don't just go right back in.
            cooldownTimer = 0f;
            canAcceptPlayer = false;
        }

    }
}
