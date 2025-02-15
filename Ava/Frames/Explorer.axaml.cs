using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Ava;

public partial class Explorer : UserControl
{
    public ObservableCollection<string> Folders { get; set; }

    public Explorer()
    {
        InitializeComponent();
        Folders = new ObservableCollection<string>();

        string exeDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..");

        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)";
            exeDirectory = Path.Combine(exeDirectory, "..");
            LoadTest(exeDirectory);
            return;
        }

        try
        {
            Console.WriteLine("Explorer загружен");
            Console.WriteLine("Explorer: подписка на GPIO");
            RasberryPINS.ButtonPressedPlusExplorer += ButtonPressedPlus;
            RasberryPINS.ButtonPressedMinusExplorer += ButtonPressedMinus;

            exeDirectory = "/home/bearshark/Downloads/My_disk";
            LoadTest(exeDirectory);
            info.Content = "Контроллер активен";
        }
        catch (Exception ex)
        {
            exeDirectory = Path.Combine(exeDirectory, "..");
            LoadTest(exeDirectory);
            info.Content = "Ошибка GPIO: " + ex.Message;
        }
    }

    private void LoadTest(string exeDirectory)
    {
        if (Directory.Exists(exeDirectory))
        {
            string[] directories = Directory.GetDirectories(exeDirectory);

            foreach (var dir in directories)
            {
                explorerLB.Items.Add(Path.GetFileName(dir));
                Folders.Add(Path.GetFileName(dir));
            }
            explorerLB.SelectedIndex = 0;
        }
    }

    private void ButtonPressedPlus()
    {
        Console.WriteLine("Explorer: кнопка +");
        if (explorerLB.SelectedIndex > 0)
            explorerLB.SelectedIndex--;
    }

    private void ButtonPressedMinus()
    {
        Console.WriteLine("Explorer: кнопка -");
        if (explorerLB.SelectedIndex < explorerLB.ItemCount - 1)
            explorerLB.SelectedIndex++;
    }
    public void Button1_Click(object source, RoutedEventArgs args)
    {
        if (explorerLB.SelectedIndex > 0)
        {
            explorerLB.SelectedIndex--;
        }
    }

    public void Button2_Click(object source, RoutedEventArgs args)
    {
        if (explorerLB.SelectedIndex < explorerLB.ItemCount - 1)
        {
            explorerLB.SelectedIndex++;
        }
    }
}