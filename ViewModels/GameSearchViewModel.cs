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
using System.Linq;
using System.Diagnostics;
using OmniMedia.Database; // Potrzebne do AppDatabase

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

        // Właściwość na zaznaczoną grę na liście wyników
        private SearchResultItemViewModel? _selectedGame;
        public SearchResultItemViewModel? SelectedGame
        {
            get => _selectedGame;
            set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
        }

        // NOWA KOMENDA: Komenda do dodawania zaznaczonej gry do kolekcji
        public ReactiveCommand<Unit, Unit> AddToCollectionCommand { get; }


        private const string RawgApiKey = "ccd369cc710a46b2930caa85edadee4a"; 
        private readonly HttpClient _httpClient = new HttpClient();

        // Konstruktor ViewModelu
        public GameSearchViewModel()
        {
            // Inicjalizacja komendy wyszukiwania
            SearchCommand = ReactiveCommand.CreateFromTask(PerformSearch,
                                                           this.WhenAnyValue(x => x.SearchQuery)
                                                               .Select(query => !string.IsNullOrWhiteSpace(query)));

            // NOWA INICJALIZACJA KOMENDY: Dodaj do kolekcji
            // Komenda będzie aktywna tylko, gdy SelectedGame nie jest null
            AddToCollectionCommand = ReactiveCommand.CreateFromTask(PerformAddToCollection,
                                                           this.WhenAnyValue(x => x.SelectedGame)
                                                                .Select(selectedGame => selectedGame != null)); // Komenda aktywna, gdy coś jest zaznaczone
        }

        // ... (metoda PerformSearch - pozostała bez zmian w tej modyfikacji) ...
        private async Task PerformSearch()
        {
            // ... (Twoja istniejąca implementacja metody PerformSearch) ...
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
                            var game = new Game
                            {
                                Title = result.Name,
                                Genre = result.Genres != null && result.Genres.Any() ? string.Join(", ", result.Genres.Select(g => g.Name)) : "N/A",
                                Platform = result.Platforms != null && result.Platforms.Any() ? string.Join(", ", result.Platforms.Select(p => p.Platform.Name)) : "N/A",
                                ThumbnailUrl = result.Background_image
                            };
                            var searchResultItem = new SearchResultItemViewModel(game);
                            SearchResults.Add(searchResultItem);
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


        // NOWA METODA: Wykonuje logikę dodawania zaznaczonej gry do bazy danych
        private async Task PerformAddToCollection()
        {
            // Sprawdzamy, czy jakaś gra jest zaznaczona
            if (SelectedGame?.GameData != null)
            {
                Debug.WriteLine($"[GameSearchViewModel] Próba dodania do kolekcji gry: {SelectedGame.GameData.Title}");

                try
                {
                    // Wywołujemy metodę zapisu do bazy danych
                    int result = await App.Database.SaveGameAsync(SelectedGame.GameData);

                    Debug.WriteLine($"[GameSearchViewModel] Zapisano grę do bazy danych. Rezultat: {result}");

                    // TODO: Poinformuj użytkownika o sukcesie (np. wyświetl komunikat w UI)
                    // Przykładowo (wymagałoby dodania właściwości tekstowej w ViewModelu i kontrolki w View)
                    // StatusMessage = $"Dodano '{SelectedGame.GameData.Title}' do kolekcji.";


                    // TODO: Zaktualizuj widok kolekcji (aby nowo dodana gra była widoczna po jego otwarciu)
                    // To wymaga mechanizmu komunikacji między ViewModelami (np. MessageBus)
                    // Na razie komunikat debugowania
                    Debug.WriteLine($"[GameSearchViewModel] Gra '{SelectedGame.GameData.Title}' dodana do bazy. Widok kolekcji wymaga odświeżenia przy otwarciu.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GameSearchViewModel] Błąd podczas zapisywania gry do bazy: {ex.Message}");
                    // TODO: Poinformuj użytkownika o błędzie zapisu
                    // Przykładowo:
                    // StatusMessage = $"Błąd zapisu gry '{SelectedGame.GameData.Title}': {ex.Message}";
                }
            }
            else
            {
                Debug.WriteLine("[GameSearchViewModel] Próba dodania do kolekcji, ale żadna gra nie jest zaznaczona.");
            }
        }


        // POMOCNICZE KLASY DO DESERIALIZACJI JSON (pozostają bez zmian)
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