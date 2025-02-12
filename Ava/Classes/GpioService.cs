using System;
using System.Device.Gpio;

namespace Ava.Classes
{
    public class GpioService
    {
        private static readonly Lazy<GpioService> _instance = new(() => new GpioService());
        public static GpioService Instance => _instance.Value;

        private readonly GpioController controller;
        private bool isInitialized = false;

        public event Action? ButtonPressedPlus;
        public event Action? ButtonPressedMinus;

        private GpioService()
        {
            controller = new GpioController();
        }

        public void InitializeGpio()
        {
            if (isInitialized)
            {
                Console.WriteLine("GpioService уже инициализирован, повторный вызов игнорируется.");
                return;
            }

            Console.WriteLine("Инициализация GPIO...");
            isInitialized = true;

            controller.OpenPin(RasberryPINS.buttonPinY, PinMode.InputPullUp);
            controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinY, PinEventTypes.Falling, ButtonPressedPlusHandler);

            controller.OpenPin(RasberryPINS.buttonPinG, PinMode.InputPullUp);
            controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinG, PinEventTypes.Falling, ButtonPressedMinusHandler);
        }

        private void ButtonPressedPlusHandler(object sender, PinValueChangedEventArgs args)
        {
            Console.WriteLine("Нажата кнопка +");
            ButtonPressedPlus?.Invoke();
        }

        private void ButtonPressedMinusHandler(object sender, PinValueChangedEventArgs args)
        {
            Console.WriteLine("Нажата кнопка -");
            ButtonPressedMinus?.Invoke();
        }

        public void Cleanup()
        {
            Console.WriteLine("Очистка GPIO...");
            controller.Dispose();
        }
    }
}