using MacGame.Behaviors;
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

            var enemyShipBehavior = new EnemyShipBehavior(40, camera, player);
            enemyShipBehavior.SetupShootAtPlayer(FireInterval, ShotSpeed, ShotSize.Medium);
            Behavior = enemyShipBehavior;
        }
    }
}
