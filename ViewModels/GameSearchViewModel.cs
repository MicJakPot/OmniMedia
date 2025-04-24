using ReactiveUI; // Potrzebne do ReactiveObject i ReactiveCommand
using System.Collections.ObjectModel; // Potrzebne do ObservableCollection
using System.Reactive; // Potrzebne do Unit (dla ReactiveCommand)
using OmniMedia.Models; // Potrzebne do modelu Game
using System;
using System.Collections.Generic; // Potrzebne do List
using System.Text;
using System.Threading.Tasks;

namespace OmniMedia.ViewModels // Przestrzeń nazw dla ViewModeli
{
    // ViewModel dla funkcjonalności wyszukiwania gier
    public class GameSearchViewModel : ViewModelBase
    {
        // Właściwość na zapytanie wyszukiwania wpisywane przez użytkownika
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        // Właściwość na listę wyników wyszukiwania gier
        // Używamy ObservableCollection, aby zmiany w liście były automatycznie widoczne w UI
        private ObservableCollection<Game> _searchResults = new ObservableCollection<Game>();
        public ObservableCollection<Game> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        // Komenda do uruchomienia wyszukiwania
        // ReactiveCommand<Unit, Unit> oznacza komendę, która nie przyjmuje parametrów (Unit) i nie zwraca wyniku (Unit)
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        // Konstruktor ViewModelu
        public GameSearchViewModel()
        {
            // Inicjalizacja komendy wyszukiwania
            // Komenda będzie aktywna (canExecute) zawsze (Observable.Return(true))
            // Po wywołaniu (execute) uruchomi metodę PerformSearch
            SearchCommand = ReactiveCommand.CreateFromTask(PerformSearch);
        }

        // Metoda asynchroniczna do wykonania wyszukiwania
        // Logika interakcji z API RAWG zostanie dodana tutaj
        private async Task PerformSearch()
        {
            // TODO: Zaimplementuj tutaj logikę:
            // 1. Pobierz wartość z właściwości SearchQuery.
            // 2. Wykonaj asynchroniczne zapytanie do API RAWG z użyciem zapytania.
            // 3. Przetwórz odpowiedź API (np. JSON) na listę obiektów Game.
            // 4. Wyczyść obecną listę SearchResults i dodaj nowe wyniki.

            // Przykładowa logikę (do usunięcia po implementacji API):
            // Symulacja wyników wyszukiwania
            await Task.Delay(500); // Symulacja opóźnienia API

            var dummyResults = new List<Game>
            {
                new Game { Title = "Wyszukana Gra 1", Genre = "Akcja", Platform = "PC", ThumbnailUrl = "avares://OmniMedia/Assets/placeholder.png" },
                new Game { Title = "Wyszukana Gra 2", Genre = "RPG", Platform = "PlayStation", ThumbnailUrl = "avares://OmniMedia/Assets/placeholder.png" }
            };

            // Wyczyść poprzednie wyniki i dodaj nowe
            SearchResults.Clear();
            foreach (var game in dummyResults)
            {
                SearchResults.Add(game);
            }

            Console.WriteLine($"Wyszukano dla zapytania: {SearchQuery}. Znaleziono {SearchResults.Count} wyników.");
        }

        // Można dodać inne komendy, np. do zapisywania wybranej gry do kolkekcji
        // public ReactiveCommand<Game, Unit> SaveGameCommand { get; } // Przykład komendy do zapisywania konkretnej gry

    }
}
