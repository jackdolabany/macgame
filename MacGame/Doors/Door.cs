using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Doors
{
    public abstract class Door : GameObject
    {
        public string GoToMap = "";
        public string GoToDoorName = "";

        protected Player _player;

        public Door(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 16);

            _player = player;
        }

        public virtual void PlayerTriedToOpen(Player player)
        {
            GlobalEvents.FireDoorEntered(this, GoToMap, GoToDoorName, Name);
        }

        public abstract void PlayerSlidingOut();

        public virtual void ComeOutOfThisDoor(Player player, bool isYeet = false)
        {
            player.WorldLocation = this.WorldLocation;
            player.Velocity = Vector2.Zero;
            player.IsInvisibleAndCantMove = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game1.Camera.IsWayOffscreen(this.CollisionRectangle)) return;

            base.Draw(spriteBatch);
        }
    }
}
