using ReactiveUI;
using System;
using System.Reactive; // Potrzebne do Unit i Interaction
using System.Threading.Tasks;
using OmniMedia.Models; // Potrzebne do modelu Movie
using System.Diagnostics; // Potrzebne do Debug



namespace OmniMedia.ViewModels
{
    // ViewModel dla formularza dodawania/edycji filmu
    public class MovieEditViewModel : ViewModelBase
    {
        // Prywatne pole przechowujące oryginalny obiekt Movie (jeśli edytujemy)
        private Movie? _originalMovie;

        // --- Właściwości ViewModelu odpowiadające polom w modelu Movie ---
        // Usunięto [Reactive], dodano pola zapasowe i RaiseAndSetIfChanged w setterach

        private string? _title;
        public string? Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string? _director;
        public string? Director
        {
            get => _director;
            set => this.RaiseAndSetIfChanged(ref _director, value);
        }

        private int? _year;
        public int? Year
        {
            get => _year;
            set => this.RaiseAndSetIfChanged(ref _year, value);
        }

        private string? _genre;
        public string? Genre
        {
            get => _genre;
            set => this.RaiseAndSetIfChanged(ref _genre, value);
        }

        private int? _durationMinutes;
        public int? DurationMinutes
        {
            get => _durationMinutes;
            set => this.RaiseAndSetIfChanged(ref _durationMinutes, value);
        }

        private bool _isOwned;
        public bool IsOwned
        {
            get => _isOwned;
            set => this.RaiseAndSetIfChanged(ref _isOwned, value);
        }

        private bool _isOnWishlist;
        public bool IsOnWishlist
        {
            get => _isOnWishlist;
            set => this.RaiseAndSetIfChanged(ref _isOnWishlist, value);
        }

        private double? _rating;
        public double? Rating
        {
            get => _rating;
            set => this.RaiseAndSetIfChanged(ref _rating, value);
        }

        private string? _plotSummary;
        public string? PlotSummary
        {
            get => _plotSummary;
            set => this.RaiseAndSetIfChanged(ref _plotSummary, value);
        }

        private string? _coverArtPath; // Ścieżka do okładki
        public string? CoverArtPath
        {
            get => _coverArtPath;
            set => this.RaiseAndSetIfChanged(ref _coverArtPath, value);
        }

        private string? _format;
        public string? Format
        {
            get => _format;
            set => this.RaiseAndSetIfChanged(ref _format, value);
        }

        private string? _location;
        public string? Location
        {
            get => _location;
            set => this.RaiseAndSetIfChanged(ref _location, value);
        }

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set => this.RaiseAndSetIfChanged(ref _notes, value);
        }

        // TODO: Dodaj więcej właściwości z polami zapasowymi i RaiseAndSetIfChanged dla innych pól z modelu Movie

        // --- Koniec Właściwości ---


