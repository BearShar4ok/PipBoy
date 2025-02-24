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
using System.Reflection;
using System.Threading;
using System.Xml;
using Tmds.DBus.Protocol;

namespace Ava;

public partial class COMPort : UserControl
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3); // ��������� �������� � 3 �������
    private DateTime lastReadTime = DateTime.Now;
    private SerialPort serialPort;

    private Dictionary<string, Dictionary<string, string>> allCommands = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, string> devices = new Dictionary<string, string>();
    private string currentDevice = "";
    private const string codeAuth = "#0000FFFF";
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
        // ��������� ���������� UART ���� �� Raspberry Pi (������ /dev/serial0 ��� /dev/ttyAMA0)
        info.Content = "Loading...";

       
        serialPort.ReadTimeout = 3000;
        //serialPort.WriteTimeout = 1000;
        Console.WriteLine("serialPort.ReadTimeout: " + serialPort.ReadTimeout);
        Console.WriteLine("serialPort.WriteTimeout: " + serialPort.WriteTimeout);
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("���� ��� ������. ������...");
                Debug.WriteLine("���� ��� ������. ������...");
            }
            serialPort.Open();
            Console.WriteLine("���� ������. �������� ������...");
            Debug.WriteLine("���� ������. �������� ������...");

            Send(codeAuth);
            // ������ ������ �� Arduino
            //while (true)
            //{
            try
            {
                string data = serialPort.ReadLine();
                if (data == null || data.Length <= 1 || string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                {
                    info.Content = "��� ��������� ���������...";
                    return;
                }
                data = "#" + data;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"�������� �� Arduino: {data} :");
                Debug.WriteLine($"�������� �� Arduino: {data} :");
                Console.ResetColor();

                if (devices.ContainsKey(data))
                {
                    currentDevice = devices[data];
                    info.Content = currentDevice;
                    LoadButtons();
                }
                else
                {
                    info.Content = "���������� �� ��������������...   |" + data + "|";
                    return;
                }
            }
            catch (TimeoutException)
            {

                if (DateTime.Now - lastReadTime > Timeout)
                {
                    Console.WriteLine("���� ��� �������� ������...");
                    Debug.WriteLine("���� ��� �������� ������...");
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
            Debug.WriteLine("������: " + ex.Message);
            Console.WriteLine("������: " + ex.Message);
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

    }
    private void Send(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        serialPort.WriteLine(text);
        Console.WriteLine(text);
        Debug.WriteLine(text);
        Console.ResetColor();
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
    private (Dictionary<string, Dictionary<string, string>>, Dictionary<string, string>) ReadJsonFile()
    {
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string filePath = Path.Combine(exeDir, "Assets", "commands.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("JSON file not found", filePath);

        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        var devices = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(data["devices"].ToString());
        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(data["settings"].ToString());

        return (devices, settings);
    }
}