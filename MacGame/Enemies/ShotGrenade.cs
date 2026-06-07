using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class ShotGrenade : Enemy
    {
        public float Fuse = 2f;
        private float _fuseTimer;
        private const float ShotSpeed = 220f;
        private const int SpriteSize = 32;

        public ShotGrenade(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(13, 9));

            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToSurviveOutsideOfWorld = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Attack = 0;
            Health = 1;

            SetCenteredCollisionRectangle(SpriteSize, SpriteSize, SpriteSize / 2, SpriteSize / 2);
        }

        public void Launch(Vector2 position, Vector2 velocity)
        {
            WorldLocation = position;
            Velocity = velocity;
            _fuseTimer = Fuse;
            Alive = true;
            Enabled = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled) return;

            _fuseTimer -= elapsed;
            if (_fuseTimer <= 0f)
            {
                Explode();
                return;
            }

            base.Update(gameTime, elapsed);
        }

        private void Explode()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            for (int i = 0; i < 12; i++)
            {
                var angle = i * MathHelper.TwoPi / 12f;
                var dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                ShotManager.FireMediumShot(WorldCenter, dir * ShotSpeed, this);
            }
            Enabled = false;
        }
    }
}
