using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class FireBlob : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 100;
        private float hopVelocity = 300;

        private const int HopsPerSeries = 3;
        private const double TurnChance = 0.3;
        private int hopsRemaining = 0;
        private float restTimer = 0f;

        public FireBlob(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var walk = new AnimationStrip(textures, Helpers.GetTileRect(0, 6), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
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

                // Randomly turn around right before a hop. This happens before the edge check
                // so we never turn and hop off a ledge.
                if (OnGround && hopsRemaining > 0 && Game1.Randy.NextDouble() < TurnChance)
                {
                    Flip();
                }

                // Turn around if you walk towards an edge
                var edgePixel = new Vector2(this.Flipped ?
                    CollisionRectangle.Left + 8 :
                    CollisionRectangle.Right - 8,
                    CollisionRectangle.Bottom + 4);

                var edgeCell = Game1.CurrentMap.GetMapSquareAtPixel(edgePixel);
                var isAboutToFall = edgeCell != null && edgeCell.Passable && !edgeCell.IsPlatform;
                if (OnGround && isAboutToFall)
                {
                    Flip();
                }

                if (OnGround)
                {
                    if (hopsRemaining > 0)
                    {
                        // Hop again as soon as we land.
                        hopsRemaining--;
                        velocity.Y = -hopVelocity;
                        velocity.X = Flipped ? -speed : speed;
                    }
                    else if (restTimer > 0)
                    {
                        // Sit still for a bit between hop series.
                        velocity.X = 0;
                        restTimer -= elapsed;
                        if (restTimer <= 0)
                        {
                            hopsRemaining = HopsPerSeries;
                        }
                    }
                    else
                    {
                        // Just landed from the last hop, start resting.
                        velocity.X = 0;
                        restTimer = 1f + (float)Game1.Randy.NextDouble();
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
