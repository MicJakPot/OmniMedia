using ReactiveUI;
using OmniMedia.Models;
using System.Reactive; // Potrzebne do Unit
using Avalonia.Media.Imaging; // Potrzebne do Bitmap
using System.Threading.Tasks; // Potrzebne do Task
using System.Diagnostics; // Potrzebne do Debug
using System; // Potrzebne do Uri
using Avalonia.Platform; // Potrzebne do AssetLoader
using System.IO; // Potrzebne do Stream
using System.Net.Http; // Potrzebne do HttpClient


namespace OmniMedia.ViewModels
{
    // ViewModel dla pojedynczego elementu na liście kolekcji filmów
    public class CollectionMovieItemViewModel : ViewModelBase
    {
        // Właściwość przechowująca dane filmu z bazy danych
        public Movie MovieData { get; }

        // Komenda "Usuń z kolekcji" dla tego elementu
        public ReactiveCommand<CollectionMovieItemViewModel, Unit> RemoveFromCollectionCommand { get; }

        // Właściwość przechowująca obrazek okładki (Bitmap)
        private Bitmap? _cover;
        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        // Prywatne pole na obrazek zastępczy - współdzielone
        private static Bitmap? _placeholderImage;
        private static readonly HttpClient _httpClient = new HttpClient(); // Klient HTTP - współdzielony


        // Konstruktor - PRZYJMUJE DWA ARGUMENTY
        public CollectionMovieItemViewModel(Movie movie, ReactiveCommand<CollectionMovieItemViewModel, Unit> removeFromCollectionCommand)
        {
            MovieData = movie; // Zapisujemy dane filmu
            RemoveFromCollectionCommand = removeFromCollectionCommand; // Przypisujemy przekazaną komendę

            // Zainicjuj obrazek zastępczy raz
            if (_placeholderImage == null)
            {
                _placeholderImage = LoadPlaceholderImage(); // TODO: Stwórz obrazek placeholderu dla filmów
            }

            // Ustaw obrazek zastępczy na początku
            Cover = _placeholderImage;

            // Rozpocznij ładowanie właściwej okładki asynchronicznie, jeśli ścieżka/URL istnieje
            if (!string.IsNullOrEmpty(MovieData.CoverArtPath))
            {
                // Na razie zakładamy, że CoverArtPath może być URL lub ścieżką lokalną
                // TODO: Zaimplementuj ładowanie z URL (HttpClient) lub z pliku lokalnego
                // Tymczasowo używamy tylko URL ładowania jak dla muzyki
                Task.Run(() => LoadCoverAsync(MovieData.CoverArtPath));
            }
            else
            {
                Debug.WriteLine($"[CollectionMovieItemViewModel] Brak ścieżki/URL okładki dla filmu: {MovieData.Title}. Użyto placeholderu.");
            }
        }

        // Metoda asynchroniczna do ładowania okładki (tymczasowo tylko z URL)
        private async Task LoadCoverAsync(string pathOrUrl)
        {
            // TODO: Rozróżnij czy to URL czy ścieżka pliku
            // Na razie ładowanie tylko z URL jak w CollectionAlbumItemViewModel
            try
            {
                // Próba załadowania z URL
                Debug.WriteLine($"[CollectionMovieItemViewModel] Próba ładowania okładki z URL: {pathOrUrl}");
                var response = await _httpClient.GetAsync(pathOrUrl);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var bitmap = new Bitmap(stream);
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Cover = bitmap;
                        Debug.WriteLine($"[CollectionMovieItemViewModel] Wczytano okładkę z URL dla filmu: {MovieData.Title}");
                    });
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"[CollectionMovieItemViewModel] Błąd HTTP podczas ładowania okładki z URL ({pathOrUrl}): {e.Message}");
                // Próba ładowania jako plik lokalny (TODO)
                // await LoadCoverFromFileAsync(pathOrUrl);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[CollectionMovieItemViewModel] Błąd podczas ładowania okładki ({pathOrUrl}): {e.Message}");
                // Próba ładowania jako plik lokalny (TODO)
                // await LoadCoverFromFileAsync(pathOrUrl);
            }
        }

        // Metoda do ładowania obrazka zastępczego
        private Bitmap? LoadPlaceholderImage()
        {
            // TODO: Stwórz plik obrazka placeholderu dla filmów i użyj poprawnej ścieżki
            var assetPath = "avares://OmniMedia/Assets/Images/movie_placeholder.png"; // <<< ZMIEŃ NA POPRAWNĄ ŚCIEŻKĘ >>>

            try
            {
                var uri = new Uri(assetPath);
                if (AssetLoader.Exists(uri))
                {
                    using (var stream = AssetLoader.Open(uri))
                    {
                        Debug.WriteLine($"[CollectionMovieItemViewModel] Wczytano obrazek placeholderu z zasobów: {assetPath}");
                        return new Bitmap(stream);
                    }
                }
                else
                {
                    Debug.WriteLine($"[CollectionMovieItemViewModel] Zas\u00f3b placeholderu nie znaleziony: {assetPath}. Upewnij si\u0119, \u017ce plik istnieje i ma Build Action = AvaloniaResource.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CollectionMovieItemViewModel] B\u0142\u0105d \u0142adowania obrazka placeholderu z zasob\u00f3w ({assetPath}): {ex.Message}");
                return null;
            }
        }


        // Właściwości "przekazujące" dane z MovieData do bindowania w XAML widoku kolekcji
        public string? Title => MovieData.Title;
        public string? Director => MovieData.Director;
        // Dodaj inne właściwości przekazujące dane, jeśli chcesz je wyświetlić na liście kolekcji
        public int? Year => MovieData.Year;
        public string? Genre => MovieData.Genre;
        public bool IsOwned => MovieData.IsOwned;
        public bool IsOnWishlist => MovieData.IsOnWishlist;
    }
}