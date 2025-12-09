using MacGame.DisplayComponents;
using MacGame.GameObjects;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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
    ///   RaiseButton - Raises another button. The args are the Name of the Button.
    ///   LowerButton - What do you think?
    ///   ReleaseGhostBox - Releases a ghost from a ghost box. Use args with the name of the ghost box.
    ///   ResetPickUpObject - Args is the name of the pick up object to reset.
    ///   
    ///  Add multiple actions like this
    ///  DownAction: OpenBlockingPiston:Door1;CloseBlockingPiston:Door2;RaiseButton:Button1
    ///  
    /// Actions are implemented in Level.cs, Level.ExecuteButtonAction.
    /// 
    /// </summary>
    public class Button : GameObject
    {

        public AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        ///  Attach a script here to call a function when the button is pressed. This will be set in the map editor.
        /// </summary>
        public List<ButtonAction> DownActions;

        public List<ButtonAction> UpActions;

        /// <summary>
        /// Put whatever you want here and the custom function up there will be able to read it.
        /// Set this in the object that wraps this in the map editor.
        /// </summary>
        public string Args = "";

        private Player _player;

        float cooldownTimer = 0f;

        public enum ButtonType
        {
            /// <summary>
            /// A regular button, you jump on it and it goes down.
            /// </summary>
            Regular,
            /// <summary>
            /// A spring button springs up if you don't stand on it or have an object on it.
            /// </summary>
            Spring,
            /// <summary>
            /// A red light that when you touch it turns green.
            /// </summary>
            Light
        }

        private ButtonType _buttonType = ButtonType.Regular;

        public ButtonType GetButtonType => _buttonType;

        /// <summary>
        /// A button that Mac can compress.
        /// </summary>
        /// <param name="isUp">If true the button is initially up. Else, down.</param>
        /// <param name="isSpringButton">If true the button is a spring button. It springs back up if Mac (or something else) isn't on it.</param>
        public Button(ContentManager content, int cellX, int cellY, Player player, bool isUp, ButtonType type)
        {

            UpActions = new List<ButtonAction>();
            DownActions = new List<ButtonAction>();

            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);

            _player = player;

            Enabled = true;

            _buttonType = type;

            // This is a button. It doesn't do anything.
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            DisplayComponent = new AnimationDisplay();

            
            Rectangle upSource;
            Rectangle downSource;

            this.SetWorldLocationCollisionRectangle(8, 4);

            Texture2D textures;

            if (_buttonType == ButtonType.Regular)
            {
                textures = content.Load<Texture2D>(@"Textures\Textures");
                upSource = Helpers.GetTileRect(13, 2);
                downSource = Helpers.GetTileRect(14, 2);
            }
            else if (_buttonType == ButtonType.Spring)
            {
                // Spring button uses slightly different graphics
                textures = content.Load<Texture2D>(@"Textures\Textures");
                upSource = Helpers.GetTileRect(9, 1);
                downSource = Helpers.GetTileRect(10, 1);
            }
            else if (_buttonType == ButtonType.Light)
            {
                textures = content.Load<Texture2D>(@"Textures\BigTextures");
                upSource = Helpers.GetBigTileRect(12, 6);
                downSource = Helpers.GetBigTileRect(13, 6);
                this.SetCenteredCollisionRectangle(16, 16, 12, 12);
            }
            else
            {
                throw new System.Exception("Unknown button type");
            }
            
            var up = new AnimationStrip(textures, upSource, 1, "up");
            up.LoopAnimation = false;
            up.FrameLength = 0.14f;
            animations.Add(up);

            var down = new AnimationStrip(textures, downSource, 1, "down");
            down.LoopAnimation = false;
            down.FrameLength = 0.14f;
            animations.Add(down);

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
                && (_buttonType != ButtonType.Regular || _player.Velocity.Y > 0) // must jump down onto regular buttons.
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
                    var go = (PickupObject)puo;
                    if (!go.IsPickedUp && go.Enabled && go.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        isColliding = true;
                    }
                }

                // Check ghost platforms
                foreach (var platform in Game1.CurrentLevel.Platforms)
                {
                    var gp = platform as GhostPlatformBase;
                    if (gp != null && gp.Enabled && gp.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        isColliding = true;
                    }
                }

                // Check enemies
                if (!isColliding)
                {
                    foreach (var enemy in Game1.CurrentLevel.Enemies)
                    {
                        if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            isColliding = true;
                            break;
                        }
                    }
                }
            }

           if (isColliding)
           {
               Trigger(isPlayerColliding);
           }

            if (!isColliding
                && _buttonType == ButtonType.Spring
                && animations.CurrentAnimationName == "down"
                && cooldownTimer <= 0)
            {
                animations.Play("up");
                SoundManager.PlaySound("Click");

                foreach (var action in UpActions)
                {
                    Game1.CurrentLevel.ExecuteButtonAction(this, action.ActionName, action.Args);
                }
            }

            base.Update(gameTime, elapsed);
        }

        public void Trigger(bool isPlayerColliding = false)
        {
            if (cooldownTimer > 0) return;

            // Should we allow enemies too?
            if (animations.CurrentAnimationName == "up")
            {
                SoundManager.PlaySound("Click");
                animations.Play("down");

                foreach (var action in DownActions)
                {
                    Game1.CurrentLevel.ExecuteButtonAction(this, action.ActionName, action.Args);
                }

                cooldownTimer = 0.4f;

                // Pop the player up a bit.
                if (_buttonType == ButtonType.Regular && isPlayerColliding)
                {
                    _player.Velocity = new Vector2(_player.Velocity.X, -300);
                }

                // Water buttons should put the other buttons up or down.
                if (this.DownActions.Any(da => da.ActionName.EndsWith("Water")))
                {
                    var actionName = this.DownActions.First(da => da.ActionName.EndsWith("Water")).ActionName;
                    var buttons = Game1.CurrentLevel.GameObjects.OfType<Button>();

                    foreach (var button in buttons)
                    {
                        if (button != this)
                        {
                            if (button.DownActions.Any(da => da.ActionName == actionName))
                            {
                                // Move other similar water buttons down.
                                button.MoveDownNoAction();
                            }
                            else if (button.DownActions.Any(da => da.ActionName.EndsWith("Water")))
                            {
                                // Other water buttons move up.
                                button.MoveUpNoAction();
                            }
                        }
                    }
                }
            }
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
