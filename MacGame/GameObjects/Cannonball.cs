using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Cannonball : PickupObject
    {

        public override float Friction => 2f;
        Cannon CannonHoldingMe;

        // True for the initial shot out of hte cannon until it hits a wall or something.
        public bool IsShootingOutOfCannon = false;
        
        public Cannonball(ContentManager content, int x, int y, Player player) : base(content, x, y, player)
        {
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(4, 7)); ;

            Enabled = true;

            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);
            IsAffectedByGravity = true;

            this.SetCenteredCollisionRectangle(8, 8);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (CannonHoldingMe == null && !IsPickedUp)
            {
                foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (gameObject is Cannon)
                    {
                        var cannon = (Cannon)gameObject;
                        if (cannon.CanAcceptCannonball())
                        {
                            if (cannon.CollisionRectangle.Contains(this.WorldCenter))
                            {
                                this.Enabled = false;
                                CannonHoldingMe = cannon;
                                cannon.LoadCannonball(this);
                                IsShootingOutOfCannon = false;
                                break;
                            }
                        }
                    }
                }
            }

            base.Update(gameTime, elapsed);

            if (OnLeftWall || OnGround || OnRightWall || OnCeiling)
            {
                IsShootingOutOfCannon = false;
            }
        }


        public void ShootOutOfCannon(Vector2 velocity)
        {
            this.Enabled = true;
            this.velocity = velocity;
            this.WorldLocation = CannonHoldingMe.WorldLocation;
            CannonHoldingMe.CannonballInside = null;
            CannonHoldingMe = null;
            IsShootingOutOfCannon = true;
        }
    }

}
