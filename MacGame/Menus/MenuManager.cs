using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public static class MenuManager
    {

        private static List<Menu> Menus = new List<Menu>();
        private static List<Menu> MenusToUpdate = new List<Menu>();

        public static bool IsMenu
        {
            get
            {
                return Menus.Count > 0;
            }
        }

        public static void ClearMenus()
        {
            Menus.Clear();
        }

        public static void AddMenu(Menu menu)
        {
            menu.ResestMenuIndex();

            // Ignore inputs for a short time if this is the first menu. 
            if (!Menus.Any())
            {
                menu.ignoreInputsTimer = 0.2f;
            }

            Menus.Add(menu);
            menu.AddedToMenuManager();
        }

        public static void RemoveTopMenu()
        {
            Menus.RemoveAt(Menus.Count - 1);
        }

        public static bool IsTopMenu(Menu menu)
        {
            if (!Menus.Any()) return false;
            return Menus[Menus.Count - 1] == menu;
        }

        public static void Update(float elapsed)
        {
            int count = Menus.Count;

            MenusToUpdate.Clear();

            foreach (var menu in Menus)
            {
                MenusToUpdate.Add(menu);
            }

            while (MenusToUpdate.Count > 0)
            {
                int i = MenusToUpdate.Count - 1;
                var menu = MenusToUpdate[i];
                MenusToUpdate.RemoveAt(i);
                menu.Update(elapsed, (i == (count - 1)));
                menu.DrawDepth = 0.3f;
                menu.DrawDepth -= menu.DrawDepth * ((float)i / (float)count); 
                i++;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = Menus.Count - 1; i >= 0; i--)
            {
                Menus[i].Draw(spriteBatch);
                if (!Menus[i].IsOverlay)
                {
                    // We don't ever want to draw inactive menus unless
                    // we have an overlay which is designed to hide the menus
                    // below
                    break;
                }
            }
        }

    }
}
