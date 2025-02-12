using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Device.Gpio;
using System.Threading;

namespace Ava.Classes
{
    public class RotaryEncoder : IDisposable
    {
        public event Action<int>? RotatedRight; // Событие, передающее новую позицию
        public event Action<int>? RotatedLeft; // Событие, передающее новую позицию


        private readonly int pinA;
        private readonly int pinB;
        private readonly GpioController controller;
        private int lastAState;
        public int Position { get; private set; } = 0;

        public RotaryEncoder(int pinA, int pinB)
        {
            this.pinA = pinA;
            this.pinB = pinB;
            controller = new GpioController();

            // Открываем пины
            controller.OpenPin(pinA, PinMode.InputPullUp);
            controller.OpenPin(pinB, PinMode.InputPullUp);

            // Читаем начальное состояние
            lastAState = controller.Read(pinA) == PinValue.High ? 1 : 0;

            // Подписываемся на события
            controller.RegisterCallbackForPinValueChangedEvent(pinA, PinEventTypes.Falling | PinEventTypes.Rising, OnPinAChanged);
        }

        private void OnPinAChanged(object sender, PinValueChangedEventArgs args)
        {
            int aState = controller.Read(pinA) == PinValue.High ? 1 : 0;
            int bState = controller.Read(pinB) == PinValue.High ? 1 : 0;

            if (aState != lastAState)
            {
                if (bState != aState)
                {
                    Position++;  // Вращение вправо
                    RotatedLeft?.Invoke(Position); // Вызываем событие
                }
                else
                {
                    Position--;  // Вращение влево
                    RotatedRight?.Invoke(Position); // Вызываем событие
                }
               

                Console.WriteLine($"Position: {Position}");
            }

            lastAState = aState;
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
