using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using OmniMedia.Database; // Potrzebne do AppDatabase
using Avalonia.Threading; // Potrzebne do Dispatcher.UIThread
using System.Collections.ObjectModel; // Potrzebne do ObservableCollection
using OmniMedia.Models; // Potrzebne do modelu Game
using OmniMedia.ViewModels;

namespace OmniMedia.ViewModels
{
    // ViewModel dla okna "Przeglądaj swoją kolekcję"
    public class CollectionWindowViewModel : ViewModelBase
    {
        // Właściwość, która przechowuje aktualnie wyświetlany ViewModel w dolnej części okna kolekcji
        private ViewModelBase? _currentCollectionContent; // Może być null na początku
        public ViewModelBase? CurrentCollectionContent
        {
            get => _currentCollectionContent;
            set => this.RaiseAndSetIfChanged(ref _currentCollectionContent, value);
        }

        // Komendy dla przycisków w tym oknie

        // Komenda dla przycisku "Kolekcja Gier"
        public ReactiveCommand<Unit, Unit> OpenGameCollectionCommand { get; }

        // Komenda dla przycisku "Kolekcja Muzyki"
        public ReactiveCommand<Unit, Unit> OpenMusicCollectionCommand { get; }

        // Komenda dla przycisku "Kolekcja Filmów"
        public ReactiveCommand<Unit, Unit> OpenMovieCollectionCommand { get; }

        // TODO: Właściwość lub kolekcja dla miniatur ostatnio dodanych treści (jeśli chcesz je wyświetlać w tym ViewModelu)
        // Na razie skupiamy się na przełączaniu widoków kolekcji
        // public ObservableCollection<RecentlyAddedItemViewModel> RecentlyAddedItems { get; }

        // Konstruktor ViewModelu
        public CollectionWindowViewModel()
        {
            // Inicjalizacja komend przycisków

            // Implementacja OpenGameCollectionCommand - ustawia CurrentCollectionContent na GameCollectionViewModel
            OpenGameCollectionCommand = ReactiveCommand.Create(() =>
            {
                CurrentCollectionContent = new GameCollectionViewModel(); // Tworzymy i ustawiamy ViewModel dla Kolekcji Gier
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Gier. Ustawiono GameCollectionViewModel."); // Komunikat debug
            });

            // Przykładowe inicjalizacje pozostałych komend (na razie puste)
            OpenMusicCollectionCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku Kolekcji Muzyki
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Muzyki");
            });

            OpenMovieCollectionCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku Kolekcji Filmów
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Filmów");
            });

            // TODO: Logika ładowania miniatur ostatnio dodanych treści (jeśli będą w tym ViewModelu)
            // Jeśli miniaturki mają być niezależne od wybranej kolekcji, ich ładowanie może być tutaj
        }

        // TODO: Możesz potrzebować ViewModelu dla pojedynczej miniaturki
        // public class RecentlyAddedItemViewModel : ViewModelBase { ... }

        // Metoda do ładowania gier (przeniesiona z GameCollectionViewModel do celów debugowania, można przenieść z powrotem)
        // Możesz usunąć tę metodę LoadGamesAsync, jeśli już ją masz w GameCollectionViewModel.cs
        /*
        private async Task LoadGamesAsync()
        {
            var dummyGames = await App.Database.GetGamesAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Tutaj docelowo zaktualizujesz kolekcję miniatur lub danych w tym ViewModelu
                System.Diagnostics.Debug.WriteLine($"[CollectionWindowViewModel] Załadowano {dummyGames.Count} gier z bazy danych.");
            });
        }
        */
    }
}