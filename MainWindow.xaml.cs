using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using spiderhead.Models;

namespace spiderhead
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Func<int, byte> asByteValue =
                (int percent) => (byte)Math.Round(percent * 2.55);

        private CurrentMedia currentMedia;
        private DispatcherTimer mediaFetchTimer;
        private List<ToolStripMenuItem> opacityOptions;
        private List<ToolStripMenuItem> positioningOptions;
        private readonly Dictionary<string, List<GradientStop>> backgroundElements;
        private readonly Dictionary<int, byte> opacityValues = new()
        {
            { 100, asByteValue(100) },
            { 90, asByteValue(90) },
            { 80, asByteValue(80) },
            { 70, asByteValue(70) },
            { 60, asByteValue(60) },
            { 50, asByteValue(50) },
        };

        public MainWindow()
        {
            ShowInTaskbar = false;
            backgroundElements = new Dictionary<string, List<GradientStop>>();

            InitializeComponent();
            InitializeBackgroundElements();

            ShowTrayIcon();
            PinWindow(UserPreferences.Default.Position);
            ChangeBackgroundOpacity(UserPreferences.Default.Opacity);
            ToggleBorderRadius();

            SetupTimer();
            GetMediaAndDraw(); //Not waiting cuz we rebels

            mediaFetchTimer.Start();
        }

        private void InitializeBackgroundElements()
        {
            backgroundElements.Add("top", new List<System.Windows.Media.GradientStop>() {
                borderColorA,
                bgColorA,
            });
            backgroundElements.Add("middle", new List<System.Windows.Media.GradientStop>() {
                borderColorB,
                bgColorB,
            });
            backgroundElements.Add("bottom", new List<System.Windows.Media.GradientStop>() {
                borderColorC,
                bgColorC,
            });

        }

        private void SetupTimer()
        {
            mediaFetchTimer = new DispatcherTimer();
            mediaFetchTimer.Interval = TimeSpan.FromSeconds(1);
            mediaFetchTimer.Tick += async (object s, EventArgs e) => await GetMediaAndDraw();
        }

        private void DisplayDefaultThumbnail()
        {
            imgCover.ImageSource = Tools.Images.BitmapImageFromByteArray(
                Tools.Images.BitmapToByteArray(Properties.Resources.cd)
            );
        }

        private ContextMenuStrip GenerateContextStrip()
        {
            ToolStripMenuItem opacitySubmenu = GenerateOpacitySubmenu();
            ToolStripMenuItem positioningSubmenu = GeneratePositioningSubmenu();
            ToolStripMenuItem topToggler = new ToolStripMenuItem("Always on top")
            {
                Checked = UserPreferences.Default.AlwaysOnTop
            };
            topToggler.Click += ToggleWindowOnTopHandler;

            ToolStripMenuItem exit = new("Exit");
            exit.Click += (object s, EventArgs e) => Close();

            ToolStripMenuItem borderToggle = new("Rounded border");
            borderToggle.Click += ToggleBorderHandler;
            borderToggle.Checked = UserPreferences.Default.RoundBorder;

            ContextMenuStrip menu = new();
            menu.Items.Add(positioningSubmenu);
            menu.Items.Add(topToggler);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(opacitySubmenu);
            menu.Items.Add(borderToggle);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exit);

            return menu;
        }

        private ToolStripMenuItem GeneratePositioningSubmenu()
        {
            positioningOptions = new List<ToolStripMenuItem>
            {
                new ToolStripMenuItem("Top Left"){ Name = "checkTopLeft" },
                new ToolStripMenuItem("Top Right"){ Name = "checkTopRight" },
                new ToolStripMenuItem("Bottom Left") { Name = "checkBotLeft" },
                new ToolStripMenuItem("Bottom Right") { Name = "checkbotRight" },
                new ToolStripMenuItem("Free") { Name = "checkFree" }
            };

            foreach (var option in positioningOptions)
            {
                option.Click += PositionHandler;
                if (UserPreferences.Default.Position == option.Text)
                {
                    option.Checked = true;
                }
            };

            ToolStripMenuItem positioningSubmenu = new ToolStripMenuItem("Pin to position");
            positioningSubmenu.DropDownItems.AddRange(positioningOptions.ToArray());
            return positioningSubmenu;
        }

        private ToolStripMenuItem GenerateOpacitySubmenu()
        {
            opacityOptions = new List<ToolStripMenuItem>();

            foreach (var pair in opacityValues)
            {
                var item = new ToolStripMenuItem($"{pair.Key}") { Name = $"checkOpacity{pair.Key}" };
                item.Click += OpacityHandler;
                if (pair.Value == UserPreferences.Default.Opacity)
                    item.Checked = true;
                opacityOptions.Add(item);
            }

            ToolStripMenuItem submenu = new ToolStripMenuItem("Opacity");
            submenu.DropDownItems.AddRange(opacityOptions.ToArray());
            return submenu;
        }

        private void ShowTrayIcon()
        {
            var trayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.logo,
                Text = "Spiderhead",
                ContextMenuStrip = GenerateContextStrip(),
                Visible = true
            };
        }

        private void KeepOnTop(object sender)
        {
            Window win = (Window)sender;
            Trace.WriteLine($"Always on top status: {UserPreferences.Default.AlwaysOnTop}");
            if (UserPreferences.Default.AlwaysOnTop)
                win.Topmost = true;
            else
                win.Topmost = false;
        }

        private void RedrawInfo(CurrentMedia media)
        {
            lblSong.Content = media.Name;
            lblAlbum.Content = media.Album;
            lblArtist.Content = media.Artist;

            var thumbnailImage = Tools.Images.BitmapImageFromByteArray(media.Art);
            imgCover.ImageSource = thumbnailImage;
        }

        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
            ShowTrayIcon();
        }

        private void ChangeBackgroundPrimaryColor(Dictionary<int, int> values)
        {
            int total = 0;
            int hue = 0;

            int i = 0;

            foreach (var pair in values)
            {
                Trace.WriteLine($"{pair.Key} : {pair.Value}");
                total += pair.Value;
                i++;
                if (i == 6)
                    break;
            }

            i = 0;
            foreach (var pair in values)
            {
                hue += pair.Key * pair.Value / total;
                i++;
                if (i == 6)
                    break;
            }

            Trace.WriteLine($"Dominant hue: {hue}");
        }

        private void ChangeBackgroundOpacity(byte opacity)
        {
            foreach (string key in backgroundElements.Keys)
                foreach (GradientStop colorStop in backgroundElements[key])
                {
                    Color color = colorStop.Color;
                    Color modifiedColor = Color.FromArgb(opacity, color.R, color.G, color.B);
                    colorStop.Color = modifiedColor;
                }
        }

        private void ToggleBorderRadius()
        {
            if (UserPreferences.Default.RoundBorder)
                UI.Styling.ChangeBorderRadius(this.borderParent, 8);
            else
                UI.Styling.ChangeBorderRadius(this.borderParent, 0);
            UserPreferences.Default.Save();
        }

        private void PinWindow(string position)
        {
            var workArea = SystemParameters.WorkArea;
            switch (position)
            {
                case "Top Left":
                    Left = workArea.Left;
                    Top = workArea.Top;
                    break;
                case "Top Right":
                    Left = workArea.Right - Width;
                    Top = workArea.Top;
                    break;
                case "Bottom Left":
                    Left = workArea.Left;
                    Top = workArea.Bottom - Height;
                    break;
                case "Bottom Right":
                    Left = workArea.Right - Width;
                    Top = workArea.Bottom - Height;
                    break;
                case "Free":
                    Left = UserPreferences.Default.FreeLeft;
                    Top = UserPreferences.Default.FreeTop;
                    break;
            }
        }

        private void UncheckAllOptionsInSubmenu(List<ToolStripMenuItem> list)
        {
            foreach (var option in list)
            {
                option.Checked = false;
            }
        }

        private async Task GetMediaAndDraw()
        {
            // TODO: Handle current playback status
            CurrentMedia fetchedMedia = await Tools.MediaController.GetMedia();
            // Return if there's no media or it hasn't changed
            if (
                (fetchedMedia == null) || (
                    currentMedia != null &&
                    fetchedMedia.Name == currentMedia.Name &&
                    fetchedMedia.HasDefaultArtwork == currentMedia.HasDefaultArtwork
                )
            )
                return;

            Trace.WriteLine(fetchedMedia);
            currentMedia = fetchedMedia;
            RedrawInfo(currentMedia);

            Trace.WriteLine("Media added to UI");
            return;
        }

        private void ToggleWindowOnTopHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            UserPreferences.Default.AlwaysOnTop = item.Checked;
            UserPreferences.Default.Save();

            KeepOnTop(this);
            if (item.Checked)
                Focus();
        }

        private void OpacityHandler(object sender, EventArgs e)
        {
            var option = (ToolStripMenuItem)sender;

            UncheckAllOptionsInSubmenu(opacityOptions);

            option.Checked = true;

            byte opacityByteValue = opacityValues[int.Parse(option.Text)];

            ChangeBackgroundOpacity(opacityByteValue);
            UserPreferences.Default.Opacity = opacityByteValue;
            UserPreferences.Default.Save();
        }

        private void PositionHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            UncheckAllOptionsInSubmenu(positioningOptions);
            item.Checked = true;
            UserPreferences.Default.Position = item.Text;

            PinWindow(item.Text);

            UserPreferences.Default.Save();
        }

        private void ToggleBorderHandler(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            Trace.WriteLine($"Check status: {item.Checked}");
            UserPreferences.Default.RoundBorder = item.Checked;
            ToggleBorderRadius();
        }

        private void SaveFreePosition()
        {
            if (UserPreferences.Default.Position != "Free")
                return;

            if (UserPreferences.Default.FreeTop != Top || UserPreferences.Default.FreeLeft != Left)
            {
                UserPreferences.Default.FreeTop = Top;
                UserPreferences.Default.FreeLeft = Left;
                UserPreferences.Default.Save();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            await GetMediaAndDraw();
            if (!await Tools.MediaController.TogglePlay())
                Trace.WriteLine("Error toggling Play/Pause");
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (!await Tools.MediaController.Next())
                Trace.WriteLine("Error setting Next()");
        }

        private async void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (!await Tools.MediaController.Previous())
                Trace.WriteLine("Error setting Previous()");
        }

        private void windowPlayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UserPreferences.Default.Position != "Free")
                return;

            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void windowPlayer_Deactivated(object sender, EventArgs e)
        {
            Trace.WriteLine("Window deactivated");
            KeepOnTop(sender);
        }

        private void windowPlayer_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Trace.WriteLine("Window lost focus");
            KeepOnTop(sender);
        }

        private void windowPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveFreePosition();
        }
    }
}
