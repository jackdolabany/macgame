namespace MacGame.Npcs
{
    /// <summary>
    /// Used to override default conversations that Npcs have with the player in the Tile Map.
    /// </summary>
    public class ConversationOverride
    {
        public ConversationOverride(ConversationSpeaker speaker, string message)
        {
            Message = message;
            Speaker = speaker;
        }

        public string Message { get; private set; }
        public ConversationSpeaker Speaker { get; private set; }
    }
}
