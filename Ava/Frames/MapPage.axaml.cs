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

    //private GpioController? controller = null;
    private readonly GpioService gpioService;
    public MapPage()
    {
        InitializeComponent();

        // ���������, �������� �� Avalonia Previewer

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
            info.Content = "Design mode (Previewer)" + imagePath;
            return;
        }
        try
        {
            gpioService = new GpioService();

            gpioService.ButtonPressedPlus += ButtonPressedPlus;
            gpioService.ButtonPressedMinus += ButtonPressedMinus;

            gpioService.InitializeGpio();
            // controller = new GpioController();
            info.Content = "CONTROLLER    " + imagePath;
            TEST_GPIO();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("������ ������������� GPIO: " + ex.Message);
        }
    }
    private void TEST_GPIO()
    {
        /*
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
        */
    }
    private void ButtonPressedPlus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            infoB1.Content = "1 work";
            if (_zoomFactor >= 2)
                return;
            ChangeZoom(0.1); // ����������� ��� �� 10%
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

    private void ButtonPressedMinus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            infoB2.Content = "2 work";
            if (_zoomFactor <= 1)
                return;
            ChangeZoom(-0.1); // ��������� ��� �� 10%
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
        _zoomFactor = Math.Clamp(_zoomFactor + zoomStep, 0.5, 5); // ������������ ������� �� 0.5 �� 5
        _scaleTransform.ScaleX = _zoomFactor;
        _scaleTransform.ScaleY = _zoomFactor;
    }
}