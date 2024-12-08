﻿using MacGame.DisplayComponents;
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

        private bool _isSpringButton;

        /// <summary>
        /// A button that Mac can compress.
        /// </summary>
        /// <param name="isUp">If true the button is initially up. Else, down.</param>
        /// <param name="isSpringButton">If true the button is a spring button. It springs back up if Mac (or something else) isn't on it.</param>
        public Button(ContentManager content, int cellX, int cellY, Player player, bool isUp, bool isSpringButton)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);

            _player = player;

            Enabled = true;

            _isSpringButton = isSpringButton;

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
            Rectangle upSource;
            Rectangle downSource;
            if (isSpringButton)
            {
                // Spring button uses slightly different graphics
                upSource = Helpers.GetTileRect(9, 1);
                downSource = Helpers.GetTileRect(10, 1);
            }
            else
            {
                upSource = Helpers.GetTileRect(13, 2);
                downSource = Helpers.GetTileRect(14, 2);
            }
            var up = new AnimationStrip(textures, upSource, 1, "up");
            up.LoopAnimation = false;
            up.FrameLength = 0.14f;
            animations.Add(up);

            var down = new AnimationStrip(textures, downSource, 1, "down");
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

            bool isColliding = false;
            bool isPlayerColliding = false;
            // Check if the player or an object is holding it down. 
            if (_player.CollisionRectangle.Intersects(this.CollisionRectangle)
                && (_isSpringButton || _player.Velocity.Y > 0) // must jump down onto regular buttons.
                && !_player.IsInWater)
            {
                isColliding = true;
                isPlayerColliding = true;
            }

            if (!isColliding)
            {
                // Check pick up objects
                foreach (var puo in Game1.CurrentLevel.PickupObjects)
                {
                    var go = puo as PickupObject;
                    if (!go.IsPickedUp && go.Enabled && go.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        isColliding = true;
                    }
                }
            }

            // Should we allow enemies too?

            if (cooldownTimer <= 0
                && isColliding
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
                if (!_isSpringButton && isPlayerColliding)
                {
                    _player.Velocity = new Vector2(_player.Velocity.X, -300);
                }

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

            if (!isColliding
                && _isSpringButton
                && animations.CurrentAnimationName == "down")
            {
                animations.Play("up");
                if (!string.IsNullOrEmpty(UpAction))
                {
                    // TODO: PlaySound
                    var type = Game1.CurrentLevel.GetType();
                    MethodInfo methodInfo = type.GetMethod(UpAction);
                    methodInfo.Invoke(Game1.CurrentLevel, null);
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
