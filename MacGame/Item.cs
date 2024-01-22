using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    public abstract class Item : GameObject
    {
        public virtual void Collect(Player player)
        {
            WhenCollected(player);
            this.Enabled = false;
        }

        public abstract void WhenCollected(Player player);

    }
}
