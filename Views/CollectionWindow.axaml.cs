using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels;
using OmniMedia.Models; // Potrzebne do modelu Movie (dla typu zwracanego przez dialog)
using System.Threading.Tasks;
using ReactiveUI; // Potrzebne do ReactiveObject, Interaction, IViewFor
using System.Reactive; // Potrzebne do Unit, IDisposable
using System.Diagnostics;

// DODANE USINGI
using Avalonia.Interactivity; // Potrzebne do RoutedEventArgs
using System.Reactive.Disposables; // Potrzebne do CompositeDisposable i metody rozszerzenia DisposeWith
using System.Reactive.Linq; // Potrzebne do Observable LINQ (jeœli potrzebne)
using OmniMedia.Views; // Potrzebne do MovieEditWindow


namespace OmniMedia.Views
{
    // Klasa okna przegl¹dania kolekcji (gier, muzyki i filmów)
    // IMPLEMENTUJE IViewFor<CollectionWindowViewModel>
    public partial class CollectionWindow : Window, IViewFor<CollectionWindowViewModel>
    {
        // Wymagana w³aœciwoœæ ViewModel przez interfejs IViewFor<TViewModel>
        public CollectionWindowViewModel? ViewModel
        {
            get => DataContext as CollectionWindowViewModel;
            set => DataContext = value;
        }

        // Wymagana w³aœciwoœæ ViewModel przez interfejs IViewFor (niewykorzystywana bezpoœrednio w widokach)
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CollectionWindowViewModel?)value;
        }

        // Pole do przechowywania obiektów IDisposable (subskrypcji), które bêd¹ usuwane przy zamkniêciu/dezaktywacji okna
        private CompositeDisposable _disposables = new CompositeDisposable();


        public CollectionWindow()
        {
            InitializeComponent();

            // Obs³uga interakcji i inne subskrypcje zostan¹ skonfigurowane w OnLoaded
        }

        // Metoda generowana przez kompilator XAML
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Nadpisujemy metodê wywo³ywan¹, gdy widok (okno) zostanie za³adowany
        protected override void OnLoaded(RoutedEventArgs e) // RoutedEventArgs z Avalonia.Interactivity
        {
            base.OnLoaded(e);
            Debug.WriteLine("[CollectionWindow] CollectionWindow Za³adowany.");

            // SprawdŸ, czy ViewModel jest dostêpny
            if (ViewModel != null)
            {
                // --- OBS£UGA INTERAKCJI Z MovieCollectionViewModel ---
                // Subskrybujemy interakcjê ShowMovieEditDialogInteraction z ViewModelu kolekcji filmów
                // Ta subskrypcja powinna byæ aktywna, gdy okno CollectionWindow jest za³adowane
                ViewModel.MovieCollection.ShowMovieEditDialogInteraction.RegisterHandler(async context =>
                {
                    Debug.WriteLine("[CollectionWindow] Obs³uga ShowMovieEditDialogInteraction: Otwieranie okna dialogowego edycji filmu.");
                    // context.Input to MovieEditViewModel przekazany przez ViewModel kolekcji filmów

                    // Tworzymy nowe okno dialogowe edycji filmu
                    var movieEditWindow = new MovieEditWindow
                    {
                        DataContext = context.Input // Ustawiamy DataContext okna dialogowego na ViewModel formularza
                    };

                    // Wyœwietlamy okno dialogowe modalnie
                    // ShowDialog<TResult> zwraca Task<TResult>, gdzie TResult to typ zwracany przez metodê Close(result) okna dialogowego
                    Debug.WriteLine("[CollectionWindow] Wywo\u0142ywanie ShowDialog<Movie?>...");
                    var dialogResult = await movieEditWindow.ShowDialog<Movie?>(this); // 'this' odnosi si\u0119 do okna CollectionWindow jako rodzica

                    Debug.WriteLine($"[CollectionWindow] Okno dialogowe edycji filmu zamkni\u0119te. Wynik: {dialogResult?.Title ?? "null"}");

                    // Zwracamy wynik okna dialogowego z powrotem do interakcji w ViewModelu
                    context.SetOutput(dialogResult); // Przekazujemy zapisany film (lub null) z powrotem do ViewModelu
                    Debug.WriteLine("[CollectionWindow] Interakcja ShowMovieEditDialogInteraction SetOutput wywo³ana.");
                }).DisposeWith(_disposables); // DisposeWith z System.Reactive.Disposables


                // TODO: Dodaj inne subskrypcje lub powi¹zania, które maj¹ byæ aktywne podczas ³adowania/aktywnoœci okna
                // np. Subskrypcje na zmiany w³aœciwoœci w ViewModelu CollectionWindow
                // Pamiêtaj, aby u¿yæ .DisposeWith(_disposables) dla ka¿dej subskrypcji!

            }
            else
            {
                Debug.WriteLine("[CollectionWindow] ERROR: DataContext nie jest typu CollectionWindowViewModel w OnLoaded.");
            }
        }

        // Nadpisujemy metodê wywo³ywan¹, gdy widok (okno) zostanie zamkniêty lub przestanie byæ aktywny
        protected override void OnUnloaded(RoutedEventArgs e) // RoutedEventArgs z Avalonia.Interactivity
        {
            base.OnUnloaded(e);
            Debug.WriteLine("[CollectionWindow] CollectionWindow Roz³adowany. Usuwanie subskrypcji.");
            // Usuñ wszystkie subskrypcje dodane do _disposables
            _disposables.Dispose();
            // Stwórz nowy CompositeDisposable na wypadek ponownego za³adowania okna
            _disposables = new CompositeDisposable();
        }

        // TODO: Opcjonalnie: Ustaw domyœlne przyciski Anuluj i Zapisz dla okna (dla klawiszy ESC/ENTER)
        // Dodaj x:Name do przycisków w XAML: x:Name="GameCollectionButton", x:Name="MusicCollectionButton", x:Name="MovieCollectionButton"
        /*
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
             // Przyk³adowe pobieranie przycisków po nazwie, jeœli chcesz ich u¿ywaæ np. do zmiany wygl¹du po klikniêciu
            // var gameButton = e.NameScope.Find<Button>("GameCollectionButton");
            // var musicButton = e.NameScope.Find<Button>("MusicCollectionButton");
            // var movieButton = e.NameScope.Find<Button>("MovieCollectionButton");
        }
        */
    }
}