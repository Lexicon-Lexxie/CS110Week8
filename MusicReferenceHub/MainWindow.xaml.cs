using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;

namespace MusicReferenceHub;

// MainWindow is a class. Put fields and methods inside these outer braces.
public partial class MainWindow : Window
{
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
        // equalizer visual values
        _eqValues = new ObservableCollection<double>(Enumerable.Repeat(8.0, 10));
        try { eqSliders.ItemsSource = _eqValues; } catch { }
        _rnd = new Random();
        _eqTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        _eqTimer.Tick += EqTimer_Tick;
    }

    private readonly DispatcherTimer _positionTimer;
    private bool _isSeeking = false;
    private ObservableCollection<double> _eqValues;
    private DispatcherTimer _eqTimer;
    private Random _rnd;
    private List<string> _playlist;
    private int _currentIndex;
    private bool _shuffle;

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog();
        dlg.Filter = "Audio files|*.mp3;*.wav;*.wma;*.aac|All files|*.*";
        if (dlg.ShowDialog(this) == true)
        {
            var path = dlg.FileName;
            try
            {
                // add to playlist and select
                _playlist.Add(path);
                try { lstPlaylist.Items.Refresh(); lstPlaylist.SelectedIndex = _playlist.Count - 1; } catch { }
                txtStatus.Text = "Added to playlist: " + path;
                // try set artwork if an image with same name.png/jpg exists
                var artPng = System.IO.Path.ChangeExtension(path, ".png");
                var artJpg = System.IO.Path.ChangeExtension(path, ".jpg");
                if (File.Exists(artPng)) imgArtwork.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(artPng));
                else if (File.Exists(artJpg)) imgArtwork.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(artJpg));
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Failed to load: " + ex.Message;
            }
        }
    }

    private void OpenInWinamp_Click(object sender, RoutedEventArgs e)
    {
        // If a media source is loaded in the MediaElement, try to open it in Winamp.
        // This attempts to launch the system winamp.exe with the current file path.
        try
        {
            // mediaPlayer.Source may be null; ensure we have a local file path.
            var source = mediaPlayer.Source;
            if (source == null || string.IsNullOrEmpty(source.LocalPath))
            {
                txtStatus.Text = "No local file loaded to open in Winamp.";
                return;
            }

            var path = source.LocalPath;
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
            try { mediaPlayer.Stop(); _positionTimer.Stop(); } catch { }
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"Failed to open in Winamp: {ex.Message}";
        }
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // if nothing loaded but playlist has items, start selected or first
            if ((mediaPlayer.Source == null || string.IsNullOrEmpty(mediaPlayer.Source.LocalPath)) && _playlist.Count > 0)
            {
                if (_currentIndex < 0)
                {
                    try { _currentIndex = lstPlaylist.SelectedIndex >= 0 ? lstPlaylist.SelectedIndex : 0; } catch { _currentIndex = 0; }
                }
                PlayIndex(_currentIndex);
                return;
            }
            mediaPlayer.Play();
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
        mediaPlayer.Pause();
        _positionTimer.Stop();
        txtStatus.Text = "Paused";
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        mediaPlayer.Stop();
        _positionTimer.Stop();
        try { sldSeek.Value = 0; } catch { }
        try { _eqTimer.Stop(); } catch { }
        txtStatus.Text = "Stopped";
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        txtStatus.Text = "Prev (not implemented)";
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        txtStatus.Text = "Next (not implemented)";
    }

    private void Shuffle_Click(object sender, RoutedEventArgs e)
    {
        _shuffle = !_shuffle;
        txtStatus.Text = _shuffle ? "Shuffle ON" : "Shuffle OFF";
    }

    private void SldVolume_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        try { mediaPlayer.Volume = sldVolume.Value; } catch { }
    }

    private void PositionTimer_Tick(object? sender, EventArgs e)
    {
        if (mediaPlayer.NaturalDuration.HasTimeSpan && !_isSeeking)
        {
            var pos = mediaPlayer.Position;
            var total = mediaPlayer.NaturalDuration.TimeSpan;
            txtPosition.Text = $"{pos.ToString(@"mm\:ss")} / {total.ToString(@"mm\:ss")}";
            sldSeek.Maximum = total.TotalSeconds;
            sldSeek.Value = pos.TotalSeconds;
        }
    }

    private void SldSeek_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isSeeking) return;
        try
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(sldSeek.Value);
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
            mediaPlayer.Position = TimeSpan.FromSeconds(sldSeek.Value);
        }
        catch { }
        _isSeeking = false;
    }

    private void AddLink_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Links chapter): Implement AddLink in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "AddLink_Click: behavior still to build.";
    }

    private void OpenLink_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Links chapter): Implement OpenLink in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "OpenLink_Click: behavior still to build.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Text files chapter): Implement Save in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Save_Click: behavior still to build.";
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Text files chapter): Implement Open in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Open_Click: behavior still to build.";
    }

    private void Image_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Artwork chapter): Implement Image in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Image_Click: behavior still to build.";
    }

    private void Media_Opened(object sender, RoutedEventArgs e)
    {
        txtStatus.Text = "Media opened";
        try
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                try { sldSeek.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds; } catch { }
            }
        }
        catch { }
    }

    private void Media_Ended(object sender, RoutedEventArgs e)
    {
        txtStatus.Text = "Finished";
        _positionTimer.Stop();
        _eqTimer.Stop();
        // auto-play next
        PlayNext();
    }

    private void PlayIndex(int index)
    {
        if (index < 0 || index >= _playlist.Count) return;
        _currentIndex = index;
        var path = _playlist[index];
        try
        {
            mediaPlayer.Source = new Uri(path);
            txtNowPlaying.Text = System.IO.Path.GetFileName(path);
            lstPlaylist.SelectedIndex = index;
            mediaPlayer.Play();
            _positionTimer.Start();
            _eqTimer.Start();
            txtStatus.Text = "Playing: " + path;
        }
        catch (Exception ex)
        {
            txtStatus.Text = "PlayIndex failed: " + ex.Message;
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
            if (next >= _playlist.Count) { try { txtStatus.Text = "Playlist finished"; } catch { } return; }
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

    private void Media_Failed(object sender, ExceptionRoutedEventArgs e)
    {
        // TODO: show a helpful file-format recovery message in the Playback chapter.
    }

    private void LstPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var idx = lstPlaylist.SelectedIndex;
        if (idx >= 0) PlayIndex(idx);
    }

    private void EqTimer_Tick(object? sender, EventArgs e)
    {
        // randomize eq bar heights
        for (int i = 0; i < _eqValues.Count; i++)
        {
            var h = 6 + _rnd.NextDouble() * 84; // range 6..90
            _eqValues[i] = h;
        }
    }
}
