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
    }
}
