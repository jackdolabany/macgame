using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigRedShip : EnemyShipBase
    {
        private const float FireInterval = 2f;
        private const float ShotSpeed = 150f;

        public BigRedShip(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var animations = new AnimationDisplay();
            DisplayComponent = animations;

            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(12, 9), 4, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.3f;
            animations.Add(idle);

            animations.Play("idle");

            SetInitialHealth(6);
            Attack = 1;

            SetCenteredCollisionRectangle(16, 16, 12, 12);

            var enemyShipBehavior = new EnemyShipBehavior(40, camera, player);
            enemyShipBehavior.SetupShootAtPlayer(FireInterval, ShotSpeed, ShotSize.Medium);
            Behavior = enemyShipBehavior;
        }

        protected override void PlayDeathEffects()
        {
            PlayMultiExplosionDeathEffect();
        }
    }
}
