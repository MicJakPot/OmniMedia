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
using OmniMedia.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using MsBox.Avalonia; // Twój działający using
using MsBox.Avalonia.Enums; // Twój działający using

namespace OmniMedia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> OpenCollectionCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGameSearchCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenMusicSearchCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenMovieSearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportDatabaseCommand { get; } // Implementacja będzie tutaj
        public ReactiveCommand<Unit, Unit> ImportDatabaseCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }

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
                var collectionWindow = new CollectionWindow
                {
                    DataContext = new CollectionWindowViewModel()
                };
                collectionWindow.Show();
                Debug.WriteLine("Kliknięto: Przeglądaj swoją Kolekcję. Otwarto CollectionWindow.");
            });

            OpenGameSearchCommand = ReactiveCommand.Create(() =>
            {
                var gameSearchWindow = new GameSearchWindow
                {
                    DataContext = new GameSearchViewModel()
                };
                gameSearchWindow.Show();
                Debug.WriteLine("Kliknięto: Szukaj Gry. Otwarto GameSearchWindow.");
            });

            OpenMusicSearchCommand = ReactiveCommand.Create(() =>
            {
                var musicSearchWindow = new MusicSearchWindow
                {
                    DataContext = new MusicSearchViewModel()
                };
                musicSearchWindow.Show();
                Debug.WriteLine("Kliknięto: Szukaj Muzyki. Otwarto MusicSearchWindow.");
            });

            OpenMovieSearchCommand = ReactiveCommand.Create(() =>
            {
                Debug.WriteLine("Kliknięto: Szukaj Filmów (TODO)");
            });

            // Implementacja ExportDatabaseCommand
            ExportDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine("Kliknięto: Eksportuj Bazę (Rozpoczynanie eksportu do XLS)");

                var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                if (desktop?.MainWindow == null)
                {
                    Debug.WriteLine("Błąd: Nie można uzyskać dostępu do okna głównego do eksportu.");
                    var err1 = MessageBoxManager.GetMessageBoxStandard(
                        "Błąd Aplikacji",
                        "Nie można uzyskać dostępu do okna aplikacji, aby wyeksportować dane.",
                        ButtonEnum.Ok);
                    await err1.ShowAsync();
                    return;
                }

                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel == null)
                {
                    Debug.WriteLine("Błąd: Nie można uzyskać dostępu do TopLevel z MainWindow do eksportu.");
                    var err2 = MessageBoxManager.GetMessageBoxStandard(
                        "Błąd Aplikacji",
                        "Wystąpił błąd podczas przygotowania okna zapisu pliku.",
                        ButtonEnum.Ok);
                    await err2.ShowAsync();
                    return;
                }

                var options = new FilePickerSaveOptions
                {
                    Title = "Zapisz bazę danych jako plik Excela",
                    SuggestedFileName = "OmniMedia_Export",
                    DefaultExtension = "xlsx",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("Plik Excela XLSX") { Patterns = new[] { "*.xlsx" } },
                        new FilePickerFileType("Plik Excela XLS (starszy format)") { Patterns = new[] { "*.xls" } },
                        new FilePickerFileType("Wszystkie pliki") { Patterns = new[] { "*" } }
                    }
                };

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

                if (file != null)
                {
                    string filePath = file.Path.LocalPath;
                    try
                    {
                        await ExportToExcel(filePath); // Wywołaj nową metodę eksportu
                        Debug.WriteLine($"Dane wyeksportowane pomyślnie do: {filePath}");

                        var success = MessageBoxManager.GetMessageBoxStandard(
                            "Eksport zakończony",
                            $"Baza danych została pomyślnie wyeksportowana do pliku:\n{filePath}",
                            ButtonEnum.Ok);
                        await success.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Błąd podczas eksportu: {ex.Message}");
                        var err3 = MessageBoxManager.GetMessageBoxStandard(
                            "Błąd Eksportu",
                            $"Wystąpił błąd podczas eksportu danych: {ex.Message}",
                            ButtonEnum.Ok);
                        await err3.ShowAsync();
                    }
                }
                else
                {
                    Debug.WriteLine("Anulowano zapis pliku.");
                }
            });

            // Implementacja ImportDatabaseCommand (pozostaje bez zmian)
            ImportDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine("Kliknięto: Importuj Bazę (Rozpoczynanie importu XLS)");

                var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                if (desktop?.MainWindow == null)
                {
                    Debug.WriteLine("Błąd: Nie można uzyskać dostępu do okna głównego.");
                    var err1 = MessageBoxManager.GetMessageBoxStandard(
                        "Błąd Aplikacji",
                        "Nie można uzyskać dostępu do okna aplikacji. Spróbuj ponownie.",
                        ButtonEnum.Ok);
                    await err1.ShowAsync();
                    return;
                }

                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel == null)
                {
                    Debug.WriteLine("Błąd: Nie można uzyskać dostępu do TopLevel z MainWindow.");
                    var err2 = MessageBoxManager.GetMessageBoxStandard(
                        "Błąd Aplikacji",
                        "Wystąpił błąd podczas próby otwarcia okna wyboru pliku.",
                        ButtonEnum.Ok);
                    await err2.ShowAsync();
                    return;
                }

                var options = new FilePickerOpenOptions
                {
                    Title = "Wybierz plik do importu",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Pliki Excela") { Patterns = new[] { "*.xls", "*.xlsx" } },
                        new FilePickerFileType("Wszystkie pliki") { Patterns = new[] { "*" } }
                    }
                };

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    var filePath = file.Path.LocalPath;
                    var ext = Path.GetExtension(filePath).ToLower();

                    try
                    {
                        if (ext == ".xls" || ext == ".xlsx")
                        {
                            await ImportFromExcel(filePath);
                            Debug.WriteLine("Dane zaimportowane pomyślnie z Excela.");

                            var success = MessageBoxManager.GetMessageBoxStandard(
                                "Import zakończony",
                                "Dane zostały pomyślnie zaimportowane z pliku Excel.",
                                ButtonEnum.Ok);
                            await success.ShowAsync();
                        }
                        else
                        {
                            Debug.WriteLine($"Nieobsługiwany format pliku: {ext}");
                            var badFormat = MessageBoxManager.GetMessageBoxStandard(
                                "Błąd importu",
                                $"Nieobsługiwany format pliku. Proszę wybrać .xls lub .xlsx (wybrano: {ext}).",
                                ButtonEnum.Ok);
                            await badFormat.ShowAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Błąd podczas importu: {ex.Message}");
                        var err3 = MessageBoxManager.GetMessageBoxStandard(
                            "Błąd importu",
                            $"Wystąpił błąd podczas importu danych: {ex.Message}",
                            ButtonEnum.Ok);
                        await err3.ShowAsync();
                    }
                }
                else
                {
                    Debug.WriteLine("Anulowano wybór pliku.");
                }
            });

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

        private Bitmap? LoadLogoImage()
        {
            var assetPath = "avares://OmniMedia/Assets/Images/OmniMedia Pro.png";
            try
            {
                var uri = new Uri(assetPath);
                if (AssetLoader.Exists(uri))
                {
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }
                else
                {
                    Debug.WriteLine($"[MainWindowViewModel] Nie znaleziono zasobu: {assetPath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowViewModel] Błąd ładowania logo ({assetPath}): {ex.Message}");
                return null;
            }
        }

        // Metoda do importu z Excela (pozostaje bez zmian)
        private async Task ImportFromExcel(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            IWorkbook workbook = Path.GetExtension(filePath).ToLower() switch
            {
                ".xlsx" => new XSSFWorkbook(fs),
                _ => new HSSFWorkbook(fs)
            };

            var gamesSheet = workbook.GetSheet("Games");
            if (gamesSheet != null)
            {
                for (int i = 1; i <= gamesSheet.LastRowNum; i++)
                {
                    var row = gamesSheet.GetRow(i);
                    if (row == null) continue;
                    string? title = GetCellValue(row.GetCell(0));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    try
                    {
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Błąd importu gry w wierszu {i}: {ex.Message}");
                    }
                }
            }

            var moviesSheet = workbook.GetSheet("Movies");
            if (moviesSheet != null)
            {
                for (int i = 1; i <= moviesSheet.LastRowNum; i++)
                {
                    var row = moviesSheet.GetRow(i);
                    if (row == null) continue;
                    string? title = GetCellValue(row.GetCell(0));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    try
                    {
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Błąd importu filmu w wierszu {i}: {ex.Message}");
                    }
                }
            }

            var musicSheet = workbook.GetSheet("MusicAlbums");
            if (musicSheet != null)
            {
                for (int i = 1; i <= musicSheet.LastRowNum; i++)
                {
                    var row = musicSheet.GetRow(i);
                    if (row == null) continue;
                    string? title = GetCellValue(row.GetCell(1));
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    try
                    {
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Błąd importu albumu w wierszu {i}: {ex.Message}");
                    }
                }
            }
        }

        // Nowa metoda do eksportu danych do Excela
        private async Task ExportToExcel(string filePath)
        {
            IWorkbook workbook;
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".xlsx")
            {
                workbook = new XSSFWorkbook(); // Tworzy nowy skoroszyt XLSX
            }
            else // Domyślnie użyjemy XLS, jeśli rozszerzenie nie jest .xlsx
            {
                workbook = new HSSFWorkbook(); // Tworzy nowy skoroszyt XLS
            }

            // Eksport gier
            var games = await _appDatabase.GetGamesAsync();
            ISheet gamesSheet = workbook.CreateSheet("Games");
            int rowIndex = 0;

            // Nagłówki dla gier
            IRow headerRowGames = gamesSheet.CreateRow(rowIndex++);
            headerRowGames.CreateCell(0).SetCellValue("Title");
            headerRowGames.CreateCell(1).SetCellValue("Genre");
            headerRowGames.CreateCell(2).SetCellValue("Platform");
            headerRowGames.CreateCell(3).SetCellValue("ReleasedDate");
            headerRowGames.CreateCell(4).SetCellValue("Rating");
            headerRowGames.CreateCell(5).SetCellValue("ThumbnailUrl");
            headerRowGames.CreateCell(6).SetCellValue("Description");
            headerRowGames.CreateCell(7).SetCellValue("Developer");
            headerRowGames.CreateCell(8).SetCellValue("Publisher");

            foreach (var game in games)
            {
                IRow row = gamesSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(game.Title);
                row.CreateCell(1).SetCellValue(game.Genre);
                row.CreateCell(2).SetCellValue(game.Platform);
                row.CreateCell(3).SetCellValue(game.ReleasedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
                row.CreateCell(4).SetCellValue(game.Rating ?? 0.0);
                row.CreateCell(5).SetCellValue(game.ThumbnailUrl);
                row.CreateCell(6).SetCellValue(game.Description);
                row.CreateCell(7).SetCellValue(game.Developer);
                row.CreateCell(8).SetCellValue(game.Publisher);
            }
            Debug.WriteLine($"Wyeksportowano {games.Count} gier.");

            // Eksport filmów
            var movies = await _movieDatabase.GetMoviesAsync();
            ISheet moviesSheet = workbook.CreateSheet("Movies");
            rowIndex = 0; // Resetuj indeks wiersza

            // Nagłówki dla filmów
            IRow headerRowMovies = moviesSheet.CreateRow(rowIndex++);
            headerRowMovies.CreateCell(0).SetCellValue("Title");
            headerRowMovies.CreateCell(1).SetCellValue("Director");
            headerRowMovies.CreateCell(2).SetCellValue("Year");
            headerRowMovies.CreateCell(3).SetCellValue("Genre");
            headerRowMovies.CreateCell(4).SetCellValue("DurationMinutes");
            headerRowMovies.CreateCell(5).SetCellValue("IsOwned");
            headerRowMovies.CreateCell(6).SetCellValue("IsOnWishlist");
            headerRowMovies.CreateCell(7).SetCellValue("Rating");
            headerRowMovies.CreateCell(8).SetCellValue("PlotSummary");
            headerRowMovies.CreateCell(9).SetCellValue("CoverArtPath");
            headerRowMovies.CreateCell(10).SetCellValue("Format");
            headerRowMovies.CreateCell(11).SetCellValue("Location");
            headerRowMovies.CreateCell(12).SetCellValue("Notes");

            foreach (var movie in movies)
            {
                IRow row = moviesSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(movie.Title);
                row.CreateCell(1).SetCellValue(movie.Director);
                row.CreateCell(2).SetCellValue(movie.Year ?? 0);
                row.CreateCell(3).SetCellValue(movie.Genre);
                row.CreateCell(4).SetCellValue(movie.DurationMinutes ?? 0);
                row.CreateCell(5).SetCellValue(movie.IsOwned);
                row.CreateCell(6).SetCellValue(movie.IsOnWishlist);
                row.CreateCell(7).SetCellValue(movie.Rating ?? 0.0);
                row.CreateCell(8).SetCellValue(movie.PlotSummary);
                row.CreateCell(9).SetCellValue(movie.CoverArtPath);
                row.CreateCell(10).SetCellValue(movie.Format);
                row.CreateCell(11).SetCellValue(movie.Location);
                row.CreateCell(12).SetCellValue(movie.Notes);
            }
            Debug.WriteLine($"Wyeksportowano {movies.Count} filmów.");

            // Eksport albumów muzycznych
            var musicAlbums = await _appDatabase.GetMusicAlbumsAsync();
            ISheet musicAlbumsSheet = workbook.CreateSheet("MusicAlbums");
            rowIndex = 0; // Resetuj indeks wiersza

            // Nagłówki dla albumów muzycznych
            IRow headerRowMusic = musicAlbumsSheet.CreateRow(rowIndex++);
            headerRowMusic.CreateCell(0).SetCellValue("iTunesId");
            headerRowMusic.CreateCell(1).SetCellValue("Title");
            headerRowMusic.CreateCell(2).SetCellValue("Artist");
            headerRowMusic.CreateCell(3).SetCellValue("CoverUrl");
            headerRowMusic.CreateCell(4).SetCellValue("Genre");
            headerRowMusic.CreateCell(5).SetCellValue("ReleasedDate");
            headerRowMusic.CreateCell(6).SetCellValue("CollectionPrice");

            foreach (var album in musicAlbums)
            {
                IRow row = musicAlbumsSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(album.iTunesId);
                row.CreateCell(1).SetCellValue(album.Title);
                row.CreateCell(2).SetCellValue(album.Artist);
                row.CreateCell(3).SetCellValue(album.CoverUrl);
                row.CreateCell(4).SetCellValue(album.Genre);
                row.CreateCell(5).SetCellValue(album.ReleasedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
                row.CreateCell(6).SetCellValue(album.CollectionPrice ?? 0.0);
            }
            Debug.WriteLine($"Wyeksportowano {musicAlbums.Count} albumów muzycznych.");

            // Zapisz skoroszyt do pliku
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fileStream);
            }
        }

        private string? GetCellValue(ICell cell)
        {
            if (cell == null) return null;

            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;

                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        return cell.DateCellValue?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                    }

                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();

                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.String:
                            return cell.StringCellValue;
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                                return cell.DateCellValue?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            else
                                return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                        case CellType.Boolean:
                            return cell.BooleanCellValue.ToString();
                        default:
                            return null;
                    }

                case CellType.Blank:
                    return null;

                case CellType.Error:
                    return $"ERROR: {cell.ErrorCellValue}";

                default:
                    return cell.ToString();
            }
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;
            return DateTime.TryParse(dateString, out var dt) ? dt : null;
        }

        private double? ParseDouble(string? doubleString)
        {
            if (string.IsNullOrWhiteSpace(doubleString))
                return null;
            return double.TryParse(doubleString, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? d
                : (double?)null;
        }

        private int? ParseInt(string? intString)
        {
            if (string.IsNullOrWhiteSpace(intString))
                return null;
            return int.TryParse(intString, out var i) ? i : (int?)null;
        }

        private bool ParseBool(string? boolString)
        {
            if (string.IsNullOrWhiteSpace(boolString))
                return false;
            if (bool.TryParse(boolString, out var b))
                return b;
            if (int.TryParse(boolString, out var i))
                return i == 1;
            return false;
        }
    }
}