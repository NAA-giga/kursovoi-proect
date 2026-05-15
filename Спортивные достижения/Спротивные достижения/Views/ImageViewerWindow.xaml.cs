using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Спротивные_достижения.Views
{
    /// <summary>
    /// Логика взаимодействия для ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private double _currentZoom = 1.0;
        private double _minZoom = 0.1;
        private double _maxZoom = 5.0;
        private Point _startPoint;
        private bool _isDragging;

        public ImageViewerWindow(string imagePath, string title)
        {
            InitializeComponent();
            txtTitle.Text = title;
            LoadImage(imagePath);

            // Подписываемся на события колесика мыши
            ImageScrollViewer.PreviewMouseWheel += ImageScrollViewer_PreviewMouseWheel;
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImageView.Source = bitmap;
                    UpdateStatusText(bitmap);
                }
                else
                {
                    ShowError($"Изображение не найдено: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private void UpdateStatusText(BitmapImage bitmap = null)
        {
            if (bitmap == null && ImageView.Source is BitmapImage bmp)
                bitmap = bmp;

            if (bitmap != null)
            {
                txtStatus.Text = $"Файл: {System.IO.Path.GetFileName(bitmap.UriSource?.LocalPath ?? "изображение")} | " +
                               $"Размер: {bitmap.PixelWidth} x {bitmap.PixelHeight} | " +
                               $"Масштаб: {_currentZoom:F1}x";
            }
            else
            {
                txtStatus.Text = $"Масштаб: {_currentZoom:F1}x";
            }
        }

        private void ShowError(string message)
        {
            txtStatus.Text = message;
            txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            ImageView.Source = null;
        }

        // Увеличение масштаба
        private void ZoomIn()
        {
            if (_currentZoom < _maxZoom)
            {
                _currentZoom = Math.Min(_currentZoom + 0.1, _maxZoom);
                ApplyZoom();
            }
        }

        // Уменьшение масштаба
        private void ZoomOut()
        {
            if (_currentZoom > _minZoom)
            {
                _currentZoom = Math.Max(_currentZoom - 0.1, _minZoom);
                ApplyZoom();
            }
        }

        // Сброс масштаба
        private void ResetZoom()
        {
            _currentZoom = 1.0;
            ApplyZoom();
        }

        // Применение масштаба
        private void ApplyZoom()
        {
            ScaleTransform scale = new ScaleTransform(_currentZoom, _currentZoom);
            ImageView.LayoutTransform = scale;

            if (ImageView.Source is BitmapImage bitmap)
            {
                UpdateStatusText(bitmap);
            }
            else
            {
                UpdateStatusText();
            }

            // Центрируем изображение
            CenterImage();
        }

        // Центрирование изображения
        private void CenterImage()
        {
            if (ImageView.Source != null && ImageScrollViewer != null)
            {
                ImageScrollViewer.ScrollToHorizontalOffset((ImageScrollViewer.ScrollableWidth) / 2);
                ImageScrollViewer.ScrollToVerticalOffset((ImageScrollViewer.ScrollableHeight) / 2);
            }
        }

        // Обработка колесика мыши
        private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Ctrl + колесо - изменение масштаба
                if (e.Delta > 0)
                    ZoomIn();
                else
                    ZoomOut();
                e.Handled = true;
            }
        }

        // Кнопка увеличения
        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        // Кнопка уменьшения
        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        // Кнопка сброса масштаба
        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            ResetZoom();
        }

        // Кнопка свернуть
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Кнопка закрыть
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Перетаскивание окна за верхнюю панель
        private void TopPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Двойной клик по верхней панели - восстанавливаем/разворачиваем
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.DragMove();
            }
        }

        // Обработка для перетаскивания изображения
        private void ImageArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Двойной клик по изображению - сброс масштаба
                ResetZoom();
                e.Handled = true;
            }
            else
            {
                _startPoint = e.GetPosition(ImageScrollViewer);
                _isDragging = true;
                ImageScrollViewer.Cursor = Cursors.SizeAll;
                ImageScrollViewer.CaptureMouse();
            }
        }

        private void ImageArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(ImageScrollViewer);
                double deltaX = currentPoint.X - _startPoint.X;
                double deltaY = currentPoint.Y - _startPoint.Y;

                ImageScrollViewer.ScrollToHorizontalOffset(ImageScrollViewer.HorizontalOffset - deltaX);
                ImageScrollViewer.ScrollToVerticalOffset(ImageScrollViewer.VerticalOffset - deltaY);

                _startPoint = currentPoint;
            }
        }

        private void ImageArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ImageScrollViewer.Cursor = Cursors.Arrow;
            ImageScrollViewer.ReleaseMouseCapture();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ResetZoom();
                e.Handled = true;
            }
        }
    }
}
