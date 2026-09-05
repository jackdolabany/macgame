using System.Collections.Generic;

namespace MacGame.Npcs
{
    /// <summary>
    /// A conversation an Npc can have with the player. If RequiredSockName is set, this conversation only
    /// plays if the player has collected that sock, otherwise it's always available.
    /// </summary>
    public class NpcConversation
    {
        public NpcConversation(string? requiredSockName, List<ConversationOverride> messages)
        {
            RequiredSockName = requiredSockName;
            Messages = messages;
        }

        /// <summary>
        /// Optional, message will only play if you have this sock.
        /// </summary>
        public string? RequiredSockName { get; private set; }

        public List<ConversationOverride> Messages { get; private set; }
    }
}
