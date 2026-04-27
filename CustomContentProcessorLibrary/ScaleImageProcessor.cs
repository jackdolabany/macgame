using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace CustomContentProcessorLibrary
{
    [ContentProcessor(DisplayName = "ScaleImageProcessor")]
    public class ScaleImageProcessor : TextureProcessor
    {
        private const int Scale = 4;

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            var inputImage = (PixelBitmapContent<Color>)input.Faces[0][0];

            var outputImage = new PixelBitmapContent<Color>(inputImage.Width * Scale, inputImage.Height * Scale);

            for (int x = 0; x < inputImage.Width; x++)
            {
                for (int y = 0; y < inputImage.Height; y++)
                {
                    var pixel = inputImage.GetPixel(x, y);
                    for (int i = 0; i < Scale; i++)
                    {
                        for (int j = 0; j < Scale; j++)
                        {
                            outputImage.SetPixel(x * Scale + i, y * Scale + j, pixel);
                        }
                    }
                }
            }

            var output = new Texture2DContent();
            output.Mipmaps.Add(outputImage);

            return base.Process(output, context);
        }
    }
}
