using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MacGame
{
    public class InputManager
    {
        public InputAction CurrentAction = new InputAction();
        public InputAction PreviousAction = new InputAction();
        public const float JOYSTICK_GIVE = 0.3f;

        public bool Enabled = true;

        /// <summary>
        /// You can check this to see if they executed combos of moves
        /// </summary>
        public Queue<InputAction> PreviousUniqueActions = new Queue<InputAction>();

        public virtual void ReadInputs()
        {
            if (!Enabled) return;

            // Enqueue the current action just before we mark it as the previous action.
            if (CurrentAction.HasAction && !CurrentAction.Equals(PreviousAction))
            {
                PreviousUniqueActions.Enqueue(CurrentAction);

                while (PreviousUniqueActions.Count > 20)
                {
                    PreviousUniqueActions.Dequeue();
                }
            }

            PreviousAction = CurrentAction;
            CurrentAction = new InputAction();

            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Left)
                || gamePad.ThumbSticks.Left.X < -JOYSTICK_GIVE
                || gamePad.DPad.Left == ButtonState.Pressed)
            {
                CurrentAction.left = true;
            }
            else if (keyState.IsKeyDown(Keys.Right)
                || gamePad.ThumbSticks.Left.X > JOYSTICK_GIVE
                || gamePad.DPad.Right == ButtonState.Pressed)
            {
                CurrentAction.right = true;
            }

            if (keyState.IsKeyDown(Keys.Up)
                || gamePad.ThumbSticks.Left.Y > JOYSTICK_GIVE
                || gamePad.DPad.Up == ButtonState.Pressed)
            {
                CurrentAction.up = true;
            }
            else if (keyState.IsKeyDown(Keys.Down)
                || gamePad.ThumbSticks.Left.Y < -JOYSTICK_GIVE
                || gamePad.DPad.Down == ButtonState.Pressed)
            {
                CurrentAction.down = true;
            }

            if (keyState.IsKeyDown(Keys.Space)
                || keyState.IsKeyDown(Keys.X)
                || gamePad.IsButtonDown(Buttons.A))
            {
                CurrentAction.jump = true;
            }

            if (keyState.IsKeyDown(Keys.Z)
                || keyState.IsKeyDown(Keys.LeftShift)
                || keyState.IsKeyDown(Keys.A)
                || gamePad.IsButtonDown(Buttons.X))
            {
                CurrentAction.action = true;
            }

            if (keyState.IsKeyDown(Keys.Escape)
                || gamePad.IsButtonDown(Buttons.Start))
            {
                CurrentAction.pause = true;
            }

            if (keyState.IsKeyDown(Keys.A)
                || keyState.IsKeyDown(Keys.Z)
                || (keyState.IsKeyDown(Keys.Enter) && keyState.IsKeyUp(Keys.LeftAlt) && keyState.IsKeyUp(Keys.RightAlt)) // enter, but not alt-enter which toggles full screen
                || gamePad.IsButtonDown(Buttons.A)
                || gamePad.IsButtonDown(Buttons.Start))
            {
                CurrentAction.acceptMenu = true;
            }
            else if (keyState.IsKeyDown(Keys.Escape)
                || keyState.IsKeyDown(Keys.S)
                || keyState.IsKeyDown(Keys.X)
                || gamePad.IsButtonDown(Buttons.B))
            {
                CurrentAction.declineMenu = true;
            }

        }

    }

    public struct InputAction
    {
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public bool jump;
        public bool action;
        public bool pause;
        public bool acceptMenu;
        public bool declineMenu;

        public bool HasAction
        {
            get
            {
                return up || down || left || right || jump || action || pause || acceptMenu || declineMenu;
            }
        }

        public bool Equals(InputAction inputAction)
        {
            return up == inputAction.up &&
                down == inputAction.down &&
                left == inputAction.left &&
                right == inputAction.right &&
                jump == inputAction.jump &&
                action == inputAction.action &&
                pause == inputAction.pause &&
                acceptMenu == inputAction.acceptMenu &&
                declineMenu == inputAction.declineMenu;
        }
    }
}
