/******************************************************************************
 * File:               Sprite.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 6, 2014
 * Description:        Class representing a single sprite.
 * Last Modified:      May 19, 2014
 * Last Modification:  Splitting into separate files.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace SpriteMapGenerator
{
    // Manages a single sprite image on a sheet
    public partial class Sprite
    {

        // Name
        protected const string DEFAULT_NAME = "sprite";
        protected const string COPY_SUFFIX = " copy";
        protected static int autoNamedSprites = 0;
        protected string name;
        public string Name
        {
            get
            {
                if (null == name || 0 == name.Length)
                {
                    ++autoNamedSprites;
                    name = DEFAULT_NAME + " " + autoNamedSprites.ToString();
                }
                return name;
            }
            set
            {
                name = Regex.Replace(value, @"[\s-[ ]]", "").Trim();
            }
        }
        public static void ResetAutoNamedSpriteCount() { autoNamedSprites = 0; }

        // Image data source
        BitmapSource source;
        public BitmapSource Source
        {
            get { return source; }
            set
            {
                if (value != source)
                {
                    source = value;
                    Size = (null == source) ? new Size(0,0)
                        : new Size(source.PixelWidth, source.PixelHeight);
                }
            }
        }

        // Size
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

        // Location
        public int X { get; set; }
        public int Y { get; set; }
        public Point Location
        {
            get { return new Point(X, Y); }
            set
            {
                X = (int)value.X;
                Y = (int)value.Y;
            }
        }
        public int Left { get { return X; } }
        public int Top { get { return Y; } }
        public int Right { get { return X + Width; } }
        public int Bottom { get { return Y + Height; } }

        // sprite boundary rectangle
        public Rect Boundary { get { return new Rect(Location, Size); } }

        // Add a suffix indicating that the sprite is a copy of another sprite
        // with the same name or add/increment a number to the end of the name.
        public void AdjustName()
        {
            if (Name == DEFAULT_NAME ||
                Name.Substring(Name.Length - COPY_SUFFIX.Length) == COPY_SUFFIX)
            {
                Name += " 2";
            }
            else if (Regex.IsMatch(Name, "^(" + DEFAULT_NAME + " |.*" + COPY_SUFFIX + " )[0-9]+$"))
            {
                string firstPart = Regex.Replace(Name, "[0-9]+$", "");
                int i;
                if (int.TryParse(Name.Substring(firstPart.Length), out i))
                {
                    Name = firstPart + (i + 1).ToString();
                }
            }
            else
            {
                Name += COPY_SUFFIX;
            }
        }

        // create a sprite xml node
        public XmlElement ToXml(XmlDocument document, bool includeImage = true)
        {
            // create the node
            if (null == document)
            {
                document = new XmlDocument();
            }
            XmlElement element = document.CreateElement("sprite");

            // add image data
            if (includeImage && source != null)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                MemoryStream memory = new MemoryStream();
                encoder.Save(memory);
                element.InnerText = Convert.ToBase64String(memory.ToArray());
            }

            // add name attributes
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = Name;
            element.Attributes.Append(nameAttribute);

            // add size attributes
            XmlAttribute xAttribute = document.CreateAttribute("x");
            xAttribute.Value = X.ToString();
            element.Attributes.Append(xAttribute);
            XmlAttribute yAttribute = document.CreateAttribute("y");
            yAttribute.Value = Y.ToString();
            element.Attributes.Append(yAttribute);
            XmlAttribute wAttribute = document.CreateAttribute("w");
            wAttribute.Value = Width.ToString();
            element.Attributes.Append(wAttribute);
            XmlAttribute hAttribute = document.CreateAttribute("h");
            hAttribute.Value = Height.ToString();
            element.Attributes.Append(hAttribute);

            // return
            return element;
        }

        // Calculate a boundary that contains all the given sprites
        public static Rect UnionBoundary(IEnumerable<Sprite> members)
        {
            Rect boundary = new Rect();
            bool hasBoundary = false;
            foreach (Sprite sprite in members)
            {
                if (hasBoundary)
                {
                    boundary.Union(sprite.Boundary);
                }
                else
                {
                    boundary = sprite.Boundary;
                    hasBoundary = true;
                }
            }
            return boundary;
        }

    }
}
