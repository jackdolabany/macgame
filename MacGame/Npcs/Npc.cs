using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame.Npcs
{
  

    public abstract class Npc : GameObject
    {
        
        public Behavior? Behavior { get; set; }
        public abstract Rectangle ConversationSourceRectangle { get; }

        public Rectangle PlayerConversationRectangle => Helpers.GetReallyBigTileRect(0, 0);

        public List<ConversationOverride> ConversationOverrides { get; set; }

        public Npc(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {

            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            IsAffectedByGravity = true;
            IsAbleToSurviveOutsideOfWorld = true; 

            ConversationOverrides = new List<ConversationOverride>();
        }

        public abstract void InitiateConversation();

        public void CheckPlayerInteractions(Player player)
        {
            // Handle the player going through a door.
            if (player.InteractButtonPressedThisFrame && this.CollisionRectangle.Intersects(player.CollisionRectangle))
            {
                if (!ConversationOverrides.Any())
                {
                    InitiateConversation();
                }
                else
                {
                    // Create messages from the overrides.
                    foreach (var conversationOverride in ConversationOverrides)
                    {
                        Rectangle conversationSourceRectangle;
                        ConversationManager.ImagePosition position;

                        switch (conversationOverride.Speaker)
                        {
                            case ConversationSpeaker.Player:
                                conversationSourceRectangle = PlayerConversationRectangle;
                                position = ConversationManager.ImagePosition.Left;
                                break;
                            case ConversationSpeaker.Npc:
                                conversationSourceRectangle = ConversationSourceRectangle;
                                position = ConversationManager.ImagePosition.Right;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        ConversationManager.AddMessage(conversationOverride.Message, conversationSourceRectangle, position);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Behavior != null)
            {
                Behavior.Update(this, gameTime, elapsed);
            }
            
            base.Update(gameTime, elapsed);
        }

        /// <summary>
        /// Decodes a string into conversations to override the defaults.
        /// </summary>
        /// <param name="codedMessage">We expect a message string in the form of 'Me:Animals here love to talk.;Mac:I'm a great listener;Me:Meow' 
        /// we'll convert that into a list of ConversationOverrides</param>
        public void CreateConversationOverride(string codedMessage)
        {
            var conversations = codedMessage.Trim().Split(';').Where(m => m.Contains(":"));

            ConversationOverrides.Clear();

            foreach (var conversation in conversations)
            {
                var parts = conversation.Trim().Split(':');
                ConversationSpeaker speaker;
                
                switch (parts[0].Trim().ToLower())
                {
                    case "npc":
                    case "me":
                        speaker = ConversationSpeaker.Npc;
                        break;
                    case "player":
                    case "mac":
                        speaker = ConversationSpeaker.Player;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var message = parts[1].Trim();
                ConversationOverrides.Add(new ConversationOverride(speaker, message));
            }
        }

        public void ISay(string message)
        {
            ConversationManager.AddMessage(message, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }

        public void MacSays(string message)
        {
            ConversationManager.AddMessage(message, PlayerConversationRectangle, ConversationManager.ImagePosition.Left);
        }
    }
    
}
