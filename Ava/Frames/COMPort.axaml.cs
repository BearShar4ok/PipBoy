using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3); // Установим задержку в 3 секунды
    private DateTime lastReadTime = DateTime.Now;
    private SerialPort serialPort;

    private Dictionary<string, Dictionary<string, int>> allCommands = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<int, string> devices = new Dictionary<int, string>();
    private string currentDevice = "";
    private const int codeAuth = 0x1F00FFFF;
    public COMPort()
    {
        InitializeComponent();


        (allCommands, devices) = ReadJsonFile();







        string portName = "/dev/serial0";
        serialPort = new SerialPort(portName, 9600);

        //new Thread(TEst).Start();
        info.Content = "Loading...";
        //FindingDevice();
        
        //GotFocus += (_, _) => { Console.WriteLine("FOCUS"); FindingDevice(); };
    }
    private void FindingDevice()
    {
        // Указываем правильный UART порт на Raspberry Pi (обычно /dev/serial0 или /dev/ttyAMA0)
        info.Content = "Loading...";

       
        serialPort.ReadTimeout = 3000;
        serialPort.BaudRate = 9600;

        //serialPort.WriteTimeout = 1000;
        Console.WriteLine("serialPort.ReadTimeout: " + serialPort.ReadTimeout);
        Console.WriteLine("serialPort.WriteTimeout: " + serialPort.WriteTimeout);
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (serialPort.IsOpen)
            {
                //serialPort.Close();
                Console.WriteLine("Порт БЫЛ открыт. Закрыт...");
                Debug.WriteLine("Порт БЫЛ открыт. Закрыт...");
            }
            else
            {
                serialPort.Open();
                Console.WriteLine("Порт открыт");
                Debug.WriteLine("Порт открыт");
            }
            Console.WriteLine("Отправка данных...");
            Debug.WriteLine("Отправка данных...");

            Send(codeAuth.ToString());
            // Чтение данных от Arduino
            //while (true)
            //{
            Console.WriteLine("Отправка данных ЗАВЕРШЕНА");
            Debug.WriteLine("Отправка данных ЗАВЕРШЕНА");
            try
            {
                string data = serialPort.ReadLine();
                if (data == null || data.Length <= 1 || string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                {
                    info.Content = "Нет доступных устройств...";

                    Console.WriteLine($"Нет доступных устройств...Получено от Arduino:{data}:");
                    Debug.WriteLine($"Нет доступных устройств...Получено от Arduino:{data}:");


                    Console.WriteLine("FindingDevice завершен в return if");
                    Debug.WriteLine("FindingDevice завершен в return if");
                    return;
                }
                int data2 = int.Parse(data);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Получено от Arduino:{data}:");
                Debug.WriteLine($"Получено от Arduino:{data}:");
                Console.ResetColor();

                if (devices.ContainsKey(data2))
                {
                    currentDevice = devices[data2];
                    info.Content = currentDevice;
                    LoadButtons();
                }
                else
                {
                    info.Content = "Устройство не поддерживается...   |" + data2 + "|";

                    Console.WriteLine("FindingDevice завершен в return else");
                    Debug.WriteLine("FindingDevice завершен в return else");
                    return;
                }
            }
            catch (TimeoutException)
            {

                if (DateTime.Now - lastReadTime > Timeout)
                {
                    Console.WriteLine("Тайм аут ожидания данных...");
                    Debug.WriteLine("Тайм аут ожидания данных...");
                    lastReadTime = DateTime.Now;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("ex :  " + ex.Message);
            }
            //}
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.WriteLine("Ошибка: " + ex.Message);
            Console.WriteLine("Ошибка: " + ex.Message);
            Console.ResetColor();
        }
        //finally
        //{
        //    Console.ForegroundColor = ConsoleColor.Red;
        //    serialPort.Close();
        //    Console.WriteLine("Close");
        //    Debug.WriteLine("Close");
        //    serialPort.Dispose();
        //
        //    Console.WriteLine("Dispose");
        //    Debug.WriteLine("Dispose");
        //    Console.ResetColor();
        //}
        Console.WriteLine("FindingDevice завершен в конце");
        Debug.WriteLine("FindingDevice завершен в конце");
    }
    private void Send(string text)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            serialPort.WriteLine(text);
            Console.WriteLine(text);
            Debug.WriteLine(text);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Ошибка в отправке: " + ex.Message);
            Console.WriteLine("Ошибка в отправке: " + ex.Message);
        }
       
    }
    public void Button1_Click(object source, RoutedEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        serialPort.Close();
        Console.WriteLine("Close");
        Debug.WriteLine("Close");
        serialPort.Dispose();

        Console.WriteLine("Dispose");
        Debug.WriteLine("Dispose");
        Console.ResetColor();
        
    }
    public void Button2_Click(object source, RoutedEventArgs args)
    {
        
    }
    public void Button3_Click(object source, RoutedEventArgs args)
    {
        FindingDevice();
    }

    private void LoadButtons()
    {
        foreach (var kvp in allCommands[currentDevice])
        {
            Button b = new Button()
            {
                Content = kvp.Key,
                Tag = kvp.Value,
            };
            b.Click += (_, _) => Send(b.Tag.ToString());
            Field.Children.Add(b);
        }
    }
    private (Dictionary<string, Dictionary<string, int>>, Dictionary<int, string>) ReadJsonFile()
    {
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string filePath = Path.Combine(exeDir, "Assets", "commands.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("JSON file not found", filePath);

        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        var devices = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(data["devices"].ToString());
        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(data["settings"].ToString());
        var settingsLast = settings.ToDictionary(k=> Convert.ToInt32(k.Key,16), k=> k.Value);

        return (devices, settingsLast);
    }
}