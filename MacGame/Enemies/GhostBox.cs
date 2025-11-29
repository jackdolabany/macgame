using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using static MacGame.Enemies.CanadaGooseBoss;
using static MacGame.Enemies.GhostBoxGhost;

namespace MacGame.Enemies
{
    /// <summary>
    /// A box that contains a ghost. When triggered by a button action, the ghost is released.
    /// The ghost flies right, bounces off walls, slowly moves down, and can trigger buttons/switches.
    /// </summary>
    public class GhostBox : Enemy
    {
        GhostBoxGhost _ghost;

        public GhostBox(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            // Box state display
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(6, 20));

            isEnemyTileColliding = false;
            Attack = 0;
            Health = 1000;
            IsAffectedByGravity = false;
            CanBeJumpedOn = false; // Can't jump on this enemy
            CanBeHitWithWeapons = false; // Can't be hit with weapons, only DestroyPickupObjectField

            SetWorldLocationCollisionRectangle(8, 8);

            // Start inactive (as a box) - but still enabled so it's in the enemies list
            Enabled = true;

            _ghost = new GhostBoxGhost(content, cellX, cellY, player, camera);
            this.AddEnemyInConstructor(_ghost);
        }

        /// <summary>
        /// Releases the ghost from the box. Can be called multiple times to re-release after death.
        /// </summary>
        public void Release()
        {
            // The ghost (this enemy) draws behind the box
            _ghost.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);
            _ghost.Release(this.WorldLocation);
        }

    }
}

