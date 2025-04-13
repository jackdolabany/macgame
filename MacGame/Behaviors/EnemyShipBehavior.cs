using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;

namespace MacGame.Behaviors
{
    public class EnemyShipBehavior : Behavior
    {
        private int _speed;
        Camera _camera;

        public EnemyShipBehavior(int speed, Camera camera)
        {
            _speed = speed;
            _camera = camera;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {
            if (!_camera.IsWayOffscreen(gameObject.CollisionRectangle))
            {
                gameObject.Velocity = new Vector2(-_speed, 0);
            }
            else
            {
                gameObject.Velocity = Vector2.Zero;
            }
        }
    }
}
