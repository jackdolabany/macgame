using Microsoft.Xna.Framework.Content.Pipeline;

namespace CustomContentProcessorLibrary
{
    [ContentProcessor(DisplayName = "ReallyBigTileBorderImageProcessor")]
    public class ReallyBigTileBorderImageProcessor : TileBorderImageProcessor
    {
        public ReallyBigTileBorderImageProcessor() : base(24, 4)
        {
        }
    }
}
