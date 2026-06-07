using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    /// <summary>
    /// Manages a pool of ShipFire particles that trail behind a moving ship or missile.
    /// </summary>
    public class ShipExhaust
    {
        private readonly CircularBuffer<ShipFire> _fires;
        private float _timer;
        public bool Enabled { get; set; }

        public float FireInterval = 0.1f;
        public Vector2 WorldLocation;
        public Vector2 Velocity;
        public float DrawDepth;

        public ShipExhaust(Texture2D textures)
        {
            _fires = new CircularBuffer<ShipFire>(10);
            for (int i = 0; i < 10; i++)
            {
                _fires.SetItem(i, new ShipFire(textures));
            }
            Enabled = true;
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled) return;

            _timer += elapsed;
            if (_timer >= FireInterval)
            {
                _timer = 0f;
                var fire = _fires.GetNextObject();
                fire.Reset();
                fire.SetDrawDepth(DrawDepth);
                fire.WorldLocation = WorldLocation;
                fire.Velocity = Velocity;
            }

            for (int i = 0; i < _fires.Length; i++)
            {
                var fire = _fires.GetItem(i);
                if (fire.Enabled)
                {
                    fire.Update(gameTime, elapsed);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Enabled) return;

            for (int i = 0; i < _fires.Length; i++)
            {
                var fire = _fires.GetItem(i);
                if (fire.Enabled)
                {
                    fire.Draw(spriteBatch);
                }
            }
        }

    }
}
