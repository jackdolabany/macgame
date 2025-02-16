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
    public class OrangeCat : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        List<ConversationChoice> gameChoices;
        private bool _isInitialized = false;

        private Sock Sock;
        bool isSockRevealed = false;

        public OrangeCat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(9, 10), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(9, 10), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetCenteredCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");

            gameChoices = new List<ConversationChoice>();
            gameChoices.Add(new ConversationChoice("Yes", () =>
            {
                Game1.CurrentLevel.EnableBomb();
                ConversationManager.AddMessage("Great! I've rigged this whole place with explosives and we're all going to blow up if you don't disarm them. Good luck!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }));
            gameChoices.Add(new ConversationChoice("No", () =>
            {
                // Do nothing;
            }));


        }

        private void Initialize()
        {
            // On this level OrangeCat hides and then later reveals this bomb.
            if (Game1.CurrentLevel.Name == "World2BombMaze")
            {
                foreach (var item in Game1.CurrentLevel.Items)
                {
                    if (item is Sock)
                    {
                        Sock = (Sock)item;
                    }
                }

                if (Sock == null)
                {
                    throw new Exception("You need a sock in the level!");
                }

                Sock.Enabled = false;
            }
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(1, 1);

        public override void InitiateConversation()
        {
            if (Game1.CurrentLevel.Name == "World2BombMaze")
            {
                if (Game1.CurrentLevel.BombTimer == 0 && !Game1.CurrentLevel.AllBombsDisabled)
                {
                    ConversationManager.AddMessage("I have this really fun game.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    ConversationManager.AddMessage("Wanna play?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, gameChoices);
                }
                else if (Game1.CurrentLevel.AllBombsDisabled && !Sock.AlreadyCollected && !Sock.Enabled && !isSockRevealed)
                {
                    ConversationManager.AddMessage("Hey you're pretty good under stress. I wish I had a good reward but I only have this stinky sock.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, 
                        () => { 
                            Sock.FadeIn();
                            isSockRevealed = true;
                        });
                }
                else
                {
                    ConversationManager.AddMessage("Somebody set up us the bomb!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }

            }
            else
            {
                ConversationManager.AddMessage("Meow", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
        }
    }
}
