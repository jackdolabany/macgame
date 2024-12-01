using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// GameObjects that the player can pick up.
    /// </summary>
    public interface IPickupObject
    {
        public void Pickup();
        public void Drop(Player player);

        public void MoveToPlayer(Player player);
        public void Kick(Player player);

        public Rectangle CollisionRectangle { get; }

        public bool CanBePickedUp { get; }
    }
}
