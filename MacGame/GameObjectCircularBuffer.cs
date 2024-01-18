using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class GameObjectCircularBuffer : CircularBuffer<GameObject>
    {
        public GameObjectCircularBuffer(int poolSize)
            : base(poolSize)
        {

        }

        public int IndexOf(GameObject obj)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (obj == objects[i])
                {
                    return i;
                }
            }
            throw new Exception("Object not found in Pool");
        }

        public void Disable()
        {
            foreach (var obj in objects)
            {
                obj.Enabled = false;
            }
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            foreach (var obj in this.objects)
            {
                if (obj.Enabled)
                {
                    obj.Update(gameTime, elapsed);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var obj in this.objects)
            {
                if (obj.Enabled)
                {
                    obj.Draw(spriteBatch);
                }
            }
        }
    }
}
