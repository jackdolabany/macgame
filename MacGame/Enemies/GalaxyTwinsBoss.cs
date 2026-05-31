using System;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Enemies
{
    public enum GalaxyTwinsBossState { Unseen, Active, Done }

    /// <summary>
    /// A coordinator of two GalaxyTwin enemies. This boss will reveal the sock and stuff
    /// like that, but the two ships are what Mac will attack.
    /// </summary>
    public class GalaxyTwinsBoss : Enemy
    {
        /// <summary>
        /// Position the ships into one of 5 locations. Like the face of a die.
        /// </summary>
        private enum BossPosition { Center, TopLeft, TopRight, BottomLeft, BottomRight }

        private static readonly BossPosition[] AllPositions = new[]
        {
            BossPosition.Center, BossPosition.TopLeft, BossPosition.TopRight,
            BossPosition.BottomLeft, BossPosition.BottomRight
        };

        private GalaxyTwinsBossState _state = GalaxyTwinsBossState.Unseen;

        private GalaxyTwin _twin1;
        private GalaxyTwin _twin2;
        private BossPosition _twin1Position = BossPosition.TopLeft;
        private BossPosition _twin2Position = BossPosition.TopRight;

        private bool _isHolding = false;
        private float _holdTimer = 0f;
        private const float HoldDuration = 2.5f;

        private bool _isInitialized = false;
        private Sock _sock;
        private bool _sockRevealed = false;
        private bool _hasLockedCamera = false;

        private const int CombinedMaxHealth = GalaxyTwin.MaxHealth * 2;

        private const int HalfSpriteHeight = 48;

        public GalaxyTwinsBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            IsPlayerColliding = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            IsAffectedByGravity = false;
            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            Health = 1;
            Attack = 0;

            // This boss doesn't actually show up, it just coordinates the twins.
            DisplayComponent = new NoDisplay();

            _twin1 = new GalaxyTwin(content, cellX, cellY, player, camera);
            _twin2 = new GalaxyTwin(content, cellX, cellY, player, camera);

            _twin1.FallDriftX = -80f;
            _twin2.FallDriftX = 80f;

            _twin1.Enabled = false;
            _twin2.Enabled = false;

            ExtraEnemiesToAddAfterConstructor.Add(_twin1);
            ExtraEnemiesToAddAfterConstructor.Add(_twin2);
        }

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock sock && sock.Name == "GalaxyTwinsSock")
                {
                    _sock = sock;
                    break;
                }
            }

            if (_sock == null)
            {
                throw new Exception("You need a sock named GalaxyTwinsSock in the level!");
            }

            if (!_sock.IsCollected)
            {
                _sock.Enabled = false;
            }

            _isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_state == GalaxyTwinsBossState.Unseen)
            {
                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    _twin1.WorldLocation = WorldLocation;
                    _twin2.WorldLocation = WorldLocation;
                    _twin1.Enabled = true;
                    _twin2.Enabled = true;
                    AssignInitialPositions();
                    _state = GalaxyTwinsBossState.Active;
                    Game1.Camera.MaxX = (int)Game1.Camera.Position.X + 32;
                    Game1.CurrentLevel.StopSpaceAutoScrolling();
                    _hasLockedCamera = true;
                }
            }

            if (_state == GalaxyTwinsBossState.Active)
            {
                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = CombinedMaxHealth;
                Game1.BossHealth = _twin1.CurrentHealth + _twin2.CurrentHealth;
                Game1.BossName = "Galaxy Twins";

                if (!_twin1.IsAlive && !_twin2.IsAlive && !_sockRevealed)
                {
                    _sockRevealed = true;
                    _state = GalaxyTwinsBossState.Done;
                    Game1.Camera.MaxX = null;
                    Game1.CurrentLevel.StartSpaceAutoScrolling();
                    TimerManager.AddNewTimer(1f, () => { _sock.FadeIn(); });
                    Dead = true;
                    Enabled = false;
                }

                if (_isHolding)
                {
                    _holdTimer += elapsed;
                    if (_holdTimer >= HoldDuration)
                    {
                        _isHolding = false;
                        AssignNewPositions();
                    }
                }
                else
                {
                    var t1Ready = !_twin1.IsAlive || _twin1.IsAtTarget;
                    var t2Ready = !_twin2.IsAlive || _twin2.IsAtTarget;
                    if (t1Ready && t2Ready)
                    {
                        _isHolding = true;
                        _holdTimer = 0f;
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        private void AssignInitialPositions()
        {
            _twin1Position = BossPosition.TopLeft;
            _twin2Position = BossPosition.TopRight;
            _twin1.SetTargetLocation(GetWorldPosition(_twin1Position));
            _twin2.SetTargetLocation(GetWorldPosition(_twin2Position));
        }

        private void AssignNewPositions()
        {
            BossPosition newT1;
            do
            {
                newT1 = AllPositions[Game1.Randy.Next(AllPositions.Length)];
            }
            while (newT1 == _twin1Position);
            _twin1Position = newT1;

            BossPosition newT2;
            do
            {
                newT2 = AllPositions[Game1.Randy.Next(AllPositions.Length)];
            }
            while (newT2 == _twin2Position || newT2 == _twin1Position);
            _twin2Position = newT2;

            if (_twin1.IsAlive) { _twin1.SetTargetLocation(GetWorldPosition(_twin1Position)); }
            if (_twin2.IsAlive) { _twin2.SetTargetLocation(GetWorldPosition(_twin2Position)); }
        }

        private Vector2 GetWorldPosition(BossPosition pos)
        {
            var vp = Game1.Camera.ViewPort;
            const int margin = HalfSpriteHeight + 48;

            Vector2 visualCenter;
            switch (pos)
            {
                case BossPosition.TopLeft:
                    visualCenter = new Vector2(vp.Left + margin, vp.Top + margin); 
                    break;
                case BossPosition.TopRight: 
                    visualCenter = new Vector2(vp.Right - margin, vp.Top + margin); 
                    break;
                case BossPosition.BottomLeft: 
                    visualCenter = new Vector2(vp.Left + margin, vp.Bottom - margin); 
                    break;
                case BossPosition.BottomRight: 
                    visualCenter = new Vector2(vp.Right - margin, vp.Bottom - margin); 
                    break;
                default: 
                    visualCenter = new Vector2(vp.Center.X, vp.Center.Y); 
                    break;
            }

            return visualCenter + new Vector2(0, HalfSpriteHeight);
        }

        public override void Kill()
        {
            // Not directly killable; ends when both twins are dead.
        }
    }
}
