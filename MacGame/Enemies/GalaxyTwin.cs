using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public enum GalaxyTwinState
    {
        Unseen,
        Idle,
        HomingMissileAttack,
        MachineGunAttack,
        SpinningMachineGunAttack,
        RingShotAttack,
        Dying,
        Dead
    }

    /// <summary>
    /// A ship that is part of the GalaxyTwinsBoss. The boss controls two of these bad boys
    /// and when they both die the boss fight is over.
    /// </summary>
    public class GalaxyTwin : Enemy
    {
        private GalaxyTwinState _state = GalaxyTwinState.Unseen;

        public const int MaxHealth = 5;
        private const float MoveSpeed = 180f;
        private const float AtTargetDistance = 6f;

        // Dying fall
        public float FallDriftX = -80f;
        private float _fallSpeedY = 0f;
        private const float FallAcceleration = 120f;
        private float _dyingTimer = 0f;
        private const float DyingDuration = 3f;
        private float _explosionTimer = 0f;

        private ShipExhaust _exhaust;

        private Vector2 _targetLocation;
        private bool _hasTarget = false;

        // Attack cycling
        private GalaxyTwinState _lastAttack = GalaxyTwinState.Idle;

        // Idle
        private float _idleTimer = 0f;
        private float IdleDuration;
        private const float IdleDurationMin = 2f;
        private const float IdleDurationMax = 3f;
        private const float FirstTimeIdleDurationMax = 6f;
        private bool _isFirstTimeIdle = true;

        // Shared attack timer
        private float _attackTimer = 0f;

        // Homing missiles
        private Missile[] _missiles = new Missile[3];
        private int _missilesLaunched = 0;
        private bool _hatchClosing = false;
        private const float MissileHatchOpenDuration = 0.35f;
        private const float MissileLaunchInterval = 0.6f;
        private const float MissileHomingDelay = 1.0f;
        private const float MissileHatchCloseDuration = 0.35f;
        private static readonly float MissileSequenceEndTime = MissileHatchOpenDuration + 2 * MissileLaunchInterval + 0.2f;
        private static readonly float HomingAttackDuration = MissileSequenceEndTime + MissileHatchCloseDuration;

        private Vector2 _gunOffset = new Vector2(12, 26);

        // Machine gun
        private float _machineGunFireTimer = 0f;
        private const float MachineGunDuration = 4f;
        private const float MachineGunInterval = 0.2f;
        private const float MachineGunBulletSpeed = 300f;

        // Spinning machine gun
        private float _spinAngle = 0f;
        private float _spinFireTimer = 0f;
        private const float SpinMachineGunDuration = 4f;
        private const float SpinMachineGunInterval = 0.1f;
        private const float SpinRotationSpeed = MathHelper.TwoPi * 1.2f;

        // Ring shot
        private const int RingShotCount = 5;
        private const float RingShotSpeed = 120f;
        private const float RingShotWobble = 0.5f;
        private const float RingShotExitDuration = 1.5f;

        public int CurrentHealth => Health;
        public bool IsAliveAndAttacking => Alive && _state != GalaxyTwinState.Dying && _state != GalaxyTwinState.Dead && _state != GalaxyTwinState.Unseen;
        public bool IsAtTarget => !_hasTarget || Vector2.Distance(WorldLocation, _targetLocation) < AtTargetDistance;

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public GalaxyTwin(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = true;
            CanBeJumpedOn = false;

            var ad = new AnimationDisplay();

            var idle = new AnimationStrip(Game1.ReallyBigTileTextures, Helpers.GetReallyBigTileRect(3, 7), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.1f;
            ad.Add(idle);

            var openHatch = new AnimationStrip(Game1.ReallyBigTileTextures, Helpers.GetReallyBigTileRect(3, 7), 3, "openHatch");
            openHatch.LoopAnimation = false;
            openHatch.FrameLength = 0.1f;
            ad.Add(openHatch);

            var closeHatch = (AnimationStrip)openHatch.Clone();
            closeHatch.Name = "closeHatch";
            closeHatch.Reverse = true;
            ad.Add(closeHatch);

            ad.Play("idle");
            DisplayComponent = ad;

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;

            SetCenteredCollisionRectangle(24, 24, 24, 12);

            for (int i = 0; i < _missiles.Length; i++)
            {
                _missiles[i] = new Missile(content, 0, 0, player, camera);
                _missiles[i].Enabled = false;
                Level.AddEnemy(_missiles[i]);
            }

            var spaceTextures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            _exhaust = new ShipExhaust(spaceTextures);
            _exhaust.Disable();
        }

        public void SetTargetLocation(Vector2 worldLocation)
        {
            _targetLocation = worldLocation;
            _hasTarget = true;
        }

        private static readonly GalaxyTwinState[] AttackStates = new[]
        {
            GalaxyTwinState.HomingMissileAttack,
            GalaxyTwinState.MachineGunAttack,
            GalaxyTwinState.SpinningMachineGunAttack,
            GalaxyTwinState.RingShotAttack,
        };

        private GalaxyTwinState GetNextAttack()
        {
            GalaxyTwinState attack;
            do
            {
                attack = AttackStates[Game1.Randy.Next(AttackStates.Length)];
            }
            while (attack == _lastAttack);
            _lastAttack = attack;
            return attack;
        }

        private void TransitionToState(GalaxyTwinState newState)
        {
            _state = newState;
            _attackTimer = 0f;

            switch (newState)
            {
                case GalaxyTwinState.Idle:
                    _idleTimer = 0f;
                    animations.Play("idle");
                    if (_isFirstTimeIdle)
                    {
                        // First time stay idle for longer to give the player a sense of unexpected dread about what's to come.
                        IdleDuration = FirstTimeIdleDurationMax;
                        _isFirstTimeIdle = false;
                    }
                    else
                    {
                        // Every other time vary your idle time a bit.
                        IdleDuration = (float)(Game1.Randy.NextDouble() * (IdleDurationMax - IdleDurationMin) + IdleDurationMin);
                    }
                    break;
                case GalaxyTwinState.HomingMissileAttack:
                    _missilesLaunched = 0;
                    _hatchClosing = false;
                    foreach (var missile in _missiles)
                    {
                        if (missile.Enabled)
                        {
                            missile.Kill();
                        }
                    }
                    animations.Play("openHatch");
                    SoundManager.PlaySound("OpenHatch");
                    break;
                case GalaxyTwinState.MachineGunAttack:
                    _machineGunFireTimer = 0f;
                    break;
                case GalaxyTwinState.SpinningMachineGunAttack:
                    _spinAngle = (float)(Game1.Randy.NextDouble() * MathHelper.TwoPi);
                    _spinFireTimer = 0f;
                    break;
                case GalaxyTwinState.RingShotAttack:
                    FireRingShots();
                    break;
            }
        }

        private void FireRingShots()
        {
            var baseDir = Vector2.Normalize(Player.CollisionCenter - WorldCenter);
            float baseAngle = (float)Math.Atan2(baseDir.Y, baseDir.X);
            for (int i = 0; i < RingShotCount; i++)
            {
                float wobble = (float)(Game1.Randy.NextDouble() * RingShotWobble * 2 - RingShotWobble);
                var dir = new Vector2(
                    (float)Math.Cos(baseAngle + wobble),
                    (float)Math.Sin(baseAngle + wobble));
                ShotManager.FireMediumRing(WorldCenter, dir * RingShotSpeed, this);
            }
            SoundManager.PlaySound("ShootRing");
        }

        public void Engage()
        {
            TransitionToState(GalaxyTwinState.Idle);
            _exhaust.Enable();
            Enabled = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == GalaxyTwinState.Unseen)
            {
                // Do nothing, wait for GalaxyTwinBoss to call Engage().
                base.Update(gameTime, elapsed);
                return;
            }

            if (_state != GalaxyTwinState.Dying && _state != GalaxyTwinState.Dead)
            {
                if (_hasTarget)
                {
                    var toTarget = _targetLocation - WorldLocation;
                    if (toTarget.Length() > AtTargetDistance)
                    {
                        toTarget.Normalize();
                        Velocity = toTarget * MoveSpeed;
                    }
                    else
                    {
                        Velocity = Vector2.Zero;
                        WorldLocation = _targetLocation;
                    }
                }

                switch (_state)
                {
                    case GalaxyTwinState.Idle:
                        _idleTimer += elapsed;
                        if (_idleTimer >= IdleDuration)
                        {
                            TransitionToState(GetNextAttack());
                        }
                        break;

                    case GalaxyTwinState.HomingMissileAttack:
                        _attackTimer += elapsed;

                        if (_missilesLaunched < 3)
                        {
                            float timeSinceOpen = _attackTimer - MissileHatchOpenDuration;
                            if (timeSinceOpen >= 0f)
                            {
                                int shouldHaveLaunched = Math.Min((int)(timeSinceOpen / MissileLaunchInterval) + 1, 3);
                                while (_missilesLaunched < shouldHaveLaunched)
                                {
                                    float baseAngle = (float)Math.Atan2(
                                        Player.CollisionCenter.Y - WorldCenter.Y,
                                        Player.CollisionCenter.X - WorldCenter.X);
                                    float spread = (_missilesLaunched - 1) * (MathHelper.Pi / 12f);
                                    var dir = new Vector2((float)Math.Cos(baseAngle + spread), (float)Math.Sin(baseAngle + spread));
                                    _missiles[_missilesLaunched].Launch(WorldCenter, dir, MissileHomingDelay);
                                    SoundManager.PlaySound("ShootMissile");
                                    _missilesLaunched++;
                                }
                            }
                        }

                        if (!_hatchClosing && _attackTimer >= MissileSequenceEndTime)
                        {
                            animations.Play("closeHatch");
                            SoundManager.PlaySound("OpenHatch");
                            _hatchClosing = true;
                        }

                        if (_attackTimer >= HomingAttackDuration)
                        {
                            TransitionToState(GalaxyTwinState.Idle);
                        }
                        break;

                    case GalaxyTwinState.MachineGunAttack:
                        _attackTimer += elapsed;
                        _machineGunFireTimer += elapsed;
                        if (_machineGunFireTimer >= MachineGunInterval)
                        {
                            _machineGunFireTimer = 0f;
                            var dir = Vector2.Normalize(Player.CollisionCenter - WorldCenter);
                            ShotManager.FireMediumShot(WorldCenter + _gunOffset, dir * MachineGunBulletSpeed, this, this.DrawDepth - Game1.MIN_DRAW_INCREMENT);
                            SoundManager.PlaySound("Shoot");
                        }
                        if (_attackTimer >= MachineGunDuration)
                        {
                            TransitionToState(GalaxyTwinState.Idle);
                        }
                        break;

                    case GalaxyTwinState.SpinningMachineGunAttack:
                        _attackTimer += elapsed;
                        _spinAngle += SpinRotationSpeed * elapsed;
                        _spinFireTimer += elapsed;
                        if (_spinFireTimer >= SpinMachineGunInterval)
                        {
                            _spinFireTimer = 0f;
                            var dir = new Vector2((float)Math.Cos(_spinAngle), (float)Math.Sin(_spinAngle));
                            ShotManager.FireMediumShot(WorldCenter + _gunOffset, dir * MachineGunBulletSpeed, this, DrawDepth - Game1.MIN_DRAW_INCREMENT);
                            SoundManager.PlaySound("Shoot");
                        }
                        if (_attackTimer >= SpinMachineGunDuration)
                        {
                            TransitionToState(GalaxyTwinState.Idle);
                        }
                        break;


                    case GalaxyTwinState.RingShotAttack:
                        _attackTimer += elapsed;
                        if (_attackTimer >= RingShotExitDuration)
                        {
                            TransitionToState(GalaxyTwinState.Idle);
                        }
                        break;
                }

                _exhaust.WorldLocation = WorldCenter + new Vector2(-48, 12);
                _exhaust.Velocity = new Vector2(-120, 0);
                _exhaust.Update(gameTime, elapsed);
            }
            else if (_state == GalaxyTwinState.Dying)
            {
                _fallSpeedY += FallAcceleration * elapsed;
                WorldLocation += new Vector2(FallDriftX, _fallSpeedY) * elapsed;

                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    _explosionTimer += elapsed;
                    if (_explosionTimer >= 0.13f)
                    {
                        _explosionTimer = 0f;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), false);
                    }
                }

                _dyingTimer += elapsed;
                if (_dyingTimer >= DyingDuration)
                {
                    Dead = true;
                    Enabled = false;
                    _state = GalaxyTwinState.Dead;
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            _exhaust.DrawDepth = depth + Game1.MIN_DRAW_INCREMENT;
        }

        public override void PlayTakeHitSound()
        {
            SoundManager.PlaySound("HitEnemy2");
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            if (_state == GalaxyTwinState.Unseen || _state == GalaxyTwinState.Dying || _state == GalaxyTwinState.Dead) return;
            base.TakeHit(attacker, damage);
        }

        public override void Kill()
        {
            _state = GalaxyTwinState.Dying;
            CanBeHitWithWeapons = false;
            Attack = 0;
            Velocity = Vector2.Zero;
            animations.Play("idle");
            _exhaust.Disable();
            foreach (var missile in _missiles)
            {
                if (missile.Enabled)
                {
                    missile.Kill();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _exhaust.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }
    }
}
