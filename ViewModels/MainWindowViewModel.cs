using OmniMedia.ViewModels.Base;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace OmniMedia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Komendy dla przycisków w MainWindow

        // Komenda dla przycisku "Przeglądaj swoją Kolekcję"
        public ReactiveCommand<Unit, Unit> OpenCollectionCommand { get; }

        // Komenda dla przycisku "Szukaj Gry"
        public ReactiveCommand<Unit, Unit> OpenGameSearchCommand { get; }

        // Komenda dla przycisku "Szukaj Muzyki"
        public ReactiveCommand<Unit, Unit> OpenMusicSearchCommand { get; }

        // Komenda dla przycisku "Szukaj Filmów"
        public ReactiveCommand<Unit, Unit> OpenMovieSearchCommand { get; }

        // Komenda dla przycisku "Eksportuj Bazę"
        public ReactiveCommand<Unit, Unit> ExportDatabaseCommand { get; }

        // Komenda dla przycisku "Importuj Bazę"
        public ReactiveCommand<Unit, Unit> ImportDatabaseCommand { get; }

        // Komenda dla przycisku "Ustawienia"
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

        // Komenda dla przycisku "O Twórcach"
        public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }

        public MainWindowViewModel()
        {
            // Inicjalizacja komend
            // Na razie implementacja jest prosta - tylko wyświetlenie komunikatu w konsoli debugowania
            // W przyszłości tutaj będzie logika nawigacji do odpowiednich widoków/ViewModeli

            OpenCollectionCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku kolekcji
                System.Diagnostics.Debug.WriteLine("Kliknięto: Przeglądaj swoją Kolekcję");
                await Task.CompletedTask; // Przykładowa asynchroniczność
            });

            OpenGameSearchCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku wyszukiwania gier
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Gry");
                await Task.CompletedTask;
            });

            OpenMusicSearchCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku wyszukiwania muzyki
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Muzyki");
                await Task.CompletedTask;
            });

            OpenMovieSearchCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku wyszukiwania filmów
                System.Diagnostics.Debug.WriteLine("Kliknięto: Szukaj Filmów");
                await Task.CompletedTask;
            });

            ExportDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę eksportu bazy danych
                System.Diagnostics.Debug.WriteLine("Kliknięto: Eksportuj Bazę");
                await Task.CompletedTask;
            });

            ImportDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę importu bazy danych
                System.Diagnostics.Debug.WriteLine("Kliknięto: Importuj Bazę");
                await Task.CompletedTask;
            });

            OpenSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku ustawień
                System.Diagnostics.Debug.WriteLine("Kliknięto: Ustawienia");
                await Task.CompletedTask;
            });

            OpenAboutCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO: Implementuj logikę otwierania widoku "O Twórcach"
                System.Diagnostics.Debug.WriteLine("Kliknięto: O Twórcach");
                await Task.CompletedTask;
            });
        }
    }
}