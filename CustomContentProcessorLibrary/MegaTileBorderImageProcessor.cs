using Microsoft.Xna.Framework.Content.Pipeline;

namespace CustomContentProcessorLibrary
{
    [ContentProcessor(DisplayName = "MegaTileBorderImageProcessor")]
    public class MegaTileBorderImageProcessor : TileBorderImageProcessor
    {
        public MegaTileBorderImageProcessor() : base(64, 4)
        {
        }
    }
}
