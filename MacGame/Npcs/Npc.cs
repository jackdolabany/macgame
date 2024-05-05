using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
  

    public abstract class Npc : GameObject
    {
        
        public Behavior? Behavior { get; set; }
        public abstract Rectangle ConversationSourceRectangle { get; }


        public Npc(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {

            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            IsAffectedByGravity = true;
            IsAbleToSurviveOutsideOfWorld = true;
        }

        public abstract void InitiateConversation();

        public void CheckPlayerInteractions(Player player)
        {
            // Handle the player going through a door.
            if (player.InteractButtonPressedThisFrame && this.CollisionRectangle.Intersects(player.CollisionRectangle))
            {
                InitiateConversation();
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Behavior != null)
            {
                Behavior.Update(this, gameTime, elapsed);
            }
            
            base.Update(gameTime, elapsed);
        }
    }
    
}
