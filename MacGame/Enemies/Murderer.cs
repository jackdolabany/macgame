using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Kinda like Jason from Friday the 13th, but certainly not him.
    /// </summary>
    public class Murderer : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        /// He'll appear after some time.
        /// </summary>
        float appearTimer;

        /// <summary>
        /// This is the Rectangle that Mac needs to be inside for the murderer to appear.
        /// In Tiled place a rectangle and call it "MurdererRectangle"
        /// </summary>
        Rectangle murdererRectangle;

        private bool _isInitialized = false;
        private Player _player;

        public Murderer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            
            var walk = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 3), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            var toss = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(2, 3), 1, "toss");
            toss.LoopAnimation = true;
            toss.FrameLength = 0.14f;
            animations.Add(toss);

            // Face the player. Either to die or to turn around.
            var face = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(3, 3), 2, "face");
            face.LoopAnimation = true;
            face.FrameLength = 0.14f;
            animations.Add(face);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 10;
            IsAffectedByGravity = true;

            SetWorldLocationCollisionRectangle(10, 23);

            this.Enabled = false;
        }

        private void Initialize()
        {
            // Find the murderer rectangle in the map.
            foreach (var obj in Game1.CurrentMap.ObjectModifiers)
            {
                if (obj.Name == "MurdererRectangle")
                {
                    murdererRectangle = obj.GetScaledRectangle();
                }
            }

            if (murdererRectangle == Rectangle.Empty)
            {
                throw new Exception("The murderer needs a rectangle named 'MurdererRectangle' on the map to know where to spawn.");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            if (Alive && !Enabled)
            {
                appearTimer += elapsed;
                if (appearTimer > 2f)
                {
                    Enabled = true;
                    appearTimer = 0f;
                    animations.Play("walk");
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}