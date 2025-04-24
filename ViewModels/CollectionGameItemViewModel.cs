using ReactiveUI;
using OmniMedia.Models;
using Avalonia.Media.Imaging; // Potrzebne do Bitmap
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Threading; // Potrzebne do Dispatcher.UIThread
using System;
using System.Diagnostics;


namespace OmniMedia.ViewModels
{
    // ViewModel dla pojedynczego elementu na liście kolekcji gier
    public class CollectionGameItemViewModel : ViewModelBase
    {
        // Właściwość przechowująca dane gry
        public Game GameData { get; }

        // Właściwość przechowująca obrazek miniaturki
        private Bitmap? _thumbnail;
        public Bitmap? Thumbnail
        {
            get => _thumbnail;
            private set => this.RaiseAndSetIfChanged(ref _thumbnail, value);
        }

        // Prywatne pole dla klienta HTTP (można współdzielić jedną instancję w aplikacji)
        private static readonly HttpClient _httpClient = new HttpClient();


        // Konstruktor - przyjmuje dane gry i rozpoczyna ładowanie miniaturki
        public CollectionGameItemViewModel(Game game)
        {
            GameData = game; // Zapisujemy dane gry

            // Rozpocznij ładowanie miniaturki asynchronicznie, jeśli URL istnieje
            if (!string.IsNullOrEmpty(GameData.ThumbnailUrl))
            {
                // Uruchamiamy ładowanie w tle, aby nie blokować UI
                Task.Run(() => LoadThumbnailAsync(GameData.ThumbnailUrl));
            }
        }

        // Metoda asynchroniczna do ładowania miniaturki z URL
        private async Task LoadThumbnailAsync(string url)
        {
            try
            {
                // Pobierz dane obrazu z URL
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Sprawdź status odpowiedzi

                // Odczytaj strumień danych obrazu
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    // Utwórz obiekt Bitmap ze strumienia danych
                    var bitmap = new Bitmap(stream);

                    // Upewnij się, że aktualizacja właściwości (UI) odbywa się w wątku UI
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Thumbnail = bitmap; // Przypisz wczytany obrazek do właściwości
                        Debug.WriteLine($"[CollectionGameItemViewModel] Wczytano miniaturkę dla gry: {GameData.Title}"); // Komunikat debug
                    });
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"[CollectionGameItemViewModel] Błąd HTTP podczas ładowania miniaturki ({url}): {e.Message}");
                // TODO: Obsługa błędów (np. ustawienie domyślnej ikonki "brak miniaturki")
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[CollectionGameItemViewModel] Błąd podczas ładowania miniaturki ({url}): {e.Message}");
                // TODO: Obsługa innych błędów
            }
        }

        // Właściwości "przekazujące" dane z GameData dla łatwiejszego bindowania w XAML
        public int Id => GameData.Id; // Dodajemy też Id dla łatwiejszego dostępu
        public string? Title => GameData.Title;
        public string? Genre => GameData.Genre;
        public string? Platform => GameData.Platform;
        public string? ThumbnailUrl => GameData.ThumbnailUrl; // Udostępniamy URL dla celów debugowania/alternatyw
        public DateTime? ReleasedDate => GameData.ReleasedDate;
        public double? Rating => GameData.Rating;
        public string? Description => GameData.Description;
        public string? Developer => GameData.Developer;
        public string? Publisher => GameData.Publisher;
    }
}