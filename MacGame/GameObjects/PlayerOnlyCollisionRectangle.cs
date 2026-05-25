using MacGame.DisplayComponents;

namespace MacGame
{
    /// <summary>
    /// An invisible, non-drawing collision wall that only blocks the player.
    /// </summary>
    public class PlayerOnlyCollisionRectangle : GameObject, ICustomCollisionObject
    {

        public PlayerOnlyCollisionRectangle()
        {
            DisplayComponent = new NoDisplay();
            IsAffectedByGravity = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            Enabled = true;
        }

        public bool DoesCollideWithObject(GameObject obj)
        {
            if (!Enabled) return false;
            return obj is Player;
        }
    }
}
