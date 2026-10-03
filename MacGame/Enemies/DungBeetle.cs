using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class DungBeetle : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 50;

        private const int ChargeRangeInTiles = 5;

        public DungBeetle(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var walk = new AnimationStrip(textures, Helpers.GetTileRect(8, 7), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.15f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 2;
            IsAffectedByGravity = true;

            SetWorldLocationCollisionRectangle(6, 7);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        /// <summary>
        /// True if the player is close by, roughly level with us, and we're walking towards them.
        /// </summary>
        private bool IsChargingPlayer()
        {
            var playerRect = Player.CollisionRectangle;

            var isLevelWithPlayer = playerRect.Bottom > CollisionRectangle.Top && playerRect.Top < CollisionRectangle.Bottom;
            if (!isLevelWithPlayer)
            {
                return false;
            }

            var xDistance = Player.CollisionCenter.X - CollisionCenter.X;
            if (Math.Abs(xDistance) > ChargeRangeInTiles * TileMap.TileSize)
            {
                return false;
            }

            var isFacingPlayer = Flipped ? xDistance < 0 : xDistance > 0;
            return isFacingPlayer;
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
                    CollisionRectangle.Left + 8 :
                    CollisionRectangle.Right - 8,
                    CollisionRectangle.Bottom + 4);

                var edgeCell = Game1.CurrentMap.GetMapSquareAtPixel(edgePixel);
                var isAboutToFall = edgeCell != null && edgeCell.Passable && !edgeCell.IsPlatform;
                if (OnGround && isAboutToFall)
                {
                    Flip();
                }

                velocity.X = IsChargingPlayer() ? speed * 2 : speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
