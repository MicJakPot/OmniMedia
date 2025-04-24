using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using OmniMedia.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace OmniMedia.ViewModels
{
    // ViewModel dla funkcjonalności wyszukiwania gier
    public class GameSearchViewModel : ViewModelBase
    {
        // TODO: ZASTĄP TEN PLACEHOLDER SWOIM KLUCZEM API RAWG
        private const string RawgApiKey = "ccd369cc710a46b2930caa85edadee4a"; // <-- Wklejony mój klucz API

        private readonly HttpClient _httpClient; // Klient HTTP do komunikacji z API

        // Właściwość na zapytanie wyszukiwania
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        // Właściwość na listę wyników wyszukiwania
        private ObservableCollection<Game> _searchResults = new ObservableCollection<Game>();
        public ObservableCollection<Game> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        // Komenda do uruchomienia wyszukiwania
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        // Konstruktor ViewModelu
        public GameSearchViewModel()
        {
            _httpClient = new HttpClient(); // Inicjalizacja klienta HTTP

            // Komenda wyszukiwania jest aktywna tylko wtedy, gdy zapytanie wyszukiwania nie jest puste
            var canSearch = this.WhenAnyValue(x => x.SearchQuery, query => !string.IsNullOrWhiteSpace(query));

            SearchCommand = ReactiveCommand.CreateFromTask(PerformSearch, canSearch);
        }

        // Metoda asynchroniczna do wykonania wyszukiwania gier z API RAWG
        private async Task PerformSearch()
        {
            SearchResults.Clear(); // Wyczyść poprzednie wyniki

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return; // Nie wyszukuj, jeśli zapytanie jest puste
            }

            try
            {
                // Budowanie adresu URL zapytania do API RAWG
                // Dokumentacja API: https://rawg.io/apidocs
                var requestUrl = $"https://api.rawg.io/api/games?key={RawgApiKey}&search={Uri.EscapeDataString(SearchQuery)}";

                // Wykonanie zapytania HTTP GET
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); // Sprawdź, czy odpowiedź ma status powodzenia (2xx)

                // Odczytanie odpowiedzi jako string (JSON)
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserializacja odpowiedzi JSON
                var rawgResult = JsonSerializer.Deserialize<RawgApiResponse>(jsonResponse);

                // Przetwarzanie wyników i dodawanie do listy SearchResults
                if (rawgResult?.Results != null)
                {
                    foreach (var rawgGame in rawgResult.Results)
                    {
                        // Mapowanie danych z obiektu RAWG na nasz model Game
                        var game = new Game
                        {
                            Title = rawgGame.Name,
                            ReleasedDate = rawgGame.Released,
                            Rating = rawgGame.Rating,
                            ThumbnailUrl = rawgGame.BackgroundImage,
                            // TODO: Mapowanie gatunków i platform może wymagać dodatkowej logiki
                            Genre = rawgGame.Genres != null && rawgGame.Genres.Count > 0 ? rawgGame.Genres[0].Name : "N/A", // Przykładowe mapowanie pierwszego gatunku
                            Platform = rawgGame.Platforms != null && rawgGame.Platforms.Count > 0 ? rawgGame.Platforms[0].PlatformDetails?.Name : "N/A" // Przykładowe mapowanie pierwszej platformy
                        };
                        SearchResults.Add(game);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                // Obsługa błędów HTTP (np. brak połączenia, nieprawidłowy klucz API)
                Console.WriteLine($"Błąd HTTP podczas wyszukiwania: {e.Message}");
                // Można wyświetlić komunikat błędu użytkownikowi
            }
            catch (JsonException e)
            {
                // Obsługa błędów deserializacji JSON
                Console.WriteLine($"Błąd deserializacji JSON: {e.Message}");
                // Można wyświetlić komunikat błędu użytkownikowi
            }
            catch (Exception e)
            {
                // Obsługa innych błędów
                Console.WriteLine($"Wystąpił błąd: {e.Message}");
                // Można wyświetlić komunikat błędu użytkownikowi
            }
        }

        // TODO: Dodaj metody do obsługi deserializacji odpowiedzi JSON z API RAWG
        // Te klasy powinny odzwierciedlać strukturę odpowiedzi API
        // Możesz je przenieść do osobnego pliku lub folderu (np. ApiModels) w przyszłości

        // Klasa reprezentująca strukturę odpowiedzi API RAWG dla wyszukiwania
        public class RawgApiResponse
        {
            [JsonPropertyName("results")] // Mapuje pole "results" z JSON na tę właściwość
            public List<RawgGameResult>? Results { get; set; }
        }

        // Klasa reprezentująca pojedynczy wynik gry w odpowiedzi API RAWG
        public class RawgGameResult
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("released")]
            public DateTime? Released { get; set; }

            [JsonPropertyName("rating")]
            public double? Rating { get; set; }

            [JsonPropertyName("background_image")]
            public string? BackgroundImage { get; set; }

            [JsonPropertyName("genres")]
            public List<RawgGenre>? Genres { get; set; } // Lista gatunków

            [JsonPropertyName("platforms")]
            public List<RawgPlatformContainer>? Platforms { get; set; } // Lista platform
        }

        // Klasa reprezentująca gatunek w odpowiedzi API RAWG
        public class RawgGenre
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        // Klasa kontener na platformę w odpowiedzi API RAWG
        public class RawgPlatformContainer
        {
            [JsonPropertyName("platform")]
            public RawgPlatform? PlatformDetails { get; set; } // Detale platformy
        }

        // Klasa reprezentująca detale platformy w odpowiedzi API RAWG
        public class RawgPlatform
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        // Pamiętać o zwolnieniu zasobów HttpClient w realnej aplikacji (np. w metodzie Dispose)
        // W tym prostym przykładzie pomijamy zarządzanie zasobami HttpClient
    }
}
