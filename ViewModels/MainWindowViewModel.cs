using OmniMedia.ViewModels.Base;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using OmniMedia.Views; // Potrzebne do odwołania do CollectionWindow i GameSearchWindow
using System.Diagnostics; // Pozostawione na potrzeby innych komunikatów

namespace OmniMedia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Komendy dla przycisków w MainWindow

        // Komenda dla przycisku "Przeglądaj swoją Kolekcję"
        public ReactiveCommand<Unit, Unit> OpenCollectionCommand { get; }

        // Komenda dla przycisku "Szukaj Gry"
        public ReactiveCommand<Unit, Unit> OpenGameSearchCommand { get; }

        // Komenda dla przycisku "Szukaj Muzyki"
        public ReactiveCommand<Unit, Unit> OpenMusicSearchCommand { get; }

        // Komenda dla przycisku "Szukaj Filmów"
        public ReactiveCommand<Unit, Unit> OpenMovieSearchCommand { get; }

        // Komenda dla przycisku "Eksportuj Bazę"
        public ReactiveCommand<Unit, Unit> ExportDatabaseCommand { get; }

        // Komenda dla przycisku "Importuj Bazę"
        public ReactiveCommand<Unit, Unit> ImportDatabaseCommand { get; }

        // Komenda dla przycisku "Ustawienia"
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

        // Komenda dla przycisku "O Twórcach"
        public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }

        // Właściwość do zarządzania wyświetlaną zawartością w głównym oknie (już nie używana do Szukaj Gry)
        private ViewModelBase? _currentContent;
        public ViewModelBase? CurrentContent
        {
            get => _currentContent;
            set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        // USUNIĘTO: Tymczasowa właściwość TestDataContext

        public MainWindowViewModel()
        {
            // Inicjalizacja komend
            // Logika nawigacji do odpowiednich widoków/okien

            // Implementacja OpenCollectionCommand - otwiera nowe okno CollectionWindow
            OpenCollectionCommand = ReactiveCommand.Create(() =>
            {
                var collectionWindow = new CollectionWindow
                {
                    DataContext = new CollectionWindowViewModel()
                };
                collectionWindow.Show();
                Debug.WriteLine("Kliknięto: Przeglądaj swoją Kolekcję. Otwarto CollectionWindow.");
            });

            // Zmieniona implementacja OpenGameSearchCommand - otwiera nowe okno GameSearchWindow
            OpenGameSearchCommand = ReactiveCommand.Create(() =>
            {
                // Tworzymy nową instancję GameSearchWindow
                var gameSearchWindow = new GameSearchWindow
                {
                    // Ustawiamy DataContext nowego okna na nowy ViewModel GameSearchViewModel
                    DataContext = new GameSearchViewModel()
                };
                // Wyświetlamy nowe okno
                gameSearchWindow.Show();
                Debug.WriteLine("Kliknięto: Szukaj Gry. Otwarto GameSearchWindow.");
            });


            // Przykładowe inicjalizacje pozostałych komend
            OpenMusicSearchCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Szukaj Muzyki");
            });

            OpenMovieSearchCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Szukaj Filmów");
            });

            ExportDatabaseCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Eksportuj Bazę");
            });

            ImportDatabaseCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Importuj Bazę");
            });

            OpenSettingsCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Ustawienia");
            });

            OpenAboutCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: O Twórcach");
            });

            // Opcjonalnie: Ustaw początkową zawartość prawej części okna głównego przy starcie
            // Np. możesz ustawić null, aby obszar był pusty, lub inny domyślny widok
            CurrentContent = null; // Ustawiamy na null, aby nie wyświetlać nic domyślnie w ContentControl
        }
    }
}