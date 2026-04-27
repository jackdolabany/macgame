using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class BigShipBoss : Enemy
    {
        private static readonly Rectangle ShipSourceRect = new Rectangle(22 * Game1.TileScale, 10 * Game1.TileScale, 175 * Game1.TileScale, 84 * Game1.TileScale);

        private int _maxHealth;
        private bool _hasBeenSeen = false;

        public BigShipBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var texture = content.Load<Texture2D>(@"Textures\BigShip");

            

            DisplayComponent = new StaticImageDisplay(texture, ShipSourceRect);
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;
            Attack = 1;
            Health = 200;
            _maxHealth = Health;

            SetCenteredCollisionRectangle(175 , 84 , 170 , 80 );

            WorldLocation += new Vector2(CollisionRectangle.Width / 2, CollisionRectangle.Height / 2);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                if (!_hasBeenSeen)
                {
                    if (!IsOnScreen())
                    {
                        base.Update(gameTime, elapsed);
                        return;
                    }
                    _hasBeenSeen = true;
                }

                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = _maxHealth;
                Game1.BossHealth = Health;
                Game1.BossName = "Big Ship";
            }
            else
            {
                Game1.DrawBossHealth = false;
            }

            base.Update(gameTime, elapsed);
        }

        public override void Kill()
        {
            Game1.DrawBossHealth = false;
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();
        }
    }
}
