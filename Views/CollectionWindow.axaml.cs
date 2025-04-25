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
using System.Reactive.Linq; // Potrzebne do Observable LINQ (je�li potrzebne)
using OmniMedia.Views; // Potrzebne do MovieEditWindow


namespace OmniMedia.Views
{
    // Klasa okna przegl�dania kolekcji (gier, muzyki i film�w)
    // IMPLEMENTUJE IViewFor<CollectionWindowViewModel>
    public partial class CollectionWindow : Window, IViewFor<CollectionWindowViewModel>
    {
        // Wymagana w�a�ciwo�� ViewModel przez interfejs IViewFor<TViewModel>
        public CollectionWindowViewModel? ViewModel
        {
            get => DataContext as CollectionWindowViewModel;
            set => DataContext = value;
        }

        // Wymagana w�a�ciwo�� ViewModel przez interfejs IViewFor (niewykorzystywana bezpo�rednio w widokach)
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CollectionWindowViewModel?)value;
        }

        // Pole do przechowywania obiekt�w IDisposable (subskrypcji), kt�re b�d� usuwane przy zamkni�ciu/dezaktywacji okna
        private CompositeDisposable _disposables = new CompositeDisposable();


        public CollectionWindow()
        {
            InitializeComponent();

            // Obs�uga interakcji i inne subskrypcje zostan� skonfigurowane w OnLoaded
        }

        // Metoda generowana przez kompilator XAML
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Nadpisujemy metod� wywo�ywan�, gdy widok (okno) zostanie za�adowany
        protected override void OnLoaded(RoutedEventArgs e) // RoutedEventArgs z Avalonia.Interactivity
        {
            base.OnLoaded(e);
            Debug.WriteLine("[CollectionWindow] CollectionWindow Za�adowany.");

            // Sprawd�, czy ViewModel jest dost�pny
            if (ViewModel != null)
            {
                // --- OBS�UGA INTERAKCJI Z MovieCollectionViewModel ---
                // Subskrybujemy interakcj� ShowMovieEditDialogInteraction z ViewModelu kolekcji film�w
                // Ta subskrypcja powinna by� aktywna, gdy okno CollectionWindow jest za�adowane
                ViewModel.MovieCollection.ShowMovieEditDialogInteraction.RegisterHandler(async context =>
                {
                    Debug.WriteLine("[CollectionWindow] Obs�uga ShowMovieEditDialogInteraction: Otwieranie okna dialogowego edycji filmu.");
                    // context.Input to MovieEditViewModel przekazany przez ViewModel kolekcji film�w

                    // Tworzymy nowe okno dialogowe edycji filmu
                    var movieEditWindow = new MovieEditWindow
                    {
                        DataContext = context.Input // Ustawiamy DataContext okna dialogowego na ViewModel formularza
                    };

                    // Wy�wietlamy okno dialogowe modalnie
                    // ShowDialog<TResult> zwraca Task<TResult>, gdzie TResult to typ zwracany przez metod� Close(result) okna dialogowego
                    Debug.WriteLine("[CollectionWindow] Wywo\u0142ywanie ShowDialog<Movie?>...");
                    var dialogResult = await movieEditWindow.ShowDialog<Movie?>(this); // 'this' odnosi si\u0119 do okna CollectionWindow jako rodzica

                    Debug.WriteLine($"[CollectionWindow] Okno dialogowe edycji filmu zamkni\u0119te. Wynik: {dialogResult?.Title ?? "null"}");

                    // Zwracamy wynik okna dialogowego z powrotem do interakcji w ViewModelu
                    context.SetOutput(dialogResult); // Przekazujemy zapisany film (lub null) z powrotem do ViewModelu
                    Debug.WriteLine("[CollectionWindow] Interakcja ShowMovieEditDialogInteraction SetOutput wywo�ana.");
                }).DisposeWith(_disposables); // DisposeWith z System.Reactive.Disposables


                // TODO: Dodaj inne subskrypcje lub powi�zania, kt�re maj� by� aktywne podczas �adowania/aktywno�ci okna
                // np. Subskrypcje na zmiany w�a�ciwo�ci w ViewModelu CollectionWindow
                // Pami�taj, aby u�y� .DisposeWith(_disposables) dla ka�dej subskrypcji!

            }
            else
            {
                Debug.WriteLine("[CollectionWindow] ERROR: DataContext nie jest typu CollectionWindowViewModel w OnLoaded.");
            }
        }

        // Nadpisujemy metod� wywo�ywan�, gdy widok (okno) zostanie zamkni�ty lub przestanie by� aktywny
        protected override void OnUnloaded(RoutedEventArgs e) // RoutedEventArgs z Avalonia.Interactivity
        {
            base.OnUnloaded(e);
            Debug.WriteLine("[CollectionWindow] CollectionWindow Roz�adowany. Usuwanie subskrypcji.");
            // Usu� wszystkie subskrypcje dodane do _disposables
            _disposables.Dispose();
            // Stw�rz nowy CompositeDisposable na wypadek ponownego za�adowania okna
            _disposables = new CompositeDisposable();
        }

        // TODO: Opcjonalnie: Ustaw domy�lne przyciski Anuluj i Zapisz dla okna (dla klawiszy ESC/ENTER)
        // Dodaj x:Name do przycisk�w w XAML: x:Name="GameCollectionButton", x:Name="MusicCollectionButton", x:Name="MovieCollectionButton"
        /*
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
             // Przyk�adowe pobieranie przycisk�w po nazwie, je�li chcesz ich u�ywa� np. do zmiany wygl�du po klikni�ciu
            // var gameButton = e.NameScope.Find<Button>("GameCollectionButton");
            // var musicButton = e.NameScope.Find<Button>("MusicCollectionButton");
            // var movieButton = e.NameScope.Find<Button>("MovieCollectionButton");
        }
        */
    }
}