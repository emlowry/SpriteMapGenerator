/******************************************************************************
 * File:               SheetCanvas_Geometry.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 19, 2014
 * Description:        SheetCanvas dimensions and collision detection.
 * Last Modified:      May 19, 2014
 * Last Modification:  Creation.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SpriteMapGenerator
{
    partial class SheetCanvas
    {
        // The size of the canvas can be retrieved by anyone but can only be set
        // within the class itself, when sprite layout changes.
        public Size Size
        {
            get { return new Size(Width, Height); }
            protected set
            {
                Width = (int)value.Width;
                Height = (int)value.Height;
            }
        }
        new public int Width
        {
            get { return (int)base.Width; }
            protected set
            {
                if (value != (int)base.Width)
                {
                    base.Width = value;
                    NotifyPropertyChanged("Size");
                    NotifyPropertyChanged("Width");
                    InvalidateVisual();
                }
            }
        }
        new public int Height
        {
            get { return (int)base.Height; }
            protected set
            {
                if (value != (int)base.Height)
                {
                    base.Height = value;
                    NotifyPropertyChanged("Size");
                    NotifyPropertyChanged("Height");
                    InvalidateVisual();
                }
            }
        }

        // Initial size, unlike regular size, can be set by others, but doing so
        // will only change the actual size of there's nothing currently on the
        // canvas
        int initialWidth, initialHeight;
        public int InitialWidth
        {
            get { return initialWidth; }
            set
            {
                if (value != initialWidth)
                {
                    initialWidth = value;
                    NotifyPropertyChanged("InitialSize");
                    NotifyPropertyChanged("InitialWidth");
                }
                if (0 == SpriteCount)
                {
                    Width = value;
                }
            }
        }
        public int InitialHeight
        {
            get { return initialHeight; }
            set
            {
                if (value != Height)
                {
                    initialHeight = value;
                    NotifyPropertyChanged("InitialSize");
                    NotifyPropertyChanged("InitialHeight");
                }
                if (0 == SpriteCount)
                {
                    Height = value;
                }
            }
        }
        public Size InitialSize
        {
            get { return new Size(InitialWidth, InitialHeight); }
            set
            {
                InitialWidth = (int)value.Width;
                InitialHeight = (int)value.Height;
            }
        }

        // Return the topmost sprite, if any, at the given location.
        public Sprite At(int x, int y) { return At(new Point(x, y)); }
        public Sprite At(Point location)
        {
            foreach (Sprite sprite in sprites.Reverse())
            {
                if (sprite.Boundary.Contains(location))
                {
                    return sprite;
                }
            }
            return null;
        }

        // Check all sprites for collisions with each other
        public void CheckCollisions()
        {
            // Empty the current collision list.
            int oldCount = colliding.Count;
            colliding.Clear();

            // If there is more than one sprite in the sheet, check them all.
            if (sprites.Count > 1)
            {
                // Make a list of sprites not yet checked for collisions and
                // loop through it.
                List<Sprite> toCheck = new List<Sprite>(sprites);
                for (int i = 0; i < toCheck.Count; ++i)
                {
                    for (int j = i + 1; j < toCheck.Count; ++j)
                    {
                        if (Colliding(toCheck[i], toCheck[j]))
                        {
                            colliding.Add(toCheck[i]);
                            colliding.Add(toCheck[j]);
                        }
                    }
                }
            }

            // Notify if count has changed
            if (oldCount != colliding.Count)
            {
                NotifyPropertyChanged("CollisionCount");
            }
            InvalidateVisual();
        }

        // Do the given pair of sprites overlap?
        protected bool Colliding(Sprite sprite1, Sprite sprite2)
        {
            if (sprite1 == sprite2 || null == sprite1 || null == sprite2)
            {
                return false;
            }
            Rect intersection = Rect.Intersect(RenderedBoundary(sprite1),
                                               RenderedBoundary(sprite2));
            return (null != intersection && !intersection.IsEmpty &&
                    intersection.Width * intersection.Height > 0);
        }

    }
}