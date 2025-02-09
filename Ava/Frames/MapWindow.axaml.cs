using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.VisualBasic;


using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Device.Gpio;
using System.Threading;
using Avalonia.Input;
using Avalonia.Media;
using System.IO;
using System.Diagnostics;
using Tmds.DBus.Protocol;
using Avalonia.Threading;

namespace Ava;

public partial class MapWindow : Window
{
   
    private double _zoomFactor = 1.0;
    private ScaleTransform _scaleTransform;

    GpioController controller = new GpioController();


    int buttonPinY = 21; // GPIO 21
    int buttonPinG = 19; // GPIO 19
    int ledPinY = 20; // GPIO 20
    int ledPinG = 16; // GPIO 16
    public MapWindow()
    {
        InitializeComponent();



        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads/publish/map.png");//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        
        
        // Убедитесь, что файл существует
        if (File.Exists(imagePath))
        {
            MapImage.Source = new Bitmap(imagePath);
            info.Content = imagePath;
        }
        else
        {
            info.Content = "No   " + imagePath;
            Debug.WriteLine("Изображение не найдено: " + imagePath);
        }
        // Получаем ScaleTransform из ресурсов
        _scaleTransform = this.Resources["MapScaleTransform"] as ScaleTransform;
        TEST_GPIO();
    }
    private void TEST_GPIO()
    {

        //controller.Write(ledPinY, PinValue.Low);
        //controller.Write(ledPinG, PinValue.Low);


        controller.OpenPin(buttonPinY, PinMode.InputPullUp);
        //controller.OpenPin(ledPinY, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(buttonPinY,
            PinEventTypes.Falling, ButtonPressedPlus);
        controller.RegisterCallbackForPinValueChangedEvent(buttonPinY,
            PinEventTypes.Rising, ButtonRealesdPlus);





        controller.OpenPin(buttonPinG, PinMode.InputPullUp);
        //controller.OpenPin(ledPinG, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(buttonPinG,
            PinEventTypes.Falling, ButtonPressedMinus);
        controller.RegisterCallbackForPinValueChangedEvent(buttonPinG,
            PinEventTypes.Rising, ButtonRealesdMinus);

    }
    private void ButtonPressedPlus(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            infoB1.Content = "1 work";
            if (_zoomFactor >= 2)
                return;
            ChangeZoom(0.1); // Увеличиваем зум на 10%
            infoB1.Content = "1 pressed";
        });
    }

    private void ButtonRealesdPlus(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {

        Dispatcher.UIThread.Post(() =>
        {
            infoB1.Content = "1 released";
        });
    }

    private void ButtonPressedMinus(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            infoB2.Content = "2 work";
            if (_zoomFactor <= 1)
                return;
            ChangeZoom(-0.1); // Уменьшаем зум на 10%
            infoB2.Content = "2 pressed";
        });
    }
    private void ButtonRealesdMinus(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            infoB2.Content = "2 released";
        });
    }

    private void ZoomInButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_zoomFactor >= 2)
            return;
        ChangeZoom(0.1); // Увеличиваем зум на 10%
    }

    private void ZoomOutButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_zoomFactor <= 1)
            return;
        ChangeZoom(-0.1); // Уменьшаем зум на 10%
    }

    private void ChangeZoom(double zoomStep)
    {
        _zoomFactor = Math.Clamp(_zoomFactor + zoomStep, 0.5, 5); // Ограничиваем масштаб от 0.5 до 5
        _scaleTransform.ScaleX = _zoomFactor;
        _scaleTransform.ScaleY = _zoomFactor;
    }

}