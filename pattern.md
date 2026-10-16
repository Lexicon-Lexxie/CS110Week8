# Pattern: Timer-Synced Slider with Guard Flags

**Category:** Timer + Event (UI that is updated by code *and* by the user)
**Project:** CS110 Week 8, Music Reference Hub (WPF / C#)

## The pattern in one sentence

A `DispatcherTimer` pushes the current playback position into a `Slider` twice a second, while two boolean flags (`_updatingSeek` and `_isSeeking`) stop the slider's own `ValueChanged` event from treating the timer's updates as if the user had dragged it.

## Where the pattern appears

All line numbers refer to `MusicReferenceHub/MainWindow.xaml.cs` unless noted.

| Piece | Location | Role |
|---|---|---|
| Slider declaration and event wiring | `MainWindow.xaml`, line 400 (`sldSeek`) | Hooks `ValueChanged`, `PreviewMouseLeftButtonDown` and `PreviewMouseLeftButtonUp` |
| Timer created | lines 29-30 (constructor) | 500 ms `DispatcherTimer`, `Tick` wired to `PositionTimer_Tick` |
| Flag fields | lines 61-63 | `_positionTimer`, `_isSeeking`, `_updatingSeek` |
| Timer to slider | `PositionTimer_Tick`, lines 545-562 | Reads the audio position and writes it to the slider and the time labels |
| Slider to audio (any value change) | `SldSeek_ValueChanged`, lines 564-573 | Seeks the audio, unless a flag says to ignore the change |
| Slider to audio (drag finished) | `SldSeek_PreviewMouseLeftButtonDown/Up`, lines 575-588 | Sets `_isSeeking` while the mouse is down and seeks once on release |
| Timer lifecycle | `Start()` at lines 476 and 776; `Stop()` at lines 489, 498, 752 and 817 | Runs only while audio is actually playing |
| Safe seek target | `AudioEngine.Position` setter, `AudioEngine.cs` lines 49-62 | Clamps to `0..TotalTime` and takes a lock |

## How it works

There are two writers to one control, and each needs to be kept from triggering the other.

```
            ┌──────────── timer writes ────────────┐
            ▼                                      │
  _audio.Position ──► sldSeek.Value        PositionTimer_Tick (500 ms)
            ▲              │
            │              ▼  ValueChanged fires for BOTH writers
            └──── SldSeek_ValueChanged ──► "is this the user, or the timer?"
```

Setting `sldSeek.Value` in code raises `ValueChanged`, exactly as a drag would. Without protection, every timer tick would call `_audio.Position = ...`, re-seeking the track twice a second (audible as stutter) and fighting the user's drag.

The code solves this with the two flags:

```csharp
// Timer: writes the slider, but marks the write as "not the user"
private void PositionTimer_Tick(object? sender, EventArgs e)
{
    if (_audio.HasTrack && _audio.TotalTime > TimeSpan.Zero && !_isSeeking)   // don't fight a drag
    {
        var pos = _audio.Position;
        var total = _audio.TotalTime;
        txtPosition.Text = $"{pos:mm\\:ss} / {total:mm\\:ss}";
        _updatingSeek = true;
        try
        {
            sldSeek.Maximum = total.TotalSeconds;
            sldSeek.Value = pos.TotalSeconds;
        }
        finally { _updatingSeek = false; }                                    // always reset
    }
}

// Event: ignores changes it did not originate
private void SldSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    if (_isSeeking || _updatingSeek) return;
    _audio.Position = TimeSpan.FromSeconds(sldSeek.Value);
}

// Drag: hold the timer off while the thumb is held, commit once on release
private void SldSeek_PreviewMouseLeftButtonDown(object s, MouseButtonEventArgs e) => _isSeeking = true;
private void SldSeek_PreviewMouseLeftButtonUp(object s, MouseButtonEventArgs e)
{
    _audio.Position = TimeSpan.FromSeconds(sldSeek.Value);
    _isSeeking = false;
}
```

## Why it works

1. **Two flags, two jobs.** `_updatingSeek` answers "did my own code just move the slider?" and lasts only a few lines. `_isSeeking` answers "does the user currently have the thumb?" and lasts for the whole drag. They cover different time spans, so neither can replace the other.
2. **`try/finally` around the guarded write.** If setting `Maximum` or `Value` throws, the flag still resets. Without the `finally`, one exception would leave `_updatingSeek` stuck on `true` and the slider would silently stop responding to the user.
3. **Everything runs on the UI thread.** `DispatcherTimer` ticks on the UI thread, so the slider can be touched directly and the flags need no locking. A `System.Timers.Timer` would need `Dispatcher.Invoke` and a different guard strategy.
4. **Seek once, not continuously.** Committing on mouse-up avoids hundreds of seek calls during a drag. `IsMoveToPointEnabled="True"` (XAML line 400) still lets a single click jump to a point because `ValueChanged` fires with both flags false.
5. **The target defends itself.** `AudioEngine.Position` clamps bad values and locks, so even an unexpected slider value cannot seek outside the file or race the audio thread.
6. **Timer only runs when needed.** It starts on play and stops on pause, stop, track end and failure. An idle player does no polling.

## How to adapt it

The pattern applies whenever **code and the user both write the same control** and the control's change event cannot tell them apart.

### Recipe

1. Pick the control and the "source of truth" it mirrors (a position, a counter, a setting).
2. Add `bool _updatingFromCode` (guards a single programmatic write) and, if the control has a press-and-hold gesture, `bool _userIsDragging`.
3. In the timer or refresh method, check `!_userIsDragging`, set `_updatingFromCode = true` inside a `try`, write the control, reset in `finally`.
4. At the top of the control's change handler, `return` early if either flag is set.
5. Do the real work (seek, save, apply) in the handler for genuine user changes, or on drag release.
6. Start the timer when the source starts changing, stop it when it can't.

### Example adaptations

| Scenario | Timer drives | User changes | What the guard prevents |
|---|---|---|---|
| Quiz countdown bar | `ProgressBar` / slider from remaining seconds | "Add 30 s" slider | Handler re-applying the same time every tick |
| Video or podcast scrubber | Slider from `MediaElement.Position` | Scrub bar | Stutter and fighting the drag |
| Auto-save indicator | Checkbox/label from "last saved" state | User toggles "auto-save" | Toggle handler firing when code updates the checkbox |
| Settings panel with presets | Sliders set from a preset | User moves a slider by hand | Preset load marking the settings as "customized" |

The last row already exists in this project in a lighter form: `ApplyEqPreset` (lines 956-962) sets slider values in code and lets `EqSlider_ValueChanged` run on purpose, because there the handler *should* react. That contrast is useful. **Decide per control whether code-driven changes should reach the handler**, and use a flag only when the answer is no.

### Minimal reusable template

```csharp
private bool _updatingFromCode;
private bool _userIsDragging;

private void RefreshTick(object? s, EventArgs e)
{
    if (_userIsDragging) return;
    _updatingFromCode = true;
    try { mySlider.Value = ReadSourceValue(); }
    finally { _updatingFromCode = false; }
}

private void MySlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
{
    if (_updatingFromCode || _userIsDragging) return;
    ApplyToSource(mySlider.Value);
}
```

## Watch-outs seen in this codebase

- **Unguarded programmatic resets.** `Stop_Click` (line 501), `Media_Ended` (line 755) and `Clear_Click` (line 403) set `sldSeek.Value = 0` without raising `_updatingSeek`. That triggers `SldSeek_ValueChanged`, which seeks to 0 a second time. It is harmless here because the engine has already rewound, but if the handler did anything costly or non-idempotent it would be a bug. When you adapt the pattern, route *all* code writes through one helper (for example `SetSeekValue(double v)`) that owns the flag.
- **Keyboard changes.** Arrow keys on a focused slider change the value with both flags false, so they seek immediately. That is usually what you want, but it is a different path from mouse-drag commit.
- **Re-entrancy is not thread-safety.** The flags protect against re-entry on one thread. If the source of truth is updated from another thread, you still need the lock that `AudioEngine` uses.
- **Magic interval.** The 500 ms tick is a trade-off between smoothness and cost. Make it a named constant if you adapt this for something that needs finer movement.
