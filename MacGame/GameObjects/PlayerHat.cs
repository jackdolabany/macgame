using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    /// <summary>
    /// A cosmetic hat Mac can pick from the hat menu.
    /// </summary>
    public abstract class PlayerHat : Headwear
    {
        public abstract string HatName { get; }

        protected PlayerHat(ContentManager content) : base(content)
        {
        }
    }
}
