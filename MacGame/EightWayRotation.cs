using System;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public enum EightWayRotationDirection
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7
    }

    public struct EightWayRotation
    {

        public EightWayRotation(EightWayRotationDirection direction)
        {
            Direction = direction;
        }

        public EightWayRotationDirection Direction { get; set; }

        public Vector2 Vector2
        {
            get
            {
                Vector2 vector;
                switch (Direction)
                {
                    case EightWayRotationDirection.Right:
                        vector = new Vector2(1, 0);
                        break;
                    case EightWayRotationDirection.DownRight:
                        vector = new Vector2(1, 1);
                        break;
                    case EightWayRotationDirection.Down:
                        vector = new Vector2(0, 1);
                        break;
                    case EightWayRotationDirection.DownLeft:
                        vector = new Vector2(-1, 1);
                        break;
                    case EightWayRotationDirection.Left:
                        vector = new Vector2(-1, 0);
                        break;
                    case EightWayRotationDirection.UpLeft:
                        vector = new Vector2(-1, -1);
                        break;
                    case EightWayRotationDirection.Up:
                        vector = new Vector2(0, -1);
                        break;
                    case EightWayRotationDirection.UpRight:
                        vector = new Vector2(1, -1);
                        break;
                    default:
                        vector = Vector2.Zero;
                        break;
                }
                vector.Normalize();
                return vector;
            }
        }

        public void MoveClockwise()
        {
            Direction = (EightWayRotationDirection)(((int)Direction + 1) % 8);
        }

        public void MoveCounterClockwise()
        {
            Direction -= 1;
            if (Direction < 0)
            {
                Direction = EightWayRotationDirection.UpLeft;
            }
        }
    }
}