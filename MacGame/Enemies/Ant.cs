using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Ant : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 30;

        public Ant(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, Helpers.GetTileRect(3, 1), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetWorldLocationCollisionRectangle(6, 7);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                // Flip if you hit a wall
                if (!Flipped && velocity.X >= 0 && OnRightWall)
                {
                    Flipped = true;
                }
                else if (Flipped && velocity.X <= 0 && OnLeftWall)
                {
                    Flipped = false;
                }

                // Turn around if you walk towards an edge
                var edgePixel = new Vector2(this.Flipped ?
                    CollisionRectangle.Left + 8:
                    CollisionRectangle.Right - 8,
                    CollisionRectangle.Bottom + 4);

                var edgeCell = Game1.CurrentMap.GetMapSquareAtPixel(edgePixel);
                if (edgeCell != null && edgeCell.Passable)
                {
                    Flip();
                }
            
                velocity.X = speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}