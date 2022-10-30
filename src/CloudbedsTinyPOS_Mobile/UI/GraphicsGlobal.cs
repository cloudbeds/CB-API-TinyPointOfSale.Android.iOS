using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

internal static class GraphicsGlobals
{

    
//    private static Brush s_LightButton = new SolidColorBrush(Color.FromArgb(250, 255, 259, 0));
    private static Brush s_LightButton = new SolidColorBrush(Color.FromRgb( 240, 240, 245));
    private static Brush s_MediumButton = new SolidColorBrush(Color.FromRgb(180, 180, 255));

    public static Brush LightButton { get { return s_LightButton; } }
    public static Brush MediumButton { get { return s_MediumButton; } }
    
}
