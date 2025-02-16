using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public abstract class ElectricSeaweed : Enemy
    {

        StaticImageDisplay regularImage;
        StaticImageDisplay electrifiedImage;

        public float electrifiedTimer = 0f;
        public float electrifiedTimerGoal = 1f;

        public float imageFlipTimer = 0f;
        public float imageFlipTimerGoal = 0.1f;

        public abstract Rectangle GetRegularImageTextureRectangle();
        public abstract Rectangle GetElectricImageTextureRectangle();

        public HashSet<ElectricSeaweed> AdjacentSeaweeds;

        public int X { get; private set; }
        public int Y { get; private set; }

        public ElectricSeaweed(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            X = cellX;
            Y = cellY;

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            regularImage = new StaticImageDisplay(textures, GetRegularImageTextureRectangle());
            electrifiedImage = new StaticImageDisplay(textures, GetElectricImageTextureRectangle());

            DisplayComponent = regularImage;

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

        public override Vector2 GetHitBackBoost(Player player)
        {
            return Vector2.Zero;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            // Do this so we don't hit the player again while we are electrified. He's been hurt enough.
            Alive = electrifiedTimer <= 0;

            if (electrifiedTimer > 0 || this.DisplayComponent == electrifiedImage)
            {
                electrifiedTimer -= elapsed;
                imageFlipTimer += elapsed;
                if (imageFlipTimer >= imageFlipTimerGoal)
                {
                    imageFlipTimer -= imageFlipTimerGoal;
                    if (DisplayComponent == regularImage)
                    {
                        DisplayComponent = electrifiedImage;
                    }
                    else
                    {
                        DisplayComponent = regularImage;
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void AfterHittingPlayer()
        {
            base.AfterHittingPlayer();

            // Make the whole group electrify as one.
            foreach (var seaweed in AdjacentSeaweeds)
            {
                seaweed.Electrify();
            }

            SoundManager.PlaySound("Electric");
        }

        public void Electrify()
        {
            electrifiedTimer = 1f;
        }

    }

    public class ElectricSeaweedUpTop : ElectricSeaweed
    {
        public ElectricSeaweedUpTop(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) {}

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(0, 2);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(1, 2);
    }

    public class ElectricSeaweedUpBottom : ElectricSeaweed
    {
        public ElectricSeaweedUpBottom(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(0, 3);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(1, 3);
    }

    public class ElectricSeaweedDownTop : ElectricSeaweed
    {
        public ElectricSeaweedDownTop(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(2, 3);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(3, 3);
    }

    public class ElectricSeaweedDownBottom : ElectricSeaweed
    {
        public ElectricSeaweedDownBottom(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(2, 2);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(3, 2);
    }

    public class ElectricSeaweedLeftTop : ElectricSeaweed
    {
        public ElectricSeaweedLeftTop(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(2, 5);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(2, 4);
    }
    public class ElectricSeaweedLeftBottom : ElectricSeaweed
    {
        public ElectricSeaweedLeftBottom(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(3, 5);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(3, 4);
    }

    public class ElectricSeaweedRightTop : ElectricSeaweed
    {
        public ElectricSeaweedRightTop(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(1, 4);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(0, 4);
    }
    public class ElectricSeaweedRightBottom : ElectricSeaweed
    {
        public ElectricSeaweedRightBottom(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera) { }

        public override Rectangle GetRegularImageTextureRectangle() => Helpers.GetTileRect(0, 4);
        public override Rectangle GetElectricImageTextureRectangle() => Helpers.GetTileRect(0, 5);
    }
}