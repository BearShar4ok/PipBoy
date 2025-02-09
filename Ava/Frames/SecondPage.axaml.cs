using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ava;

public partial class SecondPage : UserControl
{
    public SecondPage()
    {
        InitializeComponent();
    }
    private void GoToFirstPage(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.Parent is ContentControl frame)
        {
            frame.Content = new FirstPage();
        }
    }
}