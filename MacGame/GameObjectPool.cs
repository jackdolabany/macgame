using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class GameObjectPool : ObjectPool<GameObject>
    {

        public GameObjectPool(int poolSize)
            : base(poolSize)
        {

        }

        public int Length { get { return objects.Count; } }

        public void Disable()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                // We need to check if they're enabled because if we return a disabled shot it's probably already in the 
                // available queue.
                if (objects[i].Enabled)
                {
                    ReturnObject(objects[i]);
                }
            }
        }

        public override void ReturnObject(GameObject obj)
        {
            obj.Enabled = false;
            base.ReturnObject(obj);
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            for (int i = 0; i < this.objects.Count; i++)
            {
                if (objects[i].Enabled)
                {
                    objects[i].Update(gameTime, elapsed);

                    // Return it if the update statement disabled it
                    if (!objects[i].Enabled)
                    {
                        ReturnObject(objects[i]);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < this.objects.Count; i++)
            {
                if (objects[i].Enabled)
                {
                    objects[i].Draw(spriteBatch);
                }
            }
        }

    }

}
