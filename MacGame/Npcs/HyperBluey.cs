using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Design;
using TileEngine;

namespace MacGame.Npcs
{
    public class HyperBluey : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public HyperBluey(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(6, 13), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(6, 13), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetCenteredCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(5, 0);

        public override void InitiateConversation()
        {
            const int totalSayings = 5;
            var randomSaying = Game1.Randy.Next(1, totalSayings + 1);

            if (randomSaying == 1)
            {
                MacSays("Wow you look like a bearded dragon like me!");
                ISay("Are you blind? I'm a Crested Gecko.");
            }
            else if (randomSaying == 2)
            {
                MacSays("Hi I'm Mac");
                ISay("My name is Hyper Bluey");
                ISay("If you don't like it you can chew on walnuts buddy");
                MacSays("I love it");
            }
            else if (randomSaying == 3)
            {
                ISay("Sometimes I wonder what it's all about");
                MacSays("Life?");
                ISay("No, this game");
            }
            else if (randomSaying == 4)
            {
                ISay("Knock Knock");
                MacSays("Who's there?");
                ISay("Guy");
                MacSays("Guy who?");
                ISay("Guy who walks into my house without knocking");
                MacSays("I don't get it");
            }
            else if (randomSaying == 5)
            {
                ISay("What did the foot say to the hand?");
                MacSays("What?");
                ISay("You're handsome.");
            }

        }
    }
}
