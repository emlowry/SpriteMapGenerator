/******************************************************************************
 * File:               SheetCanvas_Arrange.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 19, 2014
 * Description:        SheetCanvas functionality for arranging sprites.
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
        // The general shape to aim for when packing sprites into a compact sheet
        protected SpriteBin.BinShape layout = SpriteBin.BinShape.Square;
        public SpriteBin.BinShape SpriteLayout
        {
            get { return layout; }
            set
            {
                if (value != layout)
                {
                    layout = value;
                    NotifyPropertyChanged("SpriteLayout");
                    NotifyPropertyChanged("SquareLayout");
                    NotifyPropertyChanged("TallLayout");
                    NotifyPropertyChanged("WideLayout");
                    if (AutoArrange)
                    {
                        Rearrange();
                    }
                }
            }
        }

        // These boolean properties can't be set to false directly - they only
        // change if one of the others is set to true.
        public bool SquareLayout
        {
            get { return (SpriteBin.BinShape.Square == SpriteLayout); }
            set
            { if (value) SpriteLayout = SpriteBin.BinShape.Square; }
        }
        public bool TallLayout
        {
            get { return (SpriteBin.BinShape.Tall == SpriteLayout); }
            set { if (value) SpriteLayout = SpriteBin.BinShape.Tall; }
        }
        public bool WideLayout
        {
            get { return (SpriteBin.BinShape.Wide == SpriteLayout); }
            set { if (value) SpriteLayout = SpriteBin.BinShape.Wide; }
        }

        // Should sprites added to the canvas be automatically arranged in a
        // collision-free and relatively compact layout when added?
        protected bool autoArrange = false;
        protected SpriteBin spriteBin = null;   // Tracks available free space
        public bool AutoArrange
        {
            get { return autoArrange; }
            set
            {
                if (value != autoArrange)
                {
                    autoArrange = value;
                    NotifyPropertyChanged("AutoArrange");

                    // If the canvas wasn't in auto-arrange mode before but is
                    // now, rearrange into an automatic layout.
                    if (value)
                    {
                        Rearrange();
                    }
                    // If the canvas *was* in auto-arrange mode before but
                    // *isn't* now, get rid of the structure tracking available
                    // freespace.
                    else
                    {
                        spriteBin = null;
                    }
                }
            }
        }

        // Adjust canvas size to fit all sprites
        public void FitToSprites()
        {
            // Don't bother if there are no sprites.
            if (sprites.Count == 0)
            {
                return;
            }

            // Calculate a boundary containing all sprites.
            Rect boundary = Sprite.UnionBoundary(sprites);

            // Adjust sprite positions so that there is at least one sprite
            // touching each edge of the canvas.
            foreach (Sprite sprite in sprites)
            {
                sprite.X -= (int)boundary.X;
                sprite.Y -= (int)boundary.Y;
            }

            // Adjust canvas dimensions
            this.Size = boundary.Size;
            InvalidateVisual();
        }

        // Arrange sprites into a collision-free, relatively compact layout
        public void Rearrange()
        {
            // Clear current collision list
            if (CollisionCount > 0)
            {
                NotifyPropertyChanged("CollisionCount");
            }
            colliding.Clear();

            if (0 == SelectionCount)
            {
                // If there are no sprites selected, arrange all of them at once.
                spriteBin = SpriteBin.Pack(sprites, SpriteLayout);
                if (null != spriteBin)
                {
                    this.Size = spriteBin.Size;
                }

                // Don't keep the map of free space while not in auto-arrange
                // mode, since it'll soon become obsolete.
                if (!AutoArrange)
                {
                    spriteBin = null;
                }
            }
            else
            {
                // If in auto-arrange mode, arrange the selected sprites into
                // a compact layout, then arrange the rest of the sprites around
                // that layout.  This can be useful when the automatic layout
                // algorithm doesn't provide the best results for sorting all
                // the sprites in one go.
                if (AutoArrange)
                {
                    // Don't arrange sprites that are in the process of being moved
                    if (DragInProgress)
                    {
                        spriteBin = SpriteBin.Pack(sprites.Except(selected), SpriteLayout);
                    }
                    else
                    {
                        spriteBin = SpriteBin.Pack(SpriteBin.Pack(selected, SpriteLayout),
                                                   sprites.Except(selected), SpriteLayout);
                    }
                    if (null != spriteBin)
                    {
                        this.Size = spriteBin.Size;
                    }
                }
                // If not in auto-arrange mode, only arrange the selected sprites
                else
                {
                    Rect boundary = Sprite.UnionBoundary(selected);
                    SpriteBin.Pack(selected, boundary.Location, SpriteLayout);
                    CheckCollisions();
                    FitToSprites();
                }
            }
            InvalidateVisual();
        }

    }
}