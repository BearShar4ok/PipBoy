using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;

namespace Ava;

public partial class Explorer : UserControl
{
    public ObservableCollection<string> Folders { get; set; }
    private readonly GpioService gpioService;
    //private GpioController? controller = null;
    public Explorer()
    {
        InitializeComponent();
        Folders = new ObservableCollection<string>();
        string exeDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..");


        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)";
            exeDirectory = Path.Combine(exeDirectory, "..");
            LoadTEst(exeDirectory);
            return;
        }
        try
        {
            // controller = new GpioController();
            gpioService = new GpioService();

            gpioService.ButtonPressedPlus += ButtonPressedPlus;
            gpioService.ButtonPressedMinus += ButtonPressedMinus;

            gpioService.InitializeGpio();
            exeDirectory = "/home/bearshark/Downloads/My_disk";
            LoadTEst(exeDirectory);
            info.Content = "CONTROLLER    ";
            TEST_GPIO();
        }
        catch (Exception ex)
        {
            exeDirectory = Path.Combine(exeDirectory, "..");
            LoadTEst(exeDirectory);
            info.Content = "Ошибка инициализации GPIO: " + ex.Message;
        }
    }
    private void LoadTEst(string exeDirectory)
    {
        if (Directory.Exists(exeDirectory))
        {
            string[] directories = Directory.GetDirectories(exeDirectory);

            foreach (var dir in directories)
            {
                var t = Path.GetFileName(dir);
                explorerLB.Items.Add(t);
                Folders.Add(t);
            }
            explorerLB.SelectedIndex = 0;
        }
        // Настроим ScrollBar для прокрутки
       
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
    private void TEST_GPIO()
    {
        /*
        //controller.Write(ledPinY, PinValue.Low);
        //controller.Write(ledPinG, PinValue.Low);


        controller.OpenPin(RasberryPINS.buttonPinY, PinMode.InputPullUp);
        //controller.OpenPin(ledPinY, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinY,
            PinEventTypes.Falling, ButtonPressedPlus);






        controller.OpenPin(RasberryPINS.buttonPinG, PinMode.InputPullUp);
        //controller.OpenPin(ledPinG, PinMode.Output);

        controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinG,
            PinEventTypes.Falling, ButtonPressedMinus);

        */
    }
    private void ButtonPressedPlus()
    {
        if (explorerLB.SelectedIndex > 0)
        {
            explorerLB.SelectedIndex--;
        }
    }

    private void ButtonPressedMinus()
    {
        if (explorerLB.SelectedIndex < explorerLB.ItemCount - 1)
        {
            explorerLB.SelectedIndex++;
        }
    }

}