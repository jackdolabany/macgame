using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class FishShip : EnemyShipBase
    {
        private readonly Player _player;

        private float _fireTimer;
        private const float FireInterval = 2f;
        private const float ShotSpeed = 150f;

        public FishShip(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetBigTileRect(14, 2));

            SetInitialHealth(6);
            Attack = 1;

            SetCenteredCollisionRectangle(16, 16, 12, 12);

            Velocity = new Vector2(-30f, 0f);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            Velocity = Vector2.Zero;

            if (!Alive || !Game1.Camera.IsObjectVisible(CollisionRectangle) || !Enabled)
            {
                base.Update(gameTime, elapsed);
                return;
            }

            _fireTimer += elapsed;
            if (_fireTimer >= FireInterval)
            {
                _fireTimer = 0f;
                var direction = Vector2.Normalize(_player.WorldCenter - WorldCenter);
                ShotManager.FireMediumShot(WorldCenter, direction * ShotSpeed, this);
                SoundManager.PlaySound("Shoot");
            }

            base.Update(gameTime, elapsed);
        }
    }
}
