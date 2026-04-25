using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class SpaceFortressBoss : Enemy
    {
        private const int MaxShips = 2;
        private const float ShipSpawnInterval = 3f;

        private MegaSpaceCannon _cannon1;
        private MegaSpaceCannon _cannon2;
        private MegaSpaceRocketLauncher _rocketLauncher;

        private readonly List<AlienShip> _ships = new List<AlienShip>();
        private float _shipSpawnTimer = 0f;

        private bool _isInitialized = false;
        private bool _hasBeenSeen = false;
        private int _maxHealth;

        /// <summary>
        /// Kind of a weird boss. Just place the tile for this boss anywhere on the map and also place one MegaSpaceRocketLauncher
        /// and 2 MegaSpaceCannons. This boss will find the 3 of them and put is name and health and crap on the screen.
        /// While you're fighting those things he will spawn random enemies and whatever he wants.
        /// </summary>
        public SpaceFortressBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsPlayerColliding = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Attack = 0;
            Health = 1;

            DisplayComponent = new NoDisplay();

            for (int i = 0; i < MaxShips; i++)
            {
                var ship = new AlienShip(content, cellX, cellY, player, camera);
                ship.Enabled = false;
                _ships.Add(ship);
                AddEnemyInConstructor(ship);
            }
        }

        private void Initialize()
        {
            var cannons = Game1.CurrentLevel.Enemies.OfType<MegaSpaceCannon>().ToList();
            var launchers = Game1.CurrentLevel.Enemies.OfType<MegaSpaceRocketLauncher>().ToList();

            if (cannons.Count != 2)
                throw new Exception($"SpaceFortressBoss requires exactly 2 MegaSpaceCannons but found {cannons.Count}.");

            if (launchers.Count != 1)
                throw new Exception($"SpaceFortressBoss requires exactly 1 MegaSpaceRocketLauncher but found {launchers.Count}.");

            _cannon1 = cannons[0];
            _cannon2 = cannons[1];
            _rocketLauncher = launchers[0];

            _maxHealth = _cannon1.CurrentHealth + _cannon2.CurrentHealth + _rocketLauncher.CurrentHealth;

            _isInitialized = true;
        }

        private int TotalHealth =>
            (!_cannon1.Dead ? _cannon1.CurrentHealth : 0) +
            (!_cannon2.Dead ? _cannon2.CurrentHealth : 0) +
            (!_rocketLauncher.Dead ? _rocketLauncher.CurrentHealth : 0);

        private bool AnyComponentOnScreen() =>
            _cannon1.IsOnScreen() || _cannon2.IsOnScreen() || _rocketLauncher.IsOnScreen();

        private bool AllComponentsDead() =>
            _cannon1.Dead && _cannon2.Dead && _rocketLauncher.Dead;

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized) Initialize();

            if (!_hasBeenSeen)
            {
                if (!AnyComponentOnScreen())
                {
                    base.Update(gameTime, elapsed);
                    return;
                }
                _hasBeenSeen = true;
            }

            if (AllComponentsDead())
            {
                Game1.DrawBossHealth = false;
                base.Update(gameTime, elapsed);
                return;
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = _maxHealth;
            Game1.BossHealth = TotalHealth;
            Game1.BossName = "Fortress";

            if (AnyComponentOnScreen())
            {
                _shipSpawnTimer -= elapsed;
                if (_shipSpawnTimer <= 0f)
                {
                    _shipSpawnTimer = ShipSpawnInterval;
                    TrySpawnShip();
                }
            }

            base.Update(gameTime, elapsed);
        }

        private void TrySpawnShip()
        {
            if (_ships.Count(s => !s.Dead) >= MaxShips) return;

            var ship = _ships.FirstOrDefault(s => s.Dead);
            if (ship == null) return;

            var viewport = Game1.Camera.ViewPort;
            var minY = viewport.Top + (3 * Game1.TileSize);
            var maxY = viewport.Bottom - (3 * Game1.TileSize);
            if (maxY <= minY) return;

            ship.Revive(new Vector2(viewport.Right + 50, minY + Game1.Randy.Next(maxY - minY)));
        }

        public override void Kill()
        {
            // Lifecycle is determined by the component enemies.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // No visual representation.
        }
    }
}
