using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordsNote.Desktop.ViewModels;

namespace WordsNote.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
        ApplyThemePalette(_viewModel.ThemeMode);
    }

    private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyThemePalette(_viewModel.ThemeMode);
    }

    private async void ReloadData_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ReloadDataAsync();
    }

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveSettingsAsync();
    }

    private void OpenLoginFromLanding_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenLoginPage();
    }

    private void OpenSettingsFromLanding_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenSettingsPage();
    }

    private void OpenLearnFromLanding_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenLearnPage();
    }

    private void OpenManageFromLanding_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenManagePage();
    }

    private void OpenPrivacyFromLanding_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenPrivacyPage();
    }

    private void BackToLandingFromPrivacy_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenLandingPage();
    }

    private void OpenLoginFromManage_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenLoginPage();
    }

    private void OpenManageFromLogin_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenManagePage();
    }

    private void ContinueLocalMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ContinueWithLocalMode();
    }

    private void LearnFlashMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetLearnMode("flash");
    }

    private void LearnTypeMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetLearnMode("learn");
    }

    private void LearnPracticeMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetLearnMode("practice");
    }

    private void LearnPrevious_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectLearnPreviousCard();
    }

    private void LearnFlip_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleLearnCardFace();
    }

    private void LearnNext_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectLearnNextCard();
    }

    private void LearnCheckTyping_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CheckLearnTypingAnswer();
    }

    private void LearnSubmitPractice_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SubmitPracticeOption();
    }

    private void LearnNextPractice_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NextPracticeQuestion();
    }

    private void PrivacyVi_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetPrivacyLanguage("vi");
    }

    private void PrivacyEn_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetPrivacyLanguage("en");
    }

    private async void LoginWithGoogleToken_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoginWithGoogleTokenAsync();
    }

    private async void LoginWithGoogleBrowser_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoginWithGoogleBrowserAsync();
    }

    private async void SyncLocalToCloud_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SyncLocalToCloudAsync();
    }

    private async void SyncCloudToLocal_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SyncCloudToLocalAsync();
    }

    private async void CreateCollection_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.CreateCollectionAsync();
    }

    private void StartCollectionEdit_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StartCollectionEdit();
    }

    private async void SaveCollectionEdit_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveCollectionEditAsync();
    }

    private void CancelCollectionEdit_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CancelCollectionEdit();
    }

    private async void DeleteCollection_Click(object sender, RoutedEventArgs e)
    {
        var confirmed = MessageBox.Show(
            "Delete selected collection and all its cards?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        await _viewModel.DeleteSelectedCollectionAsync();
    }

    private async void SaveCard_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveCardAsync();
    }

    private void ClearCardForm_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearCardForm();
    }

    private async void EditSelectedCard_Click(object sender, RoutedEventArgs e)
    {
        var selectedCard = _viewModel.ManageSelectedCard;
        if (selectedCard is null)
        {
            MessageBox.Show(
                "Select a card first.",
                "Edit card",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var dialog = new CardEditDialog(selectedCard, _viewModel.ThemeMode)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await _viewModel.UpdateCardAsync(selectedCard.Id, dialog.FrontText, dialog.BackText, dialog.HintText, dialog.TagsText);
    }

    private async void DeleteSelectedCard_Click(object sender, RoutedEventArgs e)
    {
        var confirmed = MessageBox.Show(
            "Delete selected card?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        await _viewModel.DeleteSelectedCardAsync();
    }

    private async void ImportCards_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ImportCardsAsync();
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LogoutAsync();
    }

    private void ApplyThemePalette(string? themeMode)
    {
        var isDark = string.Equals(themeMode, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDark)
        {
            SetBrushColor("WnBg", "#121826");
            SetBrushColor("WnSurface", "#1A2234");
            SetBrushColor("WnSurfaceSoft", "#222D43");
            SetBrushColor("WnBorder", "#34415B");
            SetBrushColor("WnPanelBorder", "#3C4B69");
            SetBrushColor("WnInk", "#E8EDF9");
            SetBrushColor("WnMuted", "#A8B5CD");
            SetBrushColor("WnPrimary", "#6A97FF");
            SetBrushColor("WnPrimarySoft", "#2A3A60");
            SetBrushColor("WnPrimaryHover", "#83ABFF");
            SetBrushColor("WnPrimaryPressed", "#4F79DB");
            SetBrushColor("WnButtonBorder", "#9AB9FF");
            SetBrushColor("WnInfoBg", "#21345A");
            SetBrushColor("WnInfoBorder", "#37558F");
            SetBrushColor("WnStatusBusy", "#7CD0FF");
            SetBrushColor("WnSuccess", "#45D0A0");
            SetBrushColor("WnPlaceholder", "#7F93BA");
            SetSystemBrushes(
                window: "#1A2234",
                control: "#1A2234",
                controlDark: "#121826",
                controlLight: "#2A3A60",
                text: "#E8EDF9",
                highlight: "#2A3A60");
            return;
        }

        SetBrushColor("WnBg", "#F7F3EC");
        SetBrushColor("WnSurface", "#FFFFFF");
        SetBrushColor("WnSurfaceSoft", "#FFF8EF");
        SetBrushColor("WnBorder", "#E6DFD3");
        SetBrushColor("WnPanelBorder", "#D1D5DB");
        SetBrushColor("WnInk", "#1F2333");
        SetBrushColor("WnMuted", "#5B6275");
        SetBrushColor("WnPrimary", "#1F4ED8");
        SetBrushColor("WnPrimarySoft", "#EDF2FF");
        SetBrushColor("WnPrimaryHover", "#1A43B8");
        SetBrushColor("WnPrimaryPressed", "#173A9C");
        SetBrushColor("WnButtonBorder", "#1639A5");
        SetBrushColor("WnInfoBg", "#EFF6FF");
        SetBrushColor("WnInfoBorder", "#BFDBFE");
        SetBrushColor("WnStatusBusy", "#0EA5E9");
        SetBrushColor("WnSuccess", "#0F766E");
        SetBrushColor("WnPlaceholder", "#94A3B8");
        SetSystemBrushes(
            window: "#FFFFFF",
            control: "#FFFFFF",
            controlDark: "#E6DFD3",
            controlLight: "#EDF2FF",
            text: "#1F2333",
            highlight: "#EDF2FF");
    }

    private void SetBrushColor(string resourceKey, string hexColor)
    {
        var color = (Color)ColorConverter.ConvertFromString(hexColor);
        Resources[resourceKey] = new SolidColorBrush(color);
    }

    private void SetSystemBrushes(
        string window,
        string control,
        string controlDark,
        string controlLight,
        string text,
        string highlight)
    {
        SetSystemBrush(SystemColors.WindowBrushKey, window);
        SetSystemBrush(SystemColors.ControlBrushKey, control);
        SetSystemBrush(SystemColors.ControlDarkBrushKey, controlDark);
        SetSystemBrush(SystemColors.ControlLightBrushKey, controlLight);
        SetSystemBrush(SystemColors.ControlTextBrushKey, text);
        SetSystemBrush(SystemColors.WindowTextBrushKey, text);
        SetSystemBrush(SystemColors.HighlightBrushKey, highlight);
        SetSystemBrush(SystemColors.HighlightTextBrushKey, text);
    }

    private void SetSystemBrush(object resourceKey, string hexColor)
    {
        var color = (Color)ColorConverter.ConvertFromString(hexColor);
        Resources[resourceKey] = new SolidColorBrush(color);
    }
}