using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OmniMedia.ViewModels; // Potrzebne do MovieEditViewModel
using System.Threading.Tasks; // Potrzebne do Task
using ReactiveUI; // Potrzebne do Interaction, WhenActivated
using System.Reactive; // Potrzebne do Unit, IDisposable
using System.Diagnostics; // Potrzebne do Debug

// DODANE USINGI (mog¹ byæ niepotrzebne lub inne w zale¿noœci od wersji ReactiveUI/Avalonia)
// using ReactiveUI.Views; // Potrzebne do IViewFor (w niektórych wersjach ReactiveUI mo¿e byæ ju¿ w ReactiveUI)
// using System.Collections.Generic; // Potrzebne do IEnumerable
// using System; // Potrzebne do IDisposable (choæ czêsto ju¿ dodane)


namespace OmniMedia.Views // Upewnij siê, ¿e namespace jest poprawny
{
    // Klasa okna dialogowego do dodawania/edycji filmu
    // Pierwotnie bez implementacji IViewFor, co powodowa³o b³¹d WhenActivated
    // public partial class MovieEditWindow : Window // Pierwotna deklaracja
    // Po dodaniu IViewFor w próbach naprawy:
    public partial class MovieEditWindow : Window, IViewFor<MovieEditViewModel> // Deklaracja z IViewFor
    {
        // Jeœli implementujesz IViewFor, potrzebujesz tych w³aœciwoœci:
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

            // --- BLOK, KTÓRY POWODOWA£ B£¥D CS1929 ---
            // U¿yj metody WhenActivated do konfigurowania powi¹zañ i obs³ug, które maj¹ byæ aktywne tylko wtedy, gdy widok jest aktywny
            // TA LINIJKA POWODOWA£A B£¥D BEZ POPRAWNEJ IMPLEMENTACJI IViewFor<TViewModel> LUB W INNYCH SYGNATURACH
            this.WhenActivated(disposables => { // Pierwotna sygnatura lub próba
                Debug.WriteLine("[MovieEditWindow] MovieEditWindow Aktywny.");

                // Upewnij siê, ¿e ViewModel jest dostêpny
                if (DataContext is MovieEditViewModel viewModel) // Sprawdzenie DataContext zamiast u¿ycia w³aœciwoœci ViewModel
                {
                    // Ustaw obs³ugê dla interakcji SaveInteraction
                    viewModel.SaveInteraction.RegisterHandler(async context =>
                    {
                        Debug.WriteLine("[MovieEditWindow] Obs³uga SaveInteraction: Zamykanie okna dialogowego...");
                        // Zwróæ zapisany obiekt Movie (context.Input) jako wynik okna dialogowego
                        Close(context.Input); // context.Input to obiekt Movie przekazany z ViewModelu

                        // PotwierdŸ wykonanie interakcji zwracaj¹c wynik.
                        // context.SetOutput(context.Input); // Ta linia dodana w póŸniejszych iteracjach

                    });

                    // Ustaw obs³ugê dla interakcji CancelInteraction
                    viewModel.CancelInteraction.RegisterHandler(async context =>
                    {
                        Debug.WriteLine("[MovieEditWindow] Obs³uga CancelInteraction: Zamykanie okna dialogowego bez zapisu...");
                        // Zamknij okno dialogowe bez zwracania wyniku (lub zwracaj¹c null)
                        Close(null); // Zwracamy null, sygnalizuj¹c brak zapisu

                        // PotwierdŸ obs³u¿enie interakcji.
                        // context.SetOutput(Unit.Default); // Ta linia dodana w póŸniejszych iteracjach
                    });

                    // TODO: Dodaj inne subskrypcje lub powi¹zania specyficzne dla widoku

                }
                else
                {
                    Debug.WriteLine("[MovieEditWindow] ERROR: DataContext nie jest typu MovieEditViewModel w WhenActivated.");
                }

                // WhenActivated oczekuje, ¿e zwrócisz IEnumerable<IDisposable> (lub void w niektórych sygnaturach/wersjach)
                // return disposables; // Ta linia lub coœ podobnego by³o na koñcu bloku
                // return new CompositeDisposable(); // Zmienione w próbach naprawy
            });
            // --- KONIEC BLOKU, KTÓRY POWODOWA£ B£¥D CS1929 ---


            // Opcjonalnie: Ustaw domyœlne przyciski Anuluj i Zapisz dla okna dialogowego
            // (Wymaga dodania x:Name do przycisków w XAML)
            // CancelButton = this.FindControl<Button>("CancelButtonNameInXaml");
            // AcceptButton = this.FindControl<Button>("SaveButtonNameInXaml");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}