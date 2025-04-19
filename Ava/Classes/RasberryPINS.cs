using Avalonia.Threading;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Ava.Classes
{
    internal class RasberryPINS
    {
        public const int buttonPinEncoder = 22;

        private GpioController? controller;
        private MainWindow MW;
        private FileSystem FS;

        private RotaryEncoder encoderSecond = new RotaryEncoder(17, 27);
        public readonly RotaryEncoder encoderMain = new RotaryEncoder(13, 6);

        private const int DebounceDelay = 400;   //антидрибезг / антизажим

        private DateTime? lastPressY = null;
        private DateTime? lastPressG = null;
        private DateTime? lastPressP = null;

        public static event Action ButtonPressedPlusExplorer;
        public static event Action ButtonPressedMinusExplorer;
        public static event Action ButtonPressedPlusMap;
        public static event Action ButtonPressedMinusMap;
        public static event Action ButtonPressedPlusCOMPort;
        public static event Action ButtonPressedMinusCOMPort;

        public static event Action ButtonPressedPressMap;
        public static event Action ButtonPressedPressExplorer;
        public static event Action ButtonPressedPressCOMPort;

        public static event Action ButtonPressedPressTextPage;
        public static event Action ButtonPressedPressImagePage;

        public RasberryPINS(MainWindow mw, FileSystem fs)
        {
            MW = mw;
            FS = fs;
        }

        public void InitializeGpio()
        {
            controller = new GpioController();
            encoderSecond.RotatedRight += (position) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    HandleRotateRight();
                });
            };
            encoderSecond.RotatedLeft += (position) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    HandleRotateLeft();
                });
            };
            Task.Run(MonitorButtons);
        }

        private void MonitorButtons()
        {
            controller.OpenPin(buttonPinEncoder, PinMode.InputPullUp);

            while (true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (controller.Read(buttonPinEncoder) == PinValue.Low)
                    {
                        if (lastPressP == null || (DateTime.Now - lastPressP.Value).TotalMilliseconds > DebounceDelay)
                        {
                            lastPressP = DateTime.Now;
                            HandleButtonRotate();
                        }
                    }
                });
                Thread.Sleep(50);
            }
        }

        private void HandleRotateRight()
        {
            Console.WriteLine("WS " + MW.FrameHOST);
            Console.WriteLine("FS " + FS.FrameHOST);
            if (MW.FrameHOST is FileSystem)
            {
                if (FS.FrameHOST is Explorer)
                {
                    ButtonPressedPlusExplorer?.Invoke();
                }
            }
            else if (MW.FrameHOST is MapPage)
            {
                ButtonPressedPlusMap?.Invoke();
            }
            else if (MW.FrameHOST is COMPort)
            {
                ButtonPressedPlusCOMPort?.Invoke();
            }
        }

        private void HandleRotateLeft()
        {
            Console.WriteLine("WS " + MW.FrameHOST);
            Console.WriteLine("FS " + FS.FrameHOST);
            if (MW.FrameHOST is FileSystem)
            {
                if (FS.FrameHOST is Explorer)
                {
                    ButtonPressedMinusExplorer?.Invoke();
                }
            }
            else if (MW.FrameHOST is MapPage)
            {
                ButtonPressedMinusMap?.Invoke();
            }
            else if (MW.FrameHOST is COMPort)
            {
                ButtonPressedMinusCOMPort?.Invoke();
            }
        }

        private void HandleButtonRotate()
        {
            Console.WriteLine("WS " + MW.FrameHOST);
            Console.WriteLine("FS " + FS.FrameHOST);
            if (MW.FrameHOST is FileSystem)
            {
                if (FS.FrameHOST is Explorer)
                {
                    ButtonPressedPressExplorer?.Invoke();
                }
                else if (FS.FrameHOST is TextPage)
                {
                    ButtonPressedPressTextPage?.Invoke();
                }
                else if (FS.FrameHOST is ImagePage)
                {
                    ButtonPressedPressImagePage?.Invoke();
                }
            }
            else if (MW.FrameHOST is MapPage)
            {
                ButtonPressedPressMap?.Invoke();
            }
            else if (MW.FrameHOST is COMPort)
            {
                ButtonPressedPressCOMPort?.Invoke();
            }
        }

        public void Cleanup()
        {
            if (controller != null)
            {
                controller.Dispose();
                controller = null;
            }
        }
    }
}