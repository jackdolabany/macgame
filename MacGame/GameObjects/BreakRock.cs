using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A passable snow brick that breaks when you stand on it. It won't reform.
    /// </summary>
    public class BreakRock : GameObject
    {
        int _cellX;
        int _cellY;

        private bool _isBroken = false;

        float reformTimer = 0f;

        AnimationDisplay AnimationDisplay => DisplayComponent as AnimationDisplay;

        public BreakRock(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 8);

            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(10, 19), 1, "idle");
            idle.LoopAnimation = false;
            ad.Add(idle);

            var breakUp = new AnimationStrip(textures, Helpers.GetTileRect(10, 19), 4, "breakUp");
            breakUp.LoopAnimation = false;
            breakUp.FrameLength = 0.1f;
            ad.Add(breakUp);

            var reform = breakUp.Clone() as AnimationStrip;
            reform.Reverse = true;
            reform.Name = "reform";
            ad.Add(reform);

            ad.Play("idle");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (!_isBroken && Game1.Player.OnGround)
            {
                // Check if player is above.
                var aboveCollisionRectangle = new Rectangle(CollisionRectangle.X, CollisionRectangle.Y - 1, CollisionRectangle.Width, 1);
                if (Game1.Player.CollisionRectangle.Intersects(aboveCollisionRectangle))
                {
                    // Break but only if the player is completely on BreakRocks.
                    // This prevents breaking when just touching the edge.
                    BreakRock? leftPixelRock = null;
                    BreakRock? rightPixelRock = null;

                    var leftPixel = new Vector2(Game1.Player.CollisionRectangle.Left, Game1.Player.CollisionRectangle.Bottom + 1);
                    var rightPixel = new Vector2(Game1.Player.CollisionRectangle.Right, Game1.Player.CollisionRectangle.Bottom + 1);

                    bool leftPixelIsPassable = Game1.CurrentMap.GetMapSquareAtPixel(leftPixel)?.Passable ?? false;
                    bool rightPixelIsPassable = Game1.CurrentMap.GetMapSquareAtPixel(rightPixel)?.Passable ?? false;

                    foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                    {
                        if (gameObject is BreakRock)
                        {
                            var breakRock = gameObject as BreakRock; 
                            if (breakRock.CollisionRectangle.Contains(leftPixel))
                            {
                                // We found a break brick at the left pixel, assign it.
                                leftPixelRock = breakRock;
                            }
                            if (breakRock.CollisionRectangle.Contains(rightPixel))
                            {
                                // We found a break brick at the right pixel, assign it.
                                rightPixelRock = breakRock;
                            }
                        }
                    }

                    bool isOnABreakRock = leftPixelRock != null || rightPixelRock != null;

                    // If the player is on BreakRocks on both sides, or if the player is on a BreakRock and one of the sides is passable, then break both.
                    if (isOnABreakRock &&
                        (leftPixelRock != null && rightPixelRock != null ||
                        (leftPixelRock != null && rightPixelIsPassable) ||
                        (rightPixelRock != null && leftPixelIsPassable)))
                    {
                        // Break both BreakRocks if they exist.
                        leftPixelRock?.Break();
                        rightPixelRock?.Break();
                    }
                }
            }

            if (AnimationDisplay.CurrentAnimationName == "breakUp" && AnimationDisplay.CurrentAnimation.FinishedPlaying)
            {
                this.Enabled = false;
            }

            if (reformTimer > 0)
            {
                reformTimer -= elapsed;
            }

            if (_isBroken && reformTimer <= 0)
            {
                // reform but only if nothing is collding with it and the timer has expired.
                foreach(var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (!(gameObject is BreakRock))
                    {
                        if (gameObject.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            // Don't reform if anything is colliding with it.
                            return;
                        }
                    }
                }

                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        // Don't reform if any enemies are colliding with it.
                        return;
                    }
                }

                if (Game1.Player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    // Don't reform if the player is standing on it.
                    return;
                }

                Reform();
            }
            
        }

        private void Reform()
        {
            _isBroken = false;
            AnimationDisplay.Play("reform");
            Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY)!.Passable = false;
            Enabled = true;
        }

        public void Break()
        {
            if (_isBroken) return;

            _isBroken = true;
            SoundManager.PlaySound("Break", 1f, -0.2f);
            AnimationDisplay.Play("breakUp");
            Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY)!.Passable = true;
            reformTimer = 0.5f;
        }

    }
}
