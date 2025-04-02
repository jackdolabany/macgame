using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    /// <summary>
    /// Sort of looks like the first boss from R-Type, but that's just a coincidence.
    /// </summary>
    public class OurTypeOfBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        /// Don't do anything until you've been seen.
        /// </summary>
        private bool _hasBeenSeen = false;

        float openAndCloseMouthTimer = 0;
        float openAndCloseMouthTimerGoal = 2.5f;

        private Player _player;

        private int MaxHealth = 50;

        private bool isInitialized = false;

        private OurTypeOfBossHead _head;
        private List<OurTypeOfBossTailPiece> _tailPieces = new List<OurTypeOfBossTailPiece>();

        public OurTypeOfBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 3), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            var closeMouth = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 3), 3, "closeMouth");
            closeMouth.LoopAnimation = false;
            closeMouth.FrameLength = 0.15f;
            closeMouth.Oscillate = true;
            animations.Add(closeMouth);

            var openMouth = (AnimationStrip)closeMouth.Clone();
            openMouth.Reverse = true;
            openMouth.Name = "openMouth";
            animations.Add(openMouth);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;

            IsAffectedByGravity = false;

            SetWorldLocationCollisionRectangle(8, 8);

            _head = new OurTypeOfBossHead(content, cellX, cellY, player, camera);
            _head.WorldLocation = this.WorldCenter + new Vector2(16, 40);
            Vector2 initialTailSpot = this.WorldLocation + new Vector2(-20, 0);
            for (int i = 0; i < 15; i++)
            {
                var tailPiece = new OurTypeOfBossTailPiece(content, cellX, cellY, player, camera);
                _tailPieces.Add(tailPiece);
                tailPiece.WorldLocation = initialTailSpot;
                initialTailSpot.X -= Game1.TileSize / 2;
            }
        }

        private void Initialize()
        {
            _head.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);

            for (int i = 0; i < _tailPieces.Count; i++)
            {
                _tailPieces[i].SetDrawDepth(this.DrawDepth + ((i + 1) * Game1.MIN_DRAW_INCREMENT));
            }

            isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (!_hasBeenSeen)
            {
                if (Game1.Camera.IsPointVisible(new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Center.Y)))
                {
                    _hasBeenSeen = true;
                }
                else
                {
                    return;
                }
            }

            openAndCloseMouthTimer += elapsed;
            if (openAndCloseMouthTimer > openAndCloseMouthTimerGoal)
            {
                openAndCloseMouthTimer = 0;
                animations.Play("openMouth").FollowedBy("closeMouth");
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Grok";

            // Move the tail pieces up and down in a sign wave
            for (int i = 0; i < _tailPieces.Count; i++)
            {
                var yPositionOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds + i) * 15;
                _tailPieces[i].WorldLocation = new Vector2(_tailPieces[i].WorldLocation.X, this.WorldLocation.Y + yPositionOffset);
            }


            // Update head and tail
            _head.Update(gameTime, elapsed);
            foreach (var tailPiece in _tailPieces)
            {
                tailPiece.Update(gameTime, elapsed);
            }

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw head and tail
            _head.Draw(spriteBatch);
            foreach (var tailPiece in _tailPieces)
            {
                tailPiece.Draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
    }
}