using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace Ava;

public partial class FileSystem : UserControl
{

    private List<Control> pages;
    int pageIndex = 0;
    private Explorer explorer = new Explorer();
    private Control currentPage;

    public object FrameHOST { get { return FrameHost.Content; } }
    public FileSystem()
    {
        InitializeComponent();
        explorer.FileOpened += OpenFile;
        pages = new List<Control>() { explorer };
        RasberryPINS.ButtonPressedPressTextPage += ComeBack;
        RasberryPINS.ButtonPressedPressImagePage += ComeBack;
        FrameHost.Content = pages[0];
    }

    private void OpenFile(string text, string dir)
    {
        if (text=="txt")
        {
            pages.Add(new TextPage(dir));
        }
        else if(text=="png")
        {
            pages.Add(new ImagePage(dir));
        }
        Dispatcher.UIThread.Post(() =>
        {
            currentPage = pages[1];
            FrameHost.Content = currentPage;
            //label.Content = (pageIndex + 1) + "/" + pages.Length;
        });
    }
    private void ComeBack()
    {
        currentPage = pages[0];
        FrameHost.Content = currentPage;
        pages.RemoveAt(1);
        Console.WriteLine("-----------");
        foreach (var item in pages)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("-----------");
    }
}