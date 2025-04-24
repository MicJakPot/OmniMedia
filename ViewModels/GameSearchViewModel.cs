using ReactiveUI;
using System.Collections.ObjectModel;
using OmniMedia.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Avalonia.Threading;
using System.Web;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq; // <-- DODAJ TĘ LINIĘ

namespace OmniMedia.ViewModels
{
    // ViewModel dla funkcjonalności wyszukiwania gier
    public class GameSearchViewModel : ViewModelBase
    {
        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        private ObservableCollection<SearchResultItemViewModel> _searchResults = new ObservableCollection<SearchResultItemViewModel>();
        public ObservableCollection<SearchResultItemViewModel> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        // TODO: Właściwość na zaznaczoną grę i Komenda do dodawania do kolekcji

        private const string RawgApiKey = "ccd369cc710a46b2930caa85edadee4a"; // Mój prywatny klucz API
        private readonly HttpClient _httpClient = new HttpClient();

        public GameSearchViewModel()
        {
            SearchCommand = ReactiveCommand.CreateFromTask(PerformSearch,
                                                           this.WhenAnyValue(x => x.SearchQuery)
                                                               .Select(query => !string.IsNullOrWhiteSpace(query)));
        }

        private async Task PerformSearch()
        {
            SearchResults.Clear();

            var encodedQuery = HttpUtility.UrlEncode(SearchQuery);
            var apiUrl = $"https://api.rawg.io/api/games?key={RawgApiKey}&search={encodedQuery}&page_size=20";

            System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Wykonuję zapytanie API: {apiUrl}");

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Otrzymano odpowiedź API (fragment): {jsonString.Substring(0, Math.Min(jsonString.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<GameApiResponse>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Results != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var result in apiResponse.Results)
                        {
                            var game = new Game // Tworzymy obiekt modelu Game
                            {
                                Title = result.Name,
                                Genre = result.Genres != null && result.Genres.Any() ? string.Join(", ", result.Genres.Select(g => g.Name)) : "N/A",
                                Platform = result.Platforms != null && result.Platforms.Any() ? string.Join(", ", result.Platforms.Select(p => p.Platform.Name)) : "N/A",
                                ThumbnailUrl = result.Background_image
                            };
                            // Tworzymy ViewModel elementu listy, przekazując mu dane gry
                            var searchResultItem = new SearchResultItemViewModel(game);
                            SearchResults.Add(searchResultItem); // Dodajemy ViewModel do kolekcji wyników
                        }
                        System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Dodano {SearchResults.Count} wyników do listy.");
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GameSearchViewModel] API zwróciło pustą listę wyników lub błąd deserializacji.");
                }
            }
            catch (HttpRequestException e)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Błąd zapytania HTTP: {e.Message}");
            }
            catch (JsonException e)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Błąd deserializacji JSON: {e.Message}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSearchViewModel] Nieoczekiwany błąd: {e.Message}");
            }
        }

        // POMOCNICZE KLASY DO DESERIALIZACJI JSON
        public class GameApiResponse
        {
            [JsonPropertyName("results")]
            public List<GameApiResult>? Results { get; set; }
        }

        public class GameApiResult
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            [JsonPropertyName("released")]
            public DateTime? Released { get; set; }
            [JsonPropertyName("background_image")]
            public string? Background_image { get; set; }
            [JsonPropertyName("rating")]
            public double? Rating { get; set; }

            [JsonPropertyName("platforms")]
            public List<GameApiPlatform>? Platforms { get; set; }
            [JsonPropertyName("genres")]
            public List<GameApiGenre>? Genres { get; set; }
        }

        public class GameApiPlatform
        {
            [JsonPropertyName("platform")]
            public PlatformDetail? Platform { get; set; }
        }

        public class PlatformDetail
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        public class GameApiGenre
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }
}