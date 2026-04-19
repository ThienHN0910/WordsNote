using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using WordsNote.Desktop.Models;
using WordsNote.Desktop.Services;
using WordsNote.Desktop.Services.Configuration;

namespace WordsNote.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const int LandingTabIndex = 0;
    private const int LearnTabIndex = 1;
    private const int ManageTabIndex = 3;
    private const int SettingsTabIndex = 4;
    private const int LoginTabIndex = 5;
    private const int PrivacyTabIndex = 2;

    private readonly WordsNoteApiClient _apiClient;
    private readonly LocalManageStorageService _localManageStorage;
    private readonly GoogleBrowserAuthService _googleBrowserAuthService;
    private readonly DesktopSettingsStorageService _settingsStorage;

    private string _apiBaseUrl = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "Ready";
    private int _selectedTabIndex;

    private string _googleClientId = string.Empty;
    private string _googleIdToken = string.Empty;
    private string _authToken = string.Empty;
    private bool _isAuthenticated;
    private string _lastSyncMessage = string.Empty;
    private string _themeMode = "light";

    private StudyDeck? _learnSelectedDeck;
    private string _learnMode = "flash";
    private int _learnCurrentIndex;
    private bool _learnShowBack;
    private string _learnTypedAnswer = string.Empty;
    private string _learnFeedback = string.Empty;
    private bool _learnFeedbackIsCorrect;
    private bool _learnAwaitingNext;
    private string _selectedPracticeOption = string.Empty;
    private string _learnPracticeFeedback = string.Empty;
    private bool _learnPracticeFeedbackIsCorrect;
    private bool _learnPracticeAwaitingNext;

    private string _newCollectionTitle = string.Empty;
    private string _newCollectionDescription = string.Empty;
    private string _manageCollectionQuery = string.Empty;
    private string _manageCollectionSort = "recent";
    private StudyDeck? _manageSelectedDeck;
    private bool _isEditingCollection;
    private string _editCollectionTitle = string.Empty;
    private string _editCollectionDescription = string.Empty;

    private string _cardFront = string.Empty;
    private string _cardBack = string.Empty;
    private string _cardHint = string.Empty;
    private string _cardTagsText = string.Empty;

    private string _manageCardQuery = string.Empty;
    private string _manageCardFilter = "all";
    private string _manageCardSort = "dueSoon";
    private StudyCard? _manageSelectedCard;
    private int _manageTotalCards;
    private int _manageDueCards;
    private int _manageMasteredCards;

    private string _importRawText = string.Empty;
    private string _importResult = string.Empty;

    private string _privacyLanguage = "vi";
    private string _privacyTitle = string.Empty;
    private string _privacySubtitle = string.Empty;
    private string _privacyPolicyText = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<StudyDeck> Decks { get; } = [];

    public ObservableCollection<StudyCard> Cards { get; } = [];

    public ObservableCollection<StudyDeck> FilteredManageDecks { get; } = [];

    public ObservableCollection<StudyCard> FilteredManageCards { get; } = [];

    public ObservableCollection<string> LearnPracticeOptions { get; } = [];

    public MainViewModel(
        WordsNoteApiClient apiClient,
        LocalManageStorageService localManageStorage,
        GoogleBrowserAuthService googleBrowserAuthService,
        DesktopSettingsStorageService settingsStorage,
        IOptions<DesktopRuntimeOptions> runtimeOptions)
    {
        _apiClient = apiClient;
        _localManageStorage = localManageStorage;
        _googleBrowserAuthService = googleBrowserAuthService;
        _settingsStorage = settingsStorage;

        var defaults = runtimeOptions.Value;
        _apiBaseUrl = defaults.ApiBaseUrl.Trim();
        _googleClientId = defaults.GoogleClientId?.Trim() ?? string.Empty;
        _themeMode = NormalizeThemeMode(defaults.ThemeMode);
    }

    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => SetProperty(ref _apiBaseUrl, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ManageModeLabel => IsAuthenticated ? "Cloud mode (Google login)" : "Local mode (offline)";

    public bool CanSyncLocalToCloud => IsAuthenticated;

    public string LastSyncMessage
    {
        get => _lastSyncMessage;
        private set => SetProperty(ref _lastSyncMessage, value);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public string GoogleClientId
    {
        get => _googleClientId;
        set => SetProperty(ref _googleClientId, value);
    }

    public string GoogleIdToken
    {
        get => _googleIdToken;
        set => SetProperty(ref _googleIdToken, value);
    }

    public string ThemeMode
    {
        get => _themeMode;
        set => SetProperty(ref _themeMode, NormalizeThemeMode(value));
    }

    public string AuthToken
    {
        get => _authToken;
        private set => SetProperty(ref _authToken, value);
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (!SetProperty(ref _isAuthenticated, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsNotAuthenticated));
            OnPropertyChanged(nameof(ManageModeLabel));
            OnPropertyChanged(nameof(CanSyncLocalToCloud));
        }
    }

    public bool IsNotAuthenticated => !IsAuthenticated;

    public StudyDeck? LearnSelectedDeck
    {
        get => _learnSelectedDeck;
        set
        {
            if (!SetProperty(ref _learnSelectedDeck, value))
            {
                return;
            }

            _learnCurrentIndex = 0;
            ResetLearnInteractionState();
            RefreshLearnComputed();
        }
    }

    public string LearnMode
    {
        get => _learnMode;
        private set
        {
            if (!SetProperty(ref _learnMode, value))
            {
                return;
            }

            ResetLearnInteractionState();
            RefreshLearnComputed();
            OnPropertyChanged(nameof(IsLearnFlashMode));
            OnPropertyChanged(nameof(IsLearnTypingMode));
            OnPropertyChanged(nameof(IsLearnPracticeMode));
        }
    }

    public bool IsLearnFlashMode => LearnMode == "flash";

    public bool IsLearnTypingMode => LearnMode == "learn";

    public bool IsLearnPracticeMode => LearnMode == "practice";

    public int LearnCurrentIndex => _learnCurrentIndex;

    public bool LearnShowBack
    {
        get => _learnShowBack;
        set => SetProperty(ref _learnShowBack, value);
    }

    public string LearnTypedAnswer
    {
        get => _learnTypedAnswer;
        set => SetProperty(ref _learnTypedAnswer, value);
    }

    public string LearnFeedback
    {
        get => _learnFeedback;
        private set => SetProperty(ref _learnFeedback, value);
    }

    public bool LearnFeedbackIsCorrect
    {
        get => _learnFeedbackIsCorrect;
        private set => SetProperty(ref _learnFeedbackIsCorrect, value);
    }

    public bool LearnAwaitingNext
    {
        get => _learnAwaitingNext;
        private set => SetProperty(ref _learnAwaitingNext, value);
    }

    public string SelectedPracticeOption
    {
        get => _selectedPracticeOption;
        set => SetProperty(ref _selectedPracticeOption, value);
    }

    public string LearnPracticeFeedback
    {
        get => _learnPracticeFeedback;
        private set => SetProperty(ref _learnPracticeFeedback, value);
    }

    public bool LearnPracticeFeedbackIsCorrect
    {
        get => _learnPracticeFeedbackIsCorrect;
        private set => SetProperty(ref _learnPracticeFeedbackIsCorrect, value);
    }

    public bool LearnPracticeAwaitingNext
    {
        get => _learnPracticeAwaitingNext;
        private set => SetProperty(ref _learnPracticeAwaitingNext, value);
    }

    public StudyCard? LearnCurrentCard => GetLearnCards().ElementAtOrDefault(_learnCurrentIndex);

    public int LearnCardCount => GetLearnCards().Count;

    public int LearnDisplayIndex => LearnCardCount == 0 ? 0 : _learnCurrentIndex + 1;

    public string NewCollectionTitle
    {
        get => _newCollectionTitle;
        set => SetProperty(ref _newCollectionTitle, value);
    }

    public string NewCollectionDescription
    {
        get => _newCollectionDescription;
        set => SetProperty(ref _newCollectionDescription, value);
    }

    public string ManageCollectionQuery
    {
        get => _manageCollectionQuery;
        set
        {
            if (SetProperty(ref _manageCollectionQuery, value))
            {
                RefreshManageDecks();
            }
        }
    }

    public string ManageCollectionSort
    {
        get => _manageCollectionSort;
        set
        {
            if (SetProperty(ref _manageCollectionSort, value))
            {
                RefreshManageDecks();
            }
        }
    }

    public StudyDeck? ManageSelectedDeck
    {
        get => _manageSelectedDeck;
        set
        {
            if (!SetProperty(ref _manageSelectedDeck, value))
            {
                return;
            }

            CancelCollectionEdit();
            ClearCardForm();
            ManageSelectedCard = null;
            RefreshManageCards();
        }
    }

    public bool IsEditingCollection
    {
        get => _isEditingCollection;
        private set => SetProperty(ref _isEditingCollection, value);
    }

    public string EditCollectionTitle
    {
        get => _editCollectionTitle;
        set => SetProperty(ref _editCollectionTitle, value);
    }

    public string EditCollectionDescription
    {
        get => _editCollectionDescription;
        set => SetProperty(ref _editCollectionDescription, value);
    }

    public string CardFront
    {
        get => _cardFront;
        set => SetProperty(ref _cardFront, value);
    }

    public string CardBack
    {
        get => _cardBack;
        set => SetProperty(ref _cardBack, value);
    }

    public string CardHint
    {
        get => _cardHint;
        set => SetProperty(ref _cardHint, value);
    }

    public string CardTagsText
    {
        get => _cardTagsText;
        set => SetProperty(ref _cardTagsText, value);
    }

    public string ManageCardQuery
    {
        get => _manageCardQuery;
        set
        {
            if (SetProperty(ref _manageCardQuery, value))
            {
                RefreshManageCards();
            }
        }
    }

    public string ManageCardFilter
    {
        get => _manageCardFilter;
        set
        {
            if (SetProperty(ref _manageCardFilter, value))
            {
                RefreshManageCards();
            }
        }
    }

    public string ManageCardSort
    {
        get => _manageCardSort;
        set
        {
            if (SetProperty(ref _manageCardSort, value))
            {
                RefreshManageCards();
            }
        }
    }

    public StudyCard? ManageSelectedCard
    {
        get => _manageSelectedCard;
        set => SetProperty(ref _manageSelectedCard, value);
    }

    public int ManageTotalCards
    {
        get => _manageTotalCards;
        private set => SetProperty(ref _manageTotalCards, value);
    }

    public int ManageDueCards
    {
        get => _manageDueCards;
        private set => SetProperty(ref _manageDueCards, value);
    }

    public int ManageMasteredCards
    {
        get => _manageMasteredCards;
        private set => SetProperty(ref _manageMasteredCards, value);
    }

    public string ImportRawText
    {
        get => _importRawText;
        set => SetProperty(ref _importRawText, value);
    }

    public string ImportResult
    {
        get => _importResult;
        private set => SetProperty(ref _importResult, value);
    }

    public string PrivacyLanguage
    {
        get => _privacyLanguage;
        set
        {
            if (!SetProperty(ref _privacyLanguage, value))
            {
                return;
            }

            UpdatePrivacyContent();
        }
    }

    public string PrivacyTitle
    {
        get => _privacyTitle;
        private set => SetProperty(ref _privacyTitle, value);
    }

    public string PrivacySubtitle
    {
        get => _privacySubtitle;
        private set => SetProperty(ref _privacySubtitle, value);
    }

    public string PrivacyPolicyText
    {
        get => _privacyPolicyText;
        private set => SetProperty(ref _privacyPolicyText, value);
    }

    public async Task InitializeAsync()
    {
        var settings = await _settingsStorage.LoadAsync();
        ApiBaseUrl = settings.ApiBaseUrl;
        if (!string.IsNullOrWhiteSpace(settings.GoogleClientId))
        {
            GoogleClientId = settings.GoogleClientId;
        }

        if (!string.IsNullOrWhiteSpace(settings.AuthToken))
        {
            AuthToken = settings.AuthToken;
            IsAuthenticated = true;
            _apiClient.SetToken(AuthToken);
        }

        ThemeMode = settings.ThemeMode;

        UpdatePrivacyContent();
        await ReloadDataAsync();
    }

    public async Task SaveSettingsAsync()
    {
        await RunBusyAsync(async () =>
        {
            ApplyApiBaseUrl();
            GoogleClientId = GoogleClientId.Trim();

            await PersistCurrentSettingsAsync();

            StatusMessage = "Settings saved.";
        }, "Unable to save desktop settings.");
    }

    public void OpenLandingPage()
    {
        SelectedTabIndex = LandingTabIndex;
    }

    public void OpenLoginPage()
    {
        SelectedTabIndex = LoginTabIndex;
    }

    public void OpenManagePage()
    {
        SelectedTabIndex = ManageTabIndex;
    }

    public void OpenLearnPage()
    {
        SelectedTabIndex = LearnTabIndex;
    }

    public void OpenSettingsPage()
    {
        SelectedTabIndex = SettingsTabIndex;
    }

    public void OpenPrivacyPage()
    {
        SelectedTabIndex = PrivacyTabIndex;
    }

    public void ApplyApiBaseUrl()
    {
        _apiClient.SetBaseUrl(ApiBaseUrl);
        StatusMessage = $"API base URL set to {_apiClient.BaseUrl}";
    }

    public async Task ReloadDataAsync()
    {
        await RunBusyAsync(async () =>
        {
            ApplyApiBaseUrl();
            if (IsAuthenticated)
            {
                _apiClient.SetToken(AuthToken);
                var deckTask = _apiClient.GetCollectionsAsync();
                var cardTask = _apiClient.GetCardsAsync();
                await Task.WhenAll(deckTask, cardTask);

                ReplaceCollection(Decks, deckTask.Result);
                ReplaceCollection(Cards, cardTask.Result);
            }
            else
            {
                _apiClient.SetToken(null);
                var localSnapshot = await _localManageStorage.LoadAsync();
                ReplaceCollection(Decks, localSnapshot.Decks);
                ReplaceCollection(Cards, localSnapshot.Cards);
            }

            EnsureSelectedDecks();
            RefreshManageDecks();
            RefreshManageCards();
            RefreshLearnComputed();
            StatusMessage = IsAuthenticated
                ? $"Loaded {Decks.Count} cloud collection(s), {Cards.Count} cloud card(s)."
                : $"Loaded {Decks.Count} local collection(s), {Cards.Count} local card(s).";
        }, "Failed to load collections/cards.");
    }

    public async Task LoginWithGoogleTokenAsync()
    {
        await RunBusyAsync(async () =>
        {
            ApplyApiBaseUrl();
            var token = await _apiClient.LoginWithGoogleTokenAsync(GoogleIdToken);
            AuthToken = token;
            IsAuthenticated = true;
            _apiClient.SetToken(token);
            GoogleIdToken = string.Empty;
            await PersistCurrentSettingsAsync();
            StatusMessage = "Google token accepted. Signed in successfully.";
            await ReloadDataAsync();
        }, "Google token login failed.");
    }

    public async Task LoginWithGoogleBrowserAsync()
    {
        await RunBusyAsync(async () =>
        {
            var idToken = await _googleBrowserAuthService.AcquireIdTokenAsync(GoogleClientId);
            GoogleIdToken = idToken;
            ApplyApiBaseUrl();
            var token = await _apiClient.LoginWithGoogleTokenAsync(idToken);
            AuthToken = token;
            IsAuthenticated = true;
            _apiClient.SetToken(token);
            await PersistCurrentSettingsAsync();
            StatusMessage = "Google browser login succeeded.";
            await ReloadDataAsync();
        }, "Google browser login failed.");
    }

    public async Task LogoutAsync()
    {
        AuthToken = string.Empty;
        IsAuthenticated = false;
        _apiClient.SetToken(null);
        await PersistCurrentSettingsAsync();
        StatusMessage = "Signed out.";
        await ReloadDataAsync();
    }

    public void SetLearnMode(string mode)
    {
        LearnMode = mode is "learn" or "practice" ? mode : "flash";
    }

    public void SelectLearnPreviousCard()
    {
        MoveLearnIndex(-1);
    }

    public void SelectLearnNextCard()
    {
        MoveLearnIndex(1);
    }

    public void ToggleLearnCardFace()
    {
        LearnShowBack = !LearnShowBack;
    }

    public void CheckLearnTypingAnswer()
    {
        var card = LearnCurrentCard;
        if (card is null)
        {
            return;
        }

        if (LearnAwaitingNext)
        {
            SelectLearnNextCard();
            return;
        }

        var isCorrect = NormalizeForCompare(LearnTypedAnswer) == NormalizeForCompare(card.Back);
        LearnFeedbackIsCorrect = isCorrect;
        LearnAwaitingNext = !isCorrect;
        LearnFeedback = isCorrect
            ? "Correct. You can continue to the next card."
            : $"Not yet. Expected answer: {card.Back}. Press check again to continue.";
    }

    public void SubmitPracticeOption()
    {
        var card = LearnCurrentCard;
        if (card is null || LearnPracticeAwaitingNext)
        {
            return;
        }

        var selected = SelectedPracticeOption;
        if (string.IsNullOrWhiteSpace(selected))
        {
            LearnPracticeFeedback = "Choose an option first.";
            LearnPracticeFeedbackIsCorrect = false;
            return;
        }

        var isCorrect = string.Equals(selected, card.Back, StringComparison.Ordinal);
        LearnPracticeFeedbackIsCorrect = isCorrect;
        LearnPracticeFeedback = isCorrect
            ? "Correct. Nice work."
            : $"Wrong. Correct answer: {card.Back}";
        LearnPracticeAwaitingNext = true;
    }

    public void NextPracticeQuestion()
    {
        SelectLearnNextCard();
    }

    public async Task CreateCollectionAsync()
    {
        var title = NewCollectionTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            StatusMessage = "Collection name is required.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            StudyDeck created;
            if (IsAuthenticated)
            {
                created = await _apiClient.CreateCollectionAsync(title, NewCollectionDescription.Trim());
                await ReloadDataAsync();
            }
            else
            {
                created = CreateLocalCollection(title, NewCollectionDescription.Trim());
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            NewCollectionTitle = string.Empty;
            NewCollectionDescription = string.Empty;
            ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == created.Id) ?? Decks.FirstOrDefault();
            StatusMessage = IsAuthenticated ? "Collection created in cloud." : "Collection created locally.";
        }, "Unable to create collection.");
    }

    public void StartCollectionEdit()
    {
        if (ManageSelectedDeck is null)
        {
            return;
        }

        IsEditingCollection = true;
        EditCollectionTitle = ManageSelectedDeck.Title;
        EditCollectionDescription = ManageSelectedDeck.Description;
    }

    public void CancelCollectionEdit()
    {
        IsEditingCollection = false;
        EditCollectionTitle = string.Empty;
        EditCollectionDescription = string.Empty;
    }

    public async Task SaveCollectionEditAsync()
    {
        if (ManageSelectedDeck is null)
        {
            return;
        }

        var title = EditCollectionTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            StatusMessage = "Collection name is required.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            if (IsAuthenticated)
            {
                await _apiClient.UpdateCollectionAsync(ManageSelectedDeck.Id, title, EditCollectionDescription.Trim());
                await ReloadDataAsync();
            }
            else
            {
                UpdateLocalCollection(ManageSelectedDeck.Id, title, EditCollectionDescription.Trim());
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == ManageSelectedDeck.Id) ?? Decks.FirstOrDefault();
            CancelCollectionEdit();
            StatusMessage = IsAuthenticated ? "Collection updated in cloud." : "Collection updated locally.";
        }, "Unable to update collection.");
    }

    public async Task DeleteSelectedCollectionAsync()
    {
        if (ManageSelectedDeck is null)
        {
            return;
        }

        var deletingId = ManageSelectedDeck.Id;
        await RunBusyAsync(async () =>
        {
            if (IsAuthenticated)
            {
                await _apiClient.DeleteCollectionAsync(deletingId);
                await ReloadDataAsync();
            }
            else
            {
                DeleteLocalCollection(deletingId);
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            ManageSelectedDeck = Decks.FirstOrDefault();
            StatusMessage = IsAuthenticated ? "Collection deleted from cloud." : "Collection deleted locally.";
        }, "Unable to delete collection.");
    }

    public async Task SaveCardAsync()
    {
        if (ManageSelectedDeck is null)
        {
            StatusMessage = "Select a collection first.";
            return;
        }

        var front = CardFront.Trim();
        var back = CardBack.Trim();
        if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
        {
            StatusMessage = "Front and back are required.";
            return;
        }

        var tags = ParseTags(CardTagsText);

        await RunBusyAsync(async () =>
        {
            if (IsAuthenticated)
            {
                await _apiClient.CreateCardAsync(ManageSelectedDeck.Id, front, back, CardHint.Trim(), tags);
                StatusMessage = "Card created in cloud.";

                var selectedDeckId = ManageSelectedDeck.Id;
                await ReloadDataAsync();
                ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == selectedDeckId) ?? Decks.FirstOrDefault();
            }
            else
            {
                CreateLocalCard(ManageSelectedDeck.Id, front, back, CardHint.Trim(), tags);
                StatusMessage = "Card created locally.";

                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            ClearCardForm();
        }, "Unable to save card.");
    }

    public async Task UpdateCardAsync(string cardId, string frontInput, string backInput, string hintInput, string tagsText)
    {
        if (ManageSelectedDeck is null)
        {
            StatusMessage = "Select a collection first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(cardId))
        {
            StatusMessage = "Select a card first.";
            return;
        }

        var front = frontInput.Trim();
        var back = backInput.Trim();
        if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
        {
            StatusMessage = "Front and back are required.";
            return;
        }

        var hint = hintInput.Trim();
        var tags = ParseTags(tagsText);

        await RunBusyAsync(async () =>
        {
            if (IsAuthenticated)
            {
                await _apiClient.UpdateCardAsync(cardId, ManageSelectedDeck.Id, front, back, hint, tags);
                var selectedDeckId = ManageSelectedDeck.Id;
                await ReloadDataAsync();
                ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == selectedDeckId) ?? Decks.FirstOrDefault();
                StatusMessage = "Card updated in cloud.";
            }
            else
            {
                UpdateLocalCard(cardId, ManageSelectedDeck.Id, front, back, hint, tags);
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
                StatusMessage = "Card updated locally.";
            }

            ManageSelectedCard = FilteredManageCards.FirstOrDefault(card => card.Id == cardId) ?? FilteredManageCards.FirstOrDefault();
        }, "Unable to update card.");
    }

    public void ClearCardForm()
    {
        CardFront = string.Empty;
        CardBack = string.Empty;
        CardHint = string.Empty;
        CardTagsText = string.Empty;
    }

    public async Task DeleteSelectedCardAsync()
    {
        if (ManageSelectedCard is null)
        {
            return;
        }

        var deletingCardId = ManageSelectedCard.Id;
        var selectedDeckId = ManageSelectedDeck?.Id;

        await RunBusyAsync(async () =>
        {
            if (IsAuthenticated)
            {
                await _apiClient.DeleteCardAsync(deletingCardId);
                await ReloadDataAsync();
            }
            else
            {
                DeleteLocalCard(deletingCardId);
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == selectedDeckId) ?? Decks.FirstOrDefault();
            ManageSelectedCard = FilteredManageCards.FirstOrDefault();

            StatusMessage = IsAuthenticated ? "Card deleted from cloud." : "Card deleted locally.";
        }, "Unable to delete card.");
    }

    public async Task ImportCardsAsync()
    {
        if (ManageSelectedDeck is null)
        {
            StatusMessage = "Select a collection first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ImportRawText))
        {
            StatusMessage = "Enter import lines first.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            ImportCardsResult result;
            if (IsAuthenticated)
            {
                result = await _apiClient.ImportCardsAsync(ManageSelectedDeck.Id, ImportRawText);
                var selectedDeckId = ManageSelectedDeck.Id;
                await ReloadDataAsync();
                ManageSelectedDeck = Decks.FirstOrDefault(deck => deck.Id == selectedDeckId) ?? Decks.FirstOrDefault();
            }
            else
            {
                result = ImportLocalCards(ManageSelectedDeck.Id, ImportRawText);
                await SaveLocalSnapshotAsync();
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }

            ImportResult = $"Imported {result.Imported} card(s), skipped {result.Skipped} line(s).";
            if (result.Imported > 0)
            {
                ImportRawText = string.Empty;
            }

            StatusMessage = IsAuthenticated ? "Import finished to cloud." : "Import finished locally.";
        }, "Unable to import cards.");
    }

    public async Task SyncLocalToCloudAsync()
    {
        if (!IsAuthenticated)
        {
            StatusMessage = "Please login with Google first to sync local data to cloud.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var localSnapshot = await _localManageStorage.LoadAsync();
            if (localSnapshot.Decks.Count == 0)
            {
                LastSyncMessage = "No local data to sync.";
                StatusMessage = LastSyncMessage;
                return;
            }

            var syncState = await _localManageStorage.LoadSyncStateAsync();

            var cloudDecks = await _apiClient.GetCollectionsAsync();
            var cloudCards = await _apiClient.GetCardsAsync();

            var cloudDeckById = cloudDecks.ToDictionary(deck => deck.Id, StringComparer.OrdinalIgnoreCase);
            var cloudDeckByMatchKey = cloudDecks
                .GroupBy(deck => SyncSnapshot.CreateDeckMatchKey(deck.Title), StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var deckMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var localDeck in localSnapshot.Decks)
            {
                syncState.DeckIdMap.TryGetValue(localDeck.Id, out var mappedCloudDeckId);
                var existing = !string.IsNullOrWhiteSpace(mappedCloudDeckId) && cloudDeckById.TryGetValue(mappedCloudDeckId, out var mappedDeck)
                    ? mappedDeck
                    : cloudDeckByMatchKey.GetValueOrDefault(SyncSnapshot.CreateDeckMatchKey(localDeck.Title));

                if (existing is null)
                {
                    existing = await _apiClient.CreateCollectionAsync(localDeck.Title, localDeck.Description);
                    cloudDecks.Add(existing);
                    cloudDeckById[existing.Id] = existing;
                    cloudDeckByMatchKey[SyncSnapshot.CreateDeckMatchKey(existing.Title)] = existing;
                }
                else
                {
                    var localFingerprint = SyncSnapshot.CreateDeckFingerprint(localDeck);
                    var cloudFingerprint = SyncSnapshot.CreateDeckFingerprint(existing);
                    if (!string.Equals(localFingerprint, cloudFingerprint, StringComparison.Ordinal))
                    {
                        existing = await _apiClient.UpdateCollectionAsync(existing.Id, localDeck.Title, localDeck.Description);
                        cloudDeckById[existing.Id] = existing;
                        cloudDeckByMatchKey[SyncSnapshot.CreateDeckMatchKey(existing.Title)] = existing;
                    }
                }

                deckMap[localDeck.Id] = existing.Id;
            }

            var cloudCardById = cloudCards.ToDictionary(card => card.Id, StringComparer.OrdinalIgnoreCase);
            var cloudCardByMatchKey = cloudCards
                .GroupBy(card => SyncSnapshot.CreateCardMatchKey(ResolveCollectionId(card), card.Front, card.Back), StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var uploadedCards = 0;
            var updatedCards = 0;
            var skippedCards = 0;
            var cardMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var localCard in localSnapshot.Cards)
            {
                if (!deckMap.TryGetValue(ResolveCollectionId(localCard), out var cloudDeckId))
                {
                    continue;
                }

                syncState.CardIdMap.TryGetValue(localCard.Id, out var mappedCloudCardId);
                var existingCloudCard = !string.IsNullOrWhiteSpace(mappedCloudCardId) && cloudCardById.TryGetValue(mappedCloudCardId, out var mappedCard)
                    ? mappedCard
                    : cloudCardByMatchKey.GetValueOrDefault(SyncSnapshot.CreateCardMatchKey(cloudDeckId, localCard.Front, localCard.Back));

                if (existingCloudCard is null)
                {
                    var created = await _apiClient.CreateCardAsync(
                        cloudDeckId,
                        localCard.Front,
                        localCard.Back,
                        localCard.Hint ?? string.Empty,
                        localCard.Tags);
                    cloudCards.Add(created);
                    cloudCardById[created.Id] = created;
                    cloudCardByMatchKey[SyncSnapshot.CreateCardMatchKey(ResolveCollectionId(created), created.Front, created.Back)] = created;
                    cardMap[localCard.Id] = created.Id;
                    uploadedCards += 1;
                }
                else
                {
                    var cloudFingerprint = SyncSnapshot.CreateCardFingerprint(existingCloudCard, ResolveCollectionId(existingCloudCard));
                    var localFingerprint = SyncSnapshot.CreateCardFingerprint(localCard, cloudDeckId);
                    if (string.Equals(localFingerprint, cloudFingerprint, StringComparison.Ordinal))
                    {
                        cardMap[localCard.Id] = existingCloudCard.Id;
                        skippedCards += 1;
                        continue;
                    }

                    var updated = await _apiClient.UpdateCardAsync(
                        existingCloudCard.Id,
                        cloudDeckId,
                        localCard.Front,
                        localCard.Back,
                        localCard.Hint ?? string.Empty,
                        localCard.Tags);
                    cloudCardById[updated.Id] = updated;
                    cloudCardByMatchKey[SyncSnapshot.CreateCardMatchKey(ResolveCollectionId(updated), updated.Front, updated.Back)] = updated;
                    cardMap[localCard.Id] = updated.Id;
                    updatedCards += 1;
                }
            }

            await _localManageStorage.SaveSyncStateAsync(new LocalCloudSyncState
            {
                DeckIdMap = deckMap,
                CardIdMap = cardMap,
            });

            LastSyncMessage = $"Synced {localSnapshot.Decks.Count} local collection(s): uploaded {uploadedCards}, updated {updatedCards}, skipped {skippedCards} unchanged card(s).";
            StatusMessage = LastSyncMessage;
            await ReloadDataAsync();
        }, "Unable to sync local data to cloud.");
    }

    public async Task SyncCloudToLocalAsync()
    {
        await RunBusyAsync(async () =>
        {
            ApplyApiBaseUrl();
            if (IsAuthenticated)
            {
                _apiClient.SetToken(AuthToken);
            }
            else
            {
                _apiClient.SetToken(null);
            }

            var deckTask = _apiClient.GetCollectionsAsync();
            var cardTask = _apiClient.GetCardsAsync();
            await Task.WhenAll(deckTask, cardTask);

            var cloudDecks = deckTask.Result;
            var cloudCards = cardTask.Result;
            await _localManageStorage.SaveAsync(cloudDecks, cloudCards);

            LastSyncMessage = $"Synced cloud -> local with {cloudDecks.Count} collection(s) and {cloudCards.Count} card(s).";
            StatusMessage = LastSyncMessage;

            if (!IsAuthenticated)
            {
                ReplaceCollection(Decks, cloudDecks);
                ReplaceCollection(Cards, cloudCards);
                EnsureSelectedDecks();
                RefreshManageDecks();
                RefreshManageCards();
                RefreshLearnComputed();
            }
        }, "Unable to sync cloud data to local.");
    }

    public void SetPrivacyLanguage(string language)
    {
        PrivacyLanguage = language == "en" ? "en" : "vi";
    }

    private StudyDeck CreateLocalCollection(string title, string description)
    {
        var now = DateTime.UtcNow.ToString("O");
        var deck = new StudyDeck
        {
            Id = $"local-deck-{Guid.NewGuid():N}",
            Title = title,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Decks.Insert(0, deck);
        return deck;
    }

    private void UpdateLocalCollection(string deckId, string title, string description)
    {
        var deck = Decks.FirstOrDefault(item => item.Id == deckId);
        if (deck is null)
        {
            throw new InvalidOperationException("Collection not found in local storage.");
        }

        deck.Title = title;
        deck.Description = description;
        deck.UpdatedAt = DateTime.UtcNow.ToString("O");
    }

    private void DeleteLocalCollection(string deckId)
    {
        var deck = Decks.FirstOrDefault(item => item.Id == deckId);
        if (deck is not null)
        {
            Decks.Remove(deck);
        }

        var cardsToDelete = Cards.Where(card => ResolveCollectionId(card) == deckId).ToList();
        foreach (var card in cardsToDelete)
        {
            Cards.Remove(card);
        }
    }

    private void CreateLocalCard(string deckId, string front, string back, string hint, List<string> tags)
    {
        var now = DateTime.UtcNow.ToString("O");
        Cards.Add(new StudyCard
        {
            Id = $"local-card-{Guid.NewGuid():N}",
            CollectionId = deckId,
            DeckId = deckId,
            Front = front,
            Back = back,
            Hint = string.IsNullOrWhiteSpace(hint) ? null : hint,
            Tags = tags,
            DueAt = now,
            LastReviewedAt = null,
            Streak = 0,
        });

        TouchLocalDeck(deckId);
    }

    private void UpdateLocalCard(string cardId, string deckId, string front, string back, string hint, List<string> tags)
    {
        var card = Cards.FirstOrDefault(item => item.Id == cardId);
        if (card is null)
        {
            throw new InvalidOperationException("Card not found in local storage.");
        }

        card.CollectionId = deckId;
        card.DeckId = deckId;
        card.Front = front;
        card.Back = back;
        card.Hint = string.IsNullOrWhiteSpace(hint) ? null : hint;
        card.Tags = tags;
        TouchLocalDeck(deckId);
    }

    private void DeleteLocalCard(string cardId)
    {
        var card = Cards.FirstOrDefault(item => item.Id == cardId);
        if (card is null)
        {
            return;
        }

        var deckId = ResolveCollectionId(card);
        Cards.Remove(card);
        TouchLocalDeck(deckId);
    }

    private ImportCardsResult ImportLocalCards(string deckId, string rawText)
    {
        var lines = rawText.Split(["\r\n", "\n"], StringSplitOptions.None);
        var imported = 0;
        var skipped = 0;

        foreach (var line in lines)
        {
            var normalized = line.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            var separatorIndex = normalized.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex == normalized.Length - 1)
            {
                skipped += 1;
                continue;
            }

            var front = normalized[..separatorIndex].Trim();
            var back = normalized[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
            {
                skipped += 1;
                continue;
            }

            CreateLocalCard(deckId, front, back, string.Empty, []);
            imported += 1;
        }

        return new ImportCardsResult
        {
            Imported = imported,
            Skipped = skipped,
        };
    }

    private void TouchLocalDeck(string deckId)
    {
        var deck = Decks.FirstOrDefault(item => item.Id == deckId);
        if (deck is not null)
        {
            deck.UpdatedAt = DateTime.UtcNow.ToString("O");
        }
    }

    private async Task SaveLocalSnapshotAsync()
    {
        await _localManageStorage.SaveAsync(Decks, Cards);
    }

    private Task PersistCurrentSettingsAsync()
    {
        return _settingsStorage.SaveAsync(new DesktopAppSettings
        {
            ApiBaseUrl = ApiBaseUrl,
            GoogleClientId = GoogleClientId,
            AuthToken = AuthToken,
            ThemeMode = ThemeMode,
        });
    }

    private bool EnsureAuthenticated()
    {
        if (IsAuthenticated)
        {
            return true;
        }

        StatusMessage = "Please sign in to use cloud manage features.";
        SelectedTabIndex = LoginTabIndex;
        return false;
    }

    private async Task RunBusyAsync(Func<Task> action, string fallbackError)
    {
        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = BuildErrorMessage(fallbackError, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildErrorMessage(string fallbackError, Exception ex)
    {
        var detail = ex.Message?.Trim();
        if (string.IsNullOrWhiteSpace(detail))
        {
            return fallbackError;
        }

        return $"{fallbackError} {detail}";
    }

    private void EnsureSelectedDecks()
    {
        if (LearnSelectedDeck is null || !Decks.Any(deck => deck.Id == LearnSelectedDeck.Id))
        {
            LearnSelectedDeck = Decks.FirstOrDefault();
        }

        if (ManageSelectedDeck is null || !Decks.Any(deck => deck.Id == ManageSelectedDeck.Id))
        {
            ManageSelectedDeck = Decks.FirstOrDefault();
        }
    }

    private void MoveLearnIndex(int delta)
    {
        var cards = GetLearnCards();
        if (cards.Count == 0)
        {
            _learnCurrentIndex = 0;
            RefreshLearnComputed();
            return;
        }

        _learnCurrentIndex = (_learnCurrentIndex + delta) % cards.Count;
        if (_learnCurrentIndex < 0)
        {
            _learnCurrentIndex += cards.Count;
        }

        ResetLearnInteractionState();
        RefreshLearnComputed();
    }

    private void ResetLearnInteractionState()
    {
        LearnShowBack = false;
        LearnTypedAnswer = string.Empty;
        LearnFeedback = string.Empty;
        LearnFeedbackIsCorrect = false;
        LearnAwaitingNext = false;
        SelectedPracticeOption = string.Empty;
        LearnPracticeFeedback = string.Empty;
        LearnPracticeFeedbackIsCorrect = false;
        LearnPracticeAwaitingNext = false;
    }

    private void RefreshLearnComputed()
    {
        BuildPracticeOptions();
        OnPropertyChanged(nameof(LearnCurrentCard));
        OnPropertyChanged(nameof(LearnCurrentIndex));
        OnPropertyChanged(nameof(LearnCardCount));
        OnPropertyChanged(nameof(LearnDisplayIndex));
    }

    private void BuildPracticeOptions()
    {
        var card = LearnCurrentCard;
        if (card is null)
        {
            ReplaceCollection(LearnPracticeOptions, []);
            return;
        }

        var distractors = GetLearnCards()
            .Where(item => item.Id != card.Id)
            .Select(item => item.Back)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToList();

        var pool = new List<string> { card.Back };
        pool.AddRange(distractors);
        var shuffled = pool.OrderBy(_ => Random.Shared.Next()).Distinct(StringComparer.Ordinal).ToList();
        ReplaceCollection(LearnPracticeOptions, shuffled);
    }

    private List<StudyCard> GetLearnCards()
    {
        var deckId = LearnSelectedDeck?.Id;
        if (string.IsNullOrWhiteSpace(deckId))
        {
            return [];
        }

        return GetCardsByDeckId(deckId);
    }

    private void RefreshManageDecks()
    {
        var query = ManageCollectionQuery.Trim().ToLowerInvariant();
        var filtered = Decks.Where(deck =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return $"{deck.Title} {deck.Description}".ToLowerInvariant().Contains(query, StringComparison.Ordinal);
        });

        filtered = ManageCollectionSort switch
        {
            "title" => filtered.OrderBy(deck => deck.Title, StringComparer.OrdinalIgnoreCase),
            "cards" => filtered.OrderByDescending(deck => GetCardsByDeckId(deck.Id).Count),
            _ => filtered.OrderByDescending(deck => ParseIsoDate(deck.UpdatedAt)),
        };

        ReplaceCollection(FilteredManageDecks, filtered);

        if (ManageSelectedDeck is null || !FilteredManageDecks.Any(deck => deck.Id == ManageSelectedDeck.Id))
        {
            ManageSelectedDeck = FilteredManageDecks.FirstOrDefault();
        }
    }

    private void RefreshManageCards()
    {
        if (ManageSelectedDeck is null)
        {
            ReplaceCollection(FilteredManageCards, []);
            ManageTotalCards = 0;
            ManageDueCards = 0;
            ManageMasteredCards = 0;
            return;
        }

        var cards = GetCardsByDeckId(ManageSelectedDeck.Id);
        var now = DateTime.UtcNow;

        ManageTotalCards = cards.Count;
        ManageDueCards = cards.Count(card => ParseIsoDate(card.DueAt) <= now);
        ManageMasteredCards = cards.Count(card => card.Streak >= 5);

        var query = ManageCardQuery.Trim().ToLowerInvariant();
        var filtered = cards.Where(card =>
        {
            var haystack = $"{card.Front} {card.Back} {card.Hint} {string.Join(' ', card.Tags)}".ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(query) && !haystack.Contains(query, StringComparison.Ordinal))
            {
                return false;
            }

            return ManageCardFilter switch
            {
                "due" => ParseIsoDate(card.DueAt) <= now,
                "mastered" => card.Streak >= 5,
                "new" => card.Streak == 0,
                _ => true,
            };
        });

        filtered = ManageCardSort switch
        {
            "frontAZ" => filtered.OrderBy(card => card.Front, StringComparer.OrdinalIgnoreCase),
            "recentReview" => filtered.OrderByDescending(card => ParseIsoDate(card.LastReviewedAt)),
            "streakDesc" => filtered.OrderByDescending(card => card.Streak),
            _ => filtered.OrderBy(card => ParseIsoDate(card.DueAt)),
        };

        ReplaceCollection(FilteredManageCards, filtered);

        if (ManageSelectedCard is null || !FilteredManageCards.Any(card => card.Id == ManageSelectedCard.Id))
        {
            ManageSelectedCard = FilteredManageCards.FirstOrDefault();
        }
    }

    private List<StudyCard> GetCardsByDeckId(string deckId)
    {
        return Cards.Where(card => ResolveCollectionId(card) == deckId).ToList();
    }

    private static string ResolveCollectionId(StudyCard card)
    {
        if (!string.IsNullOrWhiteSpace(card.CollectionId))
        {
            return card.CollectionId;
        }

        return card.DeckId ?? string.Empty;
    }

    private static DateTime ParseIsoDate(string? raw)
    {
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return DateTime.MinValue;
    }

    private static List<string> ParseTags(string tagsText)
    {
        return tagsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeForCompare(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var lower = builder.ToString().ToLowerInvariant().Normalize(NormalizationForm.FormC);
        return Regex.Replace(lower, "\\s+", " ").Trim();
    }

    private void UpdatePrivacyContent()
    {
        if (PrivacyLanguage == "en")
        {
            PrivacyTitle = "Privacy Policy";
            PrivacySubtitle = "How WordsNote stores and protects data across web, extension, and desktop app.";
            PrivacyPolicyText = string.Join(Environment.NewLine + Environment.NewLine,
            [
                "1. Scope",
                "Applies to WordsNote web app, browser extension, and this WPF desktop client.",
                "",
                "2. Data We Collect",
                "Manage mode can use authenticated API calls; learning data includes collections/cards you create.",
                "Desktop app can persist auth token in local desktop settings to restore login state between app restarts.",
                "",
                "3. Usage",
                "Data is used for Flashcards, Learn, Practice, and manage workflows.",
                "",
                "4. Storage & Sync",
                "Desktop app reads/writes against backend APIs. Backend storage behavior is controlled by server deployment.",
                "",
                "5. Security",
                "Use HTTPS backend endpoints in production, and do not share JWT tokens.",
                "",
                "6. Contact",
                "Contact your deployment administrator for privacy requests."
            ]);
            return;
        }

        PrivacyTitle = "Chinh sach bao mat";
        PrivacySubtitle = "Mo ta cach WordsNote luu tru va bao ve du lieu tren web, extension va desktop app WPF.";
        PrivacyPolicyText = string.Join(Environment.NewLine + Environment.NewLine,
        [
            "1. Pham vi",
            "Ap dung cho web app, browser extension va ung dung desktop WPF WordsNote.",
            "",
            "2. Du lieu thu thap",
            "Che do Manage su dung API co xac thuc; du lieu hoc gom collections/cards ban tao.",
            "Desktop app co the luu auth token trong settings local de khoi phuc dang nhap sau khi mo lai ung dung.",
            "",
            "3. Muc dich su dung",
            "Du lieu duoc dung cho Flashcards, Learn, Practice va quy trinh quan ly hoc.",
            "",
            "4. Luu tru va dong bo",
            "Desktop app doc/ghi qua backend API. Cach luu tru thuc te phu thuoc moi truong server.",
            "",
            "5. Bao mat",
            "Nen dung backend HTTPS trong production va khong chia se JWT token.",
            "",
            "6. Lien he",
            "Lien he quan tri vien he thong trien khai de gui yeu cau ve quyen rieng tu."
        ]);
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string NormalizeThemeMode(string? value)
    {
        return string.Equals(value?.Trim(), "dark", StringComparison.OrdinalIgnoreCase)
            ? "dark"
            : "light";
    }
}
