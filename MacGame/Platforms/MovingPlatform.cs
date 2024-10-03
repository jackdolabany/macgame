using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// A moving platform that you can inherit from and subclasses can move in different directions or be static.
    /// </summary>
    public class MovingPlatform : Platform
    {

        private Vector2 _moveDirection;
        public Vector2 MoveDirection
        {
            get
            {
                return _moveDirection;
            }
            set
            {
                value.Normalize();
                _moveDirection = value;
            }
        }

        public float MoveSpeed { get; set; } = 100f;

        /// <summary>
        /// The distance to move in blocks.
        /// </summary>
        public int MoveBlocks { get; set; } = 6;

        Vector2 startPosition;

        public MovingPlatform(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {
            // We only want the top 3 pixels of the tile.
            const int tileHeight = 12;

            var sourcerect = Helpers.GetTileRect(10, 14);
            sourcerect.Height = tileHeight;

            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures/Textures"), sourcerect);
            this.CollisionRectangle = new Rectangle(-(Game1.TileSize / 2), -tileHeight, Game1.TileSize, tileHeight);

            startPosition = WorldLocation;
        }

        protected void Initialize()
        {
            velocity = MoveSpeed * MoveDirection;
        }

        /// <summary>
        /// Static Platforms have this helper because multiple tiles on the map are static platforms. This allows us to replace
        /// the image with whatever is coming from the map tile as we load this.
        /// </summary>
        public void SetTextureRectangle(Texture2D texture, Rectangle source)
        {
            this.DisplayComponent = new StaticImageDisplay(texture, source);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            // If moving away from start location
            bool isMovingAwayFromStart = Vector2.Dot(MoveDirection, WorldLocation - startPosition) > 0;

            // Max move distance is half of blocks times tile size because they move half the distance in either direction.
            var maxMoveDistance = (MoveBlocks / 2 * Game1.TileSize);

            if (isMovingAwayFromStart && Vector2.Distance(startPosition, WorldLocation) > maxMoveDistance)
            {
                Reverse();
            }

            base.Update(gameTime, elapsed);

        }

        public void Reverse()
        {
            MoveDirection *= -1;
            Velocity = MoveSpeed * MoveDirection;
        }
    }
}
