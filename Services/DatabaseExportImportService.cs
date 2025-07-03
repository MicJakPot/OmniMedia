using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OmniMedia.Models;
using OmniMedia.Database;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ClosedXML.Excel;

namespace OmniMedia.Services
{
    // Serwis odpowiedzialny za eksport danych bazy do pliku PDF
    // oraz import danych z pliku XLS/XLSX
    public static class DatabaseExportImportService
    {
        // Eksportuje całą zawartość bazy do pliku PDF w podanej ścieżce
        public static async Task ExportDatabaseToPdfAsync(string pdfPath)
        {
            // Pobieramy dane z baz
            var games = await App.Database.GetGamesAsync();
            var albums = await App.Database.GetMusicAlbumsAsync();
            var movies = await App.MovieDatabase.GetMoviesAsync();

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            double y = 40;

            // Tytuł dokumentu
            gfx.DrawString("OmniMedia - Eksport bazy", new XFont("Arial", 20, XFontStyle.Bold), XBrushes.Black, new XPoint(40, y));
            y += 40;

            void WriteSection(string title, IEnumerable<string> lines)
            {
                gfx.DrawString(title, new XFont("Arial", 16, XFontStyle.Bold), XBrushes.Black, new XPoint(40, y));
                y += 25;
                foreach (var line in lines)
                {
                    if (y > page.Height - 40)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                    gfx.DrawString(line, new XFont("Arial", 12, XFontStyle.Regular), XBrushes.Black, new XPoint(60, y));
                    y += 20;
                }
                y += 20;
            }

            WriteSection("Gry", games.Select(g => $"{g.Title} ({g.Platform})"));
            WriteSection("Albumy muzyczne", albums.Select(a => $"{a.Artist} - {a.Title}"));
            WriteSection("Filmy", movies.Select(m => $"{m.Title} ({m.Year})"));

            document.Save(pdfPath);
        }

        // Importuje dane z pliku XLS/XLSX
        // Zakładamy prostą strukturę: kolumna A - typ (Game/MusicAlbum/Movie)
        // kolumna B - pierwszy tytuł/pola dodatkowe zależnie od typu
        public static async Task ImportFromXlsAsync(string xlsPath)
        {
            using var workbook = new XLWorkbook(xlsPath);
            var ws = workbook.Worksheet(1);
            foreach (var row in ws.RowsUsed().Skip(1)) // pomijamy nagłówek
            {
                var type = row.Cell(1).GetString();
                if (string.Equals(type, "Game", StringComparison.OrdinalIgnoreCase))
                {
                    var game = new Game
                    {
                        Title = row.Cell(2).GetString(),
                        Platform = row.Cell(3).GetString()
                    };
                    await App.Database.SaveGameAsync(game);
                }
                else if (string.Equals(type, "MusicAlbum", StringComparison.OrdinalIgnoreCase))
                {
                    var album = new MusicAlbum
                    {
                        Artist = row.Cell(2).GetString(),
                        Title = row.Cell(3).GetString()
                    };
                    await App.Database.SaveMusicAlbumAsync(album);
                }
                else if (string.Equals(type, "Movie", StringComparison.OrdinalIgnoreCase))
                {
                    var movie = new Movie
                    {
                        Title = row.Cell(2).GetString(),
                        Director = row.Cell(3).GetString()
                    };
                    await App.MovieDatabase.SaveMovieAsync(movie);
                }
            }
        }
    }
}
