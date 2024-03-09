using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Beetle : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationY;
        private float maxTravelDistance = 8;
        private bool goingUp = false;
        public Beetle(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, new Rectangle(7 * Game1.TileSize, 5 * Game1.TileSize, 8, 8), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isTileColliding = true;
            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 7);

            startLocationY = this.WorldLocation.Y;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.White, 30f);

            this.Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive)
            {
                this.velocity.Y = speed;
                if (goingUp)
                {
                    this.velocity.Y *= -1;
                    this.Rotation = 0f;
                }
                else
                {
                    this.Rotation = MathHelper.Pi;
                }
            }

            // when moving up if the tile above isn't a vine, start moving down.
            // ditto for moving down.
            if (goingUp)
            {
                var tileAbove = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldLocation + new Vector2(0, -Game1.TileSize - 1));
                if (tileAbove == null || !tileAbove.IsVine)
                {
                    goingUp = false;
                }
            }
            else
            {
                var tileAbove = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldLocation + new Vector2(0, 1));
                if (tileAbove == null || !tileAbove.IsVine)
                {
                    goingUp = true;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}