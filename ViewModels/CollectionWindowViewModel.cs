using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using OmniMedia.Database;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using OmniMedia.Models;

// DODANE USINGI dla ViewModelów kolekcji
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

        // DODANE WŁAŚCIWOŚCI: Instancje ViewModelów dla poszczególnych kolekcji
        // Tworzymy je raz w konstruktorze i przechowujemy
        public GameCollectionViewModel GameCollection { get; private set; }
        public MusicCollectionViewModel MusicCollection { get; private set; }
        // TODO: Dodaj właściwość dla kolekcji filmów, gdy będzie gotowa


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
            // Inicjalizacja instancji ViewModelów poszczególnych kolekcji
            GameCollection = new GameCollectionViewModel();
            MusicCollection = new MusicCollectionViewModel();

            // Ustawiamy domyślnie wyświetlaną kolekcję na kolekcję gier przy starcie okna
            CurrentCollectionContent = GameCollection;


            // Inicjalizacja komend przycisków

            // OpenGameCollectionCommand - ustawia CurrentCollectionContent na przechowywaną instancję GameCollection
            OpenGameCollectionCommand = ReactiveCommand.Create(() =>
            {
                CurrentCollectionContent = GameCollection; // Ustawiamy CurrentCollectionContent na ZAINICJOWANĄ wcześniej instancję
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Gier. Ustawiono GameCollectionViewModel.");
            });

            // OpenMusicCollectionCommand - ustawia CurrentCollectionContent na przechowywaną instancję MusicCollection
            OpenMusicCollectionCommand = ReactiveCommand.Create(() => {
                CurrentCollectionContent = MusicCollection; // Ustawiamy CurrentCollectionContent na ZAINICJOWANĄ wcześniej instancję
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Muzyki. Ustawiono MusicCollectionViewModel.");
            });

            // Implementacja komendy dla przycisku "Kolekcja Filmów" (TODO)
            OpenMovieCollectionCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku Kolekcji Filmów
                // CurrentCollectionContent = MovieCollection; // Ustawiamy CurrentCollectionContent na instancję kolekcji filmów (gdy będzie gotowa)
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Filmów (TODO)");
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