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

    /// <summary>
    /// To give NPCs conversations, decorate them with an Object modifier with a Convo property.
    /// They can be simple, or more complex. Examples:
    ///
    /// Convo - Me:The marks on your head look like stars in the sky
    /// Convo - Me:Animals here love to talk.;Mac:I'm a great listener;Me:Meow
    ///
    /// You can add more Convo properties named Convo2, Convo3, etc if an Npc needs more than one possible
    /// conversation. Normally the last one listed wins, but any of them can be gated behind owning a specific
    /// sock by starting the value with Sock:SockName. A conversation with a satisfied Sock requirement always
    /// takes priority over a plain, ungated conversation. Example:
    ///
    /// Convo - Me:Hey how are you?;Mac:Fine
    /// Convo2 - Sock:GrokSock;Me:Wow I can't believe you beat Grok!
    ///
    /// Here Convo2 will play once the player has GrokSock, and Convo plays otherwise.
    /// </summary>
    public abstract class Npc : GameObject
    {

        public Behavior? Behavior { get; set; }
        public abstract Rectangle ConversationSourceRectangle { get; }

        public Rectangle PlayerConversationRectangle => Helpers.GetReallyBigTileRect(0, 0);

        /// <summary>
        /// The possible conversations for this Npc, in the order they were declared (see AddConversation).
        /// </summary>
        public List<NpcConversation> Conversations { get; set; }

        public Npc(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {

            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            IsAffectedByGravity = true;
            IsAbleToSurviveOutsideOfWorld = true;

            Conversations = new List<NpcConversation>();
        }

        public abstract void InitiateConversation();

        public virtual bool CanInteract => true;

        public virtual void CheckPlayerInteractions(Player player)
        {
            // Handle the player talking to the NPC.
            if (player.InteractButtonPressedThisFrame && this.CollisionRectangle.Intersects(player.NpcRectangle))
            {
                var conversationToPlay = GetConversationToPlay();

                if (conversationToPlay != null)
                {
                    PlayConversationMessages(conversationToPlay.Messages);
                }
                else
                {
                    InitiateConversation();
                }
            }
        }

        /// <summary>
        /// Picks which conversation should play. A conversation gated behind a sock the player has beats a
        /// plain, ungated one. If more than one candidate qualifies, the last one listed wins.
        /// </summary>
        private NpcConversation? GetConversationToPlay()
        {
            NpcConversation? fallback = null;
            NpcConversation? sockMatch = null;

            foreach (var conversation in Conversations)
            {
                if (conversation.RequiredSockName == null)
                {
                    fallback = conversation;
                }
                else if (Game1.StorageState.HasCollectedSock(conversation.RequiredSockName))
                {
                    sockMatch = conversation;
                }
            }

            return sockMatch ?? fallback;
        }

        private void PlayConversationMessages(List<ConversationOverride> messages)
        {
            foreach (var conversationOverride in messages)
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

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Behavior != null)
            {
                Behavior.Update(this, gameTime, elapsed);
            }

            base.Update(gameTime, elapsed);
        }

        /// <summary>
        /// Decodes a Convo property value into a conversation and adds it to Conversations. We expect a
        /// string in the form of 'Me:Animals here love to talk.;Mac:I'm a great listener;Me:Meow', optionally
        /// prefixed with 'Sock:SockName;' to gate the conversation behind owning that sock.
        /// </summary>
        public void AddConversation(string codedMessage)
        {
            var parts = codedMessage.Trim().Split(';').Where(m => m.Contains(":")).ToList();

            string? requiredSockName = null;

            if (parts.Count > 0 && parts[0].Trim().StartsWith("Sock:", StringComparison.OrdinalIgnoreCase))
            {
                requiredSockName = parts[0].Trim().Split(':')[1].Trim();
                parts = parts.Skip(1).ToList();
            }

            var messages = ParseConversationParts(parts);

            Conversations.Add(new NpcConversation(requiredSockName, messages));
        }

        private static List<ConversationOverride> ParseConversationParts(IEnumerable<string> conversationParts)
        {
            var messages = new List<ConversationOverride>();

            foreach (var conversation in conversationParts)
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
                messages.Add(new ConversationOverride(speaker, message));
            }

            return messages;
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
