using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive; // Potrzebne do Unit, Interaction
using System.Threading.Tasks;
using System.Diagnostics;
using OmniMedia.Models;
using OmniMedia.Database;
using Avalonia.Threading;
using System.Reactive.Linq; // Potrzebne do Observable.Select

// Potrzebne do komunikatów (jeśli zdecydujemy się na okna dialogowe)
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;


namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku kolekcji filmów
    // UWAGA: UPEWNIJ SIĘ, ŻE NAZWA KLASY JEST TAKA JAK PONIŻEJ
    public class MovieCollectionViewModel : ViewModelBase
    {
        // Kolekcja ViewModels elementów listy kolekcji filmów
        private ObservableCollection<CollectionMovieItemViewModel> _collectionItems = new ObservableCollection<CollectionMovieItemViewModel>();
        public ObservableCollection<CollectionMovieItemViewModel> CollectionItems
        {
            get => _collectionItems;
            set => this.RaiseAndSetIfChanged(ref _collectionItems, value);
        }

        // Komenda "Usuń z kolekcji"
        public ReactiveCommand<CollectionMovieItemViewModel, Unit> RemoveFromCollectionCommand { get; }

        // DODANA KOMENDA: Komenda "Dodaj Film"
        public ReactiveCommand<Unit, Unit> AddMovieCommand { get; }

        // DODANA INTERAKCJA: Interakcja do wyświetlenia okna dialogowego dodawania/edycji filmu
        // Input: ViewModel formularza edycji filmu (MovieEditViewModel)
        // Output: Zapisany obiekt Movie (lub null, jeśli anulowano)
        public Interaction<MovieEditViewModel, Movie?> ShowMovieEditDialogInteraction { get; } = new Interaction<MovieEditViewModel, Movie?>();


        // Właściwość wskazująca, czy ładowanie kolekcji jest w toku
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }


        // Konstruktor
        // UWAGA: UPEWNIJ SIĘ, ŻE NAZWA KONSTRUKTORA JEST TAKA JAK NAZWA KLASY
        public MovieCollectionViewModel()
        {
            // Inicjalizacja komendy "Usuń z kolekcji"
            RemoveFromCollectionCommand = ReactiveCommand.CreateFromTask<CollectionMovieItemViewModel, Unit>(
                RemoveFromCollection,
                this.WhenAnyValue(x => x.IsBusy).Select(isBusy => !isBusy)
            );

            // DODANA INICJALIZACJA KOMENDY "Dodaj Film"
            AddMovieCommand = ReactiveCommand.CreateFromTask(AddMovie);

            // Rozpocznij ładowanie filmów z bazy danych przy tworzeniu ViewModelu
            Task.Run(LoadMoviesAsync);
        }


        // Metoda ładująca filmy z bazy danych
        private async Task<Unit> LoadMoviesAsync()
        {
            Debug.WriteLine("[MovieCollectionViewModel] Rozpoczynam ładowanie filmów z bazy danych...");
            IsBusy = true;

            try
            {
                // Pobierz wszystkie filmy z bazy danych filmów
                var movies = await App.MovieDatabase.GetMoviesAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CollectionItems.Clear();
                    Debug.WriteLine($"[MovieCollectionViewModel] Wyczy\u015Bcono obecn\u0105 list\u0119 kolekcji film\u00f3w ({CollectionItems.Count} element\u00f3w przed czyszczeniem).");
                });


                if (movies != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine($"[MovieCollectionViewModel] Przetwarzanie {movies.Count} film\u00f3w z bazy.");
                        foreach (var movie in movies)
                        {
                            // Tworzymy ViewModel elementu listy dla filmu (TERAZ KLASA CollectionMovieItemViewModel JUŻ MA KONSTRUKTOR Z 2 ARGUMENTAMI)
                            var movieVm = new CollectionMovieItemViewModel(movie, RemoveFromCollectionCommand);
                            CollectionItems.Add(movieVm);
                        }
                        Debug.WriteLine($"[MovieCollectionViewModel] Dodano {CollectionItems.Count} film\u00f3w do kolekcji.");
                    });
                }
                else
                {
                    Debug.WriteLine("[MovieCollectionViewModel] Baza danych film\u00f3w zwr\u00f3ci\u0142a pust\u0105 list\u0119 film\u00f3w.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MovieCollectionViewModel] B\u0142\u0105d podczas \u0142adowania film\u00f3w z bazy danych: {ex.Message}");
                await ShowMessage("B\u0142\u0105d \u0142adowania kolekcji film\u00f3w", $"Wyst\u0105pi\u0142 b\u0142\u0105d podczas \u0142adowania kolekcji film\u00f3w: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[MovieCollectionViewModel] Zako\u0144czono \u0142adowanie film\u00f3w.");
            }
            return Unit.Default;
        }

        // IMPLEMENTACJA METODY DO USUWANIA FILMU Z KOLEKCJI
        private async Task<Unit> RemoveFromCollection(CollectionMovieItemViewModel movieVm)
        {
            if (movieVm?.MovieData == null) return Unit.Default;

            Debug.WriteLine($"[MovieCollectionViewModel] Pr\u00f3ba usuni\u0119cia filmu '{movieVm.MovieData.Title}' (ID bazy: {movieVm.MovieData.Id}) z kolekcji...");

            try
            {
                // Usuń film z bazy danych filmów
                int result = await App.MovieDatabase.DeleteMovieAsync(movieVm.MovieData);

                Debug.WriteLine($"[MovieCollectionViewModel] Usuni\u0119to film z bazy danych film\u00f3w. Rezultat: {result}");

                if (result > 0)
                {
                    Debug.WriteLine($"[MovieCollectionViewModel] Film '{movieVm.MovieData.Title}' pomy\u015Blnie usuni\u0119ty z bazy.");
                    // Usuń ViewModel elementu z ObservableCollection (MUSI BYĆ W WĄTKU UI)
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CollectionItems.Remove(movieVm);
                        Debug.WriteLine($"[MovieCollectionViewModel] Usuni\u0119to ViewModel filmu z ObservableCollection. Pozosta\u0142o {CollectionItems.Count} element\u00f3w.");
                    });

                    await ShowMessage("Usuni\u0119to film", $"Film '{movieVm.MovieData.Title}' re\u017Cysera '{movieVm.MovieData.Director}' zosta\u0142 usuni\u0119ty z kolekcji.");
                }
                else
                {
                    Debug.WriteLine($"[MovieCollectionViewModel] Nie uda\u0142o si\u0119 usun\u0105\u0107 filmu '{movieVm.MovieData.Title}' z bazy danych film\u00f3w (result by\u0142 0).");
                    await ShowMessage("B\u0142\u0105d usuwania", $"Nie uda\u0142o si\u0119 usun\u0105\u0107 filmu '{movieVm.MovieData.Title}' z kolekcji.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MovieCollectionViewModel] B\u0142\u0105d podczas usuwania filmu z bazy: {ex.Message}");
                await ShowMessage("B\u0142\u0105d usuwania", $"Wyst\u0105pi\u0142 b\u0142\u0105d podczas usuwania filmu '{movieVm.MovieData.Title}': {ex.Message}");
            }
            return Unit.Default;
        }

        // IMPLEMENTACJA METODY DLA KOMENDY "Dodaj Film"
        private async Task<Unit> AddMovie()
        {
            Debug.WriteLine("[MovieCollectionViewModel] Komenda AddMovie wykonana. Tworzenie ViewModelu formularza edycji.");
            // Tworzymy nowy ViewModel dla formularza edycji (dla dodania nowego filmu)
            var movieEditViewModel = new MovieEditViewModel();

            // Wywołujemy interakcję, aby poprosić widok o wyświetlenie okna dialogowego
            // Oczekujemy na wynik (zapisany obiekt Movie lub null)
            Debug.WriteLine("[MovieCollectionViewModel] Wywo\u0142ywanie interakcji ShowMovieEditDialogInteraction...");
            var savedMovie = await ShowMovieEditDialogInteraction.Handle(movieEditViewModel);
            Debug.WriteLine($"[MovieCollectionViewModel] Interakcja zako\u0144czona. Wynik (zapisany film): {savedMovie?.Title ?? "null"}");


            // Jeśli użytkownik zapisał film (wynik nie jest null)
            if (savedMovie != null)
            {
                Debug.WriteLine($"[MovieCollectionViewModel] Film '{savedMovie.Title}' zosta\u0142 zapisany. Od\u015Bwie\u017Canie listy kolekcji.");
                // Odświeżamy listę filmów w kolekcji, ładując ją ponownie z bazy
                await RefreshCollectionAsync();
            }
            else
            {
                Debug.WriteLine("[MovieCollectionViewModel] Film nie zosta\u0142 zapisany (anulowano lub b\u0142\u0105d). Nie od\u015Bwie\u017Cam listy.");
            }

            return Unit.Default;
        }

        // Metoda do odświeżania listy kolekcji filmów
        public async Task RefreshCollectionAsync()
        {
            Debug.WriteLine("[MovieCollectionViewModel] Odświeżanie listy kolekcji filmów...");
            // Ponownie ładujemy filmy z bazy i aktualizujemy CollectionItems
            await LoadMoviesAsync();
            Debug.WriteLine("[MovieCollectionViewModel] Lista kolekcji filmów odświeżona.");
        }


        // Metoda pomocnicza do wyświetlania prostego komunikatu
        private async Task ShowMessage(string title, string message)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    Debug.WriteLine($"[MovieCollectionViewModel] KOMUNIKAT (TODO: UI DIALOG): {title} - {message}");
                    // TODO: Użyj MessageBox.Avalonia lub innej metody do wyświetlenia dialogu w UI
                }
                else
                {
                    Debug.WriteLine($"[MovieCollectionViewModel] KOMUNIKAT (brak aktywnego okna g\u0142\u00f3wnego): {title} - {message}");
                }
            }
            else
            {
                Debug.WriteLine($"[MovieCollectionViewModel] KOMUNIKAT (tryb non-desktop): {title} - {message}");
            }
            // Zwróć Task, aby metoda była poprawna jako async Task
        }

        // TODO: Dodaj inne właściwości lub komendy
    }
}