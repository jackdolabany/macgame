using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame
{
    public class Button : GameObject
    {

        public AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        ///  Attach a script here to call a function when the button is pressed. This will be set in the map editor.
        /// </summary>
        public string DownAction = "";

        public string UpAction = "";

        /// <summary>
        /// Set from the map editor.
        /// </summary>
        public string Name = "";

        private Player _player;

        float cooldownTimer = 0f;

        public Button(ContentManager content, int cellX, int cellY, Player player, bool isUp)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);

            _player = player;

            Enabled = true;

            // This is a button. It doesn't do anything.
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var up = new AnimationStrip(textures, Helpers.GetTileRect(13, 2), 1, "up");
            up.LoopAnimation = false;
            up.FrameLength = 0.14f;
            animations.Add(up);

            var down = new AnimationStrip(textures, Helpers.GetTileRect(14, 2), 1, "down");
            down.LoopAnimation = false;
            down.FrameLength = 0.14f;
            animations.Add(down);

            this.SetCenteredCollisionRectangle(8, 4);

            if (isUp)
            {
                animations.Play("up");
            }
            else
            {
                animations.Play("down");
            }

        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (cooldownTimer > 0)
            {
                cooldownTimer -= elapsed;
            }

            if (cooldownTimer <= 0
                && _player.CollisionRectangle.Intersects(this.CollisionRectangle)
                && _player.Velocity.Y > 0
                && !_player.IsInWater
                && animations.CurrentAnimationName == "up")
            {
                // TODO: Play sound

                animations.Play("down");
                if (!string.IsNullOrEmpty(DownAction))
                {
                    // TODO: PlaySound
                    var type = Game1.CurrentLevel.GetType();
                    MethodInfo methodInfo = type.GetMethod(DownAction);
                    methodInfo.Invoke(Game1.CurrentLevel, null);
                }

                cooldownTimer = 0.5f;

                // Pop the player up a bit.
                _player.Velocity = new Vector2(_player.Velocity.X, -300);

                // Water buttons should put the other buttons up or down.
                if (this.DownAction.EndsWith("Water"))
                {
                    var buttons = Game1.CurrentLevel.GameObjects.OfType<Button>();

                    foreach (var button in buttons)
                    {
                        if (button != this)
                        {
                            if (button.DownAction == this.DownAction)
                            {
                                button.MoveDownNoAction();
                            }
                            else if (button.DownAction.EndsWith("Water"))
                            {
                                button.MoveUpNoAction();
                            }
                        }
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public void MoveUpNoAction()
        {
            animations.Play("up");
        }

        public void MoveDownNoAction()
        {
            animations.Play("down");
        }
    }
}
