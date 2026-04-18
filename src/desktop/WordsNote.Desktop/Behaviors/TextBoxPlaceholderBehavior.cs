using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WordsNote.Desktop.Behaviors;

public static class TextBoxPlaceholderBehavior
{
    private static readonly Dictionary<TextBox, PlaceholderAdorner> ActiveAdorners = [];

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
        "Placeholder",
        typeof(string),
        typeof(TextBoxPlaceholderBehavior),
        new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public static string GetPlaceholder(DependencyObject obj)
    {
        return (string)obj.GetValue(PlaceholderProperty);
    }

    public static void SetPlaceholder(DependencyObject obj, string value)
    {
        obj.SetValue(PlaceholderProperty, value);
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.Loaded -= OnTextBoxLoaded;
        textBox.Loaded += OnTextBoxLoaded;

        textBox.TextChanged -= OnTextChanged;
        textBox.TextChanged += OnTextChanged;

        textBox.GotFocus -= OnFocusChanged;
        textBox.GotFocus += OnFocusChanged;

        textBox.LostFocus -= OnFocusChanged;
        textBox.LostFocus += OnFocusChanged;

        UpdateAdorner(textBox);
    }

    private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateAdorner(textBox);
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateAdorner(textBox);
        }
    }

    private static void OnFocusChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateAdorner(textBox);
        }
    }

    private static void UpdateAdorner(TextBox textBox)
    {
        var placeholder = GetPlaceholder(textBox);
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            RemoveAdorner(textBox);
            return;
        }

        if (!string.IsNullOrWhiteSpace(textBox.Text) || textBox.IsKeyboardFocused)
        {
            RemoveAdorner(textBox);
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer is null)
        {
            return;
        }

        if (!ActiveAdorners.TryGetValue(textBox, out var adorner))
        {
            adorner = new PlaceholderAdorner(textBox, placeholder);
            ActiveAdorners[textBox] = adorner;
            layer.Add(adorner);
            return;
        }

        adorner.PlaceholderText = placeholder;
        adorner.InvalidateVisual();
    }

    private static void RemoveAdorner(TextBox textBox)
    {
        if (!ActiveAdorners.TryGetValue(textBox, out var adorner))
        {
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(textBox);
        layer?.Remove(adorner);
        ActiveAdorners.Remove(textBox);
    }

    private sealed class PlaceholderAdorner : Adorner
    {
        public PlaceholderAdorner(UIElement adornedElement, string placeholderText)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            PlaceholderText = placeholderText;
        }

        public string PlaceholderText { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (AdornedElement is not TextBox textBox)
            {
                return;
            }

            var dpi = VisualTreeHelper.GetDpi(textBox);
            var formattedText = new FormattedText(
                PlaceholderText,
                System.Globalization.CultureInfo.CurrentUICulture,
                textBox.FlowDirection,
                new Typeface(textBox.FontFamily, textBox.FontStyle, FontWeights.Normal, textBox.FontStretch),
                textBox.FontSize,
                new SolidColorBrush(Color.FromRgb(122, 134, 160)),
                dpi.PixelsPerDip);

            var origin = new Point(textBox.Padding.Left + 2, textBox.Padding.Top + 1);
            drawingContext.DrawText(formattedText, origin);
        }
    }
}
