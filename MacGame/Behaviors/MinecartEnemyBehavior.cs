using MacGame.Enemies;
using Microsoft.Xna.Framework;

namespace MacGame.Behaviors
{
    public class MinecartEnemyBehavior : Behavior
    {
        private bool _jump = false;

        private Player _player;

        private bool wasVisible = false;

        Vector2 startLocation;

        const float speed = 200f;

        public MinecartEnemyBehavior(Player player, bool jump)
        {
            this._player = player;
            _jump = jump;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {

            var enemy = (Enemy)gameObject;

            if (startLocation == Vector2.Zero)
            {
                startLocation = enemy.WorldLocation;
            }

            var isVisible = Game1.Camera.IsPointVisible(this.startLocation);
            if (isVisible)
            {
                if (isVisible && !wasVisible && enemy.WorldLocation == startLocation && !enemy.Enabled)
                {
                    // First time on the screen, set velocity moving towards the player.
                    enemy.Enabled = true;
                    enemy.Alive = true;
                    var direction = _player.WorldCenter - enemy.WorldCenter;
                    if (direction.X > 0)
                    {
                        enemy.Velocity = new Vector2(speed, 0);
                    }
                    else
                    {
                        enemy.Velocity = new Vector2(-speed, 0);
                    }
                }
                else
                {
                    if (_jump && enemy.OnGround)
                    { 
                        // Check that they're heading towards each other.
                        if (enemy.Velocity.X > 0 && _player.Velocity.X < 0 || enemy.Velocity.X < 0 && _player.Velocity.X > 0)
                        {
                            // And they're moving towards each other
                            if (enemy.Velocity.X > 0 && _player.WorldCenter.X > enemy.WorldCenter.X || enemy.Velocity.X < 0 && _player.WorldCenter.X < enemy.WorldCenter.X)
                            {
                                // And the player's close enough.
                                if (Vector2.Distance(enemy.WorldLocation, _player.WorldLocation) < 150f)
                                {
                                    // Jump!
                                    enemy.Velocity = new Vector2(enemy.Velocity.X, -500);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // if off screen, reset to start location.
                if (enemy.Enabled)
                {
                    enemy.Enabled = false;
                    enemy.WorldLocation = startLocation;
                }
            }

            wasVisible = isVisible;
        }
    }
}
