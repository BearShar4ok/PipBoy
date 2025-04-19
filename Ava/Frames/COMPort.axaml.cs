using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Tmds.DBus.Protocol;

namespace Ava;

public partial class COMPort : UserControl
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);
    private DateTime lastReadTime = DateTime.Now;
    private SerialPort serialPort;

    //private Dictionary<string, Dictionary<string, int>> allCommands = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, Dictionary<string, string>> allCommands = new Dictionary<string, Dictionary<string, string>>();
    //private Dictionary<int, string> devices = new Dictionary<int, string>();
    private Dictionary<string, string> devices = new Dictionary<string, string>();
    private string currentDevice = "";
    private const string codeAuth = "1F00FFFF";
    private List<Button> allButtons = new List<Button>();
    private int nowButtonIndex = 0;

    public COMPort()
    {
        InitializeComponent();

        (allCommands, devices) = ReadJsonFile();

        FillAllButtons();

        string portName = "/dev/serial0";
        serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.Two);

        info.Content = "Loading...";
        try
        {
            Console.WriteLine("Explorer загружен");
            RasberryPINS.ButtonPressedPlusCOMPort += ButtonPressedPrevious;
            RasberryPINS.ButtonPressedMinusCOMPort += ButtonPressedNext;
            RasberryPINS.ButtonPressedPressCOMPort += ButtonPressedPress;
        }
        catch (Exception ex)
        {
            info.Content = "Ошибка GPIO: " + ex.Message;
        }
    }
    private void FindingDevice()
    {
        info.Content = "Loading...";

        serialPort.ReadTimeout = 3000;

        Console.WriteLine("serialPort.ReadTimeout: " + serialPort.ReadTimeout);
        Console.WriteLine("serialPort.WriteTimeout: " + serialPort.WriteTimeout);
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (serialPort.IsOpen)
            {
                Console.WriteLine("Порт БЫЛ открыт. Закрыт...");
            }
            else
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                Thread.Sleep(1000);
                Console.WriteLine("Порт открыт");
            }
            Console.WriteLine("Отправка данных...");

            Send(codeAuth);
            
            Console.WriteLine("Отправка данных ЗАВЕРШЕНА");
            try
            {
                string data = serialPort.ReadLine();
                data = data.Trim();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Получено от Arduino:{data}:");
                Console.ResetColor();
                if (data == null || data.Length <= 1 || string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                {
                    info.Content = "Нет доступных устройств...";
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Нет доступных устройств...Получено от Arduino:{data}:");

                    Console.WriteLine("FindingDevice завершен в return if");
                    Console.ResetColor();
                    return;
                }
                if (devices.ContainsKey(data))
                {
                    currentDevice = devices[data];
                    info.Content = currentDevice;
                    LoadButtons();
                }
                else
                {
                    info.Content = "Устройство не поддерживается...   |" + data + "|";

                    Console.WriteLine("FindingDevice завершен в return else");
                    return;
                }
            }
            catch (TimeoutException)
            {
                if (DateTime.Now - lastReadTime > Timeout)
                {
                    Console.WriteLine("Тайм аут ожидания данных...");
                    lastReadTime = DateTime.Now;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine("ArgumentOutOfRangeException:  " + ex.Message);
                serialPort.Close();
                Thread.Sleep(100);
                serialPort.Open();
                Thread.Sleep(100);
                Console.WriteLine("open");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex :  " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: " + ex.Message);
            Console.ResetColor();
        }
        Console.WriteLine("FindingDevice завершен в конце");
    }
    private void Send(string text)
    {
        serialPort.DiscardInBuffer();
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            serialPort.WriteLine(text);
            Console.WriteLine(text);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка в отправке: " + ex.Message);
        }

    }
    public void Button1_Click(object source, RoutedEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        serialPort.Close();
        Console.WriteLine("Close");
        serialPort.Dispose();

        Console.WriteLine("Dispose");
        Console.ResetColor();

    }
    public void Button3_Click(object source, RoutedEventArgs args)
    {
        FindingDevice();
    }
    void ListenAndAnalyse()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        try
        {
            string data = serialPort.ReadLine();
            if (data == null || data.Length <= 1 || string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine("Пустой ответ");
                return;
            }
            Console.WriteLine($"Получено от Arduino:{data}:");
            Console.ResetColor();
        }
        catch (TimeoutException)
        {
            if (DateTime.Now - lastReadTime > Timeout)
            {
                Console.WriteLine("Тайм аут ожидания данных...");
                lastReadTime = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ex :  " + ex.Message);
        }
        Console.ResetColor();
    }
    private void FillAllButtons()
    {
        allButtons.Clear();
        foreach (var item in Field.Children)
        {
            if (item is Button)
                allButtons.Add(item as Button);
        }

        foreach (var item in Buttons.Children)
        {
            if (item is Button)
                allButtons.Add(item as Button);
        }

        
        nowButtonIndex = 0;
        SelectButton();
    }
    private void LoadButtons()
    {
        Buttons.Children.Clear();
        foreach (var kvp in allCommands[currentDevice])
        {
            Button b = new Button()
            {
                Content = kvp.Key,
                Tag = kvp.Value,
            };
            b.Click += (_, _) => { Send(b.Tag.ToString()); ListenAndAnalyse(); };
            Buttons.Children.Add(b);
        }
        FillAllButtons();
    }

    private void SelectButton()
    {
        foreach (var button in allButtons)
        {
            button.Background = new SolidColorBrush(Colors.DarkGray);
        }
        allButtons[nowButtonIndex].Background = new SolidColorBrush(Colors.ForestGreen);
    }
    private void ButtonPressedPress()
    {
        Console.WriteLine("PORT: кнопка нажата");
        allButtons[nowButtonIndex].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }
    private void ButtonPressedPrevious()
    {
        Console.WriteLine("PORT: кнопка -");
        if (nowButtonIndex > 0)
            nowButtonIndex--;
        encoderInfo.Content = nowButtonIndex;
        SelectButton();
    }

    private void ButtonPressedNext()
    {
        Console.WriteLine("PORT: кнопка +");
        if (nowButtonIndex < allButtons.Count - 1)
            nowButtonIndex++;
        encoderInfo.Content = nowButtonIndex;
        SelectButton();
    }
    //private (Dictionary<string, Dictionary<string, int>>, Dictionary<int, string>) ReadJsonFile()
    private (Dictionary<string, Dictionary<string, string>>, Dictionary<string, string>) ReadJsonFile()
    {
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string filePath = Path.Combine(exeDir, "Assets", "commands.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("JSON file not found", filePath);

        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        //var devices = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(data["devices"].ToString());
        var devices = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(data["devices"].ToString());
        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(data["settings"].ToString());
        //var settingsLast = settings.ToDictionary(k=> Convert.ToInt32(k.Key,16), k=> k.Value);

        return (devices, settings);
    }
}