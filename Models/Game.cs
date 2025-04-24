using System;
using System.Collections.Generic; // Możliwe, że potrzebne do listy gatunków/platform
using System.Text;
using SQLite;

namespace OmniMedia.Models // Przestrzeń nazw dla modeli danych
{
    // Klasa reprezentująca grę w aplikacji
    public class Game
    {
        // Unikalny identyfikator gry (przyda się w bazie danych)
        [PrimaryKey, AutoIncrement] 
        public int Id { get; set; }

        // Podstawowe informacje o grze, wszystkie sgtringi mogą być nullowalne
        public string? Title { get; set; }
        public string? Genre { get; set; } // Można zmienić na listę stringów lub osobną klasę w przyszłości
        public string? Platform { get; set; } // Można zmienić na listę stringów lub osobną klasę w przyszłości
        public DateTime? ReleasedDate { get; set; } // Data wydania (używamy nullable DateTime)
        public double? Rating { get; set; } // Ocena (używamy nullable double)

        // Ścieżka lub URL do miniatury/okładki gry
        public string? ThumbnailUrl { get; set; }

        // Dodatkowe informacje (można rozszerzyć w przyszłości)
        public string? Description { get; set; } // Np. krótki opis
        public string? Developer { get; set; }
        public string? Publisher { get; set; }

        // Konstruktor (opcjonalnie, ale przydatny)
        public Game()
        {
            // Domyślny konstruktor
        }

        // Można dodać metody specyficzne dla modelu Game w przyszłości
    }
}
