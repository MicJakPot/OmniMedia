using SQLite; // Potrzebne do atrybutów bazy danych
using System; // Potrzebne do typów takich jak string, int, DateTime

namespace OmniMedia.Models
{
    // Klasa reprezentująca film przechowywany w naszej manualnej kolekcji filmów
    public class Movie
    {
        // Unikalny identyfikator filmu w bazie danych (klucz główny)
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Podstawowe dane o filmie
        public string? Title { get; set; } // Tytuł filmu (np. "Incepcja")
        public string? Director { get; set; } // Reżyser (np. "Christopher Nolan")
        public int? Year { get; set; } // Rok wydania (np. 2010)
        public string? Genre { get; set; } // Gatunek (np. "Sci-fi, Akcja")
        public int? DurationMinutes { get; set; } // Czas trwania w minutach (np. 148)

        // Dane dotyczące statusu posiadania i listy życzeń
        public bool IsOwned { get; set; } = false; // Czy użytkownik posiada ten film? (domyślnie False)
        public bool IsOnWishlist { get; set; } = false; // Czy film znajduje się na liście życzeń? (domyślnie False)

        // Dane dodatkowe i metadane
        public double? Rating { get; set; } // Ocena użytkownika lub średnia (np. 8.8)
        [MaxLength(1000)] // Ograniczamy długość streszczenia
        public string? PlotSummary { get; set; } // Krótkie streszczenie fabuły
        public string? CoverArtPath { get; set; } // Ścieżka do pliku okładki na dysku lub URL (jeśli ładowana z zewnątrz)

        // Dane fizyczne/formatowe
        public string? Format { get; set; } // Format nośnika (np. "Blu-ray", "DVD", "Cyfrowa")
        public string? Location { get; set; } // Miejsce przechowywania (np. "Półka A, Box 3")
        [MaxLength(500)] // Ograniczamy długość notatek
        public string? Notes { get; set; } // Dodatkowe notatki użytkownika

        // TODO: Możesz dodać inne pola, jeśli będą potrzebne, np.
        // public string? Actors { get; set; } // Lista głównych aktorów
        // public string? Language { get; set; } // Język filmu/audio

        // Domyślny konstruktor (wymagany przez SQLite-Net)
        public Movie()
        {
        }

        // TODO: Można dodać konstruktor ułatwiający tworzenie obiektów, jeśli będzie potrzebny
        // np. public Movie(string title, string director, int year) { ... }
    }
}
