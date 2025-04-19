using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace Ava;

public partial class ImagePage : UserControl
{
    private string _filename;
    public ImagePage()
    {
        InitializeComponent();
    }
    public ImagePage(string path)//string text
    {
        InitializeComponent();
        //textField.Text = text;

        _filename = path;
        LoadImage();
    }
    private void LoadImage()
    {
        Console.WriteLine("_filename: " + _filename);
        if (!File.Exists(_filename))
        {
            Console.WriteLine("ÂÛרוכ ג 1");
            return;
        }






        Output.Source = new Bitmap(_filename);
        
        Console.WriteLine("ÂÎÒ פמעמ");


    }
}