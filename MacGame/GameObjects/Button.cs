using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Decorate the buttons with DownAction, UpAction, and Args.
    /// 
    /// Actions can be
    ///   HighWater/MedWater/LowWater - no args, move water high/med/low. This only applies on World 2 where water can go up and down.
    ///   CloseBlockingPiston - Args are the name of the Door. Close a blocking piston door.
    ///   OpenBlockingPiston - Args are the name of the Door. Open a blocking piston door.
    ///   ShootCannon - Shoots a cannon, which might have a cannon ball. Args are the name of the cannon.
    ///   BreakBricks - Break a series of BreakBricks. The args are the GroupName property of the BreakBricks.
    ///   SolidifyGhostBlock - Makes an open Ghost Block become solid. The args are the Name of the GhostBlock.
    ///   ResetGhostPlatform - Resets a ghost platform to its original position. The args are the Name of the GhostPlatform.
    /// </summary>
    public class Button : GameObject
    {

        public AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        ///  Attach a script here to call a function when the button is pressed. This will be set in the map editor.
        /// </summary>
        public string DownAction = "";

        public string UpAction = "";

        /// <summary>
        /// Put whatever you want here and the custom function up there will be able to read it.
        /// Set this in the object that wraps this in the map editor.
        /// </summary>
        public string Args = "";

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

            this.SetWorldLocationCollisionRectangle(8, 4);

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
                SoundManager.PlaySound("Click");
                animations.Play("down");
                if (!string.IsNullOrEmpty(DownAction))
                {
                    Game1.CurrentLevel.ButtonAction(this, DownAction, Args);
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
                SoundManager.PlaySound("Click");
                if (!string.IsNullOrEmpty(UpAction))
                {
                    Game1.CurrentLevel.ButtonAction(this, UpAction, Args);
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
