using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class WaterBomb : GameObject
    {

        AnimationDisplay animationDisplay => (AnimationDisplay)DisplayComponent;

        private Player _player;

        private enum BombState
        {
            /// <summary>
            ///  You need to talk to the NPC to enable the bombs.
            /// </summary>
            NotYetEnabled,

            /// <summary>
            /// The timer is ticking and we could blow!
            /// </summary>
            Active,

            /// <summary>
            /// Mac is actively disabling this bomb.
            /// </summary>
            Disabling,

            /// <summary>
            /// Mac disabled this bomb.
            /// </summary>
            Disabled
        }

        BombState _state;

        public WaterBomb(ContentManager content, int cellX, int cellY, Player player) : base()
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;

            var active = new AnimationStrip(textures, Helpers.GetBigTileRect(4, 9), 2, "active");
            active.LoopAnimation = true;
            active.FrameLength = 0.2f;
            ad.Add(active);

            var disabled = new AnimationStrip(textures, Helpers.GetBigTileRect(6, 9), 1, "disabled");
            disabled.LoopAnimation = false;
            ad.Add(disabled);

            SetCenteredCollisionRectangle(8, 16);

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize, (cellY + 2) * TileMap.TileSize);
            Enabled = true;

            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;

            animationDisplay.Play("active");

            _player = player;

            // Change once NPC enables this.
            _state = BombState.Active;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == BombState.Active && _player.CollisionRectangle.Contains(this.CollisionCenter))
            {
                _player.StartDisableWaterBomb(this);
                _state = BombState.Disabling;
            }

            base.Update(gameTime, elapsed);
        }

        /// <summary>
        /// I'm disabled!
        /// </summary>
        public void Disable()
        {
            SoundManager.PlaySound("PowerUp");
            animationDisplay.Play("disabled");
            _state = BombState.Disabled;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_state != BombState.NotYetEnabled)
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
