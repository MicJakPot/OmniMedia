using OmniMedia.ViewModels.Base;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using OmniMedia.Views; // Potrzebne do odwołania do CollectionWindow
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

        // Właściwość do zarządzania wyświetlaną zawartością w głównym oknie (jeśli używana)
        private ViewModelBase? _currentContent; // Używamy ViewModelBase? jeśli może być null
        public ViewModelBase? CurrentContent
        {
            get => _currentContent;
            set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }


        public MainWindowViewModel()
        {
            // Inicjalizacja komend
            // Logika nawigacji do odpowiednich widoków/okien

            // Zmieniona implementacja OpenCollectionCommand - otwiera nowe okno CollectionWindow
            OpenCollectionCommand = ReactiveCommand.Create(() => // Zmieniono na Create, bo Show() nie jest asynchroniczne
            {
                // Tworzymy nową instancję CollectionWindow
                var collectionWindow = new CollectionWindow
                {
                    // Ustawiamy DataContext nowego okna na nowy ViewModel CollectionWindowViewModel
                    DataContext = new CollectionWindowViewModel()
                };
                // Wyświetlamy nowe okno
                collectionWindow.Show();

                System.Diagnostics.Debug.WriteLine("Kliknięto: Przeglądaj swoją Kolekcję. Otwarto CollectionWindow."); // Opcjonalny komunikat
            });

            // Implementacja OpenGameSearchCommand - teraz ustawia CurrentContent w MainWindowViewModel
            OpenGameSearchCommand = ReactiveCommand.Create(() =>
            {
                // TODO: Zaimplementuj logikę otwierania widoku wyszukiwania gier w głównym oknie
                // Ustawienie CurrentContent na GameSearchViewModel
                CurrentContent = new GameSearchViewModel();
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Gry. Ustawiono GameSearchViewModel jako CurrentContent.");
            });


            // Przykładowe inicjalizacje pozostałych komend (możesz dostosować)
            OpenMusicSearchCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku wyszukiwania muzyki
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Muzyki");
            });

            OpenMovieSearchCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku wyszukiwania filmów
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Filmów");
            });

            ExportDatabaseCommand = ReactiveCommand.Create(() => {
                // TODO: Logika eksportu bazy danych
                System.Diagnostics.Debug.WriteLine("Kliknięto: Eksportuj Bazę");
            });

            ImportDatabaseCommand = ReactiveCommand.Create(() => {
                // TODO: Logika importu bazy danych
                System.Diagnostics.Debug.WriteLine("Kliknięto: Importuj Bazę");
            });

            OpenSettingsCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku ustawień
                System.Diagnostics.Debug.WriteLine("Kliknięto: Ustawienia");
            });

            OpenAboutCommand = ReactiveCommand.Create(() => {
                // TODO: Logika otwierania widoku "O Twórcach"
                System.Diagnostics.Debug.WriteLine("Kliknięto: O Twórcach");
            });


            // Opcjonalnie: Ustaw początkową zawartość prawej części okna głównego przy starcie
            // np. pusty ViewModel, widok powitalny, lub domyślny widok kolekcji (jeśli zdecydujesz się na wyświetlanie kolekcji w głównym oknie)
            // CurrentContent = new SomeDefaultViewModel();
        }
    }
}