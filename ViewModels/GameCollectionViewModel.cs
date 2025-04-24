using ReactiveUI;
using System.Collections.ObjectModel;
using OmniMedia.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; // Potrzebne do Task
using Avalonia.Threading; // Potrzebne do Dispatcher.UIThread
using System.Diagnostics; // Potrzebne do Debug.WriteLine
using OmniMedia.Database; // Potrzebne do AppDatabase
using System.Reactive; // Potrzebne do Unit
using System.Reactive.Linq; // Potrzebne do Select
using System.Linq; // Potrzebne do FirstOrDefault

namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku Kolekcji Gier
    public class GameCollectionViewModel : ViewModelBase
    {
        // Właściwość na kolekcję gier do wyświetlenia
        private ObservableCollection<Game> _games = new ObservableCollection<Game>();
        public ObservableCollection<Game> Games
        {
            get => _games;
            set => this.RaiseAndSetIfChanged(ref _games, value);
        }

        // NOWA WŁAŚCIWOŚĆ: Właściwość na zaznaczoną grę w kolekcji
        // Typem jest Game, bo to obiekty Game są w kolekcji Games
        private Game? _selectedGame;
        public Game? SelectedGame
        {
            get => _selectedGame;
            set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
        }

        // NOWA KOMENDA: Komenda do usuwania zaznaczonej gry z kolekcji
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
        private async Task LoadGamesAsync()
        {
            try
            {
                Debug.WriteLine("[GameCollectionViewModel] Rozpoczynam ładowanie gier z bazy danych.");
                var gamesFromDb = await App.Database.GetGamesAsync(); // Pobierz gry z bazy

                // Upewniamy się, że aktualizacja ObservableCollection odbywa się w wątku UI
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Games.Clear(); // Wyczyść obecną kolekcję
                    foreach (var game in gamesFromDb)
                    {
                        Games.Add(game); // Dodaj gry pobrane z bazy
                    }
                    Debug.WriteLine($"[GameCollectionViewModel] Załadowano {Games.Count} gier z bazy danych.");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GameCollectionViewModel] Błąd podczas ładowania gier z bazy danych: {ex.Message}");
                // TODO: Poinformuj użytkownika o błędzie
            }
        }


        // Metoda asynchroniczna do usuwania zaznaczonej gry z bazy danych
        private async Task PerformRemoveGameAsync()
        {
            // Sprawdzamy, czy jakaś gra jest zaznaczona
            if (SelectedGame != null)
            {
                Debug.WriteLine($"[GameCollectionViewModel] Próba usunięcia gry z kolekcji: {SelectedGame.Title}");

                try
                {
                    // WYWOŁAJ METODĘ USUWANIA Z BAZY DANYCH
                    // Używamy metody DeleteGameAsync, którą dodaliśmy w AppDatabase.cs
                    int result = await App.Database.DeleteGameAsync(SelectedGame);

                    Debug.WriteLine($"[GameCollectionViewModel] Wynik usuwania gry z bazy: {result}");

                    // Jeśli usuwanie z bazy się powiodło (result > 0), usuń grę z ObservableCollection Games
                    if (result > 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // Znajdujemy i usuwamy grę z kolekcji Games
                            // Używamy Id do znalezienia gry, bo SelectedGame może być innym obiektem niż ten w kolekcji, ale ma to samo Id
                            var gameToRemove = Games.FirstOrDefault(g => g.Id == SelectedGame.Id);
                            if (gameToRemove != null)
                            {
                                Games.Remove(gameToRemove); // Usuń grę z listy wyświetlanej w UI
                                Debug.WriteLine($"[GameCollectionViewModel] Usunięto grę '{gameToRemove.Title}' z listy w UI.");
                            }
                            SelectedGame = null; // Odznaczamy element po usunięciu
                        });

                        // TODO: Poinformuj użytkownika o sukcesie usunięcia (np. wyświetl komunikat w UI)
                        Debug.WriteLine($"[GameCollectionViewModel] Gra '{SelectedGame.Title}' pomyślnie usunięta.");

                    }
                    else
                    {
                        Debug.WriteLine($"[GameCollectionViewModel] Nie udało się usunąć gry '{SelectedGame.Title}' z bazy danych (result był 0).");
                        // TODO: Poinformuj użytkownika o błędzie/braku usunięcia
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GameCollectionViewModel] Błąd podczas usuwania gry z bazy: {ex.Message}");
                    // TODO: Poinformuj użytkownika o błędzie
                }
            }
            else
            {
                Debug.WriteLine("[GameCollectionViewModel] Próba usunięcia z kolekcji, ale żadna gra nie jest zaznaczona.");
            }
        }

        // TODO: Możesz potrzebować ViewModelu dla pojedynczej miniaturki (jeśli miniatury ostatnio dodanych są w tym ViewModelu)
        // public class RecentlyAddedItemViewModel : ViewModelBase { ... }

    }
}
