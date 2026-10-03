using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A Venus fly trap that sits in place and chomps. Mac can't jump on it, but weapons will kill it.
    /// </summary>
    public class FlyTrap : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public FlyTrap(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var chomp = new AnimationStrip(textures, Helpers.GetTileRect(2, 7), 2, "chomp");
            chomp.LoopAnimation = true;
            chomp.FrameLength = 0.2f;
            animations.Add(chomp);

            animations.Play("chomp");

            isEnemyTileColliding = false;
            IsAffectedByGravity = false;

            Attack = 1;
            Health = 1;
            CanBeJumpedOn = false;
            CanBeHitWithWeapons = true;

            SetWorldLocationCollisionRectangle(6, 7);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }
    }
}
