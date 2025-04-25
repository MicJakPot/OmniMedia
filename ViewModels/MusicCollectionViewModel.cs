using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive; // Potrzebne do Unit, Interaction
using System.Threading.Tasks;
using System.Diagnostics;
using OmniMedia.Models; // Potrzebne do modelu MusicAlbum
using OmniMedia.Database; // Potrzebne do AppDatabase
using Avalonia.Threading;
using System.Reactive.Linq; // Potrzebne do Observable.Select

// Potrzebne do komunikatów (jeśli zdecydujemy się na okna dialogowe)
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;


namespace OmniMedia.ViewModels
{
    // ViewModel dla widoku kolekcji albumów muzycznych
    public class MusicCollectionViewModel : ViewModelBase
    {
        // Kolekcja ViewModels elementów listy kolekcji albumów
        private ObservableCollection<CollectionAlbumItemViewModel> _collectionItems = new ObservableCollection<CollectionAlbumItemViewModel>();
        public ObservableCollection<CollectionAlbumItemViewModel> CollectionItems
        {
            get => _collectionItems;
            set => this.RaiseAndSetIfChanged(ref _collectionItems, value);
        }

        // Komenda "Usuń z kolekcji"
        public ReactiveCommand<CollectionAlbumItemViewModel, Unit> RemoveFromCollectionCommand { get; }

        // Właściwość wskazująca, czy ładowanie kolekcji jest w toku
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }


        // Konstruktor
        public MusicCollectionViewModel()
        {
            // Inicjalizacja komendy "Usuń z kolekcji"
            RemoveFromCollectionCommand = ReactiveCommand.CreateFromTask<CollectionAlbumItemViewModel, Unit>(
                RemoveFromCollection, // Metoda do wykonania
                this.WhenAnyValue(x => x.IsBusy).Select(isBusy => !isBusy) // Komenda nieaktywna podczas ładowania
            );

            // Rozpocznij ładowanie albumów z bazy danych przy tworzeniu ViewModelu
            Task.Run(LoadAlbumsAsync);
        }


        // Metoda ładująca albumy muzyczne z bazy danych
        private async Task<Unit> LoadAlbumsAsync()
        {
            Debug.WriteLine("[MusicCollectionViewModel] Rozpoczynam ładowanie albumów z bazy danych...");
            IsBusy = true;

            try
            {
                // Pobierz wszystkie albumy z głównej bazy danych
                var albums = await App.Database.GetMusicAlbumsAsync(); // Używamy App.Database

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CollectionItems.Clear();
                    Debug.WriteLine($"[MusicCollectionViewModel] Wyczy\u015Bcono obecn\u0105 list\u0119 kolekcji album\u00f3w ({CollectionItems.Count} element\u00f3w przed czyszczeniem).");
                });


                if (albums != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine($"[MusicCollectionViewModel] Przetwarzanie {albums.Count} album\u00f3w z bazy.");
                        foreach (var album in albums)
                        {
                            // Tworzymy ViewModel elementu listy dla albumu
                            var albumVm = new CollectionAlbumItemViewModel(album, RemoveFromCollectionCommand);
                            CollectionItems.Add(albumVm);
                        }
                        Debug.WriteLine($"[MusicCollectionViewModel] Dodano {CollectionItems.Count} album\u00f3w do kolekcji.");
                    });
                }
                else
                {
                    Debug.WriteLine("[MusicCollectionViewModel] Baza danych zwr\u00f3ci\u0142a pust\u0105 list\u0119 album\u00f3w.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MusicCollectionViewModel] B\u0142\u0105d podczas \u0142adowania album\u00f3w z bazy danych: {ex.Message}");
                await ShowMessage("B\u0142\u0105d \u0142adowania kolekcji", $"Wyst\u0105pi\u0142 b\u0142\u0105d podczas \u0142adowania kolekcji album\u00f3w: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[MusicCollectionViewModel] Zako\u0144czono \u0142adowanie album\u00f3w.");
            }
            return Unit.Default;
        }

        // IMPLEMENTACJA METODY DO USUWANIA ALBUMU Z KOLEKCJI
        private async Task<Unit> RemoveFromCollection(CollectionAlbumItemViewModel albumVm)
        {
            if (albumVm?.AlbumData == null) return Unit.Default;

            Debug.WriteLine($"[MusicCollectionViewModel] Pr\u00f3ba usuni\u0119cia albumu '{albumVm.AlbumData.Title}' (ID bazy: {albumVm.AlbumData.Id}) z kolekcji...");

            try
            {
                // Usuń album z bazy danych
                int result = await App.Database.DeleteMusicAlbumAsync(albumVm.AlbumData);

                Debug.WriteLine($"[MusicCollectionViewModel] Usuni\u0119to album z bazy danych. Rezultat: {result}");

                if (result > 0)
                {
                    Debug.WriteLine($"[MusicCollectionViewModel] Album '{albumVm.AlbumData.Title}' pomy\u015Blnie usuni\u0119ty z bazy.");
                    // Usuń ViewModel elementu z ObservableCollection (MUSI BYĆ W WĄTKU UI)
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CollectionItems.Remove(albumVm);
                        Debug.WriteLine($"[MusicCollectionViewModel] Usuni\u0119to ViewModel albumu z ObservableCollection. Pozosta\u0142o {CollectionItems.Count} element\u00f3w.");
                    });

                    await ShowMessage("Usuni\u0119to album", $"Album '{albumVm.AlbumData.Title}' artysty '{albumVm.AlbumData.Artist}' zosta\u0142 usuni\u0119ty z kolekcji.");
                }
                else
                {
                    Debug.WriteLine($"[MusicCollectionViewModel] Nie uda\u0142o si\u0119 usun\u0105\u0107 albumu '{albumVm.AlbumData.Title}' z bazy danych (result by\u0142 0).");
                    await ShowMessage("B\u0142\u0105d usuwania", $"Nie uda\u0142o si\u0119 usun\u0105\u0107 albumu '{albumVm.AlbumData.Title}' z kolekcji.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MusicCollectionViewModel] B\u0142\u0105d podczas usuwania albumu z bazy: {ex.Message}");
                await ShowMessage("B\u0142\u0105d usuwania", $"Wyst\u0105pi\u0142 b\u0142\u0105d podczas usuwania albumu '{albumVm.AlbumData.Title}': {ex.Message}");
            }
            return Unit.Default;
        }

        // Metoda pomocnicza do wyświetlania prostego komunikatu
        private async Task ShowMessage(string title, string message)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    Debug.WriteLine($"[MusicCollectionViewModel] KOMUNIKAT (TODO: UI DIALOG): {title} - {message}");
                    // TODO: Użyj MessageBox.Avalonia lub innej metody do wyświetlenia dialogu w UI
                }
                else
                {
                    Debug.WriteLine($"[MusicCollectionViewModel] KOMUNIKAT (brak aktywnego okna g\u0142\u00f3wnego): {title} - {message}");
                }
            }
            else
            {
                Debug.WriteLine($"[MusicCollectionViewModel] KOMUNIKAT (tryb non-desktop): {title} - {message}");
            }
        }

        // TODO: Dodaj inne właściwości lub komendy
    }
}
