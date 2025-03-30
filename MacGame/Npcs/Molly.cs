using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace MacGame.Npcs
{
    public class Molly : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        Sock MollySock;
        private bool _isInitialized = false;

        public Molly(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(2, 14), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(2, 14), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(2, 0);

        public override void InitiateConversation()
        {
            if (Game1.CurrentLevel.Name.Equals("World2MollyHouse", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!Game1.StorageState.HasDancedForDaisy)
                {
                    ConversationManager.AddMessage("Everyone's doing this new dance it's called the Salami Mode Shuffle. It goes like this up, up, down, down, left, right, left, right, jump! My best friend Daisy would LOVE to see it! She's the coolest and my absolute bestie!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
                else if (!MollySock.IsCollected)
                {
                    Action revealSock = () => {
                        if (!MollySock.Enabled)
                        {
                            MollySock.FadeIn();
                        }
                    };

                    ConversationManager.AddMessage("OMG you did the dance for Daisy!? I bet she loved it! Thank you so much for showing her, you can have this disgusting sock as a token of my appreciation.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, revealSock);
                }
                else
                {
                    ConversationManager.AddMessage("I wonder what Daisy is doing right now.She's my BFF.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
            }
            else
            {
                ConversationManager.AddMessage("Meow.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
        }

        private void Initialize()
        {
            // Find a sock named "MollySock" which is expected in her level.
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    var sock = (Sock)item;
                    if (sock.Name == "MollySock")
                    {
                        MollySock = sock;
                        MollySock.Enabled = false;
                        break;
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            // Molly does custom stuff on this level. She tells you about a dance and then rewards you if you show the dance to Daisy.
            bool isMollysLevel = Game1.CurrentLevel.Name == "World2MollyHouse";

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            if (isMollysLevel)
            {
                
            }
            base.Update(gameTime, elapsed);
        }
    }
}
