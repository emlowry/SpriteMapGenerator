/******************************************************************************
 * File:               SheetCanvas.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 6, 2014
 * Description:        Canvas representing the sprite sheet as a whole.
 * Last Modified:      May 19, 2014
 * Last Modification:  Breaking into several files.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace SpriteMapGenerator
{
    partial class SheetCanvas : Canvas, INotifyPropertyChanged
    {
        // Pens used for outlining normal, selected, and colliding sprites,
        // respectively.
        protected static Pen normalOutline =
            new Pen(AnimatedPattern.Create(Colors.Lime, Colors.Yellow), 1);
        protected static Pen selectedOutline =
            new Pen(AnimatedPattern.Create(Colors.Blue, Colors.Aqua), 1);
        protected static Pen collidingOutline =
            new Pen(AnimatedPattern.Create(Colors.Red, Colors.Magenta), 1);

        // Track all the sprites and which ones are selected and which ones are
        // colliding.
        protected HashSet<Sprite> sprites = new HashSet<Sprite>();
        protected HashSet<Sprite> selected = new HashSet<Sprite>();
        protected HashSet<Sprite> colliding = new HashSet<Sprite>();
        public int SpriteCount { get { return sprites.Count; } }
        public int SelectionCount { get { return selected.Count; } }
        public int CollisionCount { get { return colliding.Count; } }

        // Make sure that custom properties that other things might listen to
        // send signals to their listeners when they change.
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            // Draw the sprite images first.  If a drag is in progress, draw them
            // at a displacement from their actual positions.
            foreach (Sprite sprite in sprites)
            {
                dc.DrawImage(sprite.Source, RenderedBoundary(sprite));
            }

            // Draw the outlines next, so that even sprites covered up by others
            // will be indicated.
            foreach (Sprite sprite in sprites)
            {
                Rect border = RenderedBoundary(sprite);

                // Shrink the border to align with pixel centers.  Otherwise, it
                // will appear blurry.
                border.X += 0.5;
                border.Y += 0.5;
                border.Height -= 1;
                border.Width -= 1;

                // Use the outline color to indicate if the sprite is currently
                // selected or colliding with other sprites.
                if (selected.Contains(sprite))
                {
                    dc.DrawRectangle(null, selectedOutline, border);
                }
                else if (colliding.Contains(sprite))
                {
                    dc.DrawRectangle(null, collidingOutline, border);
                }
                else
                {
                    dc.DrawRectangle(null, normalOutline, border);
                }
            }
        }

        // Load sprites from the descendents of the given XML node.
        public void LoadXml(XmlNode node)
        {
            List<Sprite> spritesToAdd = new List<Sprite>();
            foreach (XmlNode sprite in node.SelectNodes(".//sprite"))
            {
                spritesToAdd.Add(new Sprite(sprite));
            }
            AddSprites(spritesToAdd);
        }

        // Generate an XML element containing all the sprites.
        public XmlElement ToXml(XmlDocument document, bool includeImages = true)
        {
            XmlElement sheet = document.CreateElement("sheet");
            foreach (Sprite sprite in sprites)
            {
                sheet.AppendChild(sprite.ToXml(document, includeImages));
            }
            return sheet;
        }

        // Generate a bitmap of the entire sprite sheet
        public BitmapFrame ToBitmap()
        {
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext dc = drawing.RenderOpen();
            foreach (Sprite sprite in sprites)
            {
                dc.DrawImage(sprite.Source, sprite.Boundary);
            }
            dc.Close();
            RenderTargetBitmap bitmap =
                new RenderTargetBitmap((int)this.Width, (int)this.Height,
                                       96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            return BitmapFrame.Create(bitmap);
        }

        // Generate XML for only the selected sprites.
        public XmlDocument SelectedXml()
        {
            // If no sprites are selected, return null.
            if (SelectionCount == 0)
            {
                return null;
            }
            XmlDocument document = new XmlDocument();

            // If only one sprite is selected, return a document containing only
            // a sprite element
            if (SelectionCount == 1)
            {
                document.AppendChild(selected.ElementAt(0).ToXml(document));
            }

            // If multiple sprites are selected, return a partial sprite sheet.
            else
            {
                XmlElement sheet = document.CreateElement("sheet");
                foreach (Sprite sprite in selected)
                {
                    sheet.AppendChild(sprite.ToXml(document));
                }
                document.AppendChild(sheet);
            }
            return document;
        }

        // Generate a bitmap of only the selected sprites
        public BitmapFrame SelectedBitmap(Brush background = null)
        {
            if (SelectionCount == 0)
            {
                return null;
            }

            // flatten selected sprites into bitmap image
            Rect boundary = Sprite.UnionBoundary(selected);
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext dc = drawing.RenderOpen();
            if (null != background)
            {
                dc.DrawRectangle(background, null,
                                 new Rect(0, 0, boundary.Width, boundary.Height));
            }
            foreach (Sprite sprite in selected)
            {
                dc.DrawImage(sprite.Source,
                             new Rect(sprite.X - boundary.X,
                                      sprite.Y - boundary.Y,
                                      sprite.Width, sprite.Height));
            }
            dc.Close();
            RenderTargetBitmap bitmap =
                new RenderTargetBitmap((int)boundary.Width, (int)boundary.Height,
                                       96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            return frame;
        }
    }
}
