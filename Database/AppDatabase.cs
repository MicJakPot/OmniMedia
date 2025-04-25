using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite; // Potrzebne do SQLiteAsyncConnection i atrybutów
using OmniMedia.Models; // Potrzebne do modeli Game i MusicAlbum
using System.Diagnostics; // Potrzebne do Debug.WriteLine

namespace OmniMedia.Database
{
    // Klasa obsługująca połączenie z bazą danych SQLite i operacje CRUD na danych
    public class AppDatabase
    {
        // Asynchroniczne połączenie z bazą danych SQLite
        private readonly SQLiteAsyncConnection _database;

        // Konstruktor - inicjalizuje połączenie z bazą danych i tworzy tabele
        public AppDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            // Ważne: tworzenie tabel powinno odbywać się asynchronicznie, ale
            // w konstruktorze używamy Wait(), aby mieć pewność, że tabele istnieją przed dalszym użyciem bazy.
            // Lepszym podejściem w bardziej złożonych aplikacjach byłoby wywołanie CreateTablesAsync
            // w punkcie wejścia aplikacji i poczekanie na jego zakończenie.
            Task.Run(() => CreateTablesAsync()).Wait(); // Wywołujemy tworzenie tabel przy starcie
        }

        // Metoda tworząca tabele w bazie danych
        public Task CreateTablesAsync()
        {
            Debug.WriteLine("[AppDatabase] Tworzenie lub weryfikacja tabel bazy danych...");
            // Tworzy tabelę Game, jeśli jeszcze nie istnieje
            // Tworzy tabelę MusicAlbum, jeśli jeszcze nie istnieje
            // CreateTablesAsync może przyjąć wiele typów modeli jednocześnie
            return _database.CreateTablesAsync<Game, MusicAlbum>();
            // Można dodać tworzenie innych tabel tutaj (np. Movie, jeśli będziesz je miał)
            // return _database.CreateTablesAsync<Game, MusicAlbum, Movie>();
        }

        // --- Operacje dla Game ---

        // Metoda pobierająca wszystkie gry z bazy danych
        public Task<List<Game>> GetGamesAsync()
        {
            Debug.WriteLine("[AppDatabase] Rozpoczynam pobieranie wszystkich gier z bazy danych...");
            // Zwraca listę wszystkich obiektów Game z tabeli
            var gamesListTask = _database.Table<Game>().ToListAsync();
            Debug.WriteLine("[AppDatabase] Zakończono pobieranie gier.");
            return gamesListTask;
        }

