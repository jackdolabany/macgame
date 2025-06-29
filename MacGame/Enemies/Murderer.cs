using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Murderer : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Murderer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
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
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            

            base.Update(gameTime, elapsed);

        }
    }
}