using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI; // Potrzebne do IViewFor
using OmniMedia.ViewModels; // Potrzebne do MovieCollectionViewModel

// TODO: Je�li b�dziesz potrzebowa� WhenActivated w tym code-behind, dodaj implementacj� IViewFor
// public partial class MovieCollectionView : UserControl, IViewFor<MovieCollectionViewModel>
// { ... w�a�ciwo�ci ViewModel ... }


namespace OmniMedia.Views // Upewnij si�, �e namespace jest poprawny
{
    // Standardowy plik code-behind dla User Control
    // Na razie nie potrzebujemy tutaj zaawansowanej logiki ani WhenActivated
    public partial class MovieCollectionView : UserControl
    {
        public MovieCollectionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}