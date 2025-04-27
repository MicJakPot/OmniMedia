using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels;
using OmniMedia.Views;
using OmniMedia.Database; // Potrzebne do AppDatabase i MovieDatabase
using System.IO; // Potrzebne do Path
using System; // Potrzebne do Lazy, Environment
using System.Threading.Tasks; // Potrzebne do Task.Run

// Dodane dla QuestPDF License
using QuestPDF.Infrastructure; // Dodaj ten using do App.cs

namespace OmniMedia
{
    public partial class App : Application
    {

        private static Lazy<AppDatabase> database = new Lazy<AppDatabase>(() =>
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            Directory.CreateDirectory(folderPath);

            var databasePath = Path.Combine(folderPath, "OmniMedia.db");

            var db = new AppDatabase(databasePath);

            Task.Run(() => db.CreateTablesAsync()).Wait(); 

            return db;
        });

        public static AppDatabase Database => database.Value;

        private static Lazy<MovieDatabase> movieDatabase = new Lazy<MovieDatabase>(() =>
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            Directory.CreateDirectory(folderPath); 

            // Œcie¿ka do pliku bazy danych filmów
            var movieDatabasePath = Path.Combine(folderPath, "Movies.db");

            var db = new MovieDatabase(movieDatabasePath);

            Task.Run(() => db.CreateTablesAsync()).Wait(); 

            return db;
        });

        public static MovieDatabase MovieDatabase => movieDatabase.Value;


        // Metoda inicjalizuj¹ca framework Avalonia
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(Database, MovieDatabase),
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}