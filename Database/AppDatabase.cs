using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite; // Potrzebne do SQLiteAsyncConnection i atrybutów
using OmniMedia.Models; // Potrzebne do modelu Game
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
            // Tworzenie tabeli Game (i innych, jeśli będą potrzebne)
            // Ważne: tworzenie tabel powinno odbywać się asynchronicznie
            // Najlepiej wywołać CreateTablesAsync zaraz po utworzeniu instancji AppDatabase
            // Task.Run(() => CreateTablesAsync()).Wait(); // Można też wywołać gdzieś na początku aplikacji
        }

        // Metoda tworząca tabele w bazie danych
        public Task CreateTablesAsync()
        {
            Debug.WriteLine("[AppDatabase] Tworzenie tabel bazy danych...");
            // Tworzy tabelę Game, jeśli jeszcze nie istnieje
            return _database.CreateTableAsync<Game>();
            // Można dodać tworzenie innych tabel tutaj
            // return _database.CreateTablesAsync<Game, Music, Movie>(); // Przykład dla wielu tabel
        }

        // Metoda pobierająca wszystkie gry z bazy danych
        public Task<List<Game>> GetGamesAsync()
        {
            Debug.WriteLine("[AppDatabase] Pobieranie wszystkich gier z bazy danych...");
            // Zwraca listę wszystkich obiektów Game z tabeli
            return _database.Table<Game>().ToListAsync();
        }

        // Metoda pobierająca konkretną grę po ID
        public Task<Game?> GetGameAsync(int id)
        {
            Debug.WriteLine($"[AppDatabase] Pobieranie gry o ID: {id} z bazy danych...");
            // Zwraca grę z konkretnym ID, lub null jeśli nie znaleziono
            return _database.Table<Game>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        // Metoda zapisująca (wstawiająca lub aktualizująca) grę w bazie danych
        public Task<int> SaveGameAsync(Game game)
        {
            Debug.WriteLine($"[AppDatabase] Zapisywanie gry '{game.Title}' do bazy danych...");
            if (game.Id != 0)
            {
                // Jeśli Id jest różne od 0, zakładamy, że rekord już istnieje i go aktualizujemy
                Debug.WriteLine($"[AppDatabase] Aktualizacja gry o ID: {game.Id}");
                return _database.UpdateAsync(game);
            }
            else
            {
                // Jeśli Id jest 0, zakładamy, że to nowy rekord i go wstawiamy
                Debug.WriteLine($"[AppDatabase] Wstawianie nowej gry: {game.Title}");
                return _database.InsertAsync(game);
            }
        }

        // NOWA METODA: Metoda usuwająca grę z bazy danych
        public Task<int> DeleteGameAsync(Game game)
        {
            Debug.WriteLine($"[AppDatabase] Usuwanie gry '{game.Title}' o ID: {game.Id} z bazy danych...");
            // Usuwa rekord odpowiadający przekazanemu obiektowi Game
            return _database.DeleteAsync(game);
        }

        // TODO: Można dodać metody dla innych operacji i typów danych (muzyka, filmy)
        // public Task<List<Music>> GetMusicAsync() { ... }
        // public Task<int> SaveMusicAsync(Music music) { ... }
        // public Task<int> DeleteMusicAsync(Music music) { ... }
        // Analogicznie dla filmów
    }
}