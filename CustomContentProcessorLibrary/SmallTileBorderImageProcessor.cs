using Microsoft.Xna.Framework.Content.Pipeline;

namespace CustomContentProcessorLibrary
{
    [ContentProcessor(DisplayName = "SmallTileBorderImageProcessor")]
    public class SmallTileBorderImageProcessor : TileBorderImageProcessor
    {
        public SmallTileBorderImageProcessor() : base (8, 4)
        {
        }
    }

}