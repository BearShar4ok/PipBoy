using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.VisualBasic;
using System.Threading;
using System.IO;
using System;
using Ava.Classes;

namespace Ava;

public partial class TextPage : UserControl
{
    private string _filename;
    public TextPage()
    {
        InitializeComponent();
    }
    public TextPage(string path)//string text
    {
        InitializeComponent();
        //textField.Text = text;

        _filename = path;
        LoadText();
    }
    private void LoadText()
    {
        Console.WriteLine("_filename: "+_filename);
        if (!File.Exists(_filename))
        {
            Console.WriteLine("ВЫшел в 1");
            return;
        }


        using (var stream = File.OpenText(_filename))
        {
            var text = stream.ReadToEnd();



            Output.Text = text;
            Console.WriteLine("TEXT:");
            Console.WriteLine(text);
            Console.WriteLine("TEXT:");
        }

    }
}