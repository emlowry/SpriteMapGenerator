/******************************************************************************
 * File:               SheetCanvas_AddClearSelect.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 19, 2014
 * Description:        SheetCanvas functionality for adding, removing, or
 *                      selecting sprites.
 * Last Modified:      May 19, 2014
 * Last Modification:  Creation.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SpriteMapGenerator
{
    partial class SheetCanvas
    {
        // Add a new sprite to the sheet, selecting it if indicated.
        public void AddSprite(Sprite sprite, bool select = false,
                              ModifierKeys? modifiers = null)
        {
            // If the sprite should be selected,
            if (select)
            {
                // look up modifier keys if not already provided,
                if (null == modifiers)
                {
                    modifiers = Keyboard.Modifiers;
                }

                // and add to selection list.
                Select(sprite, (ModifierKeys)modifiers);
            }

            // If not actually selecting a sprite, there's no need to do
            // anything more.
            if (null == sprite)
            {
                return;
            }

            // If there's a sprite in the sheet with the same name as this one,
            // adjust the name until it is unique.
            while (sprites.Count(otherSprite => (otherSprite.Name == sprite.Name)) > 0)
            {
                sprite.AdjustName();
            }

            // Add the sprite to the sprite list.
            sprites.Add(sprite);
            NotifyPropertyChanged("SpriteCount");

            // If the canvas is in auto-arrange mode, change its position to
            // fit into a new or existing block of free space.
            if (AutoArrange)
            {
                spriteBin = SpriteBin.Pack(spriteBin, sprite, SpriteLayout);
                CheckCollisions();
            }
            else
            {
                // Otherwise, check all the other sprites to see if they collide
                // with this one.
                foreach (Sprite otherSprite in sprites)
                {
                    if (Colliding(sprite, otherSprite))
                    {
                        colliding.Add(sprite);
                        colliding.Add(otherSprite);
                    }
                }

                // If so, notify listeners of the change in the number of
                // collisions.
                if (colliding.Contains(sprite))
                {
                    NotifyPropertyChanged("CollisionCount");
                }
            }

            // Adjust canvas size to contain the new sprite, if neccessary.
            Rect boundary = RenderedBoundary(sprite);
            this.Width = (sprites.Count == 1) ? (int)boundary.Right
                            : Math.Max(this.Width, (int)boundary.Right);
            this.Height = (sprites.Count == 1) ? (int)boundary.Bottom
                            : Math.Max(this.Height, (int)boundary.Bottom);
            InvalidateVisual();
        }

        // Add multiple new sprites to the sheet, selecting them if indicated.
        public void AddSprites(IEnumerable<Sprite> spritesToAdd,
                               bool select = false,
                               ModifierKeys? modifiers = null)
        {
            // If selecting, look up modifier keys if not already provided
            if (select && null == modifiers)
            {
                modifiers = Keyboard.Modifiers;
            }
            if (null == spritesToAdd || 0 == spritesToAdd.Count())
            {
                Select(null, modifiers);
                return;
            }

            // Add each sprite.
            foreach (Sprite sprite in spritesToAdd)
            {
                if (null != sprite)
                {
                    sprites.Add(sprite);
                }

                // If selecting, add a shift to the modifiers so that all of the
                // added sprites will be selected.
                if (select)
                {
                    Select(sprite, modifiers);
                    modifiers = (ModifierKeys)modifiers | ModifierKeys.Shift;
                }
            }
            NotifyPropertyChanged("SpriteCount");

            // Adjust canvas dimensions
            if (AutoArrange)
            {
                spriteBin = SpriteBin.Pack(spriteBin, spritesToAdd, SpriteLayout);
                this.Size = spriteBin.Size;
            }
            else
            {
                CheckCollisions();
                this.Width = Math.Max(
                    (SpriteCount > spritesToAdd.Count() ? this.Width : 0),
                    spritesToAdd.Max(sprite => (null == sprite ? 0 : sprite.Right)));
                this.Height = Math.Max(
                    (SpriteCount > spritesToAdd.Count() ? this.Height : 0),
                    spritesToAdd.Max(sprite => (null == sprite ? 0 : sprite.Bottom)));
            }
            InvalidateVisual();
        }

        // Remove all sprites.
        public void ClearAll()
        {
            sprites.Clear();
            selected.Clear();
            colliding.Clear();
            NotifyPropertyChanged("SpriteCount");
            NotifyPropertyChanged("SelectionCount");
            NotifyPropertyChanged("CollisionCount");
            spriteBin = null;
            InvalidateVisual();
        }

        // Remove selected sprites.
        public void ClearSelected()
        {
            foreach (Sprite sprite in selected)
            {
                sprites.Remove(sprite);
            }
            if (selected.Count > 0)
            {
                NotifyPropertyChanged("SpriteCount");
                NotifyPropertyChanged("SelectedCount");
            }
            selected.Clear();
            if (AutoArrange)
            {
                Rearrange();
            }
            else
            {
                CheckCollisions();
            }
        }

        // Select the given sprite
        public void Select(Sprite sprite, ModifierKeys? modifiers = null)
        {
            // If the modifier keys weren't given, look them up.
            if (null == modifiers)
            {
                modifiers = Keyboard.Modifiers;
            }

            // If the control key is pressed and the sprite is selected, or if
            // the Alt key is pressed, remove the given sprite from the
            // selection list if present.
            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt ||
                ((modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                 null != sprite && selected.Contains(sprite)))
            {
                if (null != sprite)
                {
                    selected.Remove(sprite);
                    NotifyPropertyChanged("SelectionCount");
                }
            }
            // Otherwise,
            else
            {
                // if neither the shift nor the control key is pressed, empty the
                // selection list,
                if ((modifiers & ModifierKeys.Shift) != ModifierKeys.Shift &&
                    (modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    selected.Clear();
                    NotifyPropertyChanged("SelectionCount");
                }
                // and select the sprite if it exists.
                if (null != sprite)
                {
                    selected.Add(sprite);
                    NotifyPropertyChanged("SelectionCount");
                }
            }
            InvalidateVisual();
        }

        // Select all sprites.
        public void SelectAll()
        {
            if (selected.Count != sprites.Count)
            {
                NotifyPropertyChanged("SelectedCount");
            }
            selected.UnionWith(sprites);
            InvalidateVisual();
        }

        // Select no sprites
        public void SelectNone()
        {
            if (selected.Count > 0)
            {
                NotifyPropertyChanged("SelectedCount");
            }
            selected.Clear();
            InvalidateVisual();
        }

        // Select unselected sprites
        public void SelectInverse()
        {
            if (selected.Count != sprites.Count - selected.Count)
            {
                NotifyPropertyChanged("SelectedCount");
            }
            selected = new HashSet<Sprite>(sprites.Except(selected));
            InvalidateVisual();
        }

    }
}