/******************************************************************************
 * File:               SheetCanvas_Drag.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 19, 2014
 * Description:        SheetCanvas functionality for dragging sprites.
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
        // Variables for keeping track of click/drag events
        protected Point mouseStartPosition, mouseEndPosition;
        protected bool movingSelected = false;
        protected bool mouseDown = false;
        protected bool mouseStartedOnSelection = false;
        protected Sprite mouseStartedOn = null;

        // Reduce typing elsewhere
        public int DragX
        {
            get
            {
                if (!movingSelected)
                {
                    return 0;
                }
                return (int)(mouseEndPosition.X - mouseStartPosition.X);
            }
        }
        public int DragY
        {
            get
            {
                if (!movingSelected)
                {
                    return 0;
                }
                return (int)(mouseEndPosition.Y - mouseStartPosition.Y);
            }
        }
        public bool DragInProgress { get { return movingSelected; } }

        // Call this when a click/drag starts and while it continues.
        public void Drag(Point position)
        {
            // If the click/drag is just starting,
            if (!mouseDown)
            {
                // record that it started and where,
                mouseDown = true;
                mouseStartPosition = position;
                mouseStartedOn = At(mouseStartPosition);

                // and if starting on a sprite that isn't selected, try selecting it.
                if (null != mouseStartedOn && !selected.Contains(mouseStartedOn))
                {
                    Select(mouseStartedOn);
                }

                // After that check, is the sprite you started on selected?
                // If so, any movement will change this from a click to a drag.
                mouseStartedOnSelection = selected.Contains(mouseStartedOn);
            }

            // Use the current position as the end position.
            mouseEndPosition = position;

            // If this is still a click instead of a drag, but the click started
            // on a sprite that is now selected and the mouse has moved from its
            // start position, turn this into a drag.
            if (!movingSelected && mouseStartedOnSelection &&
                mouseStartPosition != mouseEndPosition)
            {
                movingSelected = true;
                if (AutoArrange)
                {
                    Rearrange();
                }
            }

            // If a drag is in process, update collision list and canvas size.
            if (movingSelected)
            {
                CheckCollisions();
                Rect unselectedBoundary = Sprite.UnionBoundary(sprites.Except(selected));
                Rect selectedBoundary = Sprite.UnionBoundary(selected);
                selectedBoundary.X += DragX;
                selectedBoundary.Y += DragY;
                Width = (int)Math.Max(selectedBoundary.Right, unselectedBoundary.Right);
                Height = (int)Math.Max(selectedBoundary.Bottom, unselectedBoundary.Bottom);
                InvalidateVisual();
            }
        }

        // Call this when a click or drag finishes.
        public void EndDrag(Point position)
        {
            // Note that the click has ended and where.
            mouseDown = false;
            mouseEndPosition = position;
            Sprite endedOn = At(mouseEndPosition);

            // If this was a drag and not just a click,
            if (movingSelected)
            {
                // If in auto-arrange mode, rearrange sprites.
                if (AutoArrange)
                {
                    spriteBin = SpriteBin.Pack(spriteBin, selected, SpriteLayout);
                    Size = spriteBin.Size;
                }

                // If not in auto-arrange mode, add displacement to the positions
                // of all selected sprites.
                else
                {
                    foreach (Sprite sprite in selected)
                    {
                        sprite.Location = RenderedBoundary(sprite).Location;
                    }
                    FitToSprites();
                }

                // note that the drag has ended.
                movingSelected = false;

                // Check for collisions
                CheckCollisions();
                InvalidateVisual();
            }

            // If this was merely a click, and it didn't start on a sprite, try
            // to select whatever the click ended on.
            else if (null == mouseStartedOn)
            {
                Select(At(mouseEndPosition));
            }

            // Clean up remaining variables.
            mouseStartedOn = null;
            mouseStartedOnSelection = false;
        }

        // Gets the boundary of the sprite as it should appear on screen instead
        // of its actual boundary (the two differ when a drag is in progress)
        protected Rect RenderedBoundary(Sprite sprite)
        {
            Rect boundary = sprite.Boundary;
            if (DragInProgress && selected.Contains(sprite))
            {
                boundary.X += DragX;
                boundary.Y += DragY;
            }
            return boundary;
        }

    }
}
