using Ava.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.IO;
using System.Reflection;

namespace Ava;

public partial class MapPage : UserControl
{
    private enum MapControlMode
    {
        Zoom,
        PanHorizontal,
        PanVertical
    }

    private MapControlMode currentMode = MapControlMode.Zoom;
    private double _zoomFactor = 1.0;
    private readonly ScaleTransform _scaleTransform;

    private TranslateTransform _translateTransform;

    public MapPage()
    {
        InitializeComponent();


        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string imagePath = Path.Combine(exeDir, "Assets", "map.jpg");

        if (File.Exists(imagePath))
        {
            MapImage.Source = new Bitmap(imagePath);
            info.Content = "YES " + imagePath;
        }
        else
        {
            info.Content = "No " + imagePath;
        }

        //_scaleTransform = this.Resources["MapScaleTransform"] as ScaleTransform;

        var transformGroup = this.Resources["MapTransformGroup"] as TransformGroup;

        _scaleTransform = transformGroup.Children[0] as ScaleTransform;
        _translateTransform = transformGroup.Children[1] as TranslateTransform;

        if (Design.IsDesignMode)
        {
            info.Content = "Design mode (Previewer)";
            return;
        }

        try
        {
            Console.WriteLine("MapPage загружен");
            RasberryPINS.ButtonPressedPlusMap += ButtonPressedPlus;
            RasberryPINS.ButtonPressedMinusMap += ButtonPressedMinus;
            RasberryPINS.ButtonPressedPressMap += ButtonPressedPress;

            info.Content = "Контроллер активен";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка GPIO: " + ex.Message);
        }
    }

    private void ButtonPressedPlus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (currentMode)
            {
                case MapControlMode.Zoom:
                    if (_zoomFactor < 2)
                        ChangeZoom(0.1);
                    break;
                case MapControlMode.PanHorizontal:
                    _translateTransform.X += 10;
                    ClampTranslation();
                    break;
                case MapControlMode.PanVertical:
                    _translateTransform.Y += 10;
                    ClampTranslation();
                    break;
            }
        });
    }

    private void ButtonPressedMinus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (currentMode)
            {
                case MapControlMode.Zoom:
                    if (_zoomFactor > 1)
                        ChangeZoom(-0.1);
                    break;
                case MapControlMode.PanHorizontal:
                    _translateTransform.X -= 10;
                    ClampTranslation();
                    break;
                case MapControlMode.PanVertical:
                    _translateTransform.Y -= 10;
                    ClampTranslation();
                    break;
            }
        });
    }
    private void ClampTranslation()
    {
        if (MapImage.Source == null || MapImage.Bounds.Width == 0 || MapImage.Bounds.Height == 0)
        return;

        double scaledWidth = MapImage.Source.Size.Width * _scaleTransform.ScaleX;
        double scaledHeight = MapImage.Source.Size.Height * _scaleTransform.ScaleY;

        var window = MapImage.GetVisualRoot() as Window;
        if (window == null)
            return;

        double containerWidth = window.Bounds.Width;
        double containerHeight = window.Bounds.Height;

        double topPanelHeight = TopPanel?.Bounds.Height ?? 0;
        double usableHeight = containerHeight - topPanelHeight;

        double minX = Math.Min(0, containerWidth - scaledWidth);
        double maxX = 0;

        double minY = Math.Min(0, usableHeight - scaledHeight);
        double maxY = -topPanelHeight;

        _translateTransform.X = Math.Clamp(_translateTransform.X, minX, maxX);
        _translateTransform.Y = Math.Clamp(_translateTransform.Y, minY, maxY);
    }

    private void ChangeZoom(double zoomStep)
    {
        _zoomFactor = Math.Clamp(_zoomFactor + zoomStep, 0.5, 5);
        _scaleTransform.ScaleX = _zoomFactor;
        _scaleTransform.ScaleY = _zoomFactor;
        ClampTranslation();
    }

    private void ButtonPressedPress()
    {
        currentMode = currentMode switch
        {
            MapControlMode.Zoom => MapControlMode.PanHorizontal,
            MapControlMode.PanHorizontal => MapControlMode.PanVertical,
            MapControlMode.PanVertical => MapControlMode.Zoom,
            _ => MapControlMode.Zoom
        };

        info.Content = $"Режим: {currentMode}";
    }
}