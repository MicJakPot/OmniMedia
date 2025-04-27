using ReactiveUI;
using System;
using System.Reactive; // Potrzebne do Unit i Interaction
using System.Threading.Tasks;
using OmniMedia.Models; // Potrzebne do modelu Movie
using System.Diagnostics; // Debug Writeline


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
        // SaveInteraction: input to Movie, output to Movie (zmieniono na Movie? w poprzedniej logice)
        // Poprawna definicja interakcji powinna być zgodna z tym, co faktycznie zwraca View/Handler
        // Jeśli handler zwraca Movie? (przez Close(result)), to output powinien być Movie?
        // Sprawdźmy definicję: Interaction<Movie, Movie?> ShowMovieEditDialogInteraction { get; } = new Interaction<MovieEditViewModel, Movie?>();
        // Tutaj jest problem! Input interakcji ShowMovieEditDialogInteraction to MovieEditViewModel, a output to Movie?.
        // Ale SaveInteraction jest wewnątrz MovieEditViewModel i ma input Movie, output Movie.
        // Handlery w MovieEditWindow dla SaveInteraction zdefiniowane są jako async context => { Close(context.Input); context.SetOutput(context.Input); }
        // Context.Input dla SaveInteraction jest Movie (typ wejściowy). Handler zamyka okno z Close(context.Input), czyli Close(Movie).
        // Ale ShowDialog<Movie?> oczekuje Movie? jako wynik. Może tu jest niezgodność?
        // Spróbujmy zmienić definicję SaveInteraction na Interaction<Movie, Movie?>

        public Interaction<Movie, Movie?> SaveInteraction { get; } = new Interaction<Movie, Movie?>(); // <<< Zmieniono Output na Movie? >>>
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
        private async Task<Movie?> SaveMovie() // Zmieniono typ zwracany na Task<Movie?>
        {
            Debug.WriteLine("[MovieEditViewModel.SaveMovie] Metoda SaveMovie rozpoczęta.");

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
                Debug.WriteLine($"[MovieEditViewModel.SaveMovie] Próba zapisu filmu '{movieToSave.Title}' do bazy danych filmów...");
                int result = await App.MovieDatabase.SaveMovieAsync(movieToSave);
                Debug.WriteLine($"[MovieEditViewModel.SaveMovie] Wynik zapisu do bazy danych: {result}. ID filmu: {movieToSave.Id}");


                if (result > 0)
                {
                    Debug.WriteLine($"[MovieEditViewModel.SaveMovie] Film '{movieToSave.Title}' pomyślnie zapisany (ID: {movieToSave.Id}). Wywoływanie interakcji SaveInteraction.");
                    // Wywołujemy interakcję, aby poinformować widok o zakończeniu zapisu
                    // AWAITUJEMY JEJ ZAKOŃCZENIE PRZEZ WIDOK
                    // Rzutowanie na Task<Movie?> jest poprawne, bo Interakcja zwraca Task<Movie?>
                    // Ale problemem może być to, że SaveInteraction.Handle(movieToSave) zwraca Task<Movie?>, a nie Task<Movie>
                    // A ten Task<Movie?> jest potem rzutowany na Task<Movie> albo na Task<Movie?>
                    // Spróbujmy usunąć jawne rzutowanie całkiem i zobaczyć co się stanie.
                    // Jeśli błąd wynika z wewnętrznego działania ReactiveUI/runtime, rzutowanie może nie pomóc.
                    // Zostawmy rzutowanie, ale upewnijmy si\u0119, \u017ce typy si\u0119 zgadzaj\u0105

                    // await (Task<Movie?>)SaveInteraction.Handle(movieToSave); // Oryginalna próba rzutowania na Task<Movie?>
                    // await (Task<Movie>)SaveInteraction.Handle(movieToSave); // Poprzednia próba rzutowania na Task<Movie>, która dała błąd

                    // Spróbujmy inaczej: uzyskać Task, a potem go awaitować.
                    // Zmieniono definicj\u0119 SaveInteraction na Interaction<Movie, Movie?>

                    var handleTask = SaveInteraction.Handle(movieToSave); // Ta metoda zwraca Task<Movie?>

                    Debug.WriteLine("[MovieEditViewModel.SaveMovie] Uzyskano Task z interakcji SaveInteraction.");

                    // Awaitujemy Task zwrócony przez Handle
                    // Tutaj zg\u0142aszany jest b\u0142\u0105d rzutowania, kt\u00f3ry m\u00f3wi o ObservableImpl.Concat<Movie> do Task<Movie>.
                    // To nadal sugeruje, \u017ce co\u015B powoduje, \u017ce wynik Handle jest widziany jako IObservable.
                    // Spr\u00f3bujmy rzutowa\u0107 Task na object, a potem na Task<Movie?>? To brzmi jak obej\u015Bcie.

                    // Rozwa\u017cmy inn\u0105 mo\u017cliwo\u015B\u0107: Czy problemem nie jest typ input (Movie), a output (Movie?) w interakcji?
                    // Handler w MovieEditWindow zamyka okno z Close(context.Input), gdzie context.Input jest typu Movie.
                    // ShowDialog oczekuje Movie? jako TResult. Mo\u017ce tu jest niezgodno\u015B\u0107 typ\u00f3w mi\u0119dzy Close(Movie) a oczekiwanym Movie? przez ShowDialog<Movie?>?
                    // Nie, rzutowanie powinno dzia\u0142a\u0107. Movie mo\u017cna rzutowa\u0107 na Movie?.

                    // Wracaj\u0105c do b\u0142\u0119du rzutowania IObservable na Task - to jest kluczowy problem.
                    // Jawne rzutowanie powinno zadzia\u0142a\u0107, je\u015Bli Handle faktycznie zwraca Task<Movie?>.
                    // Je\u015Bli nadal sypie b\u0142\u0119dem rzutowania na Observable, to co\u015B jest fundamentalnie nie tak z setupem ReactiveUI/await.

                    // Pr\u00f3ba rzutowania na Task<Movie?> ponownie, aby by\u0142o zgodne z definicj\u0105 interakcji.
                    await (Task<Movie?>)handleTask; // <<< ZMIENIONA LINIA (u\u017cywa handleTask i rzutuje na Task<Movie?>) >>>


                    Debug.WriteLine("[MovieEditViewModel.SaveMovie] Interakcja SaveInteraction zakończona.");
                    return movieToSave; // Zwracamy zapisany obiekt Movie
                }
                else
                {
                    Debug.WriteLine($"[MovieEditViewModel.SaveMovie] Zapis do bazy danych zwr\u00f3ci\u0142 0. Niepowodzenie zapisu.");
                    // TODO: Pokaż komunikat o błędzie zapisu (możesz użyć Interaction lub innej metody komunikacji)
                    return null; // Zwracamy null w przypadku błędu zapisu
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MovieEditViewModel.SaveMovie] WYJ\u0104TEK PODCZAS ZAPISU FILMU: {ex.Message}"); // <-- DODAJ T\u0116 LINIJ\u0116 DO CATCH
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
            Debug.WriteLine("[MovieEditViewModel] Interakcja CancelInteraction zakończona."); // <-- DODAJ T\u0116 LINIJ\u0118
            return Unit.Default;
        }


        // TODO: Dodać inne metody lub właściwości, jeśli będą potrzebne
        // np. LoadCoverArtPreview(string path) do wyświetlenia podglądu okładki w formularzu
        // np. BrowseForCoverArtCommand do otwarcia dialogu wyboru pliku
    }
}