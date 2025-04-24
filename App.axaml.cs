using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels;
using OmniMedia.Views;
using OmniMedia.Database;
using System.IO;
using System;
using System.Threading.Tasks;

namespace OmniMedia
{
    public partial class App : Application
    {
        // Statyczna w³aœciwoœæ dla instancji bazy danych
        // U¿ywamy Lazy<T> do leniwej inicjalizacji (tworzona, gdy jest potrzebna)
        private static Lazy<AppDatabase> database = new Lazy<AppDatabase>(() =>
        {
            // Okreœl œcie¿kê do pliku bazy danych
            // Baza danych zostanie zapisana w katalogu danych aplikacji u¿ytkownika
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij siê, ¿e folder istnieje
            Directory.CreateDirectory(folderPath);
            var databasePath = Path.Combine(folderPath, "OmniMedia.db");

            // Utwórz now¹ instancjê bazy danych
            var db = new AppDatabase(databasePath);

            // Stwórz tabele asynchronicznie (choæ tutaj zwracamy instancjê, inicjalizacjê tabel najlepiej zrobiæ zaraz po uzyskaniu instancji)
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U¿ywamy Wait() tylko dla prostoty w inicjalizacji

            return db;
        });

        // Publiczna w³aœciwoœæ, aby uzyskaæ dostêp do instancji bazy danych
        public static AppDatabase Database => database.Value;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

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