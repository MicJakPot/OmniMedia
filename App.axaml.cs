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
        // Statyczna w�a�ciwo�� dla instancji bazy danych
        // U�ywamy Lazy<T> do leniwej inicjalizacji (tworzona, gdy jest potrzebna)
        private static Lazy<AppDatabase> database = new Lazy<AppDatabase>(() =>
        {
            // Okre�l �cie�k� do pliku bazy danych
            // Baza danych zostanie zapisana w katalogu danych aplikacji u�ytkownika
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniMedia");
            // Upewnij si�, �e folder istnieje
            Directory.CreateDirectory(folderPath);
            var databasePath = Path.Combine(folderPath, "OmniMedia.db");

            // Utw�rz now� instancj� bazy danych
            var db = new AppDatabase(databasePath);

            // Stw�rz tabele asynchronicznie (cho� tutaj zwracamy instancj�, inicjalizacj� tabel najlepiej zrobi� zaraz po uzyskaniu instancji)
            Task.Run(() => db.CreateTablesAsync()).Wait(); // U�ywamy Wait() tylko dla prostoty w inicjalizacji

            return db;
        });

        // Publiczna w�a�ciwo��, aby uzyska� dost�p do instancji bazy danych
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
                // Dla aplikacji SingleView, je�li masz takie zastosowanie
                // singleView.MainView = new Views.MainView();
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}