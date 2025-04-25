using SQLite; // Potrzebne do atrybutów bazy danych
using System; // Potrzebne do DateTime


namespace OmniMedia.Models
{
    // Klasa reprezentująca album muzyczny przechowywany w naszej kolekcji (bazie danych)
    public class MusicAlbum
    {
        // Unikalny identyfikator albumu w naszej bazie danych
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Id albumu w iTunes (przydatne do wyszukania szczegółów itp.)
        public int iTunesId { get; set; } // Nazwa właściwości może być inna niż w API/bibliotece, jeśli mapujemy ręcznie

        // Tytuł albumu
        public string? Title { get; set; }

        // Nazwa artysty
        public string? Artist { get; set; }

        // Adres URL do okładki albumu
        public string? CoverUrl { get; set; } // Będziemy przechowywać URL okładki w naszej bazie
        public string? Genre { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public double? CollectionPrice { get; set; }
        
        public MusicAlbum()
        {
        }

        // Opcjonalny konstruktor do tworzenia obiektu z danych zwróconych przez iTunesSearch.Library
        // Zakładamy, że iTunesSearch.Library ma klasę wynikową np. AlbumResult z polami CollectionName, ArtistName, ArtworkUrl100
        public MusicAlbum(int iTunesId, string artist, string title, string coverUrl)
        {
            this.iTunesId = iTunesId;
            Artist = artist;
            Title = title;
            CoverUrl = coverUrl;

        }

        // TODO: Możesz dodać metody lub logikę specyficzną dla modelu MusicAlbum w przyszłości
    }
}
