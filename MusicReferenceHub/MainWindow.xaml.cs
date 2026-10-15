using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;

namespace MusicReferenceHub;

// MainWindow is a class. Put fields and methods inside these outer braces.
public partial class MainWindow : Window
{
    private const int VisualizerBars = 19;
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".wma", ".aac", ".m4a" };

    public MainWindow()
    {
        InitializeComponent();
        // TODO: connect your collection in the Data chapter.
        // initialize timers and eq visual
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += PositionTimer_Tick;
        // playlist and index
        _playlist = new List<string>();
        _currentIndex = -1;
        _shuffle = false;
        // bind playlist to listbox if present
        try { lstPlaylist.ItemsSource = _playlist; } catch { }
        // spectrum analyzer bar heights (0..16 pixels)
        _eqValues = new ObservableCollection<double>(Enumerable.Repeat(0.0, VisualizerBars));
        try { eqSliders.ItemsSource = _eqValues; } catch { }
        _rnd = new Random();
        _eqTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _eqTimer.Tick += EqTimer_Tick;
        // scrolling song title
        _marqueeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(60) };
        _marqueeTimer.Tick += MarqueeTimer_Tick;
        _marqueeTimer.Start();
        UpdatePlaylistInfo();
        BuildEqualizer();
        // the audio engine tells us when a track ends or fails
        _audio.TrackEnded += (_, _) => Media_Ended();
        _audio.PlaybackFailed += (_, ex) => Media_Failed(ex);
        Closed += (_, _) => _audio.Dispose();
        PreviewKeyDown += Window_PreviewKeyDown;
        BuildContextMenu();
        // a chrome-less window overhangs the screen edge when maximized, so pull it back in
        StateChanged += (_, _) => outer.Margin = WindowState == WindowState.Maximized
            ? new Thickness(SystemParameters.WindowResizeBorderThickness.Left + 1) : new Thickness(0);
    }

    private readonly AudioEngine _audio = new();   // NAudio player (decoding, EQ, volume, balance, spectrum)
    private readonly DispatcherTimer _positionTimer;
    private bool _isSeeking = false;
    private bool _updatingSeek = false;
    private ObservableCollection<double> _eqValues;
    private DispatcherTimer _eqTimer;
    private DispatcherTimer _marqueeTimer;
    private Random _rnd;
    private List<string> _playlist;
    private int _currentIndex;
    private bool _shuffle;
    private bool _repeat;
    private bool _isPlaying;
    private string _trackText = "";
    private string _playGlyph = "\u25A0";
    private bool _shaded;
    private bool _showRemaining;
    private int _visMode;   // 0 = bars, 1 = oscilloscope, 2 = off

    /// <summary>Index of the playlist row that is playing (drawn in white). -1 when none.</summary>
    public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register(
        nameof(CurrentIndex), typeof(int), typeof(MainWindow), new PropertyMetadata(-1));
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    // ---------- Winamp-style window chrome ----------

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.ClickCount == 2 && ReferenceEquals(sender, mainTitleBar)) { ToggleShade(); return; }
        try { DragMove(); } catch { }
    }

    private const double DesignWidth = 609, DesignHeight = 438;

    private void Outer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        double w = outer.ActualWidth, h = outer.ActualHeight;
        if (w <= 0 || h <= 0) return;
        double scale = Math.Min(w / DesignWidth, h / DesignHeight);
        uiScale.ScaleX = scale;
        uiScale.ScaleY = scale;
        scaleHost.Width = w / scale;
        scaleHost.Height = h / scale;
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SetPlayState(string glyph)
    {
        _playGlyph = glyph;
        try { txtPlayState.Text = glyph; } catch { }
        UpdateShadeTitle();
    }

    private void SetTrackText(string text)
    {
        _trackText = text;
        txtNowPlaying.Text = text;
        marqueeShift.X = 0;
        UpdateShadeTitle();
    }

    // ---------- Windowshade, clock mode, visualizer modes ----------

    private void ToggleShade()
    {
        _shaded = !_shaded;
        mainFrame.Height = _shaded ? 16 : 116;
        UpdateShadeTitle();
    }

    private void Shade_Click(object sender, RoutedEventArgs e)
    {
        ToggleShade();
    }

    private void UpdateShadeTitle()
    {
        try
        {
            if (!_shaded) { txtMainTitle.Text = "WINAMP"; return; }
            string t = string.IsNullOrEmpty(_trackText) ? "Music Reference Hub" : _trackText;
            if (t.Length > 30) t = t.Substring(0, 30) + "...";
            txtMainTitle.Text = _playGlyph + " " + ledTime.Text + "  " + t;
        }
        catch { }
    }

    private string FormatLed(TimeSpan pos, TimeSpan total)
    {
        if (_showRemaining && total > TimeSpan.Zero)
        {
            var left = total - pos;
            if (left < TimeSpan.Zero) left = TimeSpan.Zero;
            return "-" + left.ToString(@"mm\:ss");
        }
        return pos.ToString(@"mm\:ss");
    }

    private void RefreshTime()
    {
        ledTime.Text = FormatLed(_audio.Position, _audio.TotalTime);
        UpdateShadeTitle();
    }

    private void LedTime_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _showRemaining = !_showRemaining;
        RefreshTime();
        e.Handled = true;
    }

    private void Visualizer_MouseDown(object sender, MouseButtonEventArgs e)
    {
        CycleVisualizer();
        e.Handled = true;
    }

    private void CycleVisualizer()
    {
        _visMode = (_visMode + 1) % 3;
        for (int i = 0; i < _eqValues.Count; i++) _eqValues[i] = 0;
        eqSliders.Visibility = _visMode == 0 ? Visibility.Visible : Visibility.Hidden;
        scopeLine.Visibility = _visMode == 1 ? Visibility.Visible : Visibility.Collapsed;
        if (_visMode == 1) DrawScope();
        if (_isPlaying && !_eqTimer.IsEnabled) _eqTimer.Start();
        txtStatus.Text = _visMode == 0 ? "Visualization: spectrum" : _visMode == 1 ? "Visualization: oscilloscope" : "Visualization: off";
    }

    private void DrawScope()
    {
        const int n = 76;
        var wave = _isPlaying ? _audio.GetWaveform(n) : null;
        var pts = new PointCollection(n);
        for (int i = 0; i < n; i++)
        {
            double v = wave != null ? wave[i] : 0;
            pts.Add(new Point(i, 8 - Math.Clamp(v * 2.0, -1, 1) * 7.5));
        }
        scopeLine.Points = pts;
    }

    // ---------- Skins and right-click menu ----------

    private sealed record Skin(string Name, string BodyT, string BodyB, string TitleT, string TitleB,
        string StripeDark, string StripeLight, string Text, string Dim, string Label,
        string FaceT, string FaceB, string TextFaceT, string TextFaceB);

    private static readonly Skin[] Skins =
    {
        new("Classic", "#5A5A72", "#34344A", "#6E7CA0", "#232B47", "#1B2236", "#8E9CC4", "#00E000", "#18A018", "#AFC0E0", "#D6DAE8", "#8A8EA4", "#70748A", "#454960"),
        new("Green Machine", "#3E5A44", "#1F3324", "#5E8A68", "#16261A", "#122016", "#7FB88A", "#8CF04A", "#4C9A38", "#A8D4B0", "#CFE0D2", "#86A08C", "#5F7F66", "#35503C"),
        new("Amber", "#6A5638", "#3A2E1C", "#A07C46", "#3A2A14", "#2C2010", "#D8A860", "#FFA800", "#B07410", "#E8D0A8", "#E4D8C0", "#A08E6C", "#8A7452", "#504024"),
        new("Midnight", "#25303F", "#10151D", "#3E5C86", "#0F1826", "#0C121C", "#5C86B8", "#38D8FF", "#1E8AA8", "#8FB0D0", "#B8C8DC", "#6A7C94", "#4A5A70", "#28323F"),
    };

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
    private static LinearGradientBrush Grad(string top, string bottom) => new LinearGradientBrush(C(top), C(bottom), 90);

    private void ApplySkin(Skin k)
    {
        var stripes = new DrawingGroup();
        stripes.Children.Add(new GeometryDrawing(new SolidColorBrush(C(k.StripeDark)), null, new RectangleGeometry(new Rect(0, 0, 4, 3))));
        stripes.Children.Add(new GeometryDrawing(new SolidColorBrush(C(k.StripeLight)), null, new RectangleGeometry(new Rect(0, 0, 4, 1))));
        Resources["WaStripes"] = new DrawingBrush(stripes)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 4, 3), ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 4, 3), ViewboxUnits = BrushMappingMode.Absolute,
        };
        Resources["WaBody"] = Grad(k.BodyT, k.BodyB);
        Resources["WaTitleBrush"] = Grad(k.TitleT, k.TitleB);
        Resources["WaPlate"] = new SolidColorBrush(C(k.TitleB));
        Resources["WaFace"] = Grad(k.FaceT, k.FaceB);
        Resources["WaTextFace"] = Grad(k.TextFaceT, k.TextFaceB);
        Resources["WaGreen"] = new SolidColorBrush(C(k.Text));
        Resources["WaLed"] = new SolidColorBrush(C(k.Text));
        Resources["WaGreenDim"] = new SolidColorBrush(C(k.Dim));
        Resources["WaLabel"] = new SolidColorBrush(C(k.Label));
        try { lstPlaylist.Items.Refresh(); } catch { }
        txtStatus.Text = "Skin: " + k.Name;
    }

    private void BuildContextMenu()
    {
        var menu = new ContextMenu();
        var skinsItem = new MenuItem { Header = "Skins" };
        foreach (var skin in Skins)
        {
            var captured = skin;
            var item = new MenuItem { Header = skin.Name };
            item.Click += (_, _) => ApplySkin(captured);
            skinsItem.Items.Add(item);
        }
        menu.Items.Add(skinsItem);

        var shade = new MenuItem { Header = "Windowshade mode" };
        shade.Click += (_, _) => ToggleShade();
        menu.Items.Add(shade);

        var eq = new MenuItem { Header = "Equalizer" };
        eq.Click += (_, _) => ToggleEqualizer(btnEqualizer.IsChecked != true);
        menu.Items.Add(eq);

        var vis = new MenuItem { Header = "Next visualization" };
        vis.Click += (_, _) => CycleVisualizer();
        menu.Items.Add(vis);

        menu.Items.Add(new Separator());
        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => Close();
        menu.Items.Add(exit);
        outer.ContextMenu = menu;
    }

    // ---------- Keyboard shortcuts (same keys as Winamp) ----------

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox) return;
        switch (e.Key)
        {
            case Key.Z: PlayPrev(); break;
            case Key.X: Play_Click(sender, e); break;
            case Key.C: Pause_Click(sender, e); break;
            case Key.V: Stop_Click(sender, e); break;
            case Key.B: PlayNext(); break;
            case Key.Left: if (_audio.HasTrack) _audio.Position -= TimeSpan.FromSeconds(5); break;
            case Key.Right: if (_audio.HasTrack) _audio.Position += TimeSpan.FromSeconds(5); break;
            case Key.Up: sldVolume.Value = Math.Clamp(sldVolume.Value + 0.05, 0, 1); break;
            case Key.Down: sldVolume.Value = Math.Clamp(sldVolume.Value - 0.05, 0, 1); break;
            case Key.Delete:
                if (lstPlaylist.IsKeyboardFocusWithin) RemoveSelected(); else return;
                break;
            default: return;
        }
        e.Handled = true;
    }

    private void UpdatePlaylistInfo()
    {
        try { txtPlaylistInfo.Text = _playlist.Count == 1 ? "1 item" : _playlist.Count + " items"; } catch { }
    }

    private void MarqueeTimer_Tick(object? sender, EventArgs e)
    {
        double box = marqueeHost.ActualWidth;
        double w = txtNowPlaying.ActualWidth;
        if (box <= 0 || w <= box) { marqueeShift.X = 0; return; }
        marqueeShift.X -= 1;
        if (marqueeShift.X < -w) marqueeShift.X = box;
    }

    // ---------- Playlist ----------

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog();
        dlg.Filter = "Audio files|*.mp3;*.wav;*.wma;*.aac;*.m4a|All files|*.*";
        dlg.Multiselect = true;
        if (dlg.ShowDialog(this) == true)
        {
            AddFiles(dlg.FileNames);
        }
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        try
        {
            int firstNew = _playlist.Count;
            string? last = null;
            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;
                // add to playlist
                _playlist.Add(path);
                last = path;
            }
            if (last == null) { txtStatus.Text = "No audio files to add."; return; }

            try { lstPlaylist.Items.Refresh(); lstPlaylist.SelectedIndex = firstNew; } catch { }
            UpdatePlaylistInfo();
            txtStatus.Text = "Added to playlist: " + last;
            // try set artwork if an image with same name.png/jpg exists
            var artPng = System.IO.Path.ChangeExtension(last, ".png");
            var artJpg = System.IO.Path.ChangeExtension(last, ".jpg");
            if (File.Exists(artPng)) imgArtwork.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(artPng));
            else if (File.Exists(artJpg)) imgArtwork.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(artJpg));
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Failed to load: " + ex.Message;
        }
    }

    private void RemoveSelected()
    {
        int idx = lstPlaylist.SelectedIndex;
        if (idx < 0 || idx >= _playlist.Count) return;
        int old = _currentIndex;
        _playlist.RemoveAt(idx);
        if (idx < _currentIndex) _currentIndex--;
        else if (idx == _currentIndex) _currentIndex = idx - 1;   // keeps playing; Next continues with the following track
        CurrentIndex = idx == old ? -1 : _currentIndex;
        try { lstPlaylist.Items.Refresh(); } catch { }
        if (_playlist.Count > 0) lstPlaylist.SelectedIndex = Math.Min(idx, _playlist.Count - 1);
        UpdatePlaylistInfo();
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        RemoveSelected();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _audio.Unload();
        _positionTimer.Stop();
        _isPlaying = false;
        SetPlayState("\u25A0");
        _playlist.Clear();
        _currentIndex = -1;
        CurrentIndex = -1;
        try { lstPlaylist.Items.Refresh(); } catch { }
        UpdatePlaylistInfo();
        SetTrackText("No local file loaded");
        txtAudioInfo.Text = "";
        ledTime.Text = "00:00";
        txtPosition.Text = "0:00 / 0:00";
        try { sldSeek.Value = 0; } catch { }
        UpdateShadeTitle();
        txtStatus.Text = "Playlist cleared";
    }

    private void Playlist_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Playlist_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            AddFiles(files.Where(f => AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant())));
        }
        e.Handled = true;
    }

    private void OpenInWinamp_Click(object sender, RoutedEventArgs e)
    {
        // If a media source is loaded in the MediaElement, try to open it in Winamp.
        // This attempts to launch the system winamp.exe with the current file path.
        try
        {
            // mediaPlayer.Source may be null; ensure we have a local file path.
            var path = _audio.CurrentPath;
            if (string.IsNullOrEmpty(path))
            {
                txtStatus.Text = "No local file loaded to open in Winamp.";
                return;
            }

            // Typical Winamp executable name
            var winampExe = "winamp.exe";
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = winampExe,
                Arguments = '"' + path + '"',
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(startInfo);
            txtStatus.Text = "Attempted to open file in Winamp.";
            // also stop our playback if running
            try { _audio.Stop(); _positionTimer.Stop(); _isPlaying = false; SetPlayState("\u25A0"); } catch { }
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"Failed to open in Winamp: {ex.Message}";
        }
    }

    // ---------- Transport ----------

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // if nothing loaded but playlist has items, start selected or first
            if (!_audio.HasTrack && _playlist.Count > 0)
            {
                if (_currentIndex < 0)
                {
                    try { _currentIndex = lstPlaylist.SelectedIndex >= 0 ? lstPlaylist.SelectedIndex : 0; } catch { _currentIndex = 0; }
                }
                PlayIndex(_currentIndex);
                return;
            }
            if (!_audio.HasTrack) return;
            _audio.Play();
            _isPlaying = true;
            SetPlayState("\u25BA");
            _positionTimer.Start();
            _eqTimer.Start();
            txtStatus.Text = "Playing";
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Play failed: " + ex.Message;
        }
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _audio.Pause();
        _positionTimer.Stop();
        _isPlaying = false;
        SetPlayState("II");
        txtStatus.Text = "Paused";
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _audio.Stop();
        _positionTimer.Stop();
        _isPlaying = false;
        SetPlayState("\u25A0");
        try { sldSeek.Value = 0; } catch { }
        try { RefreshTime(); txtPosition.Text = "0:00 / 0:00"; } catch { }
        // the analyzer bars fall back to zero, then the timer stops itself
        txtStatus.Text = "Stopped";
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        PlayPrev();
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        PlayNext();
    }

    private void Shuffle_Click(object sender, RoutedEventArgs e)
    {
        _shuffle = !_shuffle;
        txtStatus.Text = _shuffle ? "Shuffle ON" : "Shuffle OFF";
    }

    private void Repeat_Click(object sender, RoutedEventArgs e)
    {
        _repeat = !_repeat;
        txtStatus.Text = _repeat ? "Repeat ON" : "Repeat OFF";
    }

    private void SldVolume_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        try { _audio.Volume = sldVolume.Value; } catch { }
    }

    private void SldBalance_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            // snap to the middle like Winamp does
            if (Math.Abs(sldBalance.Value) < 0.08) sldBalance.Value = 0;
            _audio.Balance = sldBalance.Value;
        }
        catch { }
    }

    private void PositionTimer_Tick(object? sender, EventArgs e)
    {
        if (_audio.HasTrack && _audio.TotalTime > TimeSpan.Zero && !_isSeeking)
        {
            var pos = _audio.Position;
            var total = _audio.TotalTime;
            txtPosition.Text = $"{pos.ToString(@"mm\:ss")} / {total.ToString(@"mm\:ss")}";
            ledTime.Text = FormatLed(pos, total);
            UpdateShadeTitle();
            _updatingSeek = true;
            try
            {
                sldSeek.Maximum = total.TotalSeconds;
                sldSeek.Value = pos.TotalSeconds;
            }
            finally { _updatingSeek = false; }
        }
    }

    private void SldSeek_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        // ignore changes made by the timer itself, otherwise playback would re-seek twice a second
        if (_isSeeking || _updatingSeek) return;
        try
        {
            _audio.Position = TimeSpan.FromSeconds(sldSeek.Value);
        }
        catch { }
    }

    private void SldSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isSeeking = true;
    }

    private void SldSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            _audio.Position = TimeSpan.FromSeconds(sldSeek.Value);
        }
        catch { }
        _isSeeking = false;
    }

    // ---------- Reference tools (to be built in the tutorial chapters) ----------

    private void AddLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var url = (txtLink.Text ?? "").Trim();
            if (string.IsNullOrEmpty(url))
            {
                txtStatus.Text = "Enter a URL to add.";
                return;
            }

            // Ensure a scheme so Process.Start works reliably
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            lstLinks.Items.Add(url);
            txtLink.Text = "";
            txtStatus.Text = "Link added: " + url;
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Failed to add link: " + ex.Message;
        }
    }

    private void OpenLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (lstLinks.SelectedItem is not string link)
            {
                txtStatus.Text = "No link selected.";
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo(link)
            {
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            txtStatus.Text = "Opened link: " + link;
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Failed to open link: " + ex.Message;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files|*.txt|All files|*.*",
                FileName = "notes.txt",
                DefaultExt = ".txt"
            };

            if (dlg.ShowDialog(this) == true)
            {
                File.WriteAllText(dlg.FileName, txtNotes.Text ?? "");
                txtStatus.Text = "Notes exported to: " + dlg.FileName;
            }
            else
            {
                txtStatus.Text = "Export cancelled.";
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Save failed: " + ex.Message;
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files|*.txt|All files|*.*",
                DefaultExt = ".txt",
                Multiselect = false
            };

            if (dlg.ShowDialog(this) == true)
            {
                txtNotes.Text = File.ReadAllText(dlg.FileName);
                txtStatus.Text = "Notes loaded: " + dlg.FileName;
            }
            else
            {
                txtStatus.Text = "Reopen cancelled.";
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Open failed: " + ex.Message;
        }
    }

    private void Image_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog(this) == true)
            {
                var uri = new Uri(dlg.FileName);
                var bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bmp.EndInit();
                imgArtwork.Source = bmp;
                try { imgButton.Source = bmp; } catch { }
                txtStatus.Text = "Artwork set: " + dlg.FileName;
            }
            else
            {
                txtStatus.Text = "Choose artwork cancelled.";
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Failed to set artwork: " + ex.Message;
        }
    }

    // ---------- Media events and playlist navigation ----------

    // NAudio replaced the MediaElement, so these are called by our code (and by the engine's events).
    private void Media_Opened()
    {
        txtStatus.Text = "Media opened";
        try
        {
            if (_audio.TotalTime > TimeSpan.Zero)
            {
                var total = _audio.TotalTime;
                try { sldSeek.Maximum = total.TotalSeconds; } catch { }
                txtAudioInfo.Text = $"{_audio.BitrateKbps} kbps  {Math.Round(_audio.SampleRate / 1000.0)} kHz  {(_audio.Channels == 1 ? "mono" : "stereo")}";
                SetTrackText(_trackText + " (" + total.ToString(@"m\:ss") + ")");
            }
        }
        catch { }
    }

    private void Media_Ended()
    {
        txtStatus.Text = "Finished";
        _positionTimer.Stop();
        _isPlaying = false;
        SetPlayState("\u25A0");
        try { sldSeek.Value = 0; RefreshTime(); } catch { }
        // auto-play next
        PlayNext();
    }

    private void PlayIndex(int index)
    {
        if (index < 0 || index >= _playlist.Count) return;
        _currentIndex = index;
        CurrentIndex = index;
        var path = _playlist[index];
        try
        {
            _audio.Load(path);
            _trackText = $"{index + 1}. {System.IO.Path.GetFileNameWithoutExtension(path)}";
            SetTrackText(_trackText);
            lstPlaylist.SelectedIndex = index;
            Media_Opened();
            _audio.Play();
            _isPlaying = true;
            SetPlayState("\u25BA");
            _positionTimer.Start();
            _eqTimer.Start();
            txtStatus.Text = "Playing: " + path;
        }
        catch (Exception ex)
        {
            Media_Failed(ex);
        }
    }

    private void PlayNext()
    {
        if (_playlist.Count == 0) return;
        if (_shuffle)
        {
            var next = _rnd.Next(0, _playlist.Count);
            PlayIndex(next);
        }
        else
        {
            var next = _currentIndex + 1;
            if (next >= _playlist.Count)
            {
                if (_repeat) next = 0;
                else { try { txtStatus.Text = "Playlist finished"; } catch { } return; }
            }
            PlayIndex(next);
        }
    }

    private void PlayPrev()
    {
        if (_playlist.Count == 0) return;
        var prev = _currentIndex - 1;
        if (prev < 0) prev = 0;
        PlayIndex(prev);
    }

    private void Media_Failed(Exception ex)
    {
        // Friendly recovery message (Playback chapter TODO): the file could not be decoded or the sound device failed.
        _positionTimer.Stop();
        _isPlaying = false;
        SetPlayState("\u25A0");
        try { _audio.Stop(); } catch { }
        txtStatus.Text = "Couldn't play that file (" + ex.Message + "). Try an MP3, WAV, WMA, AAC or M4A file.";
    }

    private void LstPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var idx = lstPlaylist.SelectedIndex;
        if (idx >= 0) PlayIndex(idx);
    }

    // ---------- Equalizer panel (slides out under the player) ----------
    // The sliders drive the real NAudio equalizer in AudioEngine (10 peaking filters + preamp).

    private static readonly string[] EqBandLabels = { "60", "170", "310", "600", "1K", "3K", "6K", "12K", "14K", "16K" };

    private static readonly Dictionary<string, double[]> EqPresetValues = new()
    {
        ["Flat"] = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        ["Bass boost"] = new double[] { 8, 6, 4, 2, 0, 0, 0, 0, 0, 0 },
        ["Treble boost"] = new double[] { 0, 0, 0, 0, 0, 2, 4, 6, 7, 8 },
        ["Rock"] = new double[] { 5, 3, -3, -5, -2, 2, 5, 6, 6, 6 },
        ["Pop"] = new double[] { -1, 2, 4, 5, 3, 0, -1, -1, -2, -2 },
        ["Classical"] = new double[] { 0, 0, 0, 0, 0, 0, -4, -4, -4, -6 },
    };

    private readonly List<Slider> _eqBandSliders = new();
    private Slider? _eqPreamp;

    private Slider MakeEqSlider(double left)
    {
        var sl = new Slider
        {
            Style = (Style)FindResource("WaVSlider"),
            Orientation = Orientation.Vertical,
            Minimum = -12,
            Maximum = 12,
            Value = 0,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            Width = 14,
            Height = 62,
        };
        Canvas.SetLeft(sl, left);
        Canvas.SetTop(sl, 25);
        sl.ValueChanged += EqSlider_ValueChanged;
        eqBands.Children.Add(sl);
        return sl;
    }

    private void AddEqLabel(string text, double left, double width, double size)
    {
        var tb = new TextBlock { Text = text, Width = width, TextAlignment = TextAlignment.Center, FontSize = size };
        Canvas.SetLeft(tb, left);
        Canvas.SetTop(tb, 88);
        eqBands.Children.Add(tb);
    }

    private void BuildEqualizer()
    {
        _eqPreamp = MakeEqSlider(36);
        AddEqLabel("PRE", 29, 28, 7);
        for (int i = 0; i < EqBandLabels.Length; i++)
        {
            double left = 76 + 19 * i;
            _eqBandSliders.Add(MakeEqSlider(left));
            AddEqLabel(EqBandLabels[i], left - 2.5, 19, 7);
        }
        UpdateEqCurve();
    }

    private void UpdateEqCurve()
    {
        var pts = new PointCollection();
        for (int i = 0; i < _eqBandSliders.Count; i++)
        {
            double x = 3 + i * (110.0 / (_eqBandSliders.Count - 1));
            double y = 9.5 - _eqBandSliders[i].Value / 12.0 * 8.5;
            pts.Add(new Point(x, y));
        }
        eqCurve.Points = pts;
    }

    private void EqSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateEqCurve();
        var slider = (Slider)sender;
        if (ReferenceEquals(slider, _eqPreamp)) _audio.PreampDb = slider.Value;
        else
        {
            int band = _eqBandSliders.IndexOf(slider);
            if (band >= 0) _audio.SetBand(band, slider.Value);
        }
    }

    private void ToggleEqualizer(bool open)
    {
        btnEqualizer.IsChecked = open;
        var anim = new DoubleAnimation(open ? 120 : 0, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        eqHost.BeginAnimation(FrameworkElement.HeightProperty, anim);
    }

    private void Equalizer_Click(object sender, RoutedEventArgs e)
    {
        ToggleEqualizer(btnEqualizer.IsChecked == true);
    }

    private void EqClose_Click(object sender, RoutedEventArgs e)
    {
        ToggleEqualizer(false);
    }

    private void EqOn_Click(object sender, RoutedEventArgs e)
    {
        bool on = btnEqOn.IsChecked == true;
        eqBands.IsEnabled = on;
        eqBands.Opacity = on ? 1.0 : 0.45;
        eqCurve.Visibility = on ? Visibility.Visible : Visibility.Hidden;
        _audio.EqEnabled = on;
    }

    private void EqPresets_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu { PlacementTarget = (UIElement)sender, Placement = PlacementMode.Bottom };
        foreach (var preset in EqPresetValues)
        {
            var values = preset.Value;
            var item = new MenuItem { Header = preset.Key };
            item.Click += (_, _) => ApplyEqPreset(values);
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    private void ApplyEqPreset(double[] values)
    {
        // each slider change updates the audio engine through EqSlider_ValueChanged
        if (_eqPreamp != null) _eqPreamp.Value = 0;
        for (int i = 0; i < _eqBandSliders.Count && i < values.Length; i++) _eqBandSliders[i].Value = values[i];
        UpdateEqCurve();
    }

    // ---------- Spectrum analyzer ----------

    private void EqTimer_Tick(object? sender, EventArgs e)
    {
        if (_visMode == 1) { DrawScope(); if (!_isPlaying) _eqTimer.Stop(); return; }
        if (_visMode == 2) { if (!_isPlaying) _eqTimer.Stop(); return; }
        // Real spectrum from the audio engine: bars jump up to the measured level and fall smoothly.
        bool anyUp = false;
        float[] spectrum = _isPlaying ? _audio.GetSpectrum(_eqValues.Count) : new float[_eqValues.Count];
        for (int i = 0; i < _eqValues.Count; i++)
        {
            double cur = _eqValues[i];
            double next;
            if (_isPlaying)
            {
                double target = spectrum[i];
                next = target > cur ? target : Math.Max(target, cur - 1.5);
            }
            else
            {
                next = Math.Max(0, cur - 1.5);
            }
            _eqValues[i] = next;
            if (next > 0) anyUp = true;
        }
        if (!_isPlaying && !anyUp) _eqTimer.Stop();
    }
}
