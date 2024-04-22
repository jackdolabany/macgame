using Microsoft.Xna.Framework.Content.Pipeline;

namespace CustomContentProcessorLibrary
{
    [ContentProcessor(DisplayName = "BigTileBorderImageProcessor")]
    public class BigTileBorderImageProcessor : TileBorderImageProcessor
    {
        public BigTileBorderImageProcessor() : base(16, 4)
        {
        }
    }
}
