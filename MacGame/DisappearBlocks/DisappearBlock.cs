using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.DisappearBlocks
{
    /// <summary>
    /// These blocks appear and then disappear based on timers and groups.
    /// </summary>
    public class DisappearBlock : GameObject
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        /// Each block is in a numbered series. This is set in the tile layer. Only one series is displayed at
        /// a time and when that series disappears the next one shows up.
        /// </summary>
        public int Series = 0;

        public string GroupName = "";
        public int CellX;
        public int CellY;
        private bool _isInitialized = false;
        private float _appearTimer = 0;

        public DisappearBlock(ContentManager content, int cellX, int cellY) : base()
        {
            CellX = cellX;
            CellY = cellY;
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;
            CollisionRectangle = new Rectangle(-TileMap.TileSize / 2, -TileMap.TileSize, TileMap.TileSize, TileMap.TileSize);

            var animations = new AnimationDisplay();
            DisplayComponent = animations;

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(7, 0), 5, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.1f;
            animations.Add(idle);

            animations.Play("idle");
        }

        public bool IsColliding(Rectangle rectangleToTest)
        {
            return new Rectangle(CellX * TileMap.TileSize, CellY * TileMap.TileSize, TileMap.TileSize, TileMap.TileSize)
                .Intersects(rectangleToTest);
        }

        public void Appear(float appearTime)
        {
            _appearTimer = appearTime;
            var cell = Game1.CurrentLevel.Map.GetMapSquareAtCell(CellX, CellY);
            cell.Passable = false;
            Enabled = true;
            animations.Play("idle");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _isInitialized = true;
                Game1.CurrentMap.GetMapSquareAtCell(CellX, CellY).Passable = false;
            }

            if (_appearTimer > 0)
            {
                _appearTimer -= elapsed;
                if (_appearTimer <= 0)
                {
                    // Disappear
                    var cell = Game1.CurrentLevel.Map.GetMapSquareAtCell(CellX, CellY);
                    cell.Passable = true;
                    Enabled = false;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
