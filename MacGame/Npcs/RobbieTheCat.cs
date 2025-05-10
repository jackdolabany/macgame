using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Npcs
{
    public class RobbieTheCat : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private Player _player;

        /// <summary>
        /// Robbie's car that is in various state of being destroyed or not shown yet.
        /// </summary>
        Car car;

        private bool _isInitialized = false;

        public RobbieTheCat(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(0, 12), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(0, 12), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");

        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 3);

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }
            base.Update(gameTime, elapsed);
        }

        private void Initialize()
        {
            // Disable the Car sock for now.
            foreach (var item in Game1.CurrentLevel.Enemies)
            {
                if (item is Car)
                {
                    car = item as Car;
                }
            }
            if (car == null)
            {
                throw new Exception("Expected a car on this map.");
            }

            car.SetDrawDepth(this.DrawDepth - Game1.MIN_DRAW_INCREMENT);

            switch(Game1.LevelState.JobState)
            {
                case JobState.NotAccepted:
                    car.Enabled = false;
                    break;
                case JobState.Accepted:
                    car.Enabled = true;
                    break;
                case JobState.CarDamaged:
                    car.Enabled = true;
                    break;
                case JobState.CarDestroyed:
                    car.Enabled = true;
                    break;
                case JobState.SockCollected:
                    car.SetToBike();
                    break;
                default:
                    break;
            }
        }

        public override void InitiateConversation()
        {
            switch (Game1.LevelState.JobState)
            {
                case JobState.NotAccepted:

                    ConversationManager.AddMessage("I borrowed money for a new car. It's coming soon and I'm so excited!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.Accepted:
                    ConversationManager.AddMessage("Check out my new ride! It's a cattyllac", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.CarDamaged:
                    ConversationManager.AddMessage("Bro! What are you doing?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.CarDestroyed:
                    ConversationManager.AddMessage("I can't believe you wrecked my ride!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.SockCollected:
                    ConversationManager.AddMessage("I get your point now, that gas guzzler was bad for the environment. I'm loving my new bike, thanks Mac!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;

                default:
                    break;

            }
        }
    }
}
