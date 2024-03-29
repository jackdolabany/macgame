using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    public class CricketCoin : Item
    {
        public CricketCoin(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var spin = new AnimationStrip(textures, new Rectangle(10 * Game1.TileSize, 2 * Game1.TileSize, 16, 16), 3, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = true;
            spin.FrameLength = 0.15f;
            animations.Add(spin);

            animations.Play("spin");

            SetCenteredCollisionRectangle(14, 14);

            IsInChest = false;
        }

        public override void WhenCollected(Player player)
        {

            player.CricketCoins += 1;

            // TODO: Stuff
            // take the player back to the main room. Reset tacos, health, etc. Save the game.


            // TODO: Play sound
            //SoundManager.PlaySound("CricketCoinCollected");
        }
    }
}
