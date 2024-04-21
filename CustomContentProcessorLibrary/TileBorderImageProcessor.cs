using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace CustomContentProcessorLibrary
{
    /// <summary>
    /// This process puts a border around tile images because when they scale they tend to grab the pixel outside
    /// of their tile to do fancy anti alaising stuff. It creates tearing.
    /// </summary>
    [ContentProcessor(DisplayName = "TileBorderImageProcessor")]
    public abstract class TileBorderImageProcessor : TextureProcessor
    {
        private int tileSize;
        private int scale;
        
        public TileBorderImageProcessor(int tileSize, int scale)
        {
            this.tileSize = tileSize;
            this.scale = scale;
        }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            PixelBitmapContent<Color> inputImage = (PixelBitmapContent<Color>)input.Faces[0][0];

            var columnCount = inputImage.Width / tileSize;
            var rowCount = inputImage.Height / tileSize;

            PixelBitmapContent<Color> outputMipmap = new PixelBitmapContent<Color>(inputImage.Width * scale + (2 * columnCount), inputImage.Height * scale + (2 * rowCount));
            var output = new Texture2DContent();

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    int x = column * tileSize;
                    int y = row * tileSize;

                    // Copy the original tile into the correct location in the output
                    BitmapContent.Copy(
                        inputImage, new Rectangle(x, y, tileSize, tileSize),
                        outputMipmap, new Rectangle(x * scale + (2 * column) + 1, y * scale + (2 * row) + 1, tileSize * scale, tileSize * scale));

                    // prepare to Fill in the borders with the adjacent pixel
                    var newLeft = x * scale + (2 * column);
                    var newRight = newLeft + tileSize * scale + 1;
                    var newTop = y * scale + (2 * row);
                    var newBottom = newTop + tileSize * scale + 1;

                    // Add borders
                    for (int i = 0; i < tileSize * scale; i++)
                    {
                        // Left
                        outputMipmap.SetPixel(newLeft, newTop + 1 + i, inputImage.GetPixel(x, y + i));
                        // Right
                        outputMipmap.SetPixel(newRight, newTop + 1 + i, inputImage.GetPixel(x + tileSize - 1, y + i));
                        // Top
                        outputMipmap.SetPixel(newLeft + 1 + i, newTop, inputImage.GetPixel(x + i, y));
                        // Bottom
                        outputMipmap.SetPixel(newLeft + 1 + i, newBottom, inputImage.GetPixel(x + i, y + tileSize - 1));

                    }

                    // corners should be a mix of the two adjecent pixels in this tile.
                    // Top Left
                    outputMipmap.SetPixel(newLeft, newTop, Color.Lerp(inputImage.GetPixel(x, y + 1), inputImage.GetPixel(x + 1, y), 0.5f));

                    // Top Right
                    outputMipmap.SetPixel(newRight, newTop, Color.Lerp(inputImage.GetPixel(x + tileSize - 2, y), inputImage.GetPixel(x + tileSize - 1, y + 1), 0.5f));

                    // Bottom Left
                    var toTheRight = inputImage.GetPixel(x + 1, y + tileSize - 1);
                    var above = inputImage.GetPixel(x, y + tileSize - 2);
                    outputMipmap.SetPixel(newLeft, newBottom, Color.Lerp(toTheRight, above, 0.5f));

                    // Bottom Right
                    outputMipmap.SetPixel(newRight, newBottom, Color.Lerp(inputImage.GetPixel(x + tileSize - 2, y + tileSize - 1), inputImage.GetPixel(x + tileSize - 1, y + tileSize - 2), 0.5f));

                }
            }

            output.Mipmaps.Add(outputMipmap);

            return base.Process(output, context);
        }
    }
}