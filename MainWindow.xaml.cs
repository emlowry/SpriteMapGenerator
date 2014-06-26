/******************************************************************************
 * File:               MainWindow.xaml.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 5, 2014
 * Description:        C# backend for the main window of the WPF App.
 * Last Modified:      May 19, 2014
 * Last Modification:  Split into separate files.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace SpriteMapGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void CopySelected()
        {
            if (sheetCanvas.SelectionCount > 0)
            {
                // Save text, both regular and xaml
                DataObject data = new DataObject();
                XmlDocument document = sheetCanvas.SelectedXml();
                StringWriter writer = new StringWriter();
                document.Save(writer);
                string xml = writer.ToString();
                data.SetText(xml, TextDataFormat.Xaml);
                data.SetText(xml, TextDataFormat.Text);

                // Save image with transparency
                MemoryStream ms = new MemoryStream();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(sheetCanvas.SelectedBitmap());
                encoder.Save(ms);
                data.SetData("PNG", ms);

                // Save image without transparency
                ms = new MemoryStream();
                data.SetImage(sheetCanvas.SelectedBitmap(Brushes.White));
                Clipboard.SetDataObject(data);
            }
        }

        /**
         * Mouse Event Handlers
         */

        void CanvasMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sheetCanvas.Drag(e.GetPosition(sheetCanvas));
            }
        }

        void CanvasLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            sheetCanvas.EndDrag(e.GetPosition(sheetCanvas));
        }

        /**
         * Edit Menu Event Handlers
         */

        void CommandBindingCut_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SelectionCount > 0);
        }

        void CommandBindingCut_Executed(object target, ExecutedRoutedEventArgs e)
        {
            CopySelected();
            sheetCanvas.ClearSelected();
        }

        void CommandBindingCopy_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SelectionCount > 0);
        }

        void CommandBindingCopy_Executed(object target, ExecutedRoutedEventArgs e)
        {
            CopySelected();
        }

        void CommandBindingPaste_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Sprite.CanCreateFromClipboard();
        }

        void CommandBindingPaste_Executed(object target, ExecutedRoutedEventArgs e)
        {
            Sprite[] sprites = Sprite.CreateFromClipboard();
            sheetCanvas.AddSprites(sprites, true, Keyboard.Modifiers & ~ModifierKeys.Control);
        }

        void CommandBindingDelete_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SelectionCount > 0);
        }

        void CommandBindingDelete_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.ClearSelected();
        }

        void CommandBindingSelectAll_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SpriteCount > 0);
        }

        void CommandBindingSelectAll_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.SelectAll();
        }

        void CommandBindingSelectNone_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SelectionCount > 0);
        }

        void CommandBindingSelectNone_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.SelectNone();
        }

        void CommandBindingSelectInverse_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SpriteCount > 0);
        }

        void CommandBindingSelectInverse_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.SelectInverse();
        }

        /**
         * Arrange Menu Event Handlers
         */

        void CommandBindingRearrange_CanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sheetCanvas.SpriteCount > 0);
        }

        void CommandBindingRearrange_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.Rearrange();
        }

        void CommandBindingAutoArrange_Executed(object target, ExecutedRoutedEventArgs e)
        {
            sheetCanvas.AutoArrange = !sheetCanvas.AutoArrange;
            //autoArrangeMenuItem.IsChecked = sheetCanvas.AutoArrange;
        }

        // Scroll up or down through sprite layout options
        void CommandBindingArrangeShapeDown_Executed(object target, ExecutedRoutedEventArgs e)
        {
            switch (sheetCanvas.SpriteLayout)
            {
                case SpriteBin.BinShape.Square: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Tall; break;
                case SpriteBin.BinShape.Tall: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Wide; break;
                default: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Square; break;
            }
        }
        void CommandBindingArrangeShapeUp_Executed(object target, ExecutedRoutedEventArgs e)
        {
            switch (sheetCanvas.SpriteLayout)
            {
                case SpriteBin.BinShape.Square: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Wide; break;
                case SpriteBin.BinShape.Wide: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Tall; break;
                default: sheetCanvas.SpriteLayout = SpriteBin.BinShape.Square; break;
            }
        }
    }
}
