using System.Collections.Generic;
using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class EnemyShip2 : EnemyShipBase
    {
        private const float MissileInterval = 2f;
        private const int MissilePoolSize = 4;

        private float missileTimer = MissileInterval;
        private List<HomingMissile> missilePool = new List<HomingMissile>();

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public EnemyShip2(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(4, 1), 1, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            Attack = 1;
            SetInitialHealth(5);

            SetCenteredCollisionRectangle(8, 8, 8, 8);

            Behavior = new EnemyShipBehavior(40, camera);

            for (int i = 0; i < MissilePoolSize; i++)
            {
                var missile = new HomingMissile(content, cellX, cellY, player, camera);
                missile.Enabled = false;
                missilePool.Add(missile);
                AddEnemyInConstructor(missile);
            }
        }

        private void LaunchMissile()
        {
            foreach (var missile in missilePool)
            {
                if (!missile.Enabled)
                {
                    missile.Launch(CollisionCenter);
                    return;
                }
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive && IsOnScreen())
            {
                missileTimer -= elapsed;
                if (missileTimer <= 0f)
                {
                    LaunchMissile();
                    missileTimer = MissileInterval;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
