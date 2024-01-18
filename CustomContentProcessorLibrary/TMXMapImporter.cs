using Squared.Tiled;
using Microsoft.Xna.Framework.Content.Pipeline;
using TileEngine;

namespace CustomContentProcessorLibrary
{
    [ContentImporter(".tmx", DefaultProcessor = "PassThroughProcessor", DisplayName = "TMX Map Importer")]
    public class TMXMapImporter : ContentImporter<TileMap>
    {
        public override TileMap Import(string filename, ContentImporterContext context)
        {
            return Map.Load(filename).PopulateFromTMXMap();
        }
    }
}
