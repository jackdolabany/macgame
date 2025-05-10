using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Enemies;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MacGame.Npcs
{
    /// <summary>
    /// Pauly is a special snake who is also a mob boss.
    /// </summary>
    public class PaulyTheSnake : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private bool _isInitialized = false;
        Sock sock;
        Car car;

        public PaulyTheSnake(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(2, 12), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.7f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock && ((Sock)item).Name == "CarSock")
                {
                    sock = item as Sock;
                    if (!sock.IsCollected)
                    {
                        // disable if not collected.
                        sock.Enabled = false;
                    }
                }
            }

            if (sock == null)
            {
                throw new Exception("Expected a sock named CarSock on this map.");
            }

            // Find the car.
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy is Car)
                {
                    car = enemy as Car;
                }
            }

            if (car == null)
            {
                throw new Exception("Expected a car on this map.");
            }

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            // Check if the sock was collected.
            if (Game1.LevelState.JobState != JobState.SockCollected && sock.IsCollected)
            {
                car.SetToBike();
                Game1.LevelState.JobState = JobState.SockCollected;
            }

            base.Update(gameTime, elapsed);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 2);

        /// <summary>
        /// The dog gives you hints to where the next sock is.
        /// </summary>
        public override void InitiateConversation()
        {

            switch (Game1.LevelState.JobState)
            {
                case JobState.NotAccepted:

                    var acceptJob = new ConversationChoice("Yes", () =>
                    {
                        Game1.LevelState.JobState = JobState.Accepted;
                        ConversationManager.AddMessage("Robby the cat owes us some money. We need you to send him a message. Capisce?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        car.Enabled = true;
                    });

                    var declineJob = new ConversationChoice("No", () =>
                    {
                        ConversationManager.AddMessage("Fugedaboudit", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    });

                    var jobChoices = new List<ConversationChoice>
                    {
                        acceptJob,
                        declineJob
                    };
                    ConversationManager.AddMessage("Hey kid", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    ConversationManager.AddMessage("Want a job?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, jobChoices);
                    break;
                case JobState.Accepted:
                    ConversationManager.AddMessage("You know what to do.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.CarDamaged:
                    ConversationManager.AddMessage("You know what to do.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case JobState.CarDestroyed:
                    Action showSock = () =>
                    {
                        sock.FadeIn();
                    };

                    if (sock.Enabled)
                    {
                        ConversationManager.AddMessage("We didn't see nuthin'", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    }
                    else
                    {
                        ConversationManager.AddMessage("Wow kid. We just wanted you to remind him. You didn't have to destroy his car. That's actually really messed up. Take this and stay away from us", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, showSock);
                    }
                    break;
                case JobState.SockCollected:
                    ConversationManager.AddMessage("We didn't see nuthin'", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
  
                default:
                    break;

            }
        }
    }
}
