using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public static class WhiteFlashManager
    {
        private static readonly List<GameObject> _objects = new List<GameObject>();

        public static void Clear()
        {
            _objects.Clear();
        }

        public static void Register(GameObject obj)
        {
            if (!_objects.Contains(obj))
            {
                _objects.Add(obj);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // Snapshot and clear so any re-registrations during drawing don't bleed over.
            var toFlash = new List<GameObject>(_objects);
            _objects.Clear();
            foreach (var obj in toFlash)
            {
                obj.Draw(spriteBatch);
            }
        }
    }
}
