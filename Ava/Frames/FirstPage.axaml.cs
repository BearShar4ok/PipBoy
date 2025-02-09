using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ava;

public partial class FirstPage : UserControl
{
    public FirstPage()
    {
        InitializeComponent();
    }
    private void GoToSecondPage(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.Parent is ContentControl frame)
        {
            frame.Content = new SecondPage();
        }
    }
}