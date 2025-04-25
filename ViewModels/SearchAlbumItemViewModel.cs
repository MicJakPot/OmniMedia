using ReactiveUI;
using OmniMedia.Models;
using Avalonia.Media.Imaging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Diagnostics;
using Avalonia.Platform; // Potrzebne do AssetLoader
using System.IO; // Potrzebne do Stream
using System.Reactive; // Potrzebne do Unit


namespace OmniMedia.ViewModels
{
    // ViewModel dla pojedynczego elementu na liście wyników wyszukiwania albumów muzycznych
    public class SearchAlbumItemViewModel : ViewModelBase
    {
        // Właściwość przechowująca dane albumu
        public MusicAlbum AlbumData { get; }

        // Właściwość przechowująca obrazek okładki (Bitmap)
        private Bitmap? _cover;
        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        // DODANA KOMENDA: Komenda "Dodaj do kolekcji" dla tego elementu
        // Ta komenda jest definiowana w nadrzędnym ViewModelu (MusicSearchViewModel)
        // i PRZEKAZYWANA do konstruktora tego ViewModelu elementu.
        // Będzie przyjmować jako parametr ten ViewModel elementu (this).
        public ReactiveCommand<SearchAlbumItemViewModel, Unit> AddToCollectionCommand { get; }


        private static Bitmap? _placeholderImage;
        private static readonly HttpClient _httpClient = new HttpClient();


        // Konstruktor - przyjmuje dane albumu i PRZEKAZANĄ KOMENDĘ dodawania
        // Zmieniono konstruktor, aby przyjmował komendę z nadrzędnego ViewModelu
        public SearchAlbumItemViewModel(MusicAlbum album, ReactiveCommand<SearchAlbumItemViewModel, Unit> addToCollectionCommand) // Konstruktor przyjmuje komendę z zewnątrz
        {
            AlbumData = album; // Zapisujemy dane albumu
            AddToCollectionCommand = addToCollectionCommand; // PRZYPISUJEMY PRZEKAZANĄ KOMENDĘ


            // Zainicjuj obrazek zastępczy raz, jeśli jeszcze tego nie zrobiliśmy
            if (_placeholderImage == null)
            {
                _placeholderImage = LoadPlaceholderImage();
            }

            // Ustaw obrazek zastępczy na początku
            Cover = _placeholderImage;

            // Rozpocznij ładowanie właściwej okładki asynchronicznie, jeśli URL istnieje
            if (!string.IsNullOrEmpty(AlbumData.CoverUrl))
            {
                // Uruchamiamy ładowanie w tle, aby nie blokować UI
                Task.Run(() => LoadCoverAsync(AlbumData.CoverUrl));
            }
            else
            {
                Debug.WriteLine($"[SearchAlbumItemViewModel] Brak URL okładki dla albumu: {AlbumData.Title}. Użyto placeholderu.");
            }
        }

        // Metoda asynchroniczna do ładowania okładki z URL
        private async Task LoadCoverAsync(string url)
        {
            try
            {
                Debug.WriteLine($"[SearchAlbumItemViewModel] Rozpoczynam ładowanie okładki z URL: {url}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var bitmap = new Bitmap(stream);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Cover = bitmap;
                        Debug.WriteLine($"[SearchAlbumItemViewModel] Wczytano okładkę dla albumu: {AlbumData.Title}");
                    });
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"[SearchAlbumItemViewModel] Błąd HTTP podczas ładowania okładki ({url}): {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[SearchAlbumItemViewModel] Błąd podczas ładowania okładki ({url}): {e.Message}");
            }
        }

        // Metoda prywatna do ładowania obrazka zastępczego (nutki) z zasobów
        private Bitmap? LoadPlaceholderImage()
        {

            var assetPath = "avares://OmniMedia/Assets/Images/musical-notes_nutka.png"; 

            try
            {
                var uri = new Uri(assetPath);
                if (AssetLoader.Exists(uri))
                {
                    using (var stream = AssetLoader.Open(uri))
                    {
                        Debug.WriteLine($"[SearchAlbumItemViewModel] Wczytano obrazek placeholderu z zasobów: {assetPath}");
                        return new Bitmap(stream);
                    }
                }
                else
                {
                    Debug.WriteLine($"[SearchAlbumItemViewModel] Zasób placeholderu nie znaleziony: {assetPath}. Upewnij się, że plik istnieje i ma Build Action = AvaloniaResource.");
                    return null; // Zwróć null, jeśli zasób nie istnieje
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchAlbumItemViewModel] Błąd ładowania obrazka placeholderu z zasobów ({assetPath}): {ex.Message}");
                return null; // Zwróć null w przypadku błędu ładowania
            }
        }


        // Właściwości "przekazujące" dane z AlbumData dla łatwiejszego bindowania w XAML
        public string? Artist => AlbumData.Artist;
        public string? Title => AlbumData.Title;

        // Dodane właściwości z AlbumData, które mogą być potrzebne w widoku (np. w widoku szczegółów lub kafelku)
        public string? Genre => AlbumData.Genre;
        public DateTime? ReleasedDate => AlbumData.ReleasedDate;
        public double? CollectionPrice => AlbumData.CollectionPrice;
    }
}