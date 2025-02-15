using Avalonia.Threading;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Ava.Classes
{
    internal class RasberryPINS
    {
        public const int buttonPinO = 21; // right switch
        public const int buttonPinG = 16; // left switch
        public const int buttonPinY = 20; // select -
        public const int buttonPinB = 19; // select +

        private GpioController? controller;
        private MainWindow MW;

        private const int DebounceDelay = 400;   //антидрибезг / антизажим

        private DateTime? lastPressY = null;
        private DateTime? lastPressG = null;

        public static event Action ButtonPressedPlusExplorer;
        public static event Action ButtonPressedPlusMap;
        public static event Action ButtonPressedMinusExplorer;
        public static event Action ButtonPressedMinusMap;

        public RasberryPINS(MainWindow mw)
        {
            MW = mw;
        }

        public void InitializeGpio()
        {
            controller = new GpioController();

            Task.Run(MonitorButtons);
        }

        private void MonitorButtons()
        {
            controller.OpenPin(buttonPinY, PinMode.InputPullUp);
            controller.OpenPin(buttonPinG, PinMode.InputPullUp);

            while (true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    // Обработка кнопки Y
                    if (controller.Read(buttonPinY) == PinValue.Low)
                    {
                        if (lastPressY == null || (DateTime.Now - lastPressY.Value).TotalMilliseconds > DebounceDelay)
                        {
                            lastPressY = DateTime.Now;
                            HandleButtonY();
                        }
                    }

                    // Обработка кнопки G
                    if (controller.Read(buttonPinG) == PinValue.Low)
                    {
                        if (lastPressG == null || (DateTime.Now - lastPressG.Value).TotalMilliseconds > DebounceDelay)
                        {
                            lastPressG = DateTime.Now;
                            HandleButtonG();
                        }
                    }
                });
                Thread.Sleep(50);
            }
        }

        private void HandleButtonY()
        {
            Console.WriteLine("НАЖАТА Y-20");
            Console.WriteLine(MW.FrameHOST);
            if (MW.FrameHOST is Explorer)
            {
                Console.WriteLine("Передача в Explorer...");
                ButtonPressedPlusExplorer?.Invoke();
                Console.WriteLine("Передача в Explorer завершена");
            }
            else if (MW.FrameHOST is MapPage)
            {
                Console.WriteLine("Передача в Map...");
                ButtonPressedPlusMap?.Invoke();
                Console.WriteLine("Передача в Map завершена");
            }
        }

        private void HandleButtonG()
        {
            Console.WriteLine("НАЖАТА G-16");
            Console.WriteLine(MW.FrameHOST);
            if (MW.FrameHOST is Explorer)
            {
                Console.WriteLine("Передача в Explorer...");
                ButtonPressedMinusExplorer?.Invoke();
                Console.WriteLine("Передача в Explorer завершена");
            }
            else if (MW.FrameHOST is MapPage)
            {
                Console.WriteLine("Передача в Map...");
                ButtonPressedMinusMap?.Invoke();
                Console.WriteLine("Передача в Map завершена");
            }
        }

        public void Cleanup()
        {
            if (controller != null)
            {
                controller.Dispose();
                controller = null;
                Console.WriteLine("GpioService: GPIO cleaned up");
            }
        }
    }
}