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
        private float _appearTimer = 0;

        private MapSquare _cell;

        private MapSquare Cell
        {
            get
            {
                if (_cell == null)
                {
                    _cell = Game1.CurrentMap.GetMapSquareAtCell(CellX, CellY)!;
                }
                return _cell;
            }
        }

        public DisappearBlock(ContentManager content, int cellX, int cellY) : base()
        {
            CellX = cellX;
            CellY = cellY;
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = false;
            CollisionRectangle = new Rectangle(-TileMap.TileSize / 2, -TileMap.TileSize, TileMap.TileSize, TileMap.TileSize);

            var animations = new AnimationDisplay();
            DisplayComponent = animations;

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(7, 0), 5, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.1f;
            animations.Add(idle);

        }

        public bool IsColliding(Rectangle rectangleToTest)
        {
            return new Rectangle(CellX * TileMap.TileSize, CellY * TileMap.TileSize, TileMap.TileSize, TileMap.TileSize)
                .Intersects(rectangleToTest);
        }

        public void Appear(float appearTime)
        {
            _appearTimer = appearTime;
            Cell.Passable = false;
            Enabled = true;
            animations.Play("idle");
        }

        public void Disappear()
        {
            _appearTimer = 0;
            Cell.Passable = true;
            Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_appearTimer > 0)
            {
                _appearTimer -= elapsed;
                if (_appearTimer <= 0)
                {
                    Disappear();
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