        // Komendy dla przycisków formularza
        public ReactiveCommand<Unit, Movie?> SaveMovieCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }


        // ReactiveUI Interactions do komunikacji z widokiem (np. do zamknięcia okna/dialogu)
        // Interaction<TInput, TOutput>
        // SaveInteraction: input to Movie, output to Movie
        public Interaction<Movie, Movie> SaveInteraction { get; } = new Interaction<Movie, Movie>();
        // CancelInteraction: input is Unit, output is Unit
        public Interaction<Unit, Unit> CancelInteraction { get; } = new Interaction<Unit, Unit>();


        // Konstruktor dla dodawania nowego filmu
        public MovieEditViewModel()
        {
            // Inicjalizacja komendy Zapisz
            var canSave = this.WhenAnyValue(x => x.Title, (title) => !string.IsNullOrWhiteSpace(title)); // Przycisk aktywny, jeśli Tytuł nie jest pusty
            SaveMovieCommand = ReactiveCommand.CreateFromTask(SaveMovie, canSave);

            // Inicjalizacja komendy Anuluj
            CancelCommand = ReactiveCommand.CreateFromTask(Cancel);
        }

        // Konstruktor dla edycji istniejącego filmu
        public MovieEditViewModel(Movie movie) : this() // Wywołuje konstruktor bezparametrowy (inicjalizuje komendy)
        {
            _originalMovie = movie; // Zapisujemy referencję do oryginalnego filmu

            // Wypełniamy właściwości ViewModelu danymi z oryginalnego filmu
            Title = movie.Title;
            Director = movie.Director;
            Year = movie.Year;
            Genre = movie.Genre;
            DurationMinutes = movie.DurationMinutes;
            IsOwned = movie.IsOwned;
            IsOnWishlist = movie.IsOnWishlist;
            Rating = movie.Rating;
            PlotSummary = movie.PlotSummary;
            CoverArtPath = movie.CoverArtPath;
            Format = movie.Format;
            Location = movie.Location;
            Notes = movie.Notes;

            // TODO: Wypełnij inne właściwości danymi z filmu
        }


        // Metoda wykonywana przez komendę SaveMovieCommand
        private async Task<Movie?> SaveMovie()
        {
            // TODO: Opcjonalna walidacja danych przed zapisem
            // if (string.IsNullOrWhiteSpace(Title))
            // {
            //     Debug.WriteLine("[MovieEditViewModel] Błąd walidacji: Tytuł jest wymagany.");
            //     return null; // Nie zapisujemy i zwracamy null
            // }


            // Tworzymy nowy obiekt Movie lub aktualizujemy istniejący (_originalMovie)
            var movieToSave = _originalMovie ?? new Movie(); // Użyj _originalMovie jeśli istnieje, w przeciwnym razie utwórz nowy

            // Kopiujemy dane z właściwości ViewModelu do obiektu Movie
            movieToSave.Title = Title;
            movieToSave.Director = Director;
            movieToSave.Year = Year;
            movieToSave.Genre = Genre;
            movieToSave.DurationMinutes = DurationMinutes;
            movieToSave.IsOwned = IsOwned;
            movieToSave.IsOnWishlist = IsOnWishlist;
            movieToSave.Rating = Rating;
            movieToSave.PlotSummary = PlotSummary;
            movieToSave.CoverArtPath = CoverArtPath;
            movieToSave.Format = Format;
            movieToSave.Location = Location;
            movieToSave.Notes = Notes;

            // TODO: Skopiuj inne właściwości z ViewModelu do movieToSave

            try
            {
                // Zapisujemy obiekt Movie do bazy danych filmów
                Debug.WriteLine($"[MovieEditViewModel] Rozpoczynam zapisywanie filmu '{movieToSave.Title}' do bazy danych filmów...");
                int result = await App.MovieDatabase.SaveMovieAsync(movieToSave);

                Debug.WriteLine($"[MovieEditViewModel] Zapisano film do bazy danych filmów. Rezultat: {result}. ID filmu: {movieToSave.Id}");

                if (result > 0)
                {
                    Debug.WriteLine($"[MovieEditViewModel] Film '{movieToSave.Title}' pomyślnie zapisany (ID: {movieToSave.Id}).");
                    // Wywołujemy Interaction i AWAITUJEMY JEGO ZAKOŃCZENIE PRZEZ WIDOK
                    // DODANO JAWNE RZUTOWANIE NA Task<Movie>
                    await (Task<Movie>)SaveInteraction.Handle(movieToSave);
                    return movieToSave; // Zwracamy zapisany obiekt Movie
                }
                else
                {
                    Debug.WriteLine($"[MovieEditViewModel] Nie udało się zapisać filmu '{movieToSave.Title}' do bazy danych filmów (result był 0).");
                    // TODO: Pokaż komunikat o błędzie zapisu (możesz użyć Interaction lub innej metody komunikacji)
                    return null; // Zwracamy null w przypadku błędu zapisu
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MovieEditViewModel] Błąd podczas zapisu filmu do bazy danych filmów: {ex.Message}");
                // TODO: Pokaż komunikat o błędzie zapisu
                return null; // Zwracamy null w przypadku błędu
            }
        }

        // Metoda wykonywana przez komendę CancelCommand
        private async Task<Unit> Cancel()
        {
            Debug.WriteLine("[MovieEditViewModel] Anulowano edycję filmu.");
            // Wywołujemy Interaction i AWAITUJEMY JEGO ZAKOŃCZENIE PRZEZ WIDOK
            // DODANO JAWNE RZUTOWANIE NA Task<Unit>
            await (Task<Unit>)CancelInteraction.Handle(Unit.Default);
            return Unit.Default;
        }


        // TODO: Dodać inne metody lub właściwości, jeśli będą potrzebne
        // np. LoadCoverArtPreview(string path) do wyświetlenia podglądu okładki w formularzu
        // np. BrowseForCoverArtCommand do otwarcia dialogu wyboru pliku
    }
}