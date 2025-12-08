using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Npcs
{
    public class Chatterbox : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        Sock ChatterboxSock;
        private bool _isInitialized = false;

        private readonly string[] conversations = new string[]
        {
            "Don't talk to me ever again",
            "Enough! I won't give it up",
            "Go already. Why are you still here?",
            "Listen, I don't have a sock for you and that's final",
            "Even if I did have a sock, do you think I'd give it to a spikey lizard breath like you?",
            "One time I tried to fart in a jar and save it",
            "You can stuff your sorries in a sack mister!",
            "Why does Ottie want these stupid smelly socks anyway?",
            "Why would I give my sock to a scruffy nerf herder like you anyway?",
            "Without my sock, life would have no meaning. I would rather die than give up my sock. Sock is everything. Sock is life.",
            "OK fine. Here, have my sock"
        };

        public Chatterbox(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            // Idle animation with 2 frames at location (0, 32)
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(0, 32), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");

            Flipped = true;
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(0, 32);

        public override void InitiateConversation()
        {
            int conversationCount = Game1.LevelState.ChatterboxConversationCount;

            // If the sock has been collected, show the final dialogue
            if (ChatterboxSock.IsCollected)
            {
                ConversationManager.AddMessage("You were right. I didn't need that stinky old sock anyway", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
            // After all conversations, reveal the sock
            else if (conversationCount == (conversations.Length - 1))
            {
                Action revealSock = () => {
                    if (!ChatterboxSock.Enabled)
                    {
                        ChatterboxSock.FadeIn();
                    }
                };

                Game1.LevelState.ChatterboxConversationCount++;
                ConversationManager.AddMessage(conversations[conversationCount], ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, revealSock);
            }
            // Show dialogues based on conversation count
            else if (conversationCount < conversations.Length)
            {
                Game1.LevelState.ChatterboxConversationCount++;

                string message = conversations[conversationCount];

                ConversationManager.AddMessage(message, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
        }

        private void Initialize()
        {
            // Find a sock named "ChatterboxSock" in the level
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    var sock = (Sock)item;
                    if (sock.Name == "ChatterboxSock")
                    {
                        ChatterboxSock = sock;
                        break;
                    }
                }
            }

            if (!ChatterboxSock.IsCollected)
            {
                ChatterboxSock.Enabled = false;
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
    }
}
