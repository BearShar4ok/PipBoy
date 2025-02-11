using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.VisualBasic;


using Avalonia;
using System;
using System.Device.Gpio;
using System.Threading;
using Avalonia.Input;
using Avalonia.Media;
using System.IO;
using System.Diagnostics;
using Tmds.DBus.Protocol;
using Avalonia.Threading;
using System.Threading.Tasks;
using Ava.Classes;
using Avalonia.Interactivity;



namespace Ava
{
    public partial class MainWindow : Window
    {
        private double _zoomFactor = 1.0;
        private ScaleTransform _scaleTransform;

        private GpioController? controller = null;
        private Control currentPage;

        private Control[] pages = new Control[] { new FirstPage("1"), new Explorer(), new MapPage(), new FirstPage("3"), new FirstPage("4"), new FirstPage("5") };
        int pageIndex = 0;

        int buttonPinY = 21; // GPIO 21
        int buttonPinG = 19; // GPIO 19
        int ledPinY = 20; // GPIO 20
        int ledPinG = 16; // GPIO 16
        public MainWindow()
        {
            InitializeComponent();

            // Проверяем, работает ли Avalonia Previewer
            if (Design.IsDesignMode)
            {
                label.Content = "Design mode (Previewer)";
                return;
            }

            try
            {
                controller = new GpioController();
                Task.Run(MonitorButtons);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка инициализации GPIO: " + ex.Message);
            }

            currentPage = pages[0]; // Загружаем стартовую страницу
            FrameHost.Content = currentPage;
        }
        private void MonitorButtons()
        {
            controller.OpenPin(RasberryPINS.buttonPinB, PinMode.InputPullUp);
            controller.OpenPin(RasberryPINS.buttonPinO, PinMode.InputPullUp);

            while (true)
            {
                if (controller.Read(RasberryPINS.buttonPinO) == PinValue.Low)
                {
                    SwitchPage(true);
                    Thread.Sleep(500); // Антидребезг
                }

                if (controller.Read(RasberryPINS.buttonPinB) == PinValue.Low)
                {
                    SwitchPage(false);
                    Thread.Sleep(500);
                }

                Thread.Sleep(50); // Снижаем нагрузку на процессор
            }
        }

        private void SwitchPage(bool isUp)
        {
            if (isUp && pageIndex < pages.Length - 1)
            {
                pageIndex++;
            }
            else if (!isUp && pageIndex > 0)
            {
                pageIndex--;
            }

            Dispatcher.UIThread.Post(() =>
            {
                label.Content = (pageIndex + 1) + "/" + pages.Length;
                currentPage = pages[pageIndex];
                FrameHost.Content = currentPage;
            });
        }

        public void Button1_Click(object source, RoutedEventArgs args)
        {
            SwitchPage(true);
        }
        public void Button2_Click(object source, RoutedEventArgs args)
        {
            SwitchPage(false);
        }
    }
}