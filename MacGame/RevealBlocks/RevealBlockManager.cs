using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MacGame;
using Microsoft.Xna.Framework;

namespace MacGame.RevealBlocks
{
    public class RevealBlockManager
    {
        private List<RevealBlock> RawBlocks;
        private List<RevealBlockGroup> Groups;
        private bool wasGroupCollision = false;
        private float timeToFullyTransparent = 0.25f;

        public RevealBlockManager()
        {
            RawBlocks = new List<RevealBlock>();
            Groups = new List<RevealBlockGroup>();
        }

        public void Reset()
        {
            RawBlocks.Clear();
            Groups.Clear();
        }

        public void AddRawBlock(RevealBlock block)
        {
            RawBlocks.Add(block);
        }

        public void OrganizeRawBlocksIntoGroups()
        {
            while (RawBlocks.Any())
            {
                var block = RawBlocks.First();
                RawBlocks.RemoveAt(0);

                var group = new RevealBlockGroup();
                this.Groups.Add(group);
                group.RevealBlocks.Add(block);

                PutMatchingBlocksInGroup(block, group);
                group.BuildCollisionRectangle();
            }

        }

        private void PutMatchingBlocksInGroup(RevealBlock block, RevealBlockGroup group)
        {
            var matches = new List<RevealBlock>();
            for (int i = RawBlocks.Count - 1; i >= 0; i--)
            {
                if ((RawBlocks[i].CellX == block.CellX && RawBlocks[i].CellY == block.CellY - 1) //on top
                    || (RawBlocks[i].CellX == block.CellX && RawBlocks[i].CellY == block.CellY + 1) // below
                    || (RawBlocks[i].CellX == block.CellX - 1 && RawBlocks[i].CellY == block.CellY) // to the left
                    || (RawBlocks[i].CellX == block.CellX + 1 && RawBlocks[i].CellY == block.CellY)) // to the right
                {
                    matches.Add(RawBlocks[i]);
                    RawBlocks.RemoveAt(i);
                    if (matches.Count == 4) break;
                }
            }
            group.RevealBlocks.AddRange(matches);
            foreach (var match in matches)
            {
                PutMatchingBlocksInGroup(match, group);
            }
        }

        public void Update(float elapsed)
        {
            bool someGroupHadACollision = false;
            foreach (var group in Groups)
            {
                bool isGroupCollision = false;
                if (Game1.Player.Enabled)
                {
                    if (group.IsColliding(Game1.Player.CollisionRectangle))
                    {
                        isGroupCollision = true;
                        someGroupHadACollision = true;
                    }
                }

                // Add or subtract time from the group based on if the player is colliding.
                if (isGroupCollision)
                {
                    group.CollisionTime += elapsed;
                    if (group.CollisionTime > timeToFullyTransparent)
                    {
                        group.CollisionTime = timeToFullyTransparent;
                    }
                }
                else
                {
                    group.CollisionTime -= elapsed;
                    if (group.CollisionTime < 0)
                    {
                        group.CollisionTime = 0;
                    }
                }

                var revealPercentage = group.CollisionTime / timeToFullyTransparent;

                // Make each block in the group more or less transparent to "Reveal" hidden areas
                foreach (var block in group.RevealBlocks)
                {
                    var layers = Game1.CurrentMap.MapCells[block.CellX][block.CellY].LayerTiles;

                    for (int i = block.CellZ; i < Game1.CurrentMap.MapDepth; i++)
                    {
                        layers[i].Color = Color.White * (1 - revealPercentage);
                    }
                }

            }

            if (someGroupHadACollision && !wasGroupCollision)
            {
                SoundManager.PlaySound("Reveal", 0.5f);
            }
            wasGroupCollision = someGroupHadACollision;

        }

    }
}
