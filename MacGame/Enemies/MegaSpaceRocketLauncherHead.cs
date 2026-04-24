using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class MegaSpaceRocketLauncherHead : Enemy
    {
        private readonly MegaSpaceRocketLauncher _launcher;

        public MegaSpaceRocketLauncherHead(ContentManager content, int cellX, int cellY, Player player, Camera camera, MegaSpaceRocketLauncher launcher)
            : base(content, cellX, cellY, player, camera)
        {
            _launcher = launcher;

            var megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");
            DisplayComponent = new StaticImageDisplay(megaTextures, Helpers.GetMegaTileRect(5, 3));

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 0;
            Health = 1000;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = true;
            InvincibleTimeAfterBeingHit = 0.5f;

            SetWorldLocationCollisionRectangle(40, 40);
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit || Dead || !Enabled) return;

            InvincibleTimer += InvincibleTimeAfterBeingHit;
            PlayTakeHitSound();
            _launcher.TakeHit(attacker, damage, force);
        }

        public override void Kill()
        {
            // Lifecycle is controlled by the launcher.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOnScreen()) return;
            base.Draw(spriteBatch);
        }
    }
}