        // Metoda pobierająca konkretną grę po ID
        public async Task<Game?> GetGameAsync(int id) // Zmieniono na async Task<Game?>
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam pobieranie gry o ID: {id} z bazy danych...");
            // Zwraca grę z konkretnym ID, lub null jeśli nie znaleziono
            var game = await _database.Table<Game>().Where(i => i.Id == id).FirstOrDefaultAsync(); // Czekamy na wynik
            Debug.WriteLine($"[AppDatabase] Zakończono pobieranie gry o ID: {id}.");
            return game;
        }

        // Metoda zapisująca (wstawiająca lub aktualizująca) grę w bazie danych
        public Task<int> SaveGameAsync(Game game)
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam zapisywanie gry '{game.Title}' (ID: {game.Id}) do bazy danych...");
            if (game.Id != 0)
            {
                // Jeśli Id jest różne od 0, zakładamy, że rekord już istnieje i go aktualizujemy
                Debug.WriteLine($"[AppDatabase] Aktualizacja gry o ID: {game.Id}");
                var result = _database.UpdateAsync(game);
                Debug.WriteLine($"[AppDatabase] Zakończono aktualizację gry o ID: {game.Id}.");
                return result;
            }
            else
            {
                // Jeśli Id jest 0, zakładamy, że to nowy rekord i go wstawiamy
                Debug.WriteLine($"[AppDatabase] Wstawianie nowej gry: {game.Title}");
                var result = _database.InsertAsync(game);
                Debug.WriteLine($"[AppDatabase] Zakończono wstawianie nowej gry: {game.Title}.");
                return result;
            }
        }

        // Metoda usuwająca grę z bazy danych
        public Task<int> DeleteGameAsync(Game game)
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam usuwanie gry '{game.Title}' o ID: {game.Id} z bazy danych...");
            // Usuwa rekord odpowiadający przekazanemu obiektowi Game
            var result = _database.DeleteAsync(game);
            Debug.WriteLine($"[AppDatabase] Zakończono usuwanie gry o ID: {game.Id}.");
            return result;
        }

        // --- Dodajemy operacje dla MusicAlbum ---

        // Metoda pobierająca wszystkie albumy muzyczne z bazy danych
        public Task<List<MusicAlbum>> GetMusicAlbumsAsync()
        {
            Debug.WriteLine("[AppDatabase] Rozpoczynam pobieranie wszystkich albumów muzycznych z bazy danych...");
            // Zwraca listę wszystkich obiektów MusicAlbum z tabeli
            var albumsListTask = _database.Table<MusicAlbum>().ToListAsync();
            Debug.WriteLine("[AppDatabase] Zakończono pobieranie albumów muzycznych.");
            return albumsListTask;
        }

        // Metoda pobierająca konkretny album muzyczny po ID
        public async Task<MusicAlbum?> GetMusicAlbumAsync(int id) // Zmieniono na async Task<MusicAlbum?>
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam pobieranie albumu muzycznego o ID: {id} z bazy danych...");
            // Zwraca album z konkretnym ID, lub null jeśli nie znaleziono
            var album = await _database.Table<MusicAlbum>().Where(i => i.Id == id).FirstOrDefaultAsync(); // Czekamy na wynik
            Debug.WriteLine($"[AppDatabase] Zakończono pobieranie albumu muzycznego o ID: {id}.");
            return album; // Zwracamy wynik (może być null)
        }

        // Metoda zapisująca (wstawiająca lub aktualizująca) album muzyczny w bazie danych
        public Task<int> SaveMusicAlbumAsync(MusicAlbum musicAlbum)
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam zapisywanie albumu '{musicAlbum.Title}' (ID: {musicAlbum.Id}) do bazy danych...");
            if (musicAlbum.Id != 0)
            {
                // Jeśli Id jest różne od 0, zakładamy, że rekord już istnieje i go aktualizujemy
                Debug.WriteLine($"[AppDatabase] Aktualizacja albumu o ID: {musicAlbum.Id}");
                var result = _database.UpdateAsync(musicAlbum);
                Debug.WriteLine($"[AppDatabase] Zakończono aktualizację albumu o ID: {musicAlbum.Id}.");
                return result;
            }
            else
            {
                // Jeśli Id jest 0, zakładamy, że to nowy rekord i go wstawiamy
                Debug.WriteLine($"[AppDatabase] Wstawianie nowego albumu: {musicAlbum.Title}");
                var result = _database.InsertAsync(musicAlbum);
                Debug.WriteLine($"[AppDatabase] Zakończono wstawianie nowego albumu: {musicAlbum.Title}.");
                return result;
            }
        }

        // Metoda usuwająca album muzyczny z bazy danych
        public Task<int> DeleteMusicAlbumAsync(MusicAlbum musicAlbum)
        {
            Debug.WriteLine($"[AppDatabase] Rozpoczynam usuwanie albumu '{musicAlbum.Title}' o ID: {musicAlbum.Id} z bazy danych...");
            // Usuwa rekord odpowiadający przekazanemu obiektowi MusicAlbum
            var result = _database.DeleteAsync(musicAlbum);
            Debug.WriteLine($"[AppDatabase] Zakończono usuwanie albumu o ID: {musicAlbum.Id}.");
            return result;
        }

        // TODO: Można dodać metody dla innych typów danych (filmy)
        // public Task<List<Movie>> GetMoviesAsync() { ... }
        // public Task<int> SaveMovieAsync(Movie movie) { ... }
        // public Task<int> DeleteMovieAsync(Movie movie) { ... }
    }
}