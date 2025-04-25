using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels; // Potrzebne do MovieEditViewModel
using System.Threading.Tasks; // Potrzebne do Task
using ReactiveUI; // Potrzebne do Interaction, WhenActivated
using System.Reactive; // Potrzebne do Unit, IDisposable
using System.Diagnostics; // Potrzebne do Debug

// DODANE USINGI (mog� by� niepotrzebne lub inne w zale�no�ci od wersji ReactiveUI/Avalonia)
// using ReactiveUI.Views; // Potrzebne do IViewFor (w niekt�rych wersjach ReactiveUI mo�e by� ju� w ReactiveUI)
// using System.Collections.Generic; // Potrzebne do IEnumerable
// using System; // Potrzebne do IDisposable (cho� cz�sto ju� dodane)


namespace OmniMedia.Views // Upewnij si�, �e namespace jest poprawny
{
    // Klasa okna dialogowego do dodawania/edycji filmu
    // Pierwotnie bez implementacji IViewFor, co powodowa�o b��d WhenActivated
    // public partial class MovieEditWindow : Window // Pierwotna deklaracja
    // Po dodaniu IViewFor w pr�bach naprawy:
    public partial class MovieEditWindow : Window, IViewFor<MovieEditViewModel> // Deklaracja z IViewFor
    {
        // Je�li implementujesz IViewFor, potrzebujesz tych w�a�ciwo�ci:
        public MovieEditViewModel? ViewModel
        {
            get => DataContext as MovieEditViewModel;
            set => DataContext = value;
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MovieEditViewModel?)value;
        }


        public MovieEditWindow()
        {
            InitializeComponent();

            // --- BLOK, KT�RY POWODOWA� B��D CS1929 ---
            // U�yj metody WhenActivated do konfigurowania powi�za� i obs�ug, kt�re maj� by� aktywne tylko wtedy, gdy widok jest aktywny
            // TA LINIJKA POWODOWA�A B��D BEZ POPRAWNEJ IMPLEMENTACJI IViewFor<TViewModel> LUB W INNYCH SYGNATURACH
            this.WhenActivated(disposables => { // Pierwotna sygnatura lub pr�ba
                Debug.WriteLine("[MovieEditWindow] MovieEditWindow Aktywny.");

                // Upewnij si�, �e ViewModel jest dost�pny
                if (DataContext is MovieEditViewModel viewModel) // Sprawdzenie DataContext zamiast u�ycia w�a�ciwo�ci ViewModel
                {
                    // Ustaw obs�ug� dla interakcji SaveInteraction
                    viewModel.SaveInteraction.RegisterHandler(async context =>
                    {
                        Debug.WriteLine("[MovieEditWindow] Obs�uga SaveInteraction: Zamykanie okna dialogowego...");
                        // Zwr�� zapisany obiekt Movie (context.Input) jako wynik okna dialogowego
                        Close(context.Input); // context.Input to obiekt Movie przekazany z ViewModelu

                        // Potwierd� wykonanie interakcji zwracaj�c wynik.
                        // context.SetOutput(context.Input); // Ta linia dodana w p�niejszych iteracjach

                    });

                    // Ustaw obs�ug� dla interakcji CancelInteraction
                    viewModel.CancelInteraction.RegisterHandler(async context =>
                    {
                        Debug.WriteLine("[MovieEditWindow] Obs�uga CancelInteraction: Zamykanie okna dialogowego bez zapisu...");
                        // Zamknij okno dialogowe bez zwracania wyniku (lub zwracaj�c null)
                        Close(null); // Zwracamy null, sygnalizuj�c brak zapisu

                        // Potwierd� obs�u�enie interakcji.
                        // context.SetOutput(Unit.Default); // Ta linia dodana w p�niejszych iteracjach
                    });

                    // TODO: Dodaj inne subskrypcje lub powi�zania specyficzne dla widoku

                }
                else
                {
                    Debug.WriteLine("[MovieEditWindow] ERROR: DataContext nie jest typu MovieEditViewModel w WhenActivated.");
                }

                // WhenActivated oczekuje, �e zwr�cisz IEnumerable<IDisposable> (lub void w niekt�rych sygnaturach/wersjach)
                // return disposables; // Ta linia lub co� podobnego by�o na ko�cu bloku
                // return new CompositeDisposable(); // Zmienione w pr�bach naprawy
            });
            // --- KONIEC BLOKU, KT�RY POWODOWA� B��D CS1929 ---


            // Opcjonalnie: Ustaw domy�lne przyciski Anuluj i Zapisz dla okna dialogowego
            // (Wymaga dodania x:Name do przycisk�w w XAML)
            // CancelButton = this.FindControl<Button>("CancelButtonNameInXaml");
            // AcceptButton = this.FindControl<Button>("SaveButtonNameInXaml");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}