/******************************************************************************
 * File:               AnimatedPattern.cs
 * Author:             Elizabeth Lowry
 * Date Created:       May 7, 2014
 * Description:        Static class with a function for generating the type of
 *                      animated pattern I use for outlining sprites on the
 *                      sheet.
 * Last Modified:      May 14, 2014
 * Last Modification:  Adding header comment.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SpriteMapGenerator
{
    // static class for generating the type of animated pattern I use for sprite
    // outlines.
    public static class AnimatedPattern
    {
        public static DrawingBrush Create(Color color1, Color color2,
                                          double wavelength = 16,
                                          double durationSeconds = 1,
                                          double angle = 45)
        {
            // Create animation
            Duration duration = (durationSeconds > 0)
                ? new Duration(TimeSpan.FromSeconds(durationSeconds))
                : Duration.Forever;
            DoubleAnimation animation = new DoubleAnimation(wavelength, duration);
            animation.RepeatBehavior = RepeatBehavior.Forever;

            // Create transforms
            TranslateTransform translate = new TranslateTransform();
            translate.BeginAnimation(TranslateTransform.XProperty, animation);
            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(translate);
            transforms.Children.Add(new RotateTransform(angle));

            // Create gradient
            GradientStopCollection stops = new GradientStopCollection();
            stops.Add(new GradientStop(Color.FromArgb((byte)0,
                                                      (byte)((color1.R + color2.R)/2),
                                                      (byte)((color1.G + color2.G)/2),
                                                      (byte)((color1.B + color2.B)/2)),
                                       0.0));
            stops.Add(new GradientStop(Color.FromArgb((byte)(color1.A/2),
                                                      color1.R,
                                                      color1.G,
                                                      color1.B),
                                       0.25));
            stops.Add(new GradientStop(Color.FromArgb((byte)((color1.A + color2.A)/2),
                                                      (byte)((color1.R + color2.R)/2),
                                                      (byte)((color1.G + color2.G)/2),
                                                      (byte)((color1.B + color2.B)/2)),
                                       0.5));
            stops.Add(new GradientStop(Color.FromArgb((byte)(color2.A/2),
                                                      color2.R,
                                                      color2.G,
                                                      color2.B),
                                       0.75));
            stops.Add(new GradientStop(Color.FromArgb((byte)0,
                                                      (byte)((color1.R + color2.R)/2),
                                                      (byte)((color1.G + color2.G)/2),
                                                      (byte)((color1.B + color2.B)/2)),
                                       1.0));
            LinearGradientBrush gradient = new LinearGradientBrush(stops, 0);

            // Create brush
            DrawingBrush brush =
                new DrawingBrush(
                    new GeometryDrawing(gradient, null,
                        new RectangleGeometry(new Rect(0, 0, wavelength, wavelength))));
            brush.Transform = transforms;
            brush.TileMode = TileMode.Tile;
            brush.Viewport = new Rect(0, 0, wavelength, wavelength);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            return brush;
        }
        public static DrawingBrush Create(Color color,
                                          double wavelength = 16,
                                          double durationSeconds = 1,
                                          double angle = 45)
        {
            return Create(color, color, wavelength, durationSeconds, angle);
        }
        public static DrawingBrush Create(double wavelength = 16,
                                          double durationSeconds = 1,
                                          double angle = 45)
        {
            return Create(Colors.Black, Colors.Black, wavelength, durationSeconds, angle);
        }
    }
}
