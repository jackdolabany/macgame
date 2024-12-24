using System;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public abstract class Spikes : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        
        Texture2D textures;

        public Spikes(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            textures = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(textures, GetTextureRectangle());

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetCenteredCollisionRectangle(4, 4);

            // Shift it up a bit since we want it actualled centered and not centered on the bottom middle pixel.
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y - 8, this.collisionRectangle.Width, this.collisionRectangle.Height);
        }

        public abstract Rectangle GetTextureRectangle();

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }


    // Unfortunately we need this class hierarchy to get the correct texture rectangle for the spikes.
    // I have no other way for the Textures to pass data into the Ctor. I should have an arbitrary string property 
    // on the tile map to pass in hints to the ctor. Maybe next time.

    public class SpikesDownRight : Spikes
    {
        public SpikesDownRight(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(6, 28);
        }
    }

    public class SpikesDown : Spikes
    {
        public SpikesDown(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(7, 28);
        }
    }

    public class SpikesDownLeft : Spikes
    {
        public SpikesDownLeft(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(8, 28);
        }
    }

    public class SpikesLeft : Spikes
    {
        public SpikesLeft(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(8, 29);
        }
    }

    public class SpikesUpLeft : Spikes
    {
        public SpikesUpLeft(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(8, 30);
        }
    }

    public class SpikesUp : Spikes
    {
        public SpikesUp(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(7, 30);
        }
    }

    public class SpikesUpRight : Spikes
    {
        public SpikesUpRight(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(6, 30);
        }
    }

    public class SpikesRight : Spikes
    {
        public SpikesRight(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        public override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(6, 29);
        }
    }



 
}