using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OmniMedia.Models; // Potrzebne do modelu Movie

namespace OmniMedia.Database
{
    // Klasa obsługująca połączenie z odrębną bazą danych filmów SQLite i operacje CRUD na danych filmów
    public class MovieDatabase
    {
        // Asynchroniczne połączenie z bazą danych filmów SQLite
        private readonly SQLiteAsyncConnection _database;

        // Konstruktor - inicjalizuje połączenie z bazą danych (dla filmów) i tworzy tabelę Movie
        public MovieDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            // Tworzenie tabeli Movie (jeśli jeszcze nie istnieje)
            // Ważne: tworzenie tabel powinno odbywać się asynchronicznie,
            // ale w konstruktorze używamy Wait(), aby mieć pewność, że tabela istnieje przed dalszym użyciem.
            Task.Run(() => CreateTablesAsync()).Wait(); // Wywołujemy tworzenie tabel przy starcie
        }

        // Metoda tworząca tabele w bazie danych filmów (tylko Movie)
        public Task CreateTablesAsync()
        {
            Debug.WriteLine("[MovieDatabase] Tworzenie lub weryfikacja tabel bazy danych filmów...");
            // Tworzy tabelę Movie, jeśli jeszcze nie istnieje
            return _database.CreateTableAsync<Movie>();
        }

        // --- Operacje dla Movie ---

        // Metoda pobierająca wszystkie filmy z bazy danych
        public Task<List<Movie>> GetMoviesAsync()
        {
            Debug.WriteLine("[MovieDatabase] Rozpoczynam pobieranie wszystkich filmów z bazy danych...");
            // Zwraca listę wszystkich obiektów Movie z tabeli
            var moviesListTask = _database.Table<Movie>().ToListAsync();
            Debug.WriteLine("[MovieDatabase] Zakończono pobieranie filmów.");
            return moviesListTask;
        }

        // Metoda pobierająca konkretny film po ID
        public async Task<Movie?> GetMovieAsync(int id) // Zmieniono na async Task<Movie?>
        {
            Debug.WriteLine($"[MovieDatabase] Rozpoczynam pobieranie filmu o ID: {id} z bazy danych...");
            // Zwraca film z konkretnym ID, lub null jeśli nie znaleziono
            var movie = await _database.Table<Movie>().Where(i => i.Id == id).FirstOrDefaultAsync();
            Debug.WriteLine($"[MovieDatabase] Zakończono pobieranie filmu o ID: {id}.");
            return movie; // Zwracamy wynik (może być null)
        }


        // Metoda zapisująca (wstawiająca lub aktualizująca) film w bazie danych
        public async Task<int> SaveMovieAsync(Movie movie) // Zmieniono na async Task<int>
        {
            Debug.WriteLine($"[MovieDatabase] Rozpoczynam zapisywanie filmu '{movie.Title}' (ID: {movie.Id}) do bazy danych...");
            if (movie.Id != 0)
            {
                // Jeśli Id jest różne od 0, zakładamy, że rekord już istnieje i go aktualizujemy
                Debug.WriteLine($"[MovieDatabase] Aktualizacja filmu o ID: {movie.Id}");
                var result = await _database.UpdateAsync(movie); // Użyto await
                Debug.WriteLine($"[MovieDatabase] Zakończono aktualizację filmu o ID: {movie.Id}.");
                return result;
            }
            else
            {
                // Jeśli Id jest 0, zakładamy, że to nowy rekord i go wstawiamy
                Debug.WriteLine($"[MovieDatabase] Wstawianie nowego filmu: {movie.Title}");
                var result = await _database.InsertAsync(movie); // Użyto await
                Debug.WriteLine($"[MovieDatabase] Zakończono wstawianie nowego filmu: {movie.Title}.");
                return result;
            }
        }

        // Metoda usuwająca film z bazy danych
        public async Task<int> DeleteMovieAsync(Movie movie) // Zmieniono na async Task<int>
        {
            Debug.WriteLine($"[MovieDatabase] Rozpoczynam usuwanie filmu '{movie.Title}' o ID: {movie.Id} z bazy danych...");
            // Usuwa rekord odpowiadający przekazanemu obiektowi Movie
            var result = await _database.DeleteAsync(movie); // Użyto await
            Debug.WriteLine($"[MovieDatabase] Zakończono usuwanie filmu o ID: {movie.Id}.");
            return result;
        }

        // TODO: Możesz dodać inne metody, jeśli będą potrzebne
        // np. GetOwnedMoviesAsync(), GetWishlistMoviesAsync()
    }
}
