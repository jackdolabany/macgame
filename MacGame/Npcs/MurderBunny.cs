using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Npcs
{
    /// <summary>
    /// This Bunny asks you to kindly kill the murderer and then gives you a reward if you do.
    /// </summary>
    public class MurderBunny : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        Sock MurderBunnySock;
        private bool _isInitialized = false;
        List<ConversationChoice> choices;

        public MurderBunny(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(6, 10), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(6, 10), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");

            choices = new List<ConversationChoice>();
            choices.Add(new ConversationChoice("Yes", () => {
                ConversationManager.AddMessage("Gee thanks, I'll give you something in return!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }));
            choices.Add(new ConversationChoice("Nah", () => {
                ConversationManager.AddMessage("Wow normally heroes don't turn down a challenge like that, but it's your game. You do you.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }));
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(1, 4);

        public override void InitiateConversation()
        {
            // Check if the murderer has been killed
            if (Game1.StorageState.HasKilledMurderer)
            {
                // After the murderer is killed
                if (MurderBunnySock.IsCollected)
                {
                    // If the sock has been collected already
                    ISay("Thanks for taking care of that little murderer problem we had!");
                }
                else
                {
                    // Reveal the sock
                    Action revealSock = () => {
                        if (!MurderBunnySock.Enabled)
                        {
                            MurderBunnySock.FadeIn();
                        }
                    };

                    ConversationManager.AddMessage("Thanks for taking care of the murderer situation!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, revealSock);
                }
            }
            else
            {
                ISay("The mean murderer keeps killing us. Can you stop him?");
                ConversationManager.AddMessage("Please Help!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, choices);
            }
        }

        private void Initialize()
        {
            // Find a sock named "MurderBunnySock" in the level
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    var sock = (Sock)item;
                    if (sock.Name == "MurdererSock")
                    {
                        MurderBunnySock = sock;
                        break;
                    }
                }
            }


            // TEST
            MurderBunnySock.Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
