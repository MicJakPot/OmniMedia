using ReactiveUI;
using OmniMedia.Models;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading; // Potrzebne do Dispatcher
using System.Net.Http; // Potrzebne do HttpClient (do ładowania obrazków)
using Avalonia.Platform; // Potrzebne do AssetLoader
using System.IO; // Potrzebne do Stream w przyszłości
using System.Reactive; // Potrzebne do Unit


namespace OmniMedia.ViewModels
{
    // ViewModel dla pojedynczego elementu na liście kolekcji albumów muzycznych
    public class CollectionAlbumItemViewModel : ViewModelBase
    {
        // Właściwość przechowująca dane albumu z bazy danych
        public MusicAlbum AlbumData { get; }

        // Właściwość przechowująca obrazek okładki (Bitmap)
        private Bitmap? _cover;
        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        // Komenda "Usuń z kolekcji" dla tego elementu
        // Będzie obsługiwana przez nadrzędny ViewModel (MusicCollectionViewModel)
        public ReactiveCommand<CollectionAlbumItemViewModel, Unit> RemoveFromCollectionCommand { get; }


        // Prywatne pole na obrazek zastępczy (nutka) - współdzielone między wszystkimi instancjami
        private static Bitmap? _placeholderImage;
        private static readonly HttpClient _httpClient = new HttpClient(); // Klient HTTP - współdzielony


        // Konstruktor - przyjmuje dane albumu z bazy i komendę usuwania z nadrzędnego ViewModelu
        public CollectionAlbumItemViewModel(MusicAlbum album, ReactiveCommand<CollectionAlbumItemViewModel, Unit> removeFromCollectionCommand)
        {
            AlbumData = album; // Zapisujemy dane albumu z bazy
            RemoveFromCollectionCommand = removeFromCollectionCommand; // Przypisujemy przekazaną komendę

            // Zainicjuj obrazek zastępczy raz, jeśli jeszcze tego nie zrobiliśmy
            if (_placeholderImage == null)
            {
                _placeholderImage = LoadPlaceholderImage();
            }

            // Ustaw obrazek zastępczy na początku
            Cover = _placeholderImage;

            // Rozpocznij ładowanie właściwej okładki asynchronicznie, jeśli URL istnieje w danych z bazy
            if (!string.IsNullOrEmpty(AlbumData.CoverUrl))
            {
                // Uruchamiamy ładowanie w tle
                Task.Run(() => LoadCoverAsync(AlbumData.CoverUrl));
            }
            else
            {
                Debug.WriteLine($"[CollectionAlbumItemViewModel] Brak URL okładki w danych bazy dla albumu: {AlbumData.Title}. Użyto placeholderu.");
            }
        }


        // Metoda asynchroniczna do ładowania okładki z URL
        private async Task LoadCoverAsync(string url)
        {
            try
            {
                Debug.WriteLine($"[CollectionAlbumItemViewModel] Rozpoczynam ładowanie okładki z URL: {url}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var bitmap = new Bitmap(stream);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Cover = bitmap; // Przypisz wczytaną okładkę w wątku UI
                        Debug.WriteLine($"[CollectionAlbumItemViewModel] Wczytano okładkę dla albumu: {AlbumData.Title}");
                    });
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"[CollectionAlbumItemViewModel] Błąd HTTP podczas ładowania okładki ({url}): {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[CollectionAlbumItemViewModel] Błąd podczas ładowania okładki ({url}): {e.Message}");
            }
        }

        // Metoda prywatna do ładowania obrazka zastępczego (nutki) z zasobów
        private Bitmap? LoadPlaceholderImage()
        {
            // Ścieżka do obrazka nutki w zasobach Avaloni. Używamy tej samej ścieżki co w wyszukiwaniu.
            var assetPath = "avares://OmniMedia/Assets/Images/musical-notes_nutka.png"; // <<< SPRAWDŹ, CZY ŚCIEŻKA JEST POPRAWNA >>>

            try
            {
                var uri = new Uri(assetPath);
                if (AssetLoader.Exists(uri))
                {
                    using (var stream = AssetLoader.Open(uri))
                    {
                        Debug.WriteLine($"[CollectionAlbumItemViewModel] Wczytano obrazek placeholderu z zasobów: {assetPath}");
                        return new Bitmap(stream);
                    }
                }
                else
                {
                    Debug.WriteLine($"[CollectionAlbumItemViewModel] Zasób placeholderu nie znaleziony: {assetPath}. Upewnij się, że plik istnieje i ma Build Action = AvaloniaResource.");
                    return null; // Zwróć null, jeśli zasób nie istnieje
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CollectionAlbumItemViewModel] Błąd ładowania obrazka placeholderu z zasobów ({assetPath}): {ex.Message}");
                return null; // Zwróć null w przypadku błędu ładowania
            }
        }


        // Właściwości "przekazujące" dane z AlbumData do bindowania w XAML widoku kolekcji
        public string? Artist => AlbumData.Artist;
        public string? Title => AlbumData.Title;
        // Możesz dodać inne właściwości, które chcesz wyświetlać w widoku kolekcji (np. Genre, ReleasedDate)
        public string? Genre => AlbumData.Genre;
        public DateTime? ReleasedDate => AlbumData.ReleasedDate;
    }
}
