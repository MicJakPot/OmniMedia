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
        // Statyczna w³aœciwoœæ dla instancji g³ównej bazy danych (gry, muzyka)
        // U¿ywamy Lazy<T> do leniwej inicjalizacji (tworzona, gdy jest potrzebna)
        private static Lazy<AppDatabase> database = new Lazy<AppDatabase>(() =>
        {
            // Okreœl œcie¿kê do folderu danych aplikacji u¿ytkownika
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij siê, ¿e folder istnieje
            Directory.CreateDirectory(folderPath);

            // Œcie¿ka do pliku g³ównej bazy danych
            var databasePath = Path.Combine(folderPath, "OmniMedia.db");

            // Utwórz now¹ instancjê g³ównej bazy danych
            var db = new AppDatabase(databasePath);

            // Stwórz tabele asynchronicznie (choæ tutaj zwracamy instancjê, inicjalizacjê tabel najlepiej zrobiæ zaraz po uzyskaniu instancji)
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U¿ywamy Wait() tylko dla prostoty w inicjalizacji

            return db;
        });

        // Publiczna w³aœciwoœæ, aby uzyskaæ dostêp do instancji g³ównej bazy danych
        public static AppDatabase Database => database.Value;


        // DODANE: Statyczna w³aœciwoœæ dla instancji bazy danych filmów
        private static Lazy<MovieDatabase> movieDatabase = new Lazy<MovieDatabase>(() =>
        {
            // Okreœl œcie¿kê do folderu danych aplikacji u¿ytkownika (ten sam folder co dla g³ównej bazy)
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij siê, ¿e folder istnieje (powinien ju¿ istnieæ po inicjalizacji g³ównej bazy)
            Directory.CreateDirectory(folderPath); // Mo¿na pomin¹æ, jeœli folder ju¿ istnieje, ale nie zaszkodzi

            // Œcie¿ka do pliku bazy danych filmów
            var movieDatabasePath = Path.Combine(folderPath, "Movies.db");

            // Utwórz now¹ instancjê bazy danych filmów
            var db = new MovieDatabase(movieDatabasePath);

            // Stwórz tabelê Movie asynchronicznie
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U¿ywamy Wait() tylko dla prostoty

            return db;
        });

        // Publiczna w³aœciwoœæ, aby uzyskaæ dostêp do instancji bazy danych filmów
        public static MovieDatabase MovieDatabase => movieDatabase.Value;


        // Metoda inicjalizuj¹ca framework Avalonia
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Metoda wywo³ywana po zakoñczeniu inicjalizacji frameworka
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
                // Dla aplikacji SingleView, jeœli masz takie zastosowanie
                // singleView.MainView = new Views.MainView();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}