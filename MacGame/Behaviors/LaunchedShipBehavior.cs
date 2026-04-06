using Microsoft.Xna.Framework;

namespace MacGame.Behaviors
{
    public class LaunchedShipBehavior : Behavior
    {
        private Player _player;

        private float _launchTimer;
        private float _launchDuration;
        private float _attackSpeed;

        private const float LaunchVelocityX = 80;
        private const float LaunchVelocityY = -100f;
        private const float AttackStartSpeed = 80f;
        private const float AttackAcceleration = 200f;
        private const float ShotSpeed = 150f;

        private enum Phase { Launch, Attack }
        private Phase _phase;

        public LaunchedShipBehavior(Player player)
        {
            _player = player;
            Reset();
        }

        public override void Reset()
        {
            _phase = Phase.Launch;
            _launchTimer = 0f;
            _launchDuration = 0.5f + Game1.Randy.NextFloat();
            _attackSpeed = AttackStartSpeed;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {
            if (_phase == Phase.Launch)
            {
                gameObject.Velocity = new Vector2(LaunchVelocityX, LaunchVelocityY);

                _launchTimer += elapsed;
                if (_launchTimer >= _launchDuration)
                {
                    // Fire one shot toward the player at the transition
                    var direction = _player.CollisionCenter - gameObject.CollisionCenter;
                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                    }
                    ShotManager.FireSmallShot(gameObject.CollisionCenter, direction * ShotSpeed);

                    _phase = Phase.Attack;
                }
            }
            else
            {
                _attackSpeed += AttackAcceleration * elapsed;
                gameObject.Velocity = new Vector2(-_attackSpeed, -30);
            }
        }
    }
}
