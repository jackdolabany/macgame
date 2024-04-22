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
                    Copy(inputImage,
                        new Rectangle(x, y, tileSize, tileSize),
                        outputMipmap,
                        new Rectangle(x * scale + (2 * column) + 1, y * scale + (2 * row) + 1, tileSize * scale, tileSize * scale));

                    // prepare to Fill in the borders with the adjacent pixel
                    var newLeft = x * scale + (2 * column);
                    var newRight = newLeft + tileSize * scale + 1;
                    var newTop = y * scale + (2 * row);
                    var newBottom = newTop + tileSize * scale + 1;

                    // Add borders
                    for (int i = 0; i < tileSize * scale; i++)
                    {
                        // Left
                        outputMipmap.SetPixel(newLeft, newTop + 1 + i, inputImage.GetPixel(x, y + i / scale));
                        // Right
                        outputMipmap.SetPixel(newRight, newTop + 1 + i, inputImage.GetPixel(x + tileSize - 1, y + i / scale));
                        // Top
                        outputMipmap.SetPixel(newLeft + 1 + i, newTop, inputImage.GetPixel(x + i / scale, y));
                        // Bottom
                        outputMipmap.SetPixel(newLeft + 1 + i, newBottom, inputImage.GetPixel(x + i / scale, y + tileSize - 1));

                    }

                    // Corners
                    // Top Left
                    outputMipmap.SetPixel(newLeft, newTop, inputImage.GetPixel(x, y));

                    // Top Right
                    outputMipmap.SetPixel(newRight, newTop, inputImage.GetPixel(x + tileSize - 1, y));

                    // Bottom Left
                    outputMipmap.SetPixel(newLeft, newBottom, inputImage.GetPixel(x, y + tileSize - 1));

                    // Bottom Right
                    outputMipmap.SetPixel(newRight, newBottom, inputImage.GetPixel(x + tileSize - 1, y + tileSize - 1));
                }
            }

            output.Mipmaps.Add(outputMipmap);

            return base.Process(output, context);
        }

        /// <summary>
        /// This method replaces BitmapContent.Copy() which would be nice but it applies smooth scaling to the image.
        /// </summary>
        public static void Copy(PixelBitmapContent<Color> sourceBitmap, Rectangle sourceRegion, PixelBitmapContent<Color> destinationBitmap, Rectangle destinationRegion)
        {

            // Who knows what this will do if you don't scale it in whole numbers.            
            var scale = destinationRegion.Width / sourceRegion.Width;

            for (int x = 0; x < sourceRegion.Width; x++)
            {
                for (int y = 0; y < sourceRegion.Height; y++)
                {
                    var sourcePixel = sourceBitmap.GetPixel(sourceRegion.X + x, sourceRegion.Y + y);

                    // for each pixel in the source, write scale * scale pixels to the destination.
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            var destinationX = destinationRegion.X + (x * scale) + i;
                            var destinationY = destinationRegion.Y + (y * scale) + j;
                            destinationBitmap.SetPixel(destinationX, destinationY, sourcePixel);
                        }
                    }

                }
            }

        }

    }
}