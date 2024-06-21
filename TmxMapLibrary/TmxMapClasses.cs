/*
Squared.Tiled
Copyright (C) 2009 Kevin Gadd

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Kevin Gadd kevin.gadd@gmail.com http://luminance.org/
*/
/*
 * Updates by Stephen Belanger - July, 13 2009
 * 
 * -added ProhibitDtd = false, so you don't need to remove the doctype line after each time you edit the map.
 * -changed everything to use SortedLists for easier referencing
 * -added objectgroups
 * -added movable and resizable objects
 * -added object images
 * -added meta property support to maps, layers, object groups and objects
 * -added non-binary encoded layer data
 * -added layer and object group transparency
*/

using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Xml;
using Microsoft.Xna.Framework;
using TileEngine;

namespace Squared.Tiled
{
    public class Tileset
    {
        public class TilePropertyList : Dictionary<string, string>
        {
        }

        public string Name;
        public int FirstTileID;
        public int TileWidth;
        public int TileHeight;
        public int ImageHeight;
        public int ImageWidth;
        public Dictionary<int, TilePropertyList> TileProperties = new Dictionary<int, TilePropertyList>();
        public string Image;

        internal static Tileset Load(XmlReader reader)
        {
            var result = new Tileset();

            result.Name = reader.GetAttribute("name");
            int.TryParse(reader.GetAttribute("firstgid"), out result.FirstTileID);
            result.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
            result.TileHeight = int.Parse(reader.GetAttribute("tileheight"));

            int currentTileId = -1;

            while (reader.Read())
            {
                var name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "image":
                                result.Image = reader.GetAttribute("source");
                                result.ImageWidth = int.Parse(reader.GetAttribute("width"));
                                result.ImageHeight = int.Parse(reader.GetAttribute("height"));
                                break;
                            case "tile":
                                currentTileId = int.Parse(reader.GetAttribute("id"));
                                break;
                            case "property":
                                {
                                    TilePropertyList props;
                                    if (!result.TileProperties.TryGetValue(currentTileId, out props))
                                    {
                                        props = new TilePropertyList();
                                        result.TileProperties[currentTileId] = props;
                                    }

                                    props[reader.GetAttribute("name")] = reader.GetAttribute("value");
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }

            return result;
        }

        public TilePropertyList GetTileProperties(int index)
        {
            index -= FirstTileID;

            if (index < 0)
                return null;

            TilePropertyList result = null;
            TileProperties.TryGetValue(index, out result);
            if (result == null)
            {
                result = new TilePropertyList();
            }

            return result;
        }

        public string TexturePath { get; set; }

        internal bool MapTileToRect(int index, ref Rectangle rect)
        {
            index -= FirstTileID;

            if (index < 0)
                return false;

            int rowSize = ImageWidth / TileWidth;
            int row = index / rowSize;
            int numRows = ImageHeight / TileHeight;
            if (row >= numRows)
                return false;

            int col = index % rowSize;

            rect.X = col * TileWidth;
            rect.Y = row * TileHeight;
            rect.Width = TileWidth;
            rect.Height = TileHeight;
            return true;
        }
    }

    [Serializable]
    public class TileInfo
    {
        public string TexturePath;
        public Rectangle Rectangle;
        public Tileset Tileset;
        public Dictionary<string, string> properties;
    }

    [Serializable]
    public class Layer
    {
        public System.Collections.Generic.SortedList<string, string> Properties = new SortedList<string, string>();

        public string Name;
        public int Width, Height;
        public float Opacity = 1;
        public int[] Tiles;
        public TileInfo[] TileInfoCache = null;

        internal static Layer Load(XmlReader reader)
        {
            var result = new Layer();

            if (reader.GetAttribute("name") != null)
                result.Name = reader.GetAttribute("name");
            if (reader.GetAttribute("width") != null)
                result.Width = int.Parse(reader.GetAttribute("width"));
            if (reader.GetAttribute("height") != null)
                result.Height = int.Parse(reader.GetAttribute("height"));
            if (reader.GetAttribute("opacity") != null)
                result.Opacity = float.Parse(reader.GetAttribute("opacity"));

            result.Tiles = new int[result.Width * result.Height];

            while (!reader.EOF)
            {
                var name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "data":
                                {
                                    if (reader.GetAttribute("encoding") != null)
                                    {
                                        var encoding = reader.GetAttribute("encoding");
                                        var compressor = reader.GetAttribute("compression");
                                        switch (encoding)
                                        {
                                            case "base64":
                                                {
                                                    int dataSize = (result.Width * result.Height * 4) + 1024;
                                                    var buffer = new byte[dataSize];
                                                    reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                                                    Stream stream = new MemoryStream(buffer, false);
                                                    if (compressor == "gzip")
                                                        stream = new GZipStream(stream, CompressionMode.Decompress, false);

                                                    using (stream)
                                                    {
                                                        using (var br = new BinaryReader(stream))
                                                        {
                                                            for (int i = 0; i < result.Tiles.Length; i++)
                                                                result.Tiles[i] = br.ReadInt32();
                                                        }
                                                    }
                                                    continue;
                                                };

                                            default:
                                                throw new Exception("Unrecognized encoding.");
                                        }
                                    }
                                    else
                                    {
                                        using (var st = reader.ReadSubtree())
                                        {
                                            int i = 0;
                                            while (!st.EOF)
                                            {
                                                switch (st.NodeType)
                                                {
                                                    case XmlNodeType.Element:
                                                        if (st.Name == "tile")
                                                        {
                                                            if (i < result.Tiles.Length)
                                                            {
                                                                result.Tiles[i] = int.Parse(st.GetAttribute("gid"));
                                                                i++;
                                                            }
                                                        }

                                                        break;
                                                    case XmlNodeType.EndElement:
                                                        break;
                                                }

                                                st.Read();
                                            }
                                        }
                                    }
                                    Console.WriteLine("It made it!");
                                }
                                break;
                            case "properties":
                                {
                                    using (var st = reader.ReadSubtree())
                                    {
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "property")
                                                    {
                                                        if (st.GetAttribute("name") != null)
                                                        {
                                                            result.Properties.Add(st.GetAttribute("name"), st.GetAttribute("value"));
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }

        public int GetTile(int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height))
                throw new InvalidOperationException();

            int index = (y * Width) + x;
            return Tiles[index];
        }

        public void BuildTileInfoCache(IList<Tileset> tilesets)
        {
            Rectangle rect = new Rectangle();
            var cache = new List<TileInfo>();
            int i = 1;

        next:
            for (int t = 0; t < tilesets.Count; t++)
            {
                if (tilesets[t].MapTileToRect(i, ref rect))
                {
                    cache.Add(new TileInfo
                    {
                        TexturePath = tilesets[t].TexturePath,
                        Tileset = tilesets[t],
                        properties = tilesets[t].GetTileProperties(i),
                        Rectangle = rect
                    });
                    i++;
                    goto next;
                }
            }

            TileInfoCache = cache.ToArray();
        }
    }

    [Serializable]
    public class ObjectGroup
    {
        public List<KeyValuePair<string, Object>> Objects = new List<KeyValuePair<string, Object>>();
        public List<KeyValuePair<string, string>> Properties = new List<KeyValuePair<string, string>>();

        public string Name;
        public int Width, Height, X, Y;
        float Opacity = 1;

        internal static ObjectGroup Load(XmlReader reader)
        {
            var result = new ObjectGroup();

            if (reader.GetAttribute("name") != null)
                result.Name = reader.GetAttribute("name");
            if (reader.GetAttribute("width") != null)
                result.Width = int.Parse(reader.GetAttribute("width"));
            if (reader.GetAttribute("height") != null)
                result.Height = int.Parse(reader.GetAttribute("height"));
            if (reader.GetAttribute("x") != null)
                result.X = int.Parse(reader.GetAttribute("x"));
            if (reader.GetAttribute("y") != null)
                result.Y = int.Parse(reader.GetAttribute("y"));
            if (reader.GetAttribute("opacity") != null)
                result.Opacity = float.Parse(reader.GetAttribute("opacity"));

            while (!reader.EOF)
            {
                var name = reader.Name;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (name)
                        {
                            case "object":
                                {
                                    using (var st = reader.ReadSubtree())
                                    {
                                        st.Read();
                                        var objects = Object.Load(st);
                                        result.Objects.Add(new KeyValuePair<string, Object>(objects.Name ?? "", objects));
                                    }
                                }
                                break;
                            case "properties":
                                {
                                    using (var st = reader.ReadSubtree())
                                    {
                                        while (!st.EOF)
                                        {
                                            switch (st.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    if (st.Name == "property")
                                                    {
                                                        st.Read();
                                                        if (st.GetAttribute("name") != null)
                                                        {
                                                            result.Properties.Add(new KeyValuePair<string, string>(st.GetAttribute("name"), st.GetAttribute("value")));
                                                        }
                                                    }

                                                    break;
                                                case XmlNodeType.EndElement:
                                                    break;
                                            }

                                            st.Read();
                                        }
                                    }
                                }
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }
    }

    [Serializable]
    public class Object
    {
        public List<KeyValuePair<string, string>> Properties = new List<KeyValuePair<string, string>>();

        public string Name, Image;
        public int Width, Height, X, Y;

        public string TextureName { get; set; }

        internal static Object Load(XmlReader reader)
        {
            var result = new Object();

            result.Name = reader.GetAttribute("name");
            result.X = (int)float.Parse(reader.GetAttribute("x"));
            result.Y = (int)float.Parse(reader.GetAttribute("y"));

            // If you click once without resizing, these can be null
            float width = 0;
            float height = 0;
            float.TryParse(reader.GetAttribute("width"), out width);
            float.TryParse(reader.GetAttribute("height"), out height);
            result.Width = (int)width;
            result.Height = (int)height;

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "properties")
                        {
                            using (var st = reader.ReadSubtree())
                            {
                                while (!st.EOF)
                                {
                                    switch (st.NodeType)
                                    {
                                        case XmlNodeType.Element:
                                            if (st.Name == "property")
                                            {
                                                if (st.GetAttribute("name") != null)
                                                {
                                                    result.Properties.Add(new KeyValuePair<string, string>(st.GetAttribute("name") ?? "", st.GetAttribute("value") ?? ""));
                                                }
                                            }

                                            break;
                                        case XmlNodeType.EndElement:
                                            break;
                                    }

                                    st.Read();
                                }
                            }
                        }
                        if (reader.Name == "image")
                        {
                            result.Image = reader.GetAttribute("source");
                        }

                        break;
                    case XmlNodeType.EndElement:
                        break;
                }

                reader.Read();
            }

            return result;
        }

    }

    [Serializable]
    public class Map
    {
        public SortedList<string, Tileset> Tilesets = new SortedList<string, Tileset>();

        //public System.Collections.Generic.SortedList<string, Layer> Layers = new SortedList<string, Layer>();
        public List<Layer> Layers = new List<Layer>();

        public System.Collections.Generic.SortedList<string, ObjectGroup> ObjectGroups = new SortedList<string, ObjectGroup>();
        public System.Collections.Generic.SortedList<string, string> Properties = new SortedList<string, string>();
        public int Width, Height;
        public int TileWidth, TileHeight;

        public static Map Load(string filename)
        {
            var result = new Map();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;

            using (var stream = System.IO.File.OpenText(filename))
            using (var reader = XmlReader.Create(stream, settings))
                while (reader.Read())
                {
                    var name = reader.Name;

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.DocumentType:
                            if (name != "map")
                                throw new Exception("Invalid map format");
                            break;
                        case XmlNodeType.Element:
                            switch (name)
                            {
                                case "map":
                                    {
                                        result.Width = int.Parse(reader.GetAttribute("width"));
                                        result.Height = int.Parse(reader.GetAttribute("height"));
                                        result.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
                                        result.TileHeight = int.Parse(reader.GetAttribute("tileheight"));
                                    }
                                    break;
                                case "tileset":
                                    {
                                        Tileset tileset = null;
                                        using (var st = reader.ReadSubtree())
                                        {
                                            st.Read();
                                            var source = st.GetAttribute("source");
                                            if (source == null)
                                            {
                                                tileset = Tileset.Load(st);
                                            }
                                            else
                                            {
                                                //read from a .tsx file!
                                                var sourcePath = Path.Combine(Path.GetDirectoryName(filename), source);
                                                using (var tsxStream = System.IO.File.OpenText(sourcePath))
                                                {
                                                    using (var tsxReader = XmlReader.Create(tsxStream, settings))
                                                    {
                                                        //Wait for it...
                                                        tsxReader.Read();
                                                        tsxReader.Read();
                                                        tsxReader.Read();
                                                        //Go!
                                                        tileset = Tileset.Load(tsxReader);
                                                    }
                                                }
                                                tileset.FirstTileID = int.Parse(st.GetAttribute("firstgid"));
                                            }
                                        }
                                        result.Tilesets.Add(tileset.Name, tileset);
                                    }
                                    break;
                                case "layer":
                                    {
                                        using (var st = reader.ReadSubtree())
                                        {
                                            st.Read();
                                            var layer = Layer.Load(st);
                                            result.Layers.Add(layer);
                                        }
                                    }
                                    break;
                                case "objectgroup":
                                    {
                                        using (var st = reader.ReadSubtree())
                                        {
                                            st.Read();
                                            var objectgroup = ObjectGroup.Load(st);
                                            result.ObjectGroups.Add(objectgroup.Name, objectgroup);
                                        }
                                    }
                                    break;
                                case "properties":
                                    {
                                        using (var st = reader.ReadSubtree())
                                        {
                                            while (!st.EOF)
                                            {
                                                switch (st.NodeType)
                                                {
                                                    case XmlNodeType.Element:
                                                        if (st.Name == "property")
                                                        {
                                                            if (st.GetAttribute("name") != null)
                                                            {
                                                                result.Properties.Add(st.GetAttribute("name"), st.GetAttribute("value"));
                                                            }
                                                        }

                                                        break;
                                                    case XmlNodeType.EndElement:
                                                        break;
                                                }

                                                st.Read();
                                            }
                                        }
                                    }
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                    }
                }

            foreach (var tileset in result.Tilesets.Values)
            {
                var path = Path.Combine("Textures", Path.GetFileNameWithoutExtension(tileset.Image));
                tileset.TexturePath = path;
            }

            return result;
        }

        public TileEngine.TileMap PopulateFromTMXMap()
        {
            var regularColor = Color.White;
            var backgroundColor = Color.Lerp(Color.White, Color.Black, 0.5f);

            var tileMap = new TileEngine.TileMap();

            tileMap.Properties = this.Properties.ToDictionary(p => p.Key, p => p.Value);

            //populate our map properties from the .tmx map
            int height = this.Height;
            int width = this.Width;
            int depth = this.Layers.Count();

            tileMap.Backgrounds = this.Properties.Where(p => p.Key.ToLower().StartsWith("background")).Select(p => p.Value.ToLower()).ToList();

            var zoom = this.Properties.Where(p => p.Key.ToLower() == "zoom").Select(p => p.Value).SingleOrDefault();
            if (zoom != null)
            {
                tileMap.Zoom = float.Parse(zoom);
            }
            else
            {
                tileMap.Zoom = 1f;
            }

            tileMap.Initialize(height, width);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileMap.MapCells[x][y] = new TileEngine.MapSquare(depth, true);
                    for (int z = 0; z < depth; z++)
                    {
                        tileMap.MapCells[x][y].LayerTiles[z] = new TileEngine.Tile();
                        tileMap.MapCells[x][y].LayerTiles[z].TileHeight = TileEngine.TileMap.TileSize;
                        tileMap.MapCells[x][y].LayerTiles[z].TileWidth = TileEngine.TileMap.TileSize;
                    }
                }
            }
            tileMap.Layers = new List<TileEngine.Layer>();

