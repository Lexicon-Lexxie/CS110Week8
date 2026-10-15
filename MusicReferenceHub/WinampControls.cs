using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicReferenceHub;

/// <summary>
/// Seven-segment LED clock like the one in classic Winamp. Set Text to something like "03:27".
/// </summary>
public class LedTime : FrameworkElement
{
    private const double DigitWidth = 9, DigitHeight = 13, Thick = 2, DigitAdvance = 12, ColonAdvance = 6;

    // segments a(top) b(top-right) c(bottom-right) d(bottom) e(bottom-left) f(top-left) g(middle)
    private static readonly string[] Segments =
    {
        "abcdef", "bc", "abdeg", "abcdg", "bcfg", "acdfg", "acdefg", "abc", "abcdefg", "abcdfg"
    };

    private static Brush MakeDefaultBrush()
    {
        var b = new SolidColorBrush(Color.FromRgb(0x00, 0xE2, 0x00));
        b.Freeze();
        return b;
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(LedTime),
        new FrameworkPropertyMetadata("00:00", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LitBrushProperty = DependencyProperty.Register(
        nameof(LitBrush), typeof(Brush), typeof(LedTime),
        new FrameworkPropertyMetadata(MakeDefaultBrush(), FrameworkPropertyMetadataOptions.AffectsRender));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Brush LitBrush
    {
        get => (Brush)GetValue(LitBrushProperty);
        set => SetValue(LitBrushProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double w = 0;
        foreach (char c in Text ?? "") w += c == ':' ? ColonAdvance : DigitAdvance;
        return new Size(w, DigitHeight);
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));   // makes the whole clock clickable
        var brush = LitBrush;
        double x = 0;
        foreach (char c in Text ?? "")
        {
            if (c == ':')
            {
                dc.DrawRectangle(brush, null, new Rect(x + 2, 3.5, 2, 2));
                dc.DrawRectangle(brush, null, new Rect(x + 2, 8, 2, 2));
                x += ColonAdvance;
            }
            else
            {
                if (c >= '0' && c <= '9') DrawDigit(dc, brush, x, Segments[c - '0']);
                else if (c == '-') DrawDigit(dc, brush, x, "g");
                x += DigitAdvance;
            }
        }
    }

    private static void DrawDigit(DrawingContext dc, Brush brush, double x, string lit)
    {
        double w = DigitWidth, t = Thick;
        foreach (char s in lit)
        {
            Rect r = s switch
            {
                'a' => new Rect(x + 1, 0, w - 2, t),
                'b' => new Rect(x + w - t, 1, t, 5.5),
                'c' => new Rect(x + w - t, 6.5, t, 5.5),
                'd' => new Rect(x + 1, DigitHeight - t, w - 2, t),
                'e' => new Rect(x, 6.5, t, 5.5),
                'f' => new Rect(x, 1, t, 5.5),
                _ => new Rect(x + 1, 5.5, w - 2, t),
            };
            dc.DrawRectangle(brush, null, r);
        }
    }
}

/// <summary>Draws the track that is currently playing in white (everything else keeps the skin colour).</summary>
public class CurrentTrackBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 1 && values[0] is int row && values[1] is int current && row == current && row >= 0)
            return Brushes.White;
        return Application.Current?.MainWindow?.TryFindResource("WaGreen") as Brush ?? Brushes.Lime;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Shows playlist rows like Winamp does: "1. File name" (no folder, no extension).</summary>
public class PlaylistNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string path = values.Length > 0 && values[0] is string s ? s : "";
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        if (values.Length > 1 && values[1] is int i && i >= 0) return $"{i + 1}. {name}";
        return name;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
