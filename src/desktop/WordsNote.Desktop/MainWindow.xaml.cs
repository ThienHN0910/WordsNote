using System.Windows;
using WordsNote.Desktop.ViewModels;

namespace WordsNote.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void ReloadData_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ReloadDataAsync();
    }

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveSettingsAsync();
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

    private void EditSelectedCard_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StartEditSelectedCard();
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
}