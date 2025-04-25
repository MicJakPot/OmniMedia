using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI; // Potrzebne do IViewFor
using OmniMedia.ViewModels; // Potrzebne do MovieCollectionViewModel

// TODO: Jeœli bêdziesz potrzebowaæ WhenActivated w tym code-behind, dodaj implementacjê IViewFor
// public partial class MovieCollectionView : UserControl, IViewFor<MovieCollectionViewModel>
// { ... w³aœciwoœci ViewModel ... }


namespace OmniMedia.Views // Upewnij siê, ¿e namespace jest poprawny
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