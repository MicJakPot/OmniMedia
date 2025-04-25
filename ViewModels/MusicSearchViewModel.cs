using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using iTunesSearch.Library;
using OmniMedia.Models;
using OmniMedia.Database;
using System.Reactive;
using Avalonia.Threading; // Potrzebne do Dispatcher

// DODANE USINGI DLA KOMUNIKATÓW
using Avalonia.Controls; // Potrzebne jeśli będziesz używać okna dialogowego
using Avalonia.Controls.ApplicationLifetimes; // Potrzebne do dostępu do głównego okna


namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku wyszukiwania muzyki
    public class MusicSearchViewModel : ViewModelBase
    {
        // Właściwość powiązana z polem tekstowym wyszukiwania
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        // Kolekcja wyników wyszukiwania albumów (ViewModele elementów listy/kafelków)
        private ObservableCollection<SearchAlbumItemViewModel> _searchResults = new ObservableCollection<SearchAlbumItemViewModel>();
        public ObservableCollection<SearchAlbumItemViewModel> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        // Właściwość wskazująca, czy operacja wyszukiwania jest w toku (dla feedbacku w UI)
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        // DODANA KOMENDA: Komenda do dodawania albumu do kolekcji
        // Ta komenda jest definiowana TUTAJ (w ViewModelu nadrzędnym)
        // i będzie wywoływana przez ViewModel elementu listy (SearchAlbumItemViewModel)
        // Właściwość musi być publiczna, aby była dostępna z innych części klasy i potencjalnie z XAML
        public ReactiveCommand<SearchAlbumItemViewModel, Unit> AddAlbumToCollectionCommand { get; }


        private CancellationTokenSource? _cancellationTokenSource;
        private static readonly iTunesSearchManager s_SearchManager = new();


        // Konstruktor
        public MusicSearchViewModel()
        {
            // Inicjalizacja komendy "Dodaj do kolekcji"
            // Komenda będzie aktywna tylko, gdy IsBusy jest false
            // CreateFromTask tworzy komendę, która zwraca Task<Unit>
            AddAlbumToCollectionCommand = ReactiveCommand.CreateFromTask<SearchAlbumItemViewModel, Unit>(
                AddAlbumToCollection, // Metoda do wykonania (zaimplementowana poniżej)
                this.WhenAnyValue(x => x.IsBusy).Select(isBusy => !isBusy) // Warunek aktywności: nie zajęty
            );


            // Reagowanie na zmiany w SearchText z opóźnieniem (throttling)
            this.WhenAnyValue(x => x.SearchText)
                // Zastosuj opóźnienie 400ms - wyszukiwanie uruchomi się 400ms po ostatniej zmianie tekstu
                .Throttle(TimeSpan.FromMilliseconds(400))
                // Upewnij się, że dalsze operacje (w tym DoSearch) wykonają się w wątku UI
                .ObserveOn(RxApp.MainThreadScheduler)
                // Subskrybuj zmiany i wywołaj metodę wyszukiwania
                .Subscribe(async searchTerm => await DoSearch(searchTerm));
        }


        // Metoda wykonująca wyszukiwanie albumów
        private async Task DoSearch(string? searchTerm)
        {
            // Anuluj poprzednie opóźnione zapytanie, jeśli istnieje
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            IsBusy = true;
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                IsBusy = false;
                return;
            }

            try
            {
                Debug.WriteLine($"[MusicSearchViewModel] Rozpocynam wyszukiwanie albumów dla: '{searchTerm}'");

                var queryResult = await s_SearchManager.GetAlbumsAsync(searchTerm).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("[MusicSearchViewModel] Wyszukiwanie anulowane.");
                    IsBusy = false;
                    return;
                }

                if (queryResult?.Albums != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var album in queryResult.Albums)
                        {
                            if (!string.IsNullOrEmpty(album.CollectionName) && !string.IsNullOrEmpty(album.ArtistName))
                            {
                                var musicAlbumData = new MusicAlbum
                                {
                                    iTunesId = (int)album.CollectionId, // Rzutowanie na int
                                    Title = album.CollectionName,
                                    Artist = album.ArtistName,
                                    CoverUrl = album.ArtworkUrl100?.Replace("100x100bb", "600x600bb"), // Zmieniamy URL na większą okładkę
                                    Genre = album.PrimaryGenreName,
                                    ReleasedDate = DateTime.TryParse(album.ReleaseDate, out DateTime parsedDate) ? (DateTime?)parsedDate : null, // Parsowanie daty
                                    CollectionPrice = album.CollectionPrice
                                };

                                // Tworzymy ViewModel elementu listy i PRZEKAZUJEMY MU KOMENDĘ DODAWANIA
                                // Ta linia wymaga, aby AddAlbumToCollectionCommand był zadeklarowany i zainicjalizowany w tej klasie
                                var albumVm = new SearchAlbumItemViewModel(musicAlbumData, AddAlbumToCollectionCommand);

                                // TODO: Subskrybuj komendę "Dodaj do kolekcji" z tego ViewModelu elementu (TA LINIJKA JEST ZBĘDNA PRZY AKTUALNYM PODEJŚCIU)
                                // albumVm.AddToCollectionCommand.Subscribe(async _ => await AddAlbumToCollection(albumVm));

                                SearchResults.Add(albumVm); // Dodaj ViewModel elementu do kolekcji wyników
                            }
                            else
                            {
                                Debug.WriteLine($"[MusicSearchViewModel] Pominięto wynik wyszukiwania (brak tytułu/artysty): {album?.ToString() ?? "null"}");
                            }
                        }
                        Debug.WriteLine($"[MusicSearchViewModel] Dodano {SearchResults.Count} wyników do listy.");
                    }, DispatcherPriority.Background);
                }
                else
                {
                    Debug.WriteLine("[MusicSearchViewModel] Wyszukiwanie zwróciło pusty lub niepoprawny wynik.");
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("[MusicSearchViewModel] Wyszukiwanie anulowane przez użytkownika.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MusicSearchViewModel] Błąd podczas wyszukiwania albumów: {ex.Message}");
                // TODO: Poinformuj użytkownika o błędzie w UI (np. Toast notification, okno dialogowe)
            }
            finally
            {
                IsBusy = false;
            }
        }

        // IMPLEMENTACJA METODY DO DODAWANIA ALBUMU DO KOLEKCJI
        // Ta metoda jest wywoływana, gdy komenda AddAlbumToCollectionCommand zostanie wykonana (przez ViewModel elementu listy)
        // Przyjmuje SearchAlbumItemViewModel jako parametr (ten, który wywołał komendę)
        private async Task<Unit> AddAlbumToCollection(SearchAlbumItemViewModel albumVm) // Zmieniono typ zwracany na Task<Unit>
        {
            if (albumVm?.AlbumData == null) return Unit.Default;


            Debug.WriteLine($"[MusicSearchViewModel] Próba dodania do kolekcji albumu: {albumVm.AlbumData.Title} (iTunes ID: {albumVm.AlbumData.iTunesId})");

            try
            {
                // TODO: Opcjonalnie: Sprawdź, czy albumu już nie ma w bazie po iTunesId, aby uniknąć duplikatów
                // Wymaga dodania metody GetMusicAlbumByiTunesIdAsync(int iTunesId) w AppDatabase
                // var existingAlbum = await App.Database.GetMusicAlbumByiTunesIdAsync(albumVm.AlbumData.iTunesId);
                // if (existingAlbum != null)
                // {
                //     Debug.WriteLine($"[MusicSearchViewModel] Album '{albumVm.AlbumData.Title}' już istnieje w kolekcji.");
                //      await ShowMessage("Album już w kolekcji", $"Album '{albumVm.AlbumData.Title}' artysty '{albumVm.AlbumData.Artist}' już znajduje się w Twojej kolekcji.");
                //     return Unit.Default; // Zwróć Unit.Default
                // }


                // Zapisz obiekt MusicAlbum do bazy danych
                // App.Database jest statyczne, więc można go użyć bezpośrednio
                int result = await App.Database.SaveMusicAlbumAsync(albumVm.AlbumData);

                Debug.WriteLine($"[MusicSearchViewModel] Zapisano album do bazy danych. Rezultat: {result}");

                if (result > 0)
                {
                    Debug.WriteLine($"[MusicSearchViewModel] Album '{albumVm.AlbumData.Title}' pomyślnie dodany do bazy.");
                    // TODO: Poinformuj użytkownika o sukcesie (np. Toast Notification lub okno dialogowe)
                    await ShowMessage("Album dodany", $"Album '{albumVm.AlbumData.Title}' artysty '{albumVm.AlbumData.Artist}' został pomyślnie dodany do kolekcji.");

                    // TODO: Opcjonalnie, usuń album z listy wyników wyszukiwania po dodaniu (w wątku UI!)
                    // Aby usunąć z listy w UI, musisz to zrobić w wątku UI
                    // await Dispatcher.UIThread.InvokeAsync(() => SearchResults.Remove(albumVm));

                }
                else
                {
                    Debug.WriteLine($"[MusicSearchViewModel] Nie udało się dodać albumu '{albumVm.AlbumData.Title}' do bazy danych.");
                    // TODO: Poinformuj użytkownika o błędzie
                    await ShowMessage("Błąd", $"Nie udało się dodać albumu '{albumVm.AlbumData.Title}' do kolekcji.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MusicSearchViewModel] Błąd podczas dodawania albumu do bazy: {ex.Message}");
                // TODO: Poinformuj użytkownika o błędzie
                await ShowMessage("Błąd", $"Wystąpił błąd podczas dodawania albumu '{albumVm.AlbumData.Title}' do kolekcji: {ex.Message}");
            }
            return Unit.Default; // Zwróć Unit.Default na końcu metody Task<Unit>
        }

        // --- Metoda pomocnicza do wyświetlania prostego komunikatu (opcjonalnie) ---
        // Wymaga, aby okna aplikacji były dostępne (np. przez IApplicationLifetime)
        private async Task ShowMessage(string title, string message)
        {
            // Ta implementacja jest uproszczona i wymaga dostępu do głównego okna lub innego sposobu wyświetlania dialogów w Avalonia.
            // W pełnej aplikacji MVVM, zazwyczaj używa się serwisów dialogowych przekazywanych przez DI lub systemu komunikatów.
            // Dla celów debugowania i prostego UI, możemy spróbować znaleźć aktywne okno.

            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    // TODO: Tutaj możesz wyświetlić Toast lub prosty dialog (np. używając MessageBox.Avalonia)
                    // Pamiętaj, aby dodać pakiet NuGet MessageBox.Avalonia jeśli chcesz z niego skorzystać
                    /*
                    var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(new MessageBox.Avalonia.Models.MessageBoxStandardParams{
                            ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                            ContentTitle = title,
                            ContentMessage = message,
                            Icon = MessageBox.Avalonia.Enums.Icon.Info, // Możesz zmienić ikonę (Error, Warning itp.)
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        });
                    await messageBoxStandardWindow.ShowDialog(mainWindow);
                    */
                    Debug.WriteLine($"[MusicSearchViewModel] KOMUNIKAT (TODO: UI DIALOG): {title} - {message}"); // Na razie tylko log
                }
                else
                {
                    Debug.WriteLine($"[MusicSearchViewModel] KOMUNIKAT (brak aktywnego okna głównego): {title} - {message}"); // Log jeśli okno główne jest null
                }
            }
            else
            {
                Debug.WriteLine($"[MusicSearchViewModel] KOMUNIKAT (tryb non-desktop): {title} - {message}"); // Log dla trybów innych niż desktop
            }
        }


        // TODO: Dodaj właściwości lub komendy dla innych interakcji z widokiem wyszukiwania muzyki
        // np. SelectedAlbum w wynikach wyszukiwania

    }
}