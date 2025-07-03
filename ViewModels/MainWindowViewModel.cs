using OmniMedia.ViewModels.Base;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using OmniMedia.Views; // Potrzebne do odwołania do CollectionWindow i GameSearchWindow
using System.Diagnostics; // Debug Writeline
using Avalonia.Media.Imaging; // Potrzebne do Bitmap
using System.IO; // Potrzebne do Stream
using Avalonia.Platform; // Potrzebne do AssetLoader
using System.Reflection; // Potrzebne do Assembly
using OmniMedia.Services; // Serwis eksportu/importu


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

        // Właściwość do zarządzania wyświetlaną zawartością w głównym oknie
        // Może być ViewModelm widoku (np. GameCollectionViewModel) lub innym obiektem (np. Bitmap dla logo)
        private object? _currentContent; // Zmieniono typ na object?, aby mógł przechowywać Bitmap
        public object? CurrentContent
        {
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        // DODANE POLE NA OBRAZEK LOGO
        private Bitmap? _logoImage;


        public MainWindowViewModel()
        {
            // Inicjalizacja komend
            // Logika nawigacji do odpowiednich widoków/okien

            // Implementacja OpenCollectionCommand - otwiera nowe okno CollectionWindow
            OpenCollectionCommand = ReactiveCommand.Create(() =>
            {
                var collectionWindow = new CollectionWindow
                {
                    DataContext = new CollectionWindowViewModel() // Tworzymy ViewModel dla okna CollectionWindow
                };
                collectionWindow.Show(); // Wyświetlamy nowe okno CollectionWindow
                Debug.WriteLine("Kliknięto: Przeglądaj swoją Kolekcję. Otwarto CollectionWindow.");
                // Po otwarciu nowego okna, możemy opcjonalnie powrócić do logo w oknie głównym
                // CurrentContent = _logoImage; // Opcjonalnie, jeśli chcesz powrócić do logo
            });

            // Implementacja OpenGameSearchCommand - otwiera nowe okno GameSearchWindow
            OpenGameSearchCommand = ReactiveCommand.Create(() =>
            {
                var gameSearchWindow = new GameSearchWindow
                {
                    DataContext = new GameSearchViewModel() // Tworzymy ViewModel dla okna GameSearchWindow
                };
                gameSearchWindow.Show(); // Wyświetlamy nowe okno GameSearchWindow
                Debug.WriteLine("Kliknięto: Szukaj Gry. Otwarto GameSearchWindow.");
                // Po otwarciu nowego okna, możemy opcjonalnie powrócić do logo w oknie głównym
                // CurrentContent = _logoImage; // Opcjonalnie, jeśli chcesz powrócić do logo
            });


            // Inicjalizacja komendy "Szykaj Muzyki"
            OpenMusicSearchCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Szukaj Muzyki. Otwarto MusicSearchWindow.");
                // Tworzymy nową instancję MusicSearchWindow
                var musicSearchWindow = new MusicSearchWindow
                {
                    // Ustawiamy DataContext nowego okna na nowy ViewModel MusicSearchViewModel
                    DataContext = new MusicSearchViewModel()
                };
                // Wyświetlamy nowe okno
                musicSearchWindow.Show();
            });

            OpenMovieSearchCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Szukaj Filmów (TODO)");
                // TODO: Implementacja otwierania okna wyszukiwania filmów
                // Opcjonalnie powrót do logo: CurrentContent = _logoImage;
            });

            ExportDatabaseCommand = ReactiveCommand.CreateFromTask(async () => {
                try
                {
                    Debug.WriteLine("Kliknięto: Eksportuj Bazę");
                    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OmniMedia");
                    Directory.CreateDirectory(folder);
                    var pdfPath = Path.Combine(folder, "OmniMediaExport.pdf");
                    await DatabaseExportImportService.ExportDatabaseToPdfAsync(pdfPath);
                    Debug.WriteLine($"[MainWindowViewModel] Zapisano eksport pod {pdfPath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindowViewModel] Błąd eksportu bazy: {ex.Message}");
                }
            });

            ImportDatabaseCommand = ReactiveCommand.CreateFromTask(async () => {
                try
                {
                    Debug.WriteLine("Kliknięto: Importuj Bazę");
                    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OmniMedia");
                    var xlsPath = Path.Combine(folder, "OmniMediaImport.xlsx");
                    if (File.Exists(xlsPath))
                    {
                        await DatabaseExportImportService.ImportFromXlsAsync(xlsPath);
                        Debug.WriteLine($"[MainWindowViewModel] Zaimportowano dane z {xlsPath}");
                    }
                    else
                    {
                        Debug.WriteLine($"[MainWindowViewModel] Plik importu nie istnieje: {xlsPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindowViewModel] Błąd importu bazy: {ex.Message}");
                }
            });

            OpenSettingsCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: Ustawienia (TODO)");
                // TODO: Implementacja otwierania okna ustawień/widoku
                // Opcjonalnie powrót do logo: CurrentContent = _logoImage;
            });

            OpenAboutCommand = ReactiveCommand.Create(() => {
                Debug.WriteLine("Kliknięto: O Twórcach (TODO)");
                // TODO: Implementacja otwierania okna "O twórcach"/widoku
                // Opcjonalnie powrót do logo: CurrentContent = _logoImage;
            });

            // DODANE: Wczytaj obrazek logo i ustaw go jako początkową zawartość (domyślny widok)
            _logoImage = LoadLogoImage();
            CurrentContent = _logoImage; // Ustaw logo jako domyślną zawartość przy starcie aplikacji
        }

        // Metoda prywatna do ładowania obrazka logo z zasobów
        private Bitmap? LoadLogoImage()
        {
            // Ścieżka do obrazka w zasobach Avaloni. 
            var assetPath = "avares://OmniMedia/Assets/Images/OmniMedia Pro.png"; 

            try
            {
                // AssetLoader.Open wymaga Uri.
                var uri = new Uri(assetPath);
                // Sprawdzamy, czy zasób istnieje przed próbą otwarcia
                if (AssetLoader.Exists(uri))
                {
                    using (var stream = AssetLoader.Open(uri))
                    {
                        return new Bitmap(stream);
                    }
                }
                else
                {
                    Debug.WriteLine($"[MainWindowViewModel] Zasób obrazka logo nie znaleziony: {assetPath}");
                    return null; // Zwróć null, jeśli zasób nie istnieje
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowViewModel] Błąd ładowania obrazka logo z zasobów ({assetPath}): {ex.Message}");
                return null; // Zwróć null w przypadku błędu podczas ładowania
            }
        }

        // TODO: Możesz dodać inne metody pomocnicze lub logikę ViewModelu

    }
}