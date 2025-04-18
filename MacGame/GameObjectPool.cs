﻿using Microsoft.Xna.Framework;
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

    }

}