            // Need to work in reverse to travers the layers from front to back.
            for (int z = this.Layers.Count() - 1; z >= 0; z--)
            {
                var layer = this.Layers[z];

                if (layer.TileInfoCache == null)
                {
                    layer.BuildTileInfoCache(this.Tilesets.Values);
                }

                var tileMapLayer = new TileEngine.Layer();
                tileMap.Layers.Add(tileMapLayer);

                tileMapLayer.Properties = layer.Properties.Select(p => new TileEngine.Property() { name = p.Key, value = p.Value }).ToList();
                tileMapLayer.IsParallax = tileMapLayer.Properties.Any(p => p.name.ToLower() == "scroll");

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {

                        if (tileMapLayer.IsParallax)
                        {
                            tileMap.MapCells[x][y].LayerTiles[z].Color = backgroundColor;
                        }
                        else
                        {
                            tileMap.MapCells[x][y].LayerTiles[z].Color = regularColor;
                        }

                        int tileIndex = layer.GetTile(x, y);

                        // .TMX maps use 0 for the when there is no tile. 
                        // We'll assume that we can always draw the texture at tile 1 and it
                        // will be empty.
                        if (tileIndex == 0)
                        {
                            tileIndex = 1;
                        }

                        var tileInfo = layer.TileInfoCache[tileIndex - 1];

                        if (tileInfo.properties.ContainsKey("LoadClass"))
                        {
                            tileMap.MapCells[x][y].LayerTiles[z].LoadClass = tileInfo.properties["LoadClass"];

                            // Reveal blocks might make formerlly impassble tiles passable. 
                            // If they reveal a future non-passable tile, it'll set this to not 
                            // passable again.
                            if (tileInfo.properties["LoadClass"] == "RevealBlock")
                            {
                                tileMap.MapCells[x][y].Passable = true;
                            }

                        }
                        else if (tileInfo.properties.ContainsKey("BlockPlayer"))
                        {
                            tileMap.MapCells[x][y].Passable = false;
                        }
                        else if (tileInfo.properties.ContainsKey("PlayerStart"))
                        {
                            tileMap.PlayerStart = new Vector2(x, y);
                            tileMap.MapCells[x][y].LayerTiles[z].LoadClass = "PlayerStart";
                        }
                        
                        if (tileInfo.properties.ContainsKey("BlockEnemy"))
                        {
                            tileMap.MapCells[x][y].EnemyPassable = false;
                        }
                        if (tileInfo.properties.ContainsKey("BlockPlatform"))
                        {
                            tileMap.MapCells[x][y].PlatformPassable = false;
                        }
                        
                        if (tileInfo.properties.ContainsKey("Sand"))
                        {
                            tileMap.MapCells[x][y].IsSand = true;
                        }
                        else if (tileInfo.properties.ContainsKey("Ice"))
                        {
                            tileMap.MapCells[x][y].IsIce = true;
                        }
                        else if (tileInfo.properties.ContainsKey("Water"))
                        {
                            tileMap.MapCells[x][y].IsWater = true;
                        }
                        else if (tileInfo.properties.ContainsKey("MinecartTrack"))
                        {
                            tileMap.MapCells[x][y].IsMinecartTrack = true;
                        }
                        else if (tileInfo.properties.ContainsKey("Ladder"))
                        {
                            tileMap.MapCells[x][y].IsLadder = true;
                        }
                        else if (tileInfo.properties.ContainsKey("Vine"))
                        {
                            tileMap.MapCells[x][y].IsVine = true;
                        }

                        // Only if the cell doesn't have the previous properties do we consider it something we should draw!
                        var tile = tileMap.MapCells[x][y].LayerTiles[z];
                        tile.TexturePath = tileInfo.TexturePath;
                            
                        // The tile ID from the layer's tile is global and stretches across multiple tilesets.
                        // we need an ID local the the tile set to draw it against the texture.
                        int localIndex = layer.GetTile(x, y) - tileInfo.Tileset.FirstTileID;
                        tile.TileIndex = localIndex;

                        // These tiles don't need to draw even though they have textures.
                        var shouldDrawTile = layer.Name.ToLower() != "collisions"
                            && !tileInfo.properties.ContainsKey("LoadClass")
                            && !tileInfo.properties.ContainsKey("PlayerStart")
                            && !tileInfo.properties.ContainsKey("Hidden");
                        tile.ShouldDraw = shouldDrawTile;

                    } // end for y
                } // end for x
            }

            // Convert the Tiled ObjectGroups thing into a List of object modifiers for our game.
            foreach (var obj in this.ObjectGroups.Values.SelectMany(og => og.Objects))
            {
                
                var om = new ObjectModifier();
                om.Rectangle = new Rectangle(obj.Value.X, obj.Value.Y, obj.Value.Width, obj.Value.Height);
                om.Name = obj.Value.Name;

                foreach(var kvp in obj.Value.Properties)
                {
                    om.Properties.Add(kvp.Key, kvp.Value);
                }

                tileMap.ObjectModifiers.Add(om);
            }

            return tileMap;
        }

    }
}
