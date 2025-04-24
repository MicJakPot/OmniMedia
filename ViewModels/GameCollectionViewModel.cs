using ReactiveUI;
using System.Collections.ObjectModel;
using OmniMedia.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniMedia.Database;
using Avalonia.Threading;
using System.Diagnostics;

namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku Kolekcji Gier
    public class GameCollectionViewModel : ViewModelBase
    {
        // Właściwość na kolekcję gier do wyświetlenia
        public ObservableCollection<Game> Games { get; set; }

        // Konstruktor ViewModelu
        public GameCollectionViewModel()
        {
            Games = new ObservableCollection<Game>(); // Inicjalizacja kolekcji

            // Wczytaj gry z bazy danych po utworzeniu ViewModelu
            // Uruchamiamy ładowanie asynchronicznie, aby nie blokować UI
            Task.Run(LoadGamesAsync);
        }

        // Metoda asynchroniczna do ładowania gier z bazy danych
        private async Task LoadGamesAsync()
        {
            // Próba pobrania z bazy danych
            var dummyGames = await App.Database.GetGamesAsync();

            // Upewniamy się, że aktualizacja ObservableCollection odbywa się w wątku UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Games.Clear();
                foreach (var game in dummyGames)
                {
                    Games.Add(game);
                }
                Debug.WriteLine($"[GameCollectionViewModel] Załadowano {Games.Count} gier z bazy danych.");
            });
        }

        // Można dodać inne właściwości lub komendy, np. do sortowania, filtrowania, usuwania gier
    }
}
