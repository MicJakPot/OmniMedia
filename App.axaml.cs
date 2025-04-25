using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels;
using OmniMedia.Views;
using OmniMedia.Database; // Potrzebne do AppDatabase i MovieDatabase
using System.IO; // Potrzebne do Path
using System; // Potrzebne do Lazy, Environment
using System.Threading.Tasks; // Potrzebne do Task.Run

namespace OmniMedia
{
    public partial class App : Application
    {
        // Statyczna w�a�ciwo�� dla instancji g��wnej bazy danych (gry, muzyka)
        // U�ywamy Lazy<T> do leniwej inicjalizacji (tworzona, gdy jest potrzebna)
        private static Lazy<AppDatabase> database = new Lazy<AppDatabase>(() =>
        {
            // Okre�l �cie�k� do folderu danych aplikacji u�ytkownika
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij si�, �e folder istnieje
            Directory.CreateDirectory(folderPath);

            // �cie�ka do pliku g��wnej bazy danych
            var databasePath = Path.Combine(folderPath, "OmniMedia.db");

            // Utw�rz now� instancj� g��wnej bazy danych
            var db = new AppDatabase(databasePath);

            // Stw�rz tabele asynchronicznie (cho� tutaj zwracamy instancj�, inicjalizacj� tabel najlepiej zrobi� zaraz po uzyskaniu instancji)
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U�ywamy Wait() tylko dla prostoty w inicjalizacji

            return db;
        });

        // Publiczna w�a�ciwo��, aby uzyska� dost�p do instancji g��wnej bazy danych
        public static AppDatabase Database => database.Value;


        // DODANE: Statyczna w�a�ciwo�� dla instancji bazy danych film�w
        private static Lazy<MovieDatabase> movieDatabase = new Lazy<MovieDatabase>(() =>
        {
            // Okre�l �cie�k� do folderu danych aplikacji u�ytkownika (ten sam folder co dla g��wnej bazy)
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij si�, �e folder istnieje (powinien ju� istnie� po inicjalizacji g��wnej bazy)
            Directory.CreateDirectory(folderPath); // Mo�na pomin��, je�li folder ju� istnieje, ale nie zaszkodzi

            // �cie�ka do pliku bazy danych film�w
            var movieDatabasePath = Path.Combine(folderPath, "Movies.db");

            // Utw�rz now� instancj� bazy danych film�w
            var db = new MovieDatabase(movieDatabasePath);

            // Stw�rz tabel� Movie asynchronicznie
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U�ywamy Wait() tylko dla prostoty

            return db;
        });

        // Publiczna w�a�ciwo��, aby uzyska� dost�p do instancji bazy danych film�w
        public static MovieDatabase MovieDatabase => movieDatabase.Value;


        // Metoda inicjalizuj�ca framework Avalonia
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Metoda wywo�ywana po zako�czeniu inicjalizacji frameworka
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                // Dla aplikacji SingleView, je�li masz takie zastosowanie
                // singleView.MainView = new Views.MainView();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}