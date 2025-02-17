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
        public const int buttonPinP = 26; // select press

        private GpioController? controller;
        private MainWindow MW;

        private const int DebounceDelay = 400;   //антидрибезг / антизажим

        private DateTime? lastPressY = null;
        private DateTime? lastPressG = null;
        private DateTime? lastPressP = null;

        public static event Action ButtonPressedPlusExplorer;
        public static event Action ButtonPressedPlusMap;
        public static event Action ButtonPressedMinusExplorer;
        public static event Action ButtonPressedMinusMap;

        public static event Action ButtonPressedPressMap;
        public static event Action ButtonPressedPressExplorer;

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
            controller.OpenPin(buttonPinP, PinMode.InputPullUp);

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

                    // Обработка кнопки Purple
                    if (controller.Read(buttonPinP) == PinValue.Low)
                    {
                        if (lastPressP == null || (DateTime.Now - lastPressP.Value).TotalMilliseconds > DebounceDelay)
                        {
                            lastPressP = DateTime.Now;
                            HandleButtonP();
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

        private void HandleButtonP()
        {
            Console.WriteLine("НАЖАТА P-26");
            Console.WriteLine(MW.FrameHOST);
            if (MW.FrameHOST is Explorer)
            {
                Console.WriteLine("Передача в Explorer...");
                ButtonPressedPressExplorer?.Invoke();
                Console.WriteLine("Передача в Explorer завершена");
            }
            else if (MW.FrameHOST is MapPage)
            {
                Console.WriteLine("Передача в Map...");
                ButtonPressedPressMap?.Invoke();
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