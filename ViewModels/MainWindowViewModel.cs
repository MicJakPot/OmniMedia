using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OmniMedia.Database;
using OmniMedia.Models;
using OmniMedia.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;            
using System.Reflection;
using System.Threading.Tasks;
using MsBox.Avalonia;              
using MsBox.Avalonia.Enums;        
using QuestPDF.Fluent;             
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;     
using ReactiveUnit = System.Reactive.Unit;

namespace OmniMedia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenCollectionCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenGameSearchCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenMusicSearchCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenMovieSearchCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> ExportDatabaseCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> ImportDatabaseCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenSettingsCommand { get; }
        public ReactiveCommand<ReactiveUnit, ReactiveUnit> OpenAboutCommand { get; }

        private object? _currentContent;
        public object? CurrentContent
        {
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        private Bitmap? _logoImage;
        private readonly AppDatabase _appDatabase;
        private readonly MovieDatabase _movieDatabase;

        public MainWindowViewModel(AppDatabase appDatabase, MovieDatabase movieDatabase)
        {
            _appDatabase = appDatabase;
            _movieDatabase = movieDatabase;

            OpenCollectionCommand = ReactiveCommand.Create(() =>
            {
                var window = new Views.CollectionWindow
                {
                    DataContext = new CollectionWindowViewModel()
                };
                window.Show();
                Debug.WriteLine("Kliknięto: Przeglądaj Kolekcję");
            });

            OpenGameSearchCommand = ReactiveCommand.Create(() =>
            {
                var window = new Views.GameSearchWindow
                {
                    DataContext = new GameSearchViewModel()
                };
                window.Show();
                Debug.WriteLine("Kliknięto: Szukaj Gry");
            });

            OpenMusicSearchCommand = ReactiveCommand.Create(() =>
            {
                var window = new Views.MusicSearchWindow
                {
                    DataContext = new MusicSearchViewModel()
                };
                window.Show();
                Debug.WriteLine("Kliknięto: Szukaj Muzyki");
            });

            OpenMovieSearchCommand = ReactiveCommand.Create(() =>
            {
                Debug.WriteLine("Kliknięto: Szukaj Filmów (TODO)");
            });

            ExportDatabaseCommand = ReactiveCommand.CreateFromTask(ExportDatabaseAsync);
            ImportDatabaseCommand = ReactiveCommand.CreateFromTask(ImportDatabaseAsync);

            OpenSettingsCommand = ReactiveCommand.Create(() =>
            {
                Debug.WriteLine("Kliknięto: Ustawienia (TODO)");
            });

            OpenAboutCommand = ReactiveCommand.Create(() =>
            {
                Debug.WriteLine("Kliknięto: O Twórcach (TODO)");
            });

            _logoImage = LoadLogoImage();
            CurrentContent = _logoImage;
        }

        // ------------------------------------------------------------------------------------------------
        // Import bazy z Excela
        private async Task ImportDatabaseAsync()
        {
            Debug.WriteLine("Rozpoczynam import...");

            var desktop = Avalonia.Application.Current?.ApplicationLifetime
                          as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", "Nie można uzyskać dostępu do okna aplikacji.", ButtonEnum.Ok)
                    .ShowAsync();
                return;
            }

            var top = TopLevel.GetTopLevel(desktop.MainWindow);
            if (top == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", "Problem z otwarciem okna wyboru pliku.", ButtonEnum.Ok)
                    .ShowAsync();
                return;
            }

            var opts = new FilePickerOpenOptions
            {
                Title = "Wybierz plik Excel",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Excel") { Patterns = new[] { "*.xls", "*.xlsx" } },
                    new FilePickerFileType("Wszystkie") { Patterns = new[] { "*" } }
                }
            };

            var files = await top.StorageProvider.OpenFilePickerAsync(opts);
            if (files == null || files.Count == 0)
            {
                Debug.WriteLine("Import anulowany.");
                return;
            }

            var path = files[0].Path.LocalPath;
            var ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext != ".xls" && ext != ".xlsx")
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", "Wybierz plik .xls lub .xlsx", ButtonEnum.Ok)
                    .ShowAsync();
                return;
            }

            try
            {
                await ImportFromExcel(path);
                await MessageBoxManager
                    .GetMessageBoxStandard("Sukces", "Import zakończony pomyślnie", ButtonEnum.Ok)
                    .ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Import error: {ex}");
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", $"Import nie powiódł się: {ex.Message}", ButtonEnum.Ok)
                    .ShowAsync();
            }
        }

        private async Task ImportFromExcel(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            IWorkbook workbook = Path.GetExtension(filePath).ToLower() switch
            {
                ".xlsx" => new XSSFWorkbook(fs),
                _ => new HSSFWorkbook(fs)
            };

            // Import “Games”
            var sheet = workbook.GetSheet("Games");
            if (sheet != null)
            {
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;
                    var title = GetCellValue(row.GetCell(0));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var game = new Game
                    {
                        Title = title,
                        Genre = GetCellValue(row.GetCell(1)),
                        Platform = GetCellValue(row.GetCell(2)),
                        ReleasedDate = ParseDate(GetCellValue(row.GetCell(3))),
                        Rating = ParseDouble(GetCellValue(row.GetCell(4))),
                        ThumbnailUrl = GetCellValue(row.GetCell(5)),
                        Description = GetCellValue(row.GetCell(6)),
                        Developer = GetCellValue(row.GetCell(7)),
                        Publisher = GetCellValue(row.GetCell(8))
                    };
                    await _appDatabase.SaveGameAsync(game);
                }
            }

            // Import “Movies”
            sheet = workbook.GetSheet("Movies");
            if (sheet != null)
            {
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;
                    var title = GetCellValue(row.GetCell(0));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var movie = new Movie
                    {
                        Title = title,
                        Director = GetCellValue(row.GetCell(1)),
                        Year = ParseInt(GetCellValue(row.GetCell(2))),
                        Genre = GetCellValue(row.GetCell(3)),
                        DurationMinutes = ParseInt(GetCellValue(row.GetCell(4))),
                        IsOwned = ParseBool(GetCellValue(row.GetCell(5))),
                        IsOnWishlist = ParseBool(GetCellValue(row.GetCell(6))),
                        Rating = ParseDouble(GetCellValue(row.GetCell(7))),
                        PlotSummary = GetCellValue(row.GetCell(8)),
                        CoverArtPath = GetCellValue(row.GetCell(9)),
                        Format = GetCellValue(row.GetCell(10)),
                        Location = GetCellValue(row.GetCell(11)),
                        Notes = GetCellValue(row.GetCell(12))
                    };
                    await _movieDatabase.SaveMovieAsync(movie);
                }
            }

            // Import “MusicAlbums”
            sheet = workbook.GetSheet("MusicAlbums");
            if (sheet != null)
            {
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;
                    var title = GetCellValue(row.GetCell(1));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var album = new MusicAlbum
                    {
                        iTunesId = ParseInt(GetCellValue(row.GetCell(0))) ?? 0,
                        Title = title,
                        Artist = GetCellValue(row.GetCell(2)),
                        CoverUrl = GetCellValue(row.GetCell(3)),
                        Genre = GetCellValue(row.GetCell(4)),
                        ReleasedDate = ParseDate(GetCellValue(row.GetCell(5))),
                        CollectionPrice = ParseDouble(GetCellValue(row.GetCell(6)))
                    };
                    await _appDatabase.SaveMusicAlbumAsync(album);
                }
            }
        }

        // ------------------------------------------------------------------------------------------------
        // Eksport bazy: wybór pomiędzy Excel i PDF
        private async Task ExportDatabaseAsync()
        {
            Debug.WriteLine("Rozpoczynam eksport...");

            var desktop = Avalonia.Application.Current?.ApplicationLifetime
                          as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", "Nie można uzyskać dostępu do okna aplikacji.", ButtonEnum.Ok)
                    .ShowAsync();
                return;
            }

            var top = TopLevel.GetTopLevel(desktop.MainWindow);
            if (top == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", "Problem z otwarciem okna zapisu pliku.", ButtonEnum.Ok)
                    .ShowAsync();
                return;
            }

            var opts = new FilePickerSaveOptions
            {
                Title = "Zapisz bazę danych",
                SuggestedFileName = "OmniMedia_Export",
                DefaultExtension = "xlsx",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Excel .xlsx") { Patterns = new[] { "*.xlsx" } },
                    new FilePickerFileType("Excel .xls")  { Patterns = new[] { "*.xls" } },
                    new FilePickerFileType("PDF")         { Patterns = new[] { "*.pdf" } },
                    new FilePickerFileType("Wszystkie")   { Patterns = new[] { "*" } }
                }
            };

            var file = await top.StorageProvider.SaveFilePickerAsync(opts);
            if (file == null)
            {
                Debug.WriteLine("Eksport anulowany.");
                return;
            }

            var path = file.Path.LocalPath;
            var ext = Path.GetExtension(path).ToLowerInvariant();

            try
            {
                if (ext == ".xlsx" || ext == ".xls")
                {
                    await ExportToExcel(path);
                    await MessageBoxManager
                        .GetMessageBoxStandard("Sukces", "Eksport do Excela zakończony", ButtonEnum.Ok)
                        .ShowAsync();
                }
                else if (ext == ".pdf")
                {
                    await ExportToPdf(path);
                    await MessageBoxManager
                        .GetMessageBoxStandard("Sukces", "Eksport do PDF zakończony", ButtonEnum.Ok)
                        .ShowAsync();
                }
                else
                {
                    await MessageBoxManager
                        .GetMessageBoxStandard("Błąd", $"Nieobsługiwany format: {ext}", ButtonEnum.Ok)
                        .ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Export error: {ex}");
                await MessageBoxManager
                    .GetMessageBoxStandard("Błąd", $"Export nie powiódł się: {ex.Message}", ButtonEnum.Ok)
                    .ShowAsync();
            }
        }

        private async Task ExportToExcel(string filePath)
        {
            IWorkbook workbook;
            if (filePath.EndsWith(".xlsx", StringComparison.InvariantCultureIgnoreCase))
                workbook = new XSSFWorkbook();
            else
                workbook = new HSSFWorkbook();

            // --- gry ---
            var games = await _appDatabase.GetGamesAsync();
            var sheetGames = workbook.CreateSheet("Games");
            int rowIndex = 0;
            var hdrGames = sheetGames.CreateRow(rowIndex++);
            hdrGames.CreateCell(0).SetCellValue("Title");
            hdrGames.CreateCell(1).SetCellValue("Genre");
            hdrGames.CreateCell(2).SetCellValue("Platform");
            hdrGames.CreateCell(3).SetCellValue("ReleasedDate");
            hdrGames.CreateCell(4).SetCellValue("Rating");
            hdrGames.CreateCell(5).SetCellValue("ThumbnailUrl");
            hdrGames.CreateCell(6).SetCellValue("Description");
            hdrGames.CreateCell(7).SetCellValue("Developer");
            hdrGames.CreateCell(8).SetCellValue("Publisher");

            foreach (var g in games)
            {
                var row = sheetGames.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(g.Title);
                row.CreateCell(1).SetCellValue(g.Genre);
                row.CreateCell(2).SetCellValue(g.Platform);
                row.CreateCell(3).SetCellValue(g.ReleasedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
                row.CreateCell(4).SetCellValue(g.Rating ?? 0.0);
                row.CreateCell(5).SetCellValue(g.ThumbnailUrl);
                row.CreateCell(6).SetCellValue(g.Description);
                row.CreateCell(7).SetCellValue(g.Developer);
                row.CreateCell(8).SetCellValue(g.Publisher);
            }

            // --- filmy ---
            var movies = await _movieDatabase.GetMoviesAsync();
            var sheetMovies = workbook.CreateSheet("Movies");
            rowIndex = 0;
            var hdrMovies = sheetMovies.CreateRow(rowIndex++);
            hdrMovies.CreateCell(0).SetCellValue("Title");
            hdrMovies.CreateCell(1).SetCellValue("Director");
            hdrMovies.CreateCell(2).SetCellValue("Year");
            hdrMovies.CreateCell(3).SetCellValue("Genre");
            hdrMovies.CreateCell(4).SetCellValue("DurationMinutes");
            hdrMovies.CreateCell(5).SetCellValue("IsOwned");
            hdrMovies.CreateCell(6).SetCellValue("IsOnWishlist");
            hdrMovies.CreateCell(7).SetCellValue("Rating");
            hdrMovies.CreateCell(8).SetCellValue("PlotSummary");
            hdrMovies.CreateCell(9).SetCellValue("CoverArtPath");
            hdrMovies.CreateCell(10).SetCellValue("Format");
            hdrMovies.CreateCell(11).SetCellValue("Location");
            hdrMovies.CreateCell(12).SetCellValue("Notes");

            foreach (var m in movies)
            {
                var row = sheetMovies.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(m.Title);
                row.CreateCell(1).SetCellValue(m.Director);
                row.CreateCell(2).SetCellValue(m.Year ?? 0);
                row.CreateCell(3).SetCellValue(m.Genre);
                row.CreateCell(4).SetCellValue(m.DurationMinutes ?? 0);
                row.CreateCell(5).SetCellValue(m.IsOwned);
                row.CreateCell(6).SetCellValue(m.IsOnWishlist);
                row.CreateCell(7).SetCellValue(m.Rating ?? 0.0);
                row.CreateCell(8).SetCellValue(m.PlotSummary);
                row.CreateCell(9).SetCellValue(m.CoverArtPath);
                row.CreateCell(10).SetCellValue(m.Format);
                row.CreateCell(11).SetCellValue(m.Location);
                row.CreateCell(12).SetCellValue(m.Notes);
            }

            // --- muzyka ---
            var albums = await _appDatabase.GetMusicAlbumsAsync();
            var sheetMusic = workbook.CreateSheet("MusicAlbums");
            rowIndex = 0;
            var hdrMusic = sheetMusic.CreateRow(rowIndex++);
            hdrMusic.CreateCell(0).SetCellValue("iTunesId");
            hdrMusic.CreateCell(1).SetCellValue("Title");
            hdrMusic.CreateCell(2).SetCellValue("Artist");
            hdrMusic.CreateCell(3).SetCellValue("CoverUrl");
            hdrMusic.CreateCell(4).SetCellValue("Genre");
            hdrMusic.CreateCell(5).SetCellValue("ReleasedDate");
            hdrMusic.CreateCell(6).SetCellValue("CollectionPrice");

            foreach (var a in albums)
            {
                var row = sheetMusic.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(a.iTunesId);
                row.CreateCell(1).SetCellValue(a.Title);
                row.CreateCell(2).SetCellValue(a.Artist);
                row.CreateCell(3).SetCellValue(a.CoverUrl);
                row.CreateCell(4).SetCellValue(a.Genre);
                row.CreateCell(5).SetCellValue(a.ReleasedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
                row.CreateCell(6).SetCellValue(a.CollectionPrice ?? 0.0);
            }

            using var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            workbook.Write(outFs);
        }

        // ------------------------------------------------------------------------------------------------
        // Eksport do PDF z użyciem QuestPDF
        private async Task ExportToPdf(string filePath)
        {
            var games = await _appDatabase.GetGamesAsync();
            var movies = await _movieDatabase.GetMoviesAsync();
            var musicAlbums = await _appDatabase.GetMusicAlbumsAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("OmniMedia – Raport Kolekcji")
                        .FontSize(18).SemiBold().AlignCenter();

                    page.Content().Column(column =>
                    {
                        column.Spacing(20);

                        if (games.Count > 0)
                        {
                            column.Item()
                                  .PaddingBottom(5)
                                  .Text("Kolekcja Gier")
                                  .FontSize(14).SemiBold();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Text("Tytuł").SemiBold();
                                    header.Cell().BorderBottom(1).Text("Gatunek").SemiBold();
                                    header.Cell().BorderBottom(1).Text("Platforma").SemiBold();
                                    header.Cell().BorderBottom(1).Text("Ocena").SemiBold();
                                });

                                foreach (var g in games)
                                {
                                    table.Cell().Text(g.Title ?? "");
                                    table.Cell().Text(g.Genre ?? "");
                                    table.Cell().Text(g.Platform ?? "");
                                    table.Cell().Text(
                                        g.Rating.HasValue
                                          ? g.Rating.Value.ToString("F1", CultureInfo.InvariantCulture)
                                          : "N/A");
                                }
                            });
                        }

                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Strona ");
                            txt.CurrentPageNumber();
                            txt.Span(" z ");
                            txt.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);
        }


        // ------------------------------------------------------------------------------------------------
        // Pomocnicze metody
        private string? GetCellValue(ICell cell)
        {
            if (cell == null) return null;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                                    ? cell.DateCellValue?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Formula => cell.CachedFormulaResultType switch
                {
                    CellType.String => cell.StringCellValue,
                    CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                                        ? cell.DateCellValue?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                        : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                    CellType.Boolean => cell.BooleanCellValue.ToString(),
                    _ => null
                },
                CellType.Blank => null,
                CellType.Error => $"ERROR: {cell.ErrorCellValue}",
                _ => cell.ToString()
            };
        }

        private DateTime? ParseDate(string? s)
        {
            return DateTime.TryParse(s, out var dt) ? dt : null;
        }

        private double? ParseDouble(string? s)
        {
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                 ? d
                 : (double?)null;
        }

        private int? ParseInt(string? s)
        {
            return int.TryParse(s, out var i) ? i : (int?)null;
        }

        private bool ParseBool(string? s)
        {
            if (bool.TryParse(s, out var b)) return b;
            if (int.TryParse(s, out var i)) return i == 1;
            return false;
        }

        private Bitmap? LoadLogoImage()
        {
            try
            {
                var uri = new Uri("avares://OmniMedia/Assets/Images/OmniMedia Pro.png");
                if (AssetLoader.Exists(uri))
                {
                    using var st = AssetLoader.Open(uri);
                    return new Bitmap(st);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logo load error: {ex.Message}");
            }
            return null;
        }
    }
}
