using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    // This is the yoyo Mac swings. Not the item to get the yoyo (Yoyo.cs).
    // It works like the diskarmor in Rygar. A normal throw shoots straight ahead and snaps back.
    // An up throw shoots ahead, swings up over Mac's head to behind him, and then comes back.
    public class MacYoyo : GameObject
    {
        private Player _player;

        /// <summary>
        /// How far the yoyo reaches from Mac. 2.5 blocks.
        /// </summary>
        private const float reach = TileMap.TileSize * 2.5f;

        // Straight throw timing. The total time to go out and come back.
        private const float straightThrowTime = 0.375f;

        // Up throw timings for each phase.
        private const float upThrowOutTime = 0.15f;
        private const float upThrowArcTime = 0.375f;
        private const float upThrowReturnTime = 0.15f;

        private bool isUpThrow;

        // 1 if Mac was facing right when he threw it, -1 if left.
        private float facing;

        private float throwTimer;

        private Texture2D _textures2;

        // The chain sprite is a single pixel in its tile.
        private Rectangle chainSource;
        private const int chainLinkCount = 5;

        private float _linkDrawDepth;

        public MacYoyo(Player player, Texture2D textures2)
        {
            _player = player;
            _textures2 = textures2;

            var chainTile = Helpers.GetTileRect(14, 14);
            chainSource = new Rectangle(chainTile.X + 3 * Game1.TileScale, chainTile.Y + 3 * Game1.TileScale, Game1.TileScale, Game1.TileScale);

            var animations = new AnimationDisplay();
            DisplayComponent = animations;

            // Alternate between two frames for a spinning effect.
            var spin = new AnimationStrip(textures2, Helpers.GetTileRect(13, 15), 2, "spin");
            spin.LoopAnimation = true;
            spin.FrameLength = 0.06f;
            animations.Add(spin);
            animations.Play("spin");
            Enabled = false;

            SetCenteredCollisionRectangle(8, 8, 6, 6);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                throwTimer += elapsed;

                // The offset from Mac as a distance and an angle. Angle 0 is in front of Mac, PI is behind him.
                float distance;
                float angle = 0f;

                if (isUpThrow)
                {
                    if (throwTimer < upThrowOutTime)
                    {
                        distance = reach * (throwTimer / upThrowOutTime);
                    }
                    else if (throwTimer < upThrowOutTime + upThrowArcTime)
                    {
                        distance = reach;
                        angle = MathHelper.Pi * ((throwTimer - upThrowOutTime) / upThrowArcTime);
                    }
                    else
                    {
                        var returnTimer = throwTimer - upThrowOutTime - upThrowArcTime;
                        distance = reach * (1f - (returnTimer / upThrowReturnTime));
                        angle = MathHelper.Pi;
                    }

                    if (throwTimer >= upThrowOutTime + upThrowArcTime + upThrowReturnTime)
                    {
                        Enabled = false;
                    }
                }
                else
                {
                    // A sine curve makes it fly out fast, slow at the end of the reach, and snap back.
                    distance = reach * (float)Math.Sin(MathHelper.Pi * (throwTimer / straightThrowTime));

                    if (throwTimer >= straightThrowTime)
                    {
                        Enabled = false;
                    }
                }

                distance = Math.Max(0f, distance);

                var localLocation = new Vector2(
                    facing * distance * (float)Math.Cos(angle),
                    -distance * (float)Math.Sin(angle));

                this.WorldLocation = _player.WorldLocation + localLocation;

                // Check collisions with enemies
                if (Enabled)
                {
                    foreach (var enemy in Game1.CurrentLevel.Enemies)
                    {
                        if (enemy.Enabled && enemy.CanBeHitWithWeapons && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            enemy.TakeHit(this, 1);
                        }
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            _linkDrawDepth = depth + Game1.MIN_DRAW_INCREMENT;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                // Draw chain links evenly spaced between Mac and the ball.
                var chainStart = _player.WorldLocation - new Vector2(0, TileMap.TileSize / 2);
                var chainEnd = this.WorldLocation - new Vector2(0, TileMap.TileSize / 2);
                var linkOrigin = new Vector2(Game1.TileScale / 2f, Game1.TileScale / 2f);
                for (int i = 1; i <= chainLinkCount; i++)
                {
                    var linkLocation = Vector2.Lerp(chainStart, chainEnd, i / (float)(chainLinkCount + 1));
                    spriteBatch.Draw(_textures2, (linkLocation - linkOrigin).ToIntegerVector(), chainSource, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, _linkDrawDepth);
                }

                base.Draw(spriteBatch);
            }
        }

        public void TryThrow(bool isFacingLeft, bool isUp)
        {
            if (Enabled) return;

            facing = isFacingLeft ? -1f : 1f;
            Flipped = isFacingLeft;
            isUpThrow = isUp;
            throwTimer = 0f;
            this.WorldLocation = _player.WorldLocation;
            this.Enabled = true;

            SoundManager.PlaySound("Kick");
        }
    }
}
