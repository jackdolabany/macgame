using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MacGame.DisappearBlocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.DisappearBlocks
{
    public class DisappearBlockManager
    {
        private List<DisappearBlock> RawBlocks;
        private Dictionary<string, DisappearBlockGroup> GroupNamesToGroups;

        public DisappearBlockManager()
        {
            RawBlocks = new List<DisappearBlock>();
            GroupNamesToGroups = new Dictionary<string, DisappearBlockGroup>();
        }

        public void Reset()
        {
            RawBlocks.Clear();
            GroupNamesToGroups.Clear();
        }

        public void AddRawBlock(DisappearBlock block)
        {
            RawBlocks.Add(block);
        }

        public void OrganizeRawBlocksIntoGroups()
        {
            // Organize into groups
            foreach (var block in RawBlocks)
            {
                if (!GroupNamesToGroups.ContainsKey(block.GroupName))
                {
                    var group = new DisappearBlockGroup(block.GroupName);
                    GroupNamesToGroups[group.GroupName] = group;
                    group.DisappearBlocks.Add(block);
                }
                else
                {
                    GroupNamesToGroups[block.GroupName].DisappearBlocks.Add(block);
                }
            }
        
            // Groups compute stats once when complete.
            foreach (var group in GroupNamesToGroups.Values)
            {
                group.BuildStats();
            }
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            foreach (var group in GroupNamesToGroups.Values)
            {
                group.Update(gameTime, elapsed);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var group in GroupNamesToGroups.Values)
            {
                group.Draw(spriteBatch);
            }
        }
    }
}
