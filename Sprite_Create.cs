/******************************************************************************
 * File:               Sprite_Create.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 19, 2014
 * Description:        Functionality for creating sprites.
 * Last Modified:      May 19, 2014
 * Last Modification:  Creation.
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
        // constructors
        public Sprite(int width = 0, int height = 0, string name = "")
        {
            Setup(name);
            Source = null;
            Size = new Size(width, height);
        }
        public Sprite(BitmapSource source, string name = "")
        {
            Setup(name);
            Source = source;
        }
        public Sprite(string filename, string name = "")
        {
            Setup(name.Length > 0 ? name : Path.GetFileNameWithoutExtension(filename));
            LoadFile(filename);
        }
        public Sprite(byte[] pngData, string name = "")
        {
            Setup(name);
            LoadPngData(pngData);
        }
        public Sprite(Stream pngStream, string name = "")
        {
            Setup(name);
            LoadPngStream(pngStream);
        }
        public Sprite(XmlNode node, string name = "")
        {
            Setup(name);
            LoadXml(node);
        }
        protected void Setup(string name)
        {
            X = 0;
            Y = 0;
            Name = name;
        }

        // load image from file
        protected void LoadFile(string filename)
        {
            Source = new BitmapImage(new Uri(filename));
        }

        // load image from a stream containing PNG data
        protected void LoadPngStream(Stream pngStream)
        {
            PngBitmapDecoder decoder =
                new PngBitmapDecoder(pngStream,
                                     BitmapCreateOptions.None,
                                     BitmapCacheOption.Default);
            if (decoder.Frames.Count > 0 && decoder.Frames[0] != null)
            {
                Source = decoder.Frames[0];
            }
        }

        // load image from raw PNG bytes
        protected void LoadPngData(byte[] pngData)
        {
            if (null == pngData)
            {
                Source = null;
                return;
            }
            LoadPngStream(new MemoryStream(pngData));
        }

        // load image and location from xml node
        protected void LoadXml(XmlNode node)
        {
            int i;

            // load name
            if ((null == Name || Name.Length == 0) &&
                node.Attributes["name"] != null &&
                node.Attributes["name"].Value.Length > 0)
            {
                Name = node.Attributes["name"].Value;
            }

            // load image file or data
            if (node.Attributes["src"] != null &&
                node.Attributes["src"].Value.Length > 0)
            {
                LoadFile(node.Attributes["src"].Value);
                if (null == Name || Name.Length == 0)
                {
                    Name = node.Attributes["src"].Value;
                }
            }
            else if (node.InnerText.Length > 0)
            {
                LoadPngData(Convert.FromBase64String(node.InnerText));
            }

            // load image dimensions, if no data is available
            else
            {
                Source = null;
                Size size = new Size(0, 0);
                if (node.Attributes["w"] != null &&
                    int.TryParse(node.Attributes["w"].Value, out i))
                {
                    size.Width = i;
                }
                if (node.Attributes["h"] != null &&
                    int.TryParse(node.Attributes["h"].Value, out i))
                {
                    size.Height = i;
                }
                this.Size = size;
            }

            // update position
            if (node.Attributes["x"] != null &&
                int.TryParse(node.Attributes["x"].Value, out i))
            {
                X = i;
            }
            if (node.Attributes["y"] != null &&
                int.TryParse(node.Attributes["y"].Value, out i))
            {
                Y = i;
            }
        }

        // does the clipboard contain data that can be converted to a sprite?
        public static bool CanCreateFromClipboard()
        {
            if (Clipboard.ContainsText(TextDataFormat.Xaml) || Clipboard.ContainsText())
            {
                string xml = Clipboard.ContainsText(TextDataFormat.Xaml)
                             ? Clipboard.GetText(TextDataFormat.Xaml)
                             : Clipboard.GetText();
                XmlDocument document = new XmlDocument();
                try
                {
                    document.LoadXml(xml);
                    if (document.SelectNodes("//sprite").Count > 0)
                    {
                        return true;
                    }
                }
                catch { }
            }
            if (Clipboard.ContainsData("PNG") || Clipboard.ContainsImage())
            {
                return true;
            }
            return false;
        }

        // return a sprite created from data on the clipboad
        public static Sprite[] CreateFromClipboard()
        {
            // Try for XML first, since it'll have position data
            List<Sprite> sprites = new List<Sprite>();
            if (Clipboard.ContainsText(TextDataFormat.Xaml) || Clipboard.ContainsText())
            {
                string xml = Clipboard.ContainsText(TextDataFormat.Xaml)
                             ? Clipboard.GetText(TextDataFormat.Xaml)
                             : Clipboard.GetText();
                XmlDocument document = new XmlDocument();
                try
                {
                    document.LoadXml(xml);
                    XmlNodeList nodes = document.SelectNodes("//sprite");
                    if (nodes.Count > 0)
                    {
                        foreach (XmlNode sprite in nodes)
                        {
                            sprites.Add(new Sprite(sprite));
                        }
                        return sprites.ToArray();
                    }
                }
                catch { }
            }

            // If XML doesn't work, try PNG data (the default image format messes
            // with transparency)
            if (Clipboard.ContainsData("PNG"))
            {
                Object pngObject = Clipboard.GetData("PNG");
                if (pngObject is MemoryStream)
                {
                    PngBitmapDecoder decoder =
                        new PngBitmapDecoder(pngObject as MemoryStream,
                                             BitmapCreateOptions.None,
                                             BitmapCacheOption.Default);
                    foreach (BitmapFrame sprite in decoder.Frames)
                    {
                        if (null != sprite)
                        {
                            sprites.Add(new Sprite(sprite));
                        }
                    }
                    SpriteBin.Pack(sprites, SpriteBin.BinShape.Square);
                    return sprites.ToArray();
                }
            }

            // If there wasn't any PNG data, try regular image data
            if (Clipboard.ContainsImage())
            {
                return new Sprite[] { new Sprite(ClipboardDibDecoder.GetBitmapFrame()) };
            }

            // If nothing works, return an empty list.
            return new Sprite[0];
        }

    }
}