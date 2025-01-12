using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A field that destroys objects like rocks, boxes, and springboards that the player may have picked up.
    /// </summary>
    public class DestroyPickupObjectField : GameObject
    {

        public AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private Player _player;

        public DestroyPickupObjectField(ContentManager content, int x, int y, Player player) : base ()
        {
            _player = player;
            Enabled = true;

            IsAffectedByGravity = false;

            this.SetCenteredCollisionRectangle(8, 8);

            this.DisplayComponent = new AnimationDisplay();

            this.WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(13, 13), 3, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.14f;

            animations.Add(idle);
            animations.Play("idle");
            // start the animation on a random frame to give it a random look
            idle.currentFrameIndex = Game1.Randy.Next(0, idle.FrameCount);

            // Add a little transparency to add to the "field" nature of this tile.
            animations.TintColor = Color.White * 0.75f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (_player.IsHoldingObject && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                _player.BreakPickupObject();
            }
            else
            {
                foreach (var puo in Game1.CurrentLevel.PickupObjects)
                {
                    if (!puo.IsPickedUp && puo.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        puo.BreakAndReset();
                    }
                }
            }

            // And why not? kill enemies
            foreach(var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    enemy.TakeHit(this, 1, Vector2.Zero);
                }
            }

            base.Update(gameTime, elapsed);
        }


    }

}
