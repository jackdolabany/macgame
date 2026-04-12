using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Npcs
{
    public class AlienMom : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private Sock _alienMomSock;
        private BabyAlien _babyAlien;
        private bool _isInitialized = false;

        private bool _hasTalkedToAlienMom = false;
        private bool _babyIsDead = false;

        /// <summary>
        /// The AlienMom lost her BabyAlien and you need to find it and bring it to her for a special sock suprise.
        /// 
        /// For AlienMom to work place her on the map, also place the BabyAlien, and finally place a sock called
        /// "AlienMomSock".
        /// </summary>
        public AlienMom(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(10, 12), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);
            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 5);

        /// <summary>
        /// A wide rectangle 4 blocks on each side of AlienMom used to detect the baby nearby.
        /// </summary>
        private Rectangle BabyDetectionRectangle => new Rectangle(
            (int)(WorldLocation.X - CollisionRectangle.Width / 2 - 4 * TileMap.TileSize),
            (int)(WorldLocation.Y - TileMap.TileSize * 2),
            CollisionRectangle.Width + 8 * TileMap.TileSize,
            TileMap.TileSize * 3);

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock sock && sock.Name == "AlienMomSock")
                {
                    _alienMomSock = sock;
                    break;
                }
            }

            if (_alienMomSock == null)
            {
                throw new Exception("AlienMomSock not found in level items");
            }

            foreach (var npc in Game1.CurrentLevel.Npcs)
            {
                if (npc is BabyAlien baby)
                {
                    _babyAlien = baby;
                    break;
                }
            }

            if (_babyAlien == null)
            {
                throw new Exception("BabyAlien not found in level npcs");
            }

            if (_alienMomSock != null && !_alienMomSock.IsCollected)
            {
                _alienMomSock.Enabled = false;
            }

            // If baby was already saved, move him back home next to mom.
            if (_alienMomSock != null && _alienMomSock.IsCollected && _babyAlien != null)
            {
                _babyAlien.WorldLocation = WorldLocation;
            }
        }

        public override void InitiateConversation()
        {
            bool babyWasSaved = _alienMomSock != null && _alienMomSock.IsCollected;

            if (babyWasSaved)
            {
                ISay("Marv is safe and sound at home, thank you so much!");
            }
            else if (_babyAlien.IsPickedUp || BabyDetectionRectangle.Intersects(_babyAlien!.CollisionRectangle))
            {
                Action revealSock = () =>
                {
                    if (_alienMomSock != null && !_alienMomSock.Enabled)
                    {
                        _alienMomSock.FadeIn();
                    }
                };
                ConversationManager.AddMessage("Thank you so much for finding my little Marv, take this!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, revealSock);
            }
            else if (!_hasTalkedToAlienMom)
            {
                _hasTalkedToAlienMom = true;
                ISay("Please find my baby! He's lost outside on the moon somewhere");
                MacSays("Don't worry alien lady, you can count on me.");
            }
            else if (_babyIsDead)
            {
                ISay("Did you find him yet?");
                MacSays("He's a slippery bugger");
                ISay("what do you mean?");
                MacSays("uh, nothing. definitely didn't yeet him off of a bottomless cliff or anything");
                ISay("That's a relief!");
            }
            else
            {
                ISay("Did you find him yet?");
                MacSays("Still looking miss!");
            }
        }

        public void HandleBabyDeath()
        {
            _babyIsDead = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (Game1.DrawAllCollisionRects)
            {
                spriteBatch.Draw(Game1.TileTextures, BabyDetectionRectangle, Game1.WhiteSourceRect, Color.Cyan * 0.2f);
            }
        }

    }
}
