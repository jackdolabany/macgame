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
    /// 
    /// This is custom shit and assumes 16 pixel tiles
    /// </summary>
    [ContentProcessor(DisplayName = "BorderTileImageProcessor")]
    public class TileImageProcessor : TextureProcessor
    {
        public const int TILE_SIZE = 16;

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            PixelBitmapContent<Color> image = (PixelBitmapContent<Color>)input.Faces[0][0];

            var columnCount = image.Width / TILE_SIZE;
            var rowCount = image.Height / TILE_SIZE;

            PixelBitmapContent<Color> mipmap = new PixelBitmapContent<Color>(image.Width + (2 * columnCount), image.Height + (2 * rowCount));
            var output = new Texture2DContent();

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    int x = column * TILE_SIZE;
                    int y = row * TILE_SIZE;

                    // Copy the original tile into the correct location in the output
                    BitmapContent.Copy(
                        image, new Rectangle(x, y, TILE_SIZE, TILE_SIZE),
                        mipmap, new Rectangle(x + (2 * column) + 1, y + (2 * row) + 1, TILE_SIZE, TILE_SIZE));

                    // prepare to Fill in the borders with the adjacent pixel
                    var newLeft = x + (2 * column);
                    var newRight = newLeft + TILE_SIZE + 1;
                    var newTop = y + (2 * row);
                    var newBottom = newTop + TILE_SIZE + 1;

                    // Add borders
                    for (int i = 0; i < TILE_SIZE; i++)
                    {
                        // Left
                        mipmap.SetPixel(newLeft, newTop + 1 + i, image.GetPixel(x, y + i).Process());
                        // Right
                        mipmap.SetPixel(newRight, newTop + 1 + i, image.GetPixel(x + TILE_SIZE - 1, y + i).Process());
                        // Top
                        mipmap.SetPixel(newLeft + 1 + i, newTop, image.GetPixel(x + i, y).Process());
                        // Bottom
                        mipmap.SetPixel(newLeft + 1 + i, newBottom, image.GetPixel(x + i, y + TILE_SIZE - 1).Process());

                    }

                    // corners should be a mix of the two adjecent pixels in this tile.
                    // Top Left
                    mipmap.SetPixel(newLeft, newTop, Color.Lerp(image.GetPixel(x, y + 1), image.GetPixel(x + 1, y), 0.5f).Process());

                    // Top Right
                    mipmap.SetPixel(newRight, newTop, Color.Lerp(image.GetPixel(x + TILE_SIZE - 2, y), image.GetPixel(x + TILE_SIZE - 1, y + 1), 0.5f).Process());

                    // Bottom Left
                    var toTheRight = image.GetPixel(x + 1, y + TILE_SIZE - 1);
                    var above = image.GetPixel(x, y + TILE_SIZE - 2);
                    mipmap.SetPixel(newLeft, newBottom, Color.Lerp(toTheRight, above, 0.5f).Process());

                    // Bottom Right
                    mipmap.SetPixel(newRight, newBottom, Color.Lerp(image.GetPixel(x + TILE_SIZE - 2, y + TILE_SIZE - 1), image.GetPixel(x + TILE_SIZE - 1, y + TILE_SIZE - 2), 0.5f).Process());

                }
            }

            output.Mipmaps.Add(mipmap);

            return base.Process(output, context);
        }

    }

    public static class helpers
    {
        /// <summary>
        /// Processes the pixel for the border so we have a single point to fudge values.
        /// </summary>
        public static Color Process(this Color color)
        {
            return color;
        }
    }
}