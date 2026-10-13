using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace MusicReferenceHub;

// MainWindow is a class. Put fields and methods inside these outer braces.
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // TODO: connect your collection in the Data chapter.
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Playback chapter): Implement Load in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Load_Click: behavior still to build.";
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Playback chapter): Implement Play in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Play_Click: behavior still to build.";
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Playback chapter): Implement Pause in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Pause_Click: behavior still to build.";
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        // TODO (Playback chapter): Implement Stop in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Stop_Click: behavior still to build.";
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
        // TODO (Playback chapter): Implement Media_Opened in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Media_Opened: behavior still to build.";
    }

    private void Media_Ended(object sender, RoutedEventArgs e)
    {
        // TODO (Playback chapter): Implement Media_Ended in the matching tutorial chapter.
        // Replace this placeholder with the handler shown in that chapter.
        txtStatus.Text = "Media_Ended: behavior still to build.";
    }

    private void Media_Failed(object sender, ExceptionRoutedEventArgs e)
    {
        // TODO: show a helpful file-format recovery message in the Playback chapter.
    }
}
