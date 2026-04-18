using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordsNote.Desktop.Models;

namespace WordsNote.Desktop;

public partial class CardEditDialog : Window
{
    public CardEditDialog(StudyCard card, string? themeMode)
    {
        InitializeComponent();

        FrontTextBox.Text = card.Front;
        BackTextBox.Text = card.Back;
        HintTextBox.Text = card.Hint ?? string.Empty;
        TagsTextBox.Text = string.Join(", ", card.Tags);

        ApplyTheme(themeMode);
    }

    public string FrontText => FrontTextBox.Text.Trim();

    public string BackText => BackTextBox.Text.Trim();

    public string HintText => HintTextBox.Text.Trim();

    public string TagsText => TagsTextBox.Text;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FrontText) || string.IsNullOrWhiteSpace(BackText))
        {
            MessageBox.Show(
                "Front and back are required.",
                "Missing fields",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ApplyTheme(string? themeMode)
    {
        var isDark = string.Equals(themeMode, "dark", StringComparison.OrdinalIgnoreCase);
        if (!isDark)
        {
            SaveButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F4ED8"));
            SaveButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1639A5"));
            SaveButton.Foreground = Brushes.White;

            CancelButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDF2FF"));
            CancelButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));
            CancelButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2333"));
            return;
        }

        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121826"));
        RootBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2234"));
        RootBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C4B69"));

        TitleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
        SubtitleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A8B5CD"));
        FrontLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
        BackLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
        HintLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
        TagsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));

        foreach (var box in new TextBox[] { FrontTextBox, BackTextBox, HintTextBox, TagsTextBox })
        {
            box.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121826"));
            box.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
            box.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C4B69"));
        }

        SaveButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A97FF"));
        SaveButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9AB9FF"));
        SaveButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121826"));

        CancelButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A3A60"));
        CancelButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C4B69"));
        CancelButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EDF9"));
    }
}
