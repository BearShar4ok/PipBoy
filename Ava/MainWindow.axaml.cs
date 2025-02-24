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
        private RotaryEncoder? encoder = null;
        private Control currentPage;
        private RasberryPINS? rasberryPINS = null;

        private Control[] pages;
        int pageIndex = 0;

        public object FrameHOST { get { return FrameHost.Content; } }


        public MainWindow()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
            {
                label.Content = "Design mode (Previewer)";
                return;
            }

            try
            {

                rasberryPINS = new RasberryPINS(this);
                rasberryPINS.InitializeGpio();

                controller = new GpioController();
                encoder = new RotaryEncoder(13, 6);

                encoder.RotatedRight += (position) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        infocode.Content = "Encoder state: " + encoder.Position;
                        SwitchPage(true);
                    });
                };
                encoder.RotatedLeft += (position) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        infocode.Content = "Encoder state: " + encoder.Position;
                        SwitchPage(false);
                    });
                };
                pages = new Control[] { new Explorer(), new MapPage(), new COMPort() };
                Task.Run(MonitorButtons);
            }
            catch (Exception ex)
            {
                pages = new Control[] { new Explorer(), new MapPage(), new COMPort() };
                Debug.WriteLine("Ошибка инициализации GPIO: " + ex.Message);
            }

            currentPage = pages[pageIndex];
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

                currentPage = pages[pageIndex];
                FrameHost.Content = currentPage;
                label.Content = (pageIndex + 1) + "/" + pages.Length;
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