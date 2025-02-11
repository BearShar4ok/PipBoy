using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ava;

public partial class FirstPage : UserControl
{
    public FirstPage(string text)
    {
        InitializeComponent();
        button.Content = text;
    }
    private void GoToSecondPage(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.Parent;
        while (parent is not ContentControl && parent is not null)
        {
            parent = (parent as Control)?.Parent;
        }

        if (parent is ContentControl frame)
        {
            frame.Content = new FirstPage("new one");
        }
    }
}