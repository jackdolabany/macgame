using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using System;
using System.Net.Http;

namespace MacGame.Behaviors
{
    public class EnemyShipBehavior : Behavior
    {
        private int _speed;
        Camera _camera;

        private float _fireTimer;
        private float _fireInterval;
        private float _shotSpeed;
        private ShotSize _shotSize;
        private Player _player;

        private static readonly Random _random = new Random();

        private float _missileTimer;
        private float _missileInterval;
        private float _missileArmDelay;
        private float _missileOnScreenTimer;
        private bool _missileArmed;

        public EnemyShipBehavior(int speed, Camera camera, Player player)
        {
            _speed = speed;
            _camera = camera;
            _player = player;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {
            var enemy = (Enemy)gameObject;
            if (enemy.Alive && !_camera.IsWayOffscreen(gameObject.CollisionRectangle))
            {
                // Speed is always relative to the auto scroll speed.
                gameObject.Velocity = Game1.CurrentLevel.AutoScrollSpeed;
                gameObject.Velocity += new Vector2(-_speed, 0);

                // Shoot a shot if it's been set up.
                if (_fireInterval > 0)
                {
                    _fireTimer += elapsed;
                    if (_fireTimer >= _fireInterval)
                    {
                        _fireTimer = 0f;
                        var direction = Vector2.Normalize(_player.WorldCenter - enemy.WorldCenter);
                        switch (_shotSize)
                        {
                            case ShotSize.Small:
                                ShotManager.FireSmallShot(enemy.WorldCenter, direction * _shotSpeed, enemy);
                                break;
                            case ShotSize.Large:
                                ShotManager.FireLargeShot(enemy.WorldCenter, direction * _shotSpeed, enemy);
                                break;
                            case ShotSize.Medium:
                                ShotManager.FireMediumShot(enemy.WorldCenter, direction * _shotSpeed, enemy);
                                break;
                            default:
                                throw new Exception("Unexpected ShotSize value: " + _shotSize);
                        }
                        SoundManager.PlaySound("Shoot");
                    }
                }

                // Launch a homing missile if it's been set up, but only once the ship has
                // been on screen for a bit so it doesn't fire the instant it appears.
                if (_missileInterval > 0)
                {
                    if (!_missileArmed)
                    {
                        _missileOnScreenTimer += elapsed;
                        if (_missileOnScreenTimer >= _missileArmDelay)
                        {
                            _missileArmed = true;
                            _missileTimer = _missileInterval;
                        }
                    }
                    else
                    {
                        _missileTimer -= elapsed;
                        if (_missileTimer <= 0f)
                        {
                            _missileTimer = _missileInterval;
                            MissileManager.LaunchHomingMissile(enemy.CollisionCenter);
                            SoundManager.PlaySound("ShootMissile");
                        }
                    }
                }
            }
            else
            {
                gameObject.Velocity = Vector2.Zero;
            }
        }

        internal void SetupShootAtPlayer(float fireInterval, float shotSpeed, ShotSize shotSize)
        {
            _fireInterval = fireInterval;
            _shotSpeed = shotSpeed;
            _shotSize = shotSize;
        }

        internal void SetupLaunchHomingMissile(float missileInterval)
        {
            _missileInterval = missileInterval;
            _missileArmDelay = 2f + (float)_random.NextDouble() * 2.5f;
            _missileOnScreenTimer = 0f;
            _missileArmed = false;
        }
    }
}
