using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Ava;

public partial class Explorer : UserControl
{
    public ObservableCollection<string> Folders { get; set; }
    private Dictionary<string, ListBoxItem> _disks = new Dictionary<string, ListBoxItem>();
    private List<string> prohibited = new List<string>() { "/run", "/boot", "/sys", "/dev", "/proc" };
    List<string> availableDiskName = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "K", "L", "M", "N", "R" };
    private bool _isKeyPressed = false;
    private int _deepOfPath;
    private const string PrevDirText = "..";
    private string _currDisk;
    private int _selectedIndex;
    private string dirNow;

    public Explorer()
    {
        InitializeComponent();
        KeyDown += AdditionalKeys;
        DevicesManager.AddDisk += disk => AddDisk(disk);
        DevicesManager.RemoveDisk += RemoveDisk;
        Folders = new ObservableCollection<string>();

        string exeDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..");

        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)";
            exeDirectory = Path.Combine(exeDirectory, "..");
            // LoadTest(exeDirectory);
            return;
        }

        try
        {
            Console.WriteLine("Explorer загружен");
            Console.WriteLine("Explorer: подписка на GPIO");
            RasberryPINS.ButtonPressedPlusExplorer += ButtonPressedPlus;
            RasberryPINS.ButtonPressedMinusExplorer += ButtonPressedMinus;
            RasberryPINS.ButtonPressedPressExplorer += ButtonPressedPress;

            exeDirectory = "/home/bearshark/Downloads/My_disk";
            //LoadTest(exeDirectory);
            info.Content = "Контроллер активен";
        }
        catch (Exception ex)
        {
            exeDirectory = Path.Combine(exeDirectory, "..");
            //LoadTest(exeDirectory);
            info.Content = "Ошибка GPIO: " + ex.Message;
        }
        DevicesManager.StartListening();
    }

    private void LoadTest(string exeDirectory)
    {
        if (Directory.Exists(exeDirectory))
        {
            string[] directories = Directory.GetDirectories(exeDirectory);

            foreach (var dir in directories)
            {
                explorerLB.Items.Add(Path.GetFileName(dir));
                Folders.Add(Path.GetFileName(dir));
            }
            explorerLB.SelectedIndex = 0;
        }
    }

    private void AddDisk(string disk, bool addToList = true)
    {
        //DispatcherPriority.Normal,
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (!addToList)
                {
                    Debug.WriteLine("In addToList");
                    Console.WriteLine("In addToList");
                    ListBoxItem lbi = new ListBoxItem()
                    {
                        Content = _disks[disk].Content,
                        Tag = _disks[disk].Tag,
                    };

                    explorerLB.Items.Add(lbi);
                    return;
                }

                var allFiles = Directory.GetFiles(disk).Select(Path.GetFileName).ToArray();
                //Debug.WriteLine("disk: " + disk);
                //Console.WriteLine("disk: " + disk);
                //Debug.WriteLine("allFiles: " + allFiles);
                //Console.WriteLine("allFiles: " + allFiles);
                /*
                if (ConfigManager.Config.IsFlashcardHack)
                {
                    if (allFiles.Contains(AccessFileToHack))
                    {
                        _disksToHack.Add(disk);
                        HackIco.Visibility = Visibility.Visible;
                        hackAllow = true;
                    }
                }
                */


                //if (!allFiles.Contains(AccessFileToReadDisk))
                //    return;


                //var fullPath = File.ReadAllText(disk + AccessFileToReadDisk);


                //string disksPath = Path.GetDirectoryName(disk);
                string disksPath = disk;

                //Debug.WriteLine("disksPath: " + disksPath);
                //Console.WriteLine("disksPath: " + disksPath);

                if (Directory.Exists(disksPath))
                {
                    //Debug.WriteLine("Exist");
                    //Console.WriteLine("Exist");
                    //LblInfo.Content = "";
                    //LblInfo.Visibility = Visibility.Hidden;


                    //var diskName = Path.GetFileNameWithoutExtension(disksPath);
                    //var diskName = GetDiskName();



                    foreach (var file in prohibited)
                    {
                        if (disk.Contains(file))
                        {
                            //Console.WriteLine("return");
                            //Debug.WriteLine("return");
                            return;
                        }
                    }
                    //Console.WriteLine(1);                                  //
                    Random r = new Random();                               //
                    //Console.WriteLine(2);                                  //
                    int index = r.Next(0, availableDiskName.Count);        //
                    //Console.WriteLine(3);                                  // I cant delete Console.WriteLine
                    string str = availableDiskName[index];                 //  because it stops working((((
                                                                           // Console.WriteLine(4);                                  //
                    availableDiskName.RemoveAt(index);                     //  I dont know why
                    //Console.WriteLine(5);                                  //
                    var diskName = str + ":/";                            //


                    Debug.WriteLine("diskName: " + diskName);
                    Console.WriteLine("diskName: " + diskName);
                    //var diskName = Path.GetFileNameWithoutExtension(fullPath);

                    var lbi = new ListBoxItem()
                    {
                        /* DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                         Content = diskName,
                         Tag = fullPath,
                         Style = (Style)App.Current.FindResource("ImageText"),
                         Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                         FontFamily = LblInfo.FontFamily,
                         FontSize = LblInfo.FontSize,*/
                        Content = diskName,
                        Tag = disk,
                    };

                    if (_deepOfPath == 0)
                        explorerLB.Items.Add(lbi);

                    _disks.Add(disk, lbi);
                }
                string disksrt = "";
                foreach (var diskk in _disks)
                {
                    disksrt += diskk.ToString();
                }
                disksrt += "|||||";
                foreach (var item in availableDiskName)
                {
                    disksrt += item;
                }
                info.Content = disksrt;
                //Focus();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddDisk erroe:   " + ex.Message);
                Debug.WriteLine("AddDisk erroe:   " + ex.Message);

            }
        });
    }
    private void RemoveDisk(string diskName)
    {
        Console.WriteLine("RemoveDisk diskName: " + diskName);
        Debug.WriteLine("RemoveDisk diskName: " + diskName);
        Dispatcher.UIThread.Post(new Action(() =>
        {
            ListBoxItem disk = null;

            if (_disks.ContainsKey(diskName))
            {

                disk = _disks[diskName];
                _disks.Remove(diskName);
            }

            try
            {
                availableDiskName.Add((disk.Content.ToString()[0]).ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("try catch: " + ex.Message);
                Debug.WriteLine("try catch: " + ex.Message);
            }

            Console.WriteLine("Content: " + disk.Content.ToString());
            Debug.WriteLine("Content: " + disk.Content.ToString());


            Console.WriteLine("Start removing");                //
            Debug.WriteLine("Start removing");                  //
            if (explorerLB.Items.Contains(disk))                //
            {                                                   //
                Console.WriteLine("Start removing2");           // I cant delete Console.WriteLine
                Debug.WriteLine("Start removing2");             //     because it stops working((((
                explorerLB.Items.Remove(disk);                  //                                  
                Console.WriteLine("End removing");              //  I dont know why
                Debug.WriteLine("End removing");                //
            }                                                   //
            Console.WriteLine("End removing2");                 //
            Debug.WriteLine("End removing2");                   //

            if (_currDisk == diskName)
            {
                explorerLB.SelectedIndex = 0;
                _selectedIndex = 0;
                _currDisk = null;
                explorerLB.Items.Clear();
                //_disks.Keys.ForEach(x => AddDisk(x, false));

                foreach (var item in _disks.Keys)
                {
                    AddDisk(item, false);
                }
            }
            if (explorerLB.Items.Count == 0)
            {
                info.Content = "Доступных дисков нет...";
                //info.Visibility = Visibility.Visible;
            }

            /*
            if (_disksToHack.Contains(diskName))
            {
                HackIco.Visibility = Visibility.Hidden;
                _disksToHack.Remove(diskName);
                hackAllow = false;
            }
            */
            string disksrt = "";
            foreach (var diskk in _disks)
            {
                disksrt += diskk.ToString();
            }
            disksrt += "|||||";
            foreach (var item in availableDiskName)
            {
                disksrt += item;
            }
            info.Content = disksrt;
        }));
    }
    private void AccessInFolderOpen(string directory)
    {
        FindFolders(directory);
        FindFiles(directory);

        explorerLB.SelectedIndex = 0;
        _selectedIndex = 0;
        explorerLB.Focus();
    }
    private void GoToFilePage(string directory)
    {
        if (Path.GetExtension(directory).Remove(0, 1) == "txt")
        {
            
        }
        //if (Addition.IsItPage(directory))
        //{
        //    var nextPage = Addition.GetPageByFilename(directory, _theme);
        //
        //    if (nextPage != default)
        //        Addition.NavigationService.Navigate(nextPage);
        //}
        //else
        //{
        //    var window = Addition.GetWindowByFilename(directory, _theme);
        //    window.ShowDialog();
        //}


    }

    private void FindFiles(string directory)
    {
        var files = Directory.GetFiles(directory).ToList();
        var directories = Directory.GetDirectories(directory);

        //for (var i = 0; i < files.Count; i++)
        //{
        //    if (files[i].Contains(ExtensionConfig) && (files.Contains(files[i].RemoveLast(ExtensionConfig)) || directories.Contains(files[i].RemoveLast(ExtensionConfig))))
        //    {
        //        files.RemoveAt(i);
        //        i--;
        //    }
        //}

        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var name = Path.GetFileNameWithoutExtension(file);

            var extension = Path.GetExtension(file).Remove(0, 1);

            var lbi = new ListBoxItem()
            {
                Content = name,
                Tag = $@"{directory}\{filename}",
                // Style = (Style)App.Current.FindResource("ImageText"),
                //Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                // FontFamily = LblInfo.FontFamily,
                // FontSize = LblInfo.FontSize,

            };

            /* if (Addition.Text.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Text]));
             else if (Addition.Image.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Image]));
             else if (Addition.Audio.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Audio]));
             else if (Addition.Video.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Video]));
             else if (Addition.Command.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Command]));
             else if (Addition.Execute.Contains(extension))
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Execute]));
             else
                 lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Default]));
            */
            explorerLB.Items.Add(lbi);
        }
    }
    private void FindFolders(string directory)
    {
        explorerLB.Items.Clear();
        var allDirectories = Directory.GetDirectories(directory);

        if (_deepOfPath > 0)
        {
            string tag = "";
            //if (directory.EndsWith("\\"))
            if (directory.EndsWith("/"))
                tag = $@"{directory}{PrevDirText}";
            else
                tag = $@"{directory}/{PrevDirText}";
            var lbi = new ListBoxItem()
            {
                //DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                Content = PrevDirText,
                Tag = $@"{directory}/{PrevDirText}",
                //Style = (Style)App.Current.FindResource("ImageText"),
                //Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                //FontFamily = LblInfo.FontFamily,
                // FontSize = LblInfo.FontSize,

            };

            explorerLB.Items.Add(lbi);
        }

        foreach (var dir in allDirectories)
        {
            var name = Path.GetFileName(dir);
            //if (name == SystemFolder) continue;
            string tag = "";
            // if (directory.EndsWith("\\"))
            if (directory.EndsWith("/"))
                tag = $@"{directory}{name}";
            else
                tag = $@"{directory}/{name}";
            var lbi = new ListBoxItem()
            {
                //DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                Content = name,
                Tag = tag,
                //Style = (Style)App.Current.FindResource("ImageText"),
                //Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                //FontFamily = LblInfo.FontFamily,
                //FontSize = LblInfo.FontSize,

            };

            explorerLB.Items.Add(lbi);
        }

    }
    private void Open(string directory, bool isFolder, bool isGoBack = false)
    {
        //if (Directory.GetFiles(directory.RemoveLast(@"\")).Contains(directory + ".config") && !isGoBack)
        dirNow = directory;
        Dispatcher.UIThread.Invoke(new Action(() => { info.Content = dirNow; }));
        if (true)
        {
            try
            {

                //var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(directory + ".config"));

                // if (!content.HasPassword)
                //{
                if (isFolder)
                    AccessInFolderOpen(directory);
                else
                    GoToFilePage(directory);
                // }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Open   " + ex);
            }
        }

    }


    private void AdditionalKeys(object sender, KeyEventArgs e)
    {
        if (e.RoutedEvent == InputElement.KeyDownEvent && !_isKeyPressed)
        {
            _isKeyPressed = true;

            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    Debug.WriteLine("Enter pressed");
                    break;
            }
        }
        else if (e.RoutedEvent == InputElement.KeyUpEvent)
        {
            _isKeyPressed = false;
        }
    }

    private void lstB_MouseDoubleClick(object? sender, PointerPressedEventArgs e)
    {
        //Dispatcher.UIThread.Invoke(() =>
        //{
        try
        {


            var lbi = (ListBoxItem)explorerLB.SelectedItem;
            if (lbi == null)
                return;

            var directory = lbi.Tag.ToString();

            if (_currDisk == null)
            {
                //_currDisk = _disks.FindKey(lbi);
                _currDisk = _disks.Keys.FirstOrDefault(key => _disks[key].Equals(lbi));
            }

            if (directory.EndsWith(PrevDirText))
            {
                if (_deepOfPath == 0) return;
                _deepOfPath--;

                if (_deepOfPath > 0)
                {
                    string pathTemp2 = directory.Remove(directory.LastIndexOf("/", StringComparison.Ordinal));
                    string pathTemp = pathTemp2.Remove(pathTemp2.LastIndexOf("/", StringComparison.Ordinal));
                    if (_deepOfPath == 1)
                        pathTemp += "/";
                    Console.WriteLine("pathTemp: " + pathTemp);
                    Debug.WriteLine("pathTemp: " + pathTemp);
                    Open(pathTemp, true, true);
                    //Open(directory.RemoveLast("\\").RemoveLast("\\"), true, true);
                    //str.Remove(str.LastIndexOf(key, StringComparison.Ordinal));
                    return;
                }

                explorerLB.SelectedIndex = 0;
                _selectedIndex = 0;
                _currDisk = null;
                explorerLB.Items.Clear();
                //_disks.Keys.ForEach(x => AddDisk(x, false));
                foreach (var item in _disks.Keys)
                {
                    AddDisk(item, false);
                }
                return;
            }

            if (IsFolder(directory))
            {
                _deepOfPath++;
                Open(directory, true);
            }
            else
            {
                Open(directory, false);
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine("Exeption lstB_MouseDoubleClick:    " + ex);
            Debug.WriteLine("Exeption lstB_MouseDoubleClick:    " + ex);
        }
        // });
    }
    private static bool IsFolder(string path) => Directory.Exists(path);
    private void ButtonPressedPress()
    {
        Console.WriteLine("Explorer: кнопка press");
        lstB_MouseDoubleClick(null, null);
    }
    private void ButtonPressedPlus()
    {
        Console.WriteLine("Explorer: кнопка +");
        if (explorerLB.SelectedIndex > 0)
            explorerLB.SelectedIndex--;
    }

    private void ButtonPressedMinus()
    {
        Console.WriteLine("Explorer: кнопка -");
        if (explorerLB.SelectedIndex < explorerLB.ItemCount - 1)
            explorerLB.SelectedIndex++;
    }
    public void Button1_Click(object source, RoutedEventArgs args)
    {
        if (explorerLB.SelectedIndex > 0)
        {
            explorerLB.SelectedIndex--;
        }
    }

    public void Button2_Click(object source, RoutedEventArgs args)
    {
        if (explorerLB.SelectedIndex < explorerLB.ItemCount - 1)
        {
            explorerLB.SelectedIndex++;
        }
    }
    public void Button3_Click(object source, RoutedEventArgs args)
    {
        lstB_MouseDoubleClick(null, null);
    }
}