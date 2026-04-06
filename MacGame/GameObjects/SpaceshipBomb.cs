using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    public class SpaceshipBomb : GameObject
    {
        private Player _player;

        private float AirBrake = 300f;

        public SpaceshipBomb(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(8, 6);

            SetCenteredCollisionRectangle(8, 8, 4, 4);

            IsAffectedByGravity = true;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                if (velocity.X > 0)
                {
                    velocity.X -= AirBrake * elapsed;
                    if (velocity.X < 0)
                    {
                        velocity.X = 0;
                    }

                }
                else if (velocity.X < 0)
                {
                    velocity.X += AirBrake * elapsed;
                    if (velocity.X > 0)
                    {
                        velocity.X = 0;
                    }
                }

                if (Game1.Camera.IsWayOffscreen(CollisionRectangle))
                {
                    Disable();
                }

                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionCenter);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    this.Break();
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                DisplayComponent.Draw(spriteBatch, WorldLocation, Flipped);
            }
        }

        public void Break()
        {
            Disable();
            EffectsManager.EnemyPop(CollisionCenter, 4, Pallette.DarkGreen, 120f);
            SoundManager.PlaySound("Break");
        }

        private void Disable()
        {
            Enabled = false;
            _player.Bombs.ReturnObject(this);
        }
    }
}
