using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OmniMedia.Views
{
    public partial class MusicSearchWindow : Window
    {
        public MusicSearchWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}