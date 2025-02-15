using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.IO;
using System.Reflection;

namespace Ava;

public partial class MapPage : UserControl
{
    private double _zoomFactor = 1.0;
    private readonly ScaleTransform _scaleTransform;

    public MapPage()
    {
        InitializeComponent();


        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string imagePath = Path.Combine(exeDir, "Assets", "map.jpg");

        if (File.Exists(imagePath))
        {
            MapImage.Source = new Bitmap(imagePath);
            info.Content = "YES " + imagePath;
        }
        else
        {
            info.Content = "No " + imagePath;
        }

        _scaleTransform = this.Resources["MapScaleTransform"] as ScaleTransform;

        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)";
            return;
        }

        try
        {
            Console.WriteLine("MapPage загружен");
            Console.WriteLine("MapPage: подписка на GPIO");
            RasberryPINS.ButtonPressedPlusMap += ButtonPressedPlus;
            RasberryPINS.ButtonPressedMinusMap += ButtonPressedMinus;

            info.Content = "Контроллер активен";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка GPIO: " + ex.Message);
        }
    }

    private void ButtonPressedPlus()
    {
        Console.WriteLine("MapPage: кнопка +");
        Dispatcher.UIThread.Post(() =>
        {
            if (_zoomFactor < 2)
                ChangeZoom(0.1);
        });
    }

    private void ButtonPressedMinus()
    {
        Console.WriteLine("MapPage: кнопка -");
        Dispatcher.UIThread.Post(() =>
        {
            if (_zoomFactor > 1)
                ChangeZoom(-0.1);
        });
    }

    private void ChangeZoom(double zoomStep)
    {
        _zoomFactor = Math.Clamp(_zoomFactor + zoomStep, 0.5, 5);
        _scaleTransform.ScaleX = _zoomFactor;
        _scaleTransform.ScaleY = _zoomFactor;
    }
}