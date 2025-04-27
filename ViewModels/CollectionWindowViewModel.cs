using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Threading.Tasks;
using OmniMedia.Database;
using Avalonia.Threading;
using System.Collections.ObjectModel;

// DODANE USINGI dla ViewModelów kolekcji
using OmniMedia.ViewModels;


namespace OmniMedia.ViewModels
{
    // ViewModel dla okna przeglądania kolekcji (gier, muzyki i filmów)
    public class CollectionWindowViewModel : ViewModelBase
    {
        // ViewModel dla kolekcji gier
        public GameCollectionViewModel GameCollection { get; private set; }

        // ViewModel dla kolekcji muzyki
        public MusicCollectionViewModel MusicCollection { get; private set; }

        // ViewModel dla kolekcji filmów
        public MovieCollectionViewModel MovieCollection { get; private set; }


        // Właściwość przechowująca aktualnie wyświetlany ViewModel kolekcji (gra, muzyka lub film)
        private ViewModelBase? _currentCollectionContent;
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
        public ReactiveCommand<Unit, Unit> OpenMovieCollectionCommand { get; } // Już istniała jako TODO


        // TODO: Właściwość lub kolekcja dla miniatur ostatnio dodanych treści (jeśli chcesz je wyświetlać w tym ViewModelu)

        // Konstruktor ViewModelu
        public CollectionWindowViewModel()
        {
            // Inicjalizujemy instancje ViewModelów poszczególnych kolekcji
            GameCollection = new GameCollectionViewModel();
            MusicCollection = new MusicCollectionViewModel();
            // Inicjalizujemy ViewModel dla filmów
            MovieCollection = new MovieCollectionViewModel();


            // Ustawiamy domyślnie wyświetlaną kolekcję na kolekcję gier (lub inną, np. filmów)
            CurrentCollectionContent = GameCollection;


            // Inicjalizacja komend przycisków

            // Implementacja OpenGameCollectionCommand
            OpenGameCollectionCommand = ReactiveCommand.Create(() =>
            {
                CurrentCollectionContent = GameCollection;
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Gier. Ustawiono GameCollectionViewModel.");
            });

            // Implementacja OpenMusicCollectionCommand
            OpenMusicCollectionCommand = ReactiveCommand.Create(() => {
                CurrentCollectionContent = MusicCollection;
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Muzyki. Ustawiono MusicCollectionViewModel.");
            });

            // ZMIENIONA IMPLEMENTACJA Komendy dla przycisku "Kolekcja Filmów"
            OpenMovieCollectionCommand = ReactiveCommand.Create(() => {
                CurrentCollectionContent = MovieCollection; // Ustawiamy CurrentCollectionContent na instancję kolekcji filmów
                System.Diagnostics.Debug.WriteLine("Kliknięto: Kolekcja Filmów. Ustawiono MovieCollectionViewModel.");
            });

            // TODO: Logika ładowania miniatur ostatnio dodanych treści
        }

        // TODO: Możemy potrzebować ViewModelu dla pojedynczej miniaturki

    }
}