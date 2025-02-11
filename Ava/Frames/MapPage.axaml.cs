using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;

using System;
using System.Reflection;



namespace Ava;

public partial class MapPage : UserControl
{
    private double _zoomFactor = 1.0;
    private ScaleTransform _scaleTransform;

    private GpioController? controller = null;
    public MapPage()
    {
        InitializeComponent();

        // Проверяем, работает ли Avalonia Previewer



        //  string imagePath = "avares://Ava/Assets/map.jpg";  // Путь к ресурсу
        //string imagePath = "avares://Ava/Assets/map.jpg";  // Путь к ресурсу
        //MapImage.Source = new Bitmap(imagePath);

        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //Console.WriteLine(exePath);

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





        //if (File.Exists(imagePath))
        //{
        //    MapImage.Source = new Bitmap(imagePath);
        //    info.Content = "YES " + imagePath;
        //    MapCanvas.Width = MapImage.Width;
        //    MapCanvas.Height = MapImage.Height;
        //}
        //else
        //{
        //    info.Content = "No " + imagePath;
        //    Debug.WriteLine("Изображение не найдено: " + imagePath);
        //}

        _scaleTransform = this.Resources["MapScaleTransform"] as ScaleTransform;
        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)" + imagePath;
            return;
        }
        try
        {
            controller = new GpioController();
            info.Content = "CONTROLLER    " + imagePath;
            TEST_GPIO();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Ошибка инициализации GPIO: " + ex.Message);
        }
    }
    private void TEST_GPIO()
    {

        //controller.Write(ledPinY, PinValue.Low);
        //controller.Write(ledPinG, PinValue.Low);


        controller.OpenPin(RasberryPINS.buttonPinY, PinMode.InputPullUp);
        //controller.OpenPin(ledPinY, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinY,
            PinEventTypes.Falling, ButtonPressedPlus);
        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinY,
            PinEventTypes.Rising, ButtonRealesdPlus);





        controller.OpenPin(RasberryPINS.buttonPinG, PinMode.InputPullUp);
        //controller.OpenPin(ledPinG, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinG,
            PinEventTypes.Falling, ButtonPressedMinus);
        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinG,
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


    private void ChangeZoom(double zoomStep)
    {
        _zoomFactor = Math.Clamp(_zoomFactor + zoomStep, 0.5, 5); // Ограничиваем масштаб от 0.5 до 5
        _scaleTransform.ScaleX = _zoomFactor;
        _scaleTransform.ScaleY = _zoomFactor;
    }
}