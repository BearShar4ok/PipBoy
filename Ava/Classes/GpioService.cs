using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ava.Classes
{
    public class GpioService
    {
        private readonly GpioController controller;
        public event Action ButtonPressedPlus;
        public event Action ButtonPressedMinus;

        public GpioService()
        {
            controller = new GpioController();
        }

        public void InitializeGpio()
        {
            // Инициализация GPIO
            controller.OpenPin(RasberryPINS.buttonPinY, PinMode.InputPullUp);
            controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinY,
                PinEventTypes.Falling, ButtonPressedPlusHandler);

            controller.OpenPin(RasberryPINS.buttonPinG, PinMode.InputPullUp);
            controller.RegisterCallbackForPinValueChangedEvent(RasberryPINS.buttonPinG,
                PinEventTypes.Falling, ButtonPressedMinusHandler);
        }

        private void ButtonPressedPlusHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            ButtonPressedPlus?.Invoke();
        }

        private void ButtonPressedMinusHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            ButtonPressedMinus?.Invoke();
        }

        public void Cleanup()
        {
            controller.Dispose();
        }
    }
}
