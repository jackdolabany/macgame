using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MacGame.DisplayComponents
{
    /// <summary>
    ///  A display component that is just a collection of display components.
    /// </summary>
    public class AggregateDisplay : DisplayComponent
    {
        public List<DisplayComponent> DisplayComponents;

        public AggregateDisplay(IEnumerable<DisplayComponent> components)
        {
            var myComponents = components.ToList();
            DisplayComponents = myComponents;
        }

        public override Color TintColor
        {
            get
            {
                return DisplayComponents[0].TintColor;
            }
            set
            {
                foreach (var dc in DisplayComponents)
                {
                    dc.TintColor = value;
                }
            }
        }

        public override Vector2 RotationAndDrawOrigin
        {
            get
            {
                return DisplayComponents[0].RotationAndDrawOrigin;
            }
            set
            {
                foreach (var dc in DisplayComponents)
                {
                    dc.RotationAndDrawOrigin = value;
                }
            }
        }

        public override float DrawDepth
        {
            get
            {
                return DisplayComponents[0].DrawDepth;
            }
            set
            {
                foreach (var dc in DisplayComponents)
                {
                    dc.DrawDepth = value;
                }
            }
        }

        public override float Scale
        {
            get
            {
                return DisplayComponents[0].Scale;
            }
            set
            {
                foreach (var dc in DisplayComponents)
                {
                    dc.Scale = value;
                }
            }
        }

        public override float Rotation
        {
            get
            {
                return DisplayComponents[0].Rotation;
            }
            set
            {
                foreach (var dc in DisplayComponents)
                {
                    dc.Rotation = value;
                }
            }
        }

        public override void Initialize()
        {
            // Do nothing
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            foreach (var dc in DisplayComponents)
            {
                dc.Update(gameTime, elapsed);
            }
            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {
            foreach (var dc in DisplayComponents)
            {
                dc.Draw(spriteBatch, position + Offset, flipped);
            }
        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            return DisplayComponents[0].GetWorldCenter(ref worldLocation);
        }
    }
}
