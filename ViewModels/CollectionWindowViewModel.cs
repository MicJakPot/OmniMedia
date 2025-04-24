using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace OmniMedia.ViewModels
{
    // ViewModel dla okna "Przeglądaj swoją kolekcję"
    public class CollectionWindowViewModel : ViewModelBase
    {

        // Komenda dla przycisku "Kolekcja Gier"
        public ReactiveCommand<Unit, Unit> OpenGameCollectionCommand { get; }

        // Komenda dla przycisku "Kolekcja Muzyki"
        public ReactiveCommand<Unit, Unit> OpenMusicCollectionCommand { get; }

        // Komenda dla przycisku "Kolekcja Filmów"
        public ReactiveCommand<Unit, Unit> OpenMovieCollectionCommand { get; }

        // TODO: Właściwość lub kolekcja dla miniatur ostatnio dodanych treści
        // np. ObservableCollection<RecentlyAddedItemViewModel> RecentlyAddedItems { get; }
        // Gdzie RecentlyAddedItemViewModel to ViewModel dla pojedynczej miniaturki

        // Konstruktor ViewModelu
        public CollectionWindowViewModel()
        {
            // Inicjalizacja komend przycisków
            OpenGameCollectionCommand = ReactiveCommand.Create(() => { /* Logika otwierania widoku Kolekcji Gier */ });
            OpenMusicCollectionCommand = ReactiveCommand.Create(() => { /* Logika otwierania widoku Kolekcji Muzyki */ });
            OpenMovieCollectionCommand = ReactiveCommand.Create(() => { /* Logika otwierania widoku Kolekcji Filmów */ });

            // TODO: Logika ładowania miniatur ostatnio dodanych treści
        }

        // Później dodać metody do interakcji z bazą danych w celu pobrania ostatnich elementów
    }

    // TODO: Możesz potrzebować ViewModelu dla pojedynczej miniaturki
    // public class RecentlyAddedItemViewModel : ViewModelBase { ... }
}