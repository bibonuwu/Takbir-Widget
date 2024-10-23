using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Takbir_Widget
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _animationTimer;
        private int _textIndex = 0;
        private int _allahAkbarCount = 0;
        private readonly string[] _texts = { "Allahu Akbar", "Subhanalla", "La ilaha illallah Muhammadur Rasulullah", "Alhamdulillah" };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            // Window configuration for taskbar-like appearance
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;

            CheckAndSetStartup();
            StartAnimationTimer();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Width - 5; // Изначально свёрнутое окно
            this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2;
            this.Width = 5;
        }

        private void ExpandWindow()
        {
            AnimateOpacity(1, 500);
            AnimateWidth(525, 500); // Изменил на 525 для соответствия ширине окна в XAML
        }

        private void CollapseWindow()
        {
            AnimateOpacity(0, 500);
            AnimateWidth(5, 500);
        }

        private void AnimateWidth(double toWidth, int durationMilliseconds)
        {
            double fromWidth = this.Width;
            double targetLeft = SystemParameters.WorkArea.Width - toWidth;

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation leftAnimation = new DoubleAnimation
            {
                From = this.Left,
                To = targetLeft,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.WidthProperty, widthAnimation);
            this.BeginAnimation(Window.LeftProperty, leftAnimation);
        }

        private void AnimateOpacity(double toOpacity, int durationMilliseconds)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);
        }

        // Проверка на добавление в автозагрузку и запрос только один раз
        private void CheckAndSetStartup()
        {
            const string registryPath = "SOFTWARE\\TakbirWidget";
            const string startupFlagKey = "StartupRequested";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (key == null)
                    {
                        // Если записи нет, создаем ключ и показываем запрос
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(registryPath))
                        {
                            if (MessageBox.Show("Добавить программу в автозагрузку?", "Автозагрузка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                AddToStartup();
                            }
                            newKey.SetValue(startupFlagKey, 1); // Устанавливаем флаг, чтобы запрос больше не показывался
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке автозагрузки: {ex.Message}");
            }
        }

        // Добавление программы в автозагрузку
        private void AddToStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Takbir", System.Reflection.Assembly.GetExecutingAssembly().Location);
                        MessageBox.Show("Программа добавлена в автозагрузку.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в автозагрузку: {ex.Message}");
            }
        }

        private void StartAnimationTimer()
        {
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromSeconds(10);
            _animationTimer.Tick += (s, e) => ToggleWindowAnimation();
            _animationTimer.Start();
        }

        private void ToggleWindowAnimation()
        {
            if (this.Width == 5)
            {
                ExpandWindow();
                ChangeLabelText();
                _animationTimer.Interval = TimeSpan.FromSeconds(3); // Показать окно на 5 секунд
            }
            else
            {
                CollapseWindow();
                _animationTimer.Interval = TimeSpan.FromSeconds(480); // Скрыть окно на 10 секунд
            }
        }

        private void ChangeLabelText()
        {
            _textIndex = (_textIndex + 1) % _texts.Length;
            ContentLabel.Text = _texts[_textIndex];

            if (_texts[_textIndex] == "Allahu Akbar")
            {
                _allahAkbarCount++;
                CounterLabel.Content = _allahAkbarCount.ToString();
            }
        }
    }
}
