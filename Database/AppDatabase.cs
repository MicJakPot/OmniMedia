using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniMedia.Models; 
using SQLite; 

namespace OmniMedia.Database
{
    // Klasa obsługująca połączenie z bazą danych SQLite
    public class AppDatabase : SQLiteAsyncConnection // Dziedziczymy z SQLiteAsyncConnection dla operacji asynchronicznych
    {
        // Konstruktor klasy AppDatabase
        public AppDatabase(string dbPath) : base(dbPath)
        {
            // Konstruktor klasy bazowej (SQLiteAsyncConnection) inicjalizuje połączenie
        }

        // Metoda inicjalizująca strukturę bazy danych (tworzy tabele)
        public async Task CreateTablesAsync()
        {
            // Tworzy tabelę dla modelu Game, jeśli jeszcze nie istnieje
            await CreateTableAsync<Game>();
            // Można dodać tworzenie tabel dla innych modeli (Music, Movie) w przyszłości
        }

        // Metoda do zapisywania (lub aktualizowania) obiektu Game w bazie
        public Task<int> SaveGameAsync(Game game)
        {
            if (game.Id != 0)
            {
                // Obiekt już istnieje (ma Id), więc aktualizujemy
                return UpdateAsync(game);
            }
            else
            {
                // Obiekt jest nowy (Id == 0), więc wstawiamy
                return InsertAsync(game);
            }
        }

        // Metoda do pobierania wszystkich gier z bazy
        public Task<List<Game>> GetGamesAsync()
        {
            // Pobiera wszystkie wiersze z tabeli Games
            return Table<Game>().ToListAsync();
        }

        // Metoda do pobierania pojedynczej gry po Id (opcjonalnie)
        public Task<Game> GetGameAsync(int id)
        {
            return Table<Game>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        // Metoda do usuwania gry (opcjonalnie)
        public Task<int> DeleteGameAsync(Game game)
        {
            return DeleteAsync(game);
        }
    }
}
