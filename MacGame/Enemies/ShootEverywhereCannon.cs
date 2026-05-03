using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class ShootEverywhereCannon : Enemy
    {
        private const float HoldTime = 1.5f;
        private const float ShootSpeed = 150f;

        private readonly Rectangle _rect1;
        private readonly Rectangle _rect2;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        private bool _onCardinalPhase = true;
        private float _timer = HoldTime;

        public bool UpsideDown { get; set; }

        public ShootEverywhereCannon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            _rect1 = Helpers.GetTileRect(5, 8);
            _rect2 = Helpers.GetTileRect(6, 8);

            DisplayComponent = new StaticImageDisplay(textures, _rect1);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 4;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetCenteredCollisionRectangle(8, 8, 6, 6);
        }

        private void FireCardinal()
        {
            float ySign = UpsideDown ? 1f : -1f;
            ShotManager.FireSmallShot(CollisionCenter, new Vector2(-1f, 0f) * ShootSpeed);
            ShotManager.FireSmallShot(CollisionCenter, new Vector2(0f, ySign) * ShootSpeed);
            ShotManager.FireSmallShot(CollisionCenter, new Vector2(1f, 0f) * ShootSpeed);
            PlaySoundIfOnScreen("Fire", 0.5f);
        }

        private void FireDiagonal()
        {
            float ySign = UpsideDown ? 0.707f : -0.707f;
            ShotManager.FireSmallShot(CollisionCenter, new Vector2(-0.707f, ySign) * ShootSpeed);
            ShotManager.FireSmallShot(CollisionCenter, new Vector2(0.707f, ySign) * ShootSpeed);
            PlaySoundIfOnScreen("Fire", 0.5f);
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            display.Effect = UpsideDown ? SpriteEffects.FlipVertically : SpriteEffects.None;

            if (Alive && IsOnScreen())
            {
                _timer -= elapsed;
                if (_timer <= 0f)
                {
                    if (_onCardinalPhase)
                    {
                        FireCardinal();
                        _onCardinalPhase = false;
                        display.Source = _rect2;
                    }
                    else
                    {
                        FireDiagonal();
                        _onCardinalPhase = true;
                        display.Source = _rect1;
                    }
                    _timer = HoldTime;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
