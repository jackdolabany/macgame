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
    /// A field that blocks the player but allows him to throw objects past it.
    /// </summary>
    public class BlockPlayerField : GameObject
    {

        public AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private Player _player;

        Vector2 _previousPlayerLocation;

        bool isInitialized = false;

        public BlockPlayerField(ContentManager content, int x, int y, Player player) : base ()
        {
            _player = player;
            Enabled = true;

            IsAffectedByGravity = false;

            this.SetWorldLocationCollisionRectangle(6, 8);

            this.DisplayComponent = new AnimationDisplay();

            this.WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            const int totalFrames = 3;

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(4, 2), totalFrames, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.14f;

            animations.Add(idle);
            animations.Play("idle");

            // start the animation on a pseudo random frame to give it a random look
            idle.currentFrameIndex = y % totalFrames;

            // Add a little transparency to add to the "field" nature of this tile.
            //animations.TintColor = Color.White * 0.75f;


        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                // Set the initial player location
                _previousPlayerLocation = _player.WorldLocation;
                isInitialized = true;
            }

            if (_player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                _player.TakeHit(1, Vector2.Zero);
                var wasPlayerToTheLeft = _previousPlayerLocation.X < this.WorldLocation.X;
                if (wasPlayerToTheLeft)
                {
                    _player.Velocity = new Vector2(-200, _player.Velocity.Y);
                }
                else
                {
                    _player.Velocity = new Vector2(200, _player.Velocity.Y);
                }
            }

            // And why not? kill enemies
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    enemy.TakeHit(this, 1, Vector2.Zero);
                }
            }
            base.Update(gameTime, elapsed);

            _previousPlayerLocation = _player.WorldLocation;

        }


    }

}
