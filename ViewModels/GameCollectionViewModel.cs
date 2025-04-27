using ReactiveUI;
using System.Collections.ObjectModel;
using OmniMedia.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Diagnostics;
using OmniMedia.Database;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku Kolekcji Gier
    public class GameCollectionViewModel : ViewModelBase
    {
        // Właściwość na kolekcję gier do wyświetlenia (teraz ViewModeli elementów)
        private ObservableCollection<CollectionGameItemViewModel> _games = new ObservableCollection<CollectionGameItemViewModel>();
        public ObservableCollection<CollectionGameItemViewModel> Games
        {
            get => _games;
            set => this.RaiseAndSetIfChanged(ref _games, value);
        }

        private CollectionGameItemViewModel? _selectedGame;
        public CollectionGameItemViewModel? SelectedGame
        {
            get => _selectedGame;
            set
            {
                Debug.WriteLine($"[GameCollectionViewModel] Właściwość SelectedGame ustawiona na: {value?.Title ?? "null"} (ID: {value?.Id.ToString() ?? "null"})");
                this.RaiseAndSetIfChanged(ref _selectedGame, value);
            }
        }
        public ReactiveCommand<Unit, Unit> RemoveGameCommand { get; }


        // TODO: Właściwość lub kolekcja dla miniatur ostatnio dodanych treści (jeśli są w tym ViewModelu)
        // public ObservableCollection<RecentlyAddedItemViewModel> RecentlyAddedItems { get; }


        // Konstruktor ViewModelu
        public GameCollectionViewModel()
        {
            // Inicjalizacja komendy usuwania
            // Komenda będzie aktywna tylko, gdy SelectedGame nie jest null
            RemoveGameCommand = ReactiveCommand.CreateFromTask(PerformRemoveGameAsync,
                                                               this.WhenAnyValue(x => x.SelectedGame)
                                                                    .Select(selectedGame => selectedGame != null));


            // Wczytaj gry z bazy danych po utworzeniu ViewModelu
            // Uruchamiamy ładowanie asynchronicznie, aby nie blokować UI
            Task.Run(LoadGamesAsync);

            // TODO: Logika ładowania miniatur ostatnio dodanych treści (jeśli są w tym ViewModelu)
        }

        // Metoda asynchroniczna do ładowania gier z bazy danych
        private async Task LoadGamesAsync() // Poprawiona nazwa i brak komentarza TODO z usuwania
        {
            try
            {
                Debug.WriteLine("[GameCollectionViewModel] Rozpoczynam ładowanie gier z bazy danych.");
                var gamesFromDb = await App.Database.GetGamesAsync(); // Pobierz gry z bazy

                Debug.WriteLine($"[AppDatabase] Zakończono pobieranie. Znaleziono {gamesFromDb.Count} gier. ID pierwszych 5: {string.Join(", ", gamesFromDb.Take(5).Select(g => $"{g.Title}: {g.Id}"))}");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Games.Clear(); // Wyczyść obecną kolekcję
                    foreach (var game in gamesFromDb)
                    {
                        // Tworzymy ViewModel elementu listy dla każdej gry pobranej z bazy
                        var collectionItem = new CollectionGameItemViewModel(game);
                        Games.Add(collectionItem); // Dodaj ViewModel do kolekcji Games
                    }
                    Debug.WriteLine($"[GameCollectionViewModel] Załadowano {Games.Count} gier do listy w UI.");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameCollectionViewModel] Błąd podczas ładowania gier z bazy danych: {ex.Message}");
            }
        }


        // Metoda asynchroniczna do usuwania zaznaczonej gry z bazy danych
        private async Task PerformRemoveGameAsync() // Upewnij się, że ta metoda jest TYLKO RAZ
        {
            if (SelectedGame != null)
            {
                Debug.WriteLine($"[GameCollectionViewModel] Próba usunięcia gry: {SelectedGame.Title} (ID: {SelectedGame.Id}) z kolekcji.");

                try
                {
                    // Wywołaj metodę usuwania z bazy danych, przekazując obiekt Game z ViewModelu elementu
                    int result = await App.Database.DeleteGameAsync(SelectedGame.GameData); // POPRAWIONE: UŻYJ SelectedGame.GameData

                    Debug.WriteLine($"[GameCollectionViewModel] Wynik usuwania gry z bazy: {result}");

                    if (result > 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // Znajdujemy i usuwamy ViewModel elementu z kolekcji Games
                            // Używamy Id zawartego obiektu Game
                            var itemToRemove = Games.FirstOrDefault(item => item.GameData.Id == SelectedGame.GameData.Id);
                            if (itemToRemove != null)
                            {
                                Games.Remove(itemToRemove); // Usuń ViewModel z listy wyświetlanej w UI

                                // USUNIĘTO: Diagnostyczne odświeżenie powiązania (nie jest już potrzebne)
                                // var currentGamesCollection = Games;
                                // Games = null;
                                // Games = currentGamesCollection;

                                Debug.WriteLine($"[GameCollectionViewModel] Liczba gier w kolekcji Games po usunięciu: {Games.Count}");
                                Debug.WriteLine($"[GameCollectionViewModel] Usunięto grę '{itemToRemove.Title}' (ID: {itemToRemove.Id}) z listy w UI.");
                            }
                            SelectedGame = null; // Odznaczamy element po usunięciu
                        });

                        // POPRAWIONE: Bezpieczne odczytanie Title po ustawieniu SelectedGame na null
                        Debug.WriteLine($"[GameCollectionViewModel] Gra '{SelectedGame?.Title ?? "Usunięta"}' pomyślnie usunięta.");

                    }
                    else
                    {
                        // POPRAWIONE: Bezpieczne odczytanie Title i Id w komunikacie o błędzie
                        Debug.WriteLine($"[GameCollectionViewModel] Nie udało się usunąć gry '{SelectedGame?.Title ?? "Nieznana"}' (ID: {SelectedGame?.Id.ToString() ?? "null"}) z bazy danych (result był 0).");
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GameCollectionViewModel] Błąd podczas usuwania gry z bazy: {ex.Message}");
                    // TODO: Poinformuj użytkownika o błędzie (np. wyświetl komunikat w UI)
                }
            }
            else
            {
                Debug.WriteLine("[GameCollectionViewModel] Próba usunięcia z kolekcji, ale żadna gra nie jest zaznaczona.");
            }
        }

        // TODO: Możemy potrzebować ViewModelu dla pojedynczej miniaturki (jeśli miniatury ostatnio dodanych są w tym ViewModelu)
        // public class RecentlyAddedItemViewModel : ViewModelBase { ... }

    }
}