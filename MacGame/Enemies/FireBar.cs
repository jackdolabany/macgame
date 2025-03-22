using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A big bar of fireballs that rotate. You can alter them with map properties.
    /// 
    /// Length: The number of fireballs in the firebar.
    /// Reverse: Whether or not the firebar should rotate in the opposite direction.
    /// Offset: whether or not to offset the fireballs 50% of the way around.
    /// 
    /// </summary>
    public class FireBar : Enemy
    {

        protected float Rotation { get; set; }

        float speed = 0.85f;

        public List<Fireball> Fireballs = new List<Fireball>();

        /// <summary>
        /// The number of fireballs in the firebar.
        /// </summary>
        protected int Length { get; set; } = 4;
        const int MAX_FIREBALLS = 10;

        protected bool Reverse { get; set; } = false;
        
        /// <summary>
        /// Set to true for the initial position to be 50% rotated at the start.
        /// </summary>
        protected bool IsOffset { get; set; } = false;

        public FireBar(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Attack = 1;
            Health = 100;
            IsAffectedByGravity = false;
            Enabled = true;

            CollisionRectangle = Rectangle.Empty;

            var texture = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(texture, Helpers.GetTileRect(7, 20));

            for (int i = 0; i < MAX_FIREBALLS; i++)
            {
                var fireball = new Fireball(content, cellX, cellY, player, camera);
                fireball.WorldLocation = WorldLocation;
                fireball.Enabled = i < Length;
                Fireballs.Add(fireball);
            }

            // Temp
            this.DisplayComponent.TintColor = Color.Transparent;

            ExtraEnemiesToAddAfterConstructor.AddRange(Fireballs);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            Rotation += elapsed * speed;

            float offSetIncrement = 30;
            float offset = 30f;

            if (IsOffset)
            {
                offset *= -1;
                offSetIncrement *= -1;
            }

            foreach (var fireball in Fireballs)
            {
                if (fireball.Enabled)
                {
                    fireball.WorldLocation = Helpers.RotateAroundOrigin(this.WorldCenter + new Vector2(offset, 0), this.WorldCenter, Rotation * (Reverse ? -1 : 1));
                    offset += offSetIncrement;
                }
            }
        }

        public override void ConsumeProperties(Dictionary<string, string> properties)
        {
            base.ConsumeProperties(properties);
            if (properties.ContainsKey("Length"))
            {
                Length = int.Parse(properties["Length"]);
                for (int i = 0; i < MAX_FIREBALLS; i++)
                {
                    Fireballs[i].Enabled = i < Length;
                }

            }

            if (properties.ContainsKey("Reverse"))
            {
                Reverse = properties["Reverse"].IsTrue();
            }

            if (properties.ContainsKey("Offset"))
            {
                IsOffset = properties["Offset"].IsTrue();
            }
        }
    }
}