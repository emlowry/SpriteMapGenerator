/******************************************************************************
 * File:               SpriteBin.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 12, 2014
 * Description:        Class implementing a sorting algorithm for packing
 *                      sprites into a collision-free, relatively compact layout
 *                      of roughly square, vertical strip, or horizontal strip
 *                      shape.
 * Last Modified:      May 14, 2014
 * Last Modification:  Adding header comment.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SpriteMapGenerator
{
    // This class only exposes static methods for arranging sprites and an enum
    // for indicating what shape to aim for for the sprite sheet as a whole.
    // The sprite packing algorithm was posted by Jake Gordon on May 7, 2011 at
    // http://codeincomplete.com/posts/2011/5/7/bin_packing/ - thanks Mr. Gordon!
    public class SpriteBin
    {
        // what shape to aim for when packing - square, tall rectangle, or wide
        // rectangle
        public enum BinShape
        {
            Square,
            Tall,
            Wide
        }

        // bin dimensions
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public Size Size
        {
            get { return new Size(Width, Height); }
            protected set
            {
                Width = (int)value.Width;
                Height = (int)value.Height;
            }
        }
        public Point Location
        {
            get { return new Point(X, Y); }
            protected set
            {
                X = (int)value.X;
                Y = (int)value.Y;
            }
        }

        // is there a sprite in this bin?
        protected bool Used = false;

        // bins within this one, below or to the right of the bin contents
        protected SpriteBin Down = null;
        protected SpriteBin Right = null;

        // the constructors can only be called by the class itself - the only
        // public function is the static Pack function.
        protected SpriteBin(int width = 0, int height = 0,
                               int x = 0, int y = 0)
        {
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }
        protected SpriteBin(int width, int height, Point location)
            : this(width, height, (int)location.X, (int)location.Y) { }
        protected SpriteBin(Size size, int x = 0, int y = 0)
            : this((int)size.Width, (int)size.Height, x, y) { }
        protected SpriteBin(Size size, Point location)
            : this((int)size.Width, (int)size.Height,
                    (int)location.X, (int)location.Y) { }

        // Arrange the given collection of sprites in a compact rectangle,
        // returning the packed bin in case more sprites need to be added later.
        public static SpriteBin Pack(IEnumerable<Sprite> sprites, int x, int y,
                                     BinShape shape = BinShape.Square)
        {
            // if there are no sprites to pack, don't return anything
            if (null == sprites || 0 == sprites.Count())
            {
                return null;
            }

            // sort sprites from largest to smallest
            IOrderedEnumerable<Sprite> sorted;
            if (SpriteBin.BinShape.Tall == shape)
            {
                sorted = sprites.OrderByDescending(sprite => sprite.Width)
                                .ThenByDescending(sprite => sprite.Height);
            }
            else if (SpriteBin.BinShape.Wide == shape)
            {
                sorted = sprites.OrderByDescending(sprite => sprite.Height)
                                .ThenByDescending(sprite => sprite.Width);
            }
            else
            {
                sorted = sprites.OrderByDescending(sprite =>
                                        Math.Max(sprite.Width, sprite.Height))
                                .ThenByDescending(sprite =>
                                        Math.Min(sprite.Width, sprite.Height));
            }

            // create a bin large enough to hold the piece with the longest side
            // plus, if the desired shape is a strip, the largest dimension
            // perpendicular to the strip direction
            SpriteBin bin = new SpriteBin(sorted.ElementAt(0).Size, x, y);
            if (BinShape.Tall == shape)
            {
                bin.Width = sprites.Max(sprite => sprite.Width);
            }
            else if (BinShape.Wide == shape)
            {
                bin.Height = sprites.Max(sprite => sprite.Height);
            }
            bin.Pack(sorted, shape);
            return bin;
        }
        public static SpriteBin Pack(IEnumerable<Sprite> sprites, Point location,
                                     BinShape shape = BinShape.Square)
        {
            return Pack(sprites, (int)location.X, (int)location.Y, shape);
        }
        public static SpriteBin Pack(IEnumerable<Sprite> sprites,
                                     BinShape shape = BinShape.Square)
        {
            return Pack(sprites, 0, 0, shape);
        }

        // Pack sprites around an existing set of free and filled space.
        public static SpriteBin Pack(SpriteBin bin, IEnumerable<Sprite> sprites,
                                     BinShape shape = BinShape.Square)
        {
            if (null == bin)
            {
                return Pack(sprites, shape);
            }
            IOrderedEnumerable<Sprite> sorted =
                sprites.OrderByDescending(sprite =>
                                          Math.Max(sprite.Width, sprite.Height))
                       .ThenByDescending(sprite =>
                                         Math.Min(sprite.Width, sprite.Height));
            bin.Pack(sorted, shape);
            return bin;
        }

        // Pack a single sprite into a bin
        public static SpriteBin Pack(Sprite sprite, int x = 0, int y = 0)
        {
            if (null == sprite)
            {
                return null;
            }
            SpriteBin bin = new SpriteBin(sprite.Size, x, y);
            bin.Used = true;
            sprite.Location = bin.Location;
            return bin;
        }
        public static SpriteBin Pack(Sprite sprite, Point location)
        {
            return Pack(sprite, (int)location.X, (int)location.Y);
        }
        public static SpriteBin Pack(SpriteBin bin, Sprite sprite,
                                     BinShape shape = BinShape.Square)
        {
            if (null == bin)
            {
                return Pack(sprite, 0, 0);
            }
            bin.Pack(sprite, shape);
            return bin;
        }

        // Place the given, ordered sequence of sprites into this bin,
        // expanding the bin as needed in the desired direction
        protected void Pack(IOrderedEnumerable<Sprite> sprites,
                            BinShape shape = BinShape.Square)
        {
            foreach (Sprite sprite in sprites)
            {
                Pack(sprite, shape);
            }
        }
        protected void Pack(Sprite sprite, BinShape shape)
        {
            if (!Pack(sprite))
            {
                Grow(sprite, shape);
            }
        }

        // Place the given sprite into the first chunk of empty space in this
        // bin that will fit it, returning true if successful
        protected bool Pack(Sprite sprite)
        {
            if (Used)
            {
                return ((null != Right && Right.Pack(sprite)) ||
                        (null != Down && Down.Pack(sprite)));
            }
            else if (Width >= sprite.Width && Height >= sprite.Height)
            {
                Used = true;
                sprite.Location = Location;
                Right = (Width == sprite.Width) ? null
                        : new SpriteBin(Width - sprite.Width, sprite.Height,
                                        X + sprite.Width, Y);
                Down = (Height == sprite.Height) ? null
                        : new SpriteBin(Width, Height - sprite.Height,
                                        X, Y + sprite.Height);
                return true;
            }
            return false;
        }

        // Increase the size of the bin to fit the given sprite, then place the
        // sprite in the expanded space
        protected void Grow(Sprite sprite, BinShape shape)
        {
            switch (shape)
            {
                case BinShape.Tall: GrowDown(sprite); break;
                case BinShape.Wide: GrowRight(sprite); break;
                default:
                {
                    // Determine which directions the bin can or should grow to
                    // fit the given sprite
                    bool canGrowRight = (Height >= sprite.Height);
                    bool canGrowDown = (Width >= sprite.Width);
                    bool shouldGrowRight = canGrowRight &&
                        (Math.Max(Height, sprite.Height) >= Width + sprite.Width);
                    bool shouldGrowDown = canGrowDown &&
                        (Math.Max(Width, sprite.Width) >= Height + sprite.Height);

                    // Grow in whichever direction makes the sheet more square
                    if ((shouldGrowRight && shouldGrowDown) ||
                        (!shouldGrowRight && !shouldGrowDown &&
                         canGrowRight == canGrowDown))
                    {
                        double rightRatio =
                            (Width + sprite.Width > Math.Max(Height, sprite.Height))
                            ? (double)(Width + sprite.Width) / Math.Max(Height, sprite.Height)
                            : (double)Math.Max(Height, sprite.Height) / (Width + sprite.Width);
                        double downRatio =
                            (Height + sprite.Height > Math.Max(Width, sprite.Width))
                            ? (double)(Height + sprite.Height) / Math.Max(Width, sprite.Width)
                            : (double)Math.Max(Width, sprite.Width) / (Height + sprite.Height);
                        if (downRatio < rightRatio)
                        {
                            GrowDown(sprite);
                        }
                        else
                        {
                            GrowRight(sprite);
                        }
                    }
                    else if (shouldGrowRight)
                    {
                        GrowRight(sprite);
                    }
                    else if (shouldGrowDown)
                    {
                        GrowDown(sprite);
                    }
                    else if (canGrowRight)
                    {
                        GrowRight(sprite);
                    }
                    else if (canGrowDown)
                    {
                        GrowDown(sprite);
                    }
                    else
                    {
                        throw new Exception("It shouldn't be possible to reach this line");
                    }
                    break;
                }
            }
        }

        // Increase the vertical size of the bin to fit the given sprite, then
        // place the sprite in the expanded space
        protected void GrowDown(Sprite sprite)
        {
            Right = (SpriteBin)this.MemberwiseClone();
            Down = new SpriteBin(Math.Max(Width, sprite.Width), sprite.Height,
                                 X, Y + Height);
            if (Width < sprite.Width)
            {
                Right.Down = (SpriteBin)Right.MemberwiseClone();
                Right.Right = new SpriteBin(sprite.Width - Width, Height,
                                            X + Width, Y);
                Right.Width = sprite.Width;
                Right.Used = true;
                Width = sprite.Width;
            }
            Height += sprite.Height;
            Used = true;
            if (!Down.Pack(sprite))
            {
                throw new Exception("It shouldn't be possible to reach this line");
            }
        }

        // Increase the horizontal size of the bin to fit the given sprite, then
        // place the sprite in the expanded space
        protected void GrowRight(Sprite sprite)
        {
            Down = (SpriteBin)this.MemberwiseClone();
            Right = new SpriteBin(sprite.Width, Math.Max(Height, sprite.Height),
                                  X + Width, Y);
            if (Height < sprite.Height)
            {
                Down.Right = (SpriteBin)Down.MemberwiseClone();
                Down.Down = new SpriteBin(Width, sprite.Height - Height,
                                          X, Y + Height);
                Down.Height = sprite.Height;
                Down.Used = true;
                Height = sprite.Height;
            }
            Width += sprite.Width;
            Used = true;
            if (!Right.Pack(sprite))
            {
                throw new Exception("It shouldn't be possible to reach this line");
            }
        }
    }
}
