# Music Reference Hub (CS110 Week 8)

**Team:** Awayzo  |  **Team Lead:** Lexxie  |  **Members:** Lexxie, Paulina, Nicolas, Rudy
**Route:** Music Reference Hub (Receipt ID `CS110-W8-MUSIC-865D79AF`)
**GitHub repository:** `<paste the tested repository URL here>`

A small Windows desktop music player (WPF / C#) for someone who wants to **find a song, play it, and keep notes and links about it** without a confusing interface.

---

## Start here: where to find each requirement

| Req | What it asks for | Where to look | Status |
|---|---|---|---|
| **R1** Define the user and utility | Story or client brief, user, problem, useful result, limits, privacy boundary, assumptions | `Evidence/8A/StoryBrief.md` (the brief). `Context.md` in the root (to add). | Brief included; add `Context.md` |
| **R2** Prototype and map the events | Interface prototype, whiteboard logic chart (input, event, method, data, output, failure, recovery), one working event | `Evidence/8A/Whiteboard 7a.jpg` (UI sketch and logic chart). `Evidence/8A/Discord-Peer-Review.png` (whiteboard next to the running app). Working event: the audio **Load, Play** path in `MusicReferenceHub/MainWindow.xaml.cs`. | Included |
| **R3** Build the selected utility | Three core functions, named event handlers, focused methods, a collection, visible status | `MusicReferenceHub/` (the WPF project). See "The three core functions" below. | Included |
| **R4** Validate, recover and save text | Reject blank or unsafe input, prove recovery, export and reopen one plain-text record | Code: `AddLink_Click`, `Save_Click`, `Open_Click`, `Media_Failed` in `MainWindow.xaml.cs`. Test proof: `TestNotes.md` in the root (to add). | Code included; add `TestNotes.md` |
| **R5** Keep the two class records | 8A and 8B whiteboards, lecture notes, collaboration, Discord records, mandatory AI test, screenshots, Git checkpoints | `Evidence/8A/` and `Evidence/8B/` (details below). Collaboration: `CS110-Week8-Team-Lead-Contract.txt` and `ActionLogTaskList.md`. Git checkpoints: the commit history on GitHub. | Included; see gaps below |
| **R6** Document one reusable pattern | `README.md` and `PATTERN.md` | This file and `PATTERN.md` in the root. Pattern: a timer-synced slider with guard flags. | Included |
| **R7** Submit the tested GitHub link | Week 8 receipt, final source and evidence on GitHub, instructor access tested, URL submitted in Canvas | Receipt: `CS110-W8-MUSIC-Route Selection.pdf` (root, and a copy in `Evidence/8A/`). Final receipt and peer check go in `Evidence/Final/` (to add). Repository URL is at the top of this file. | Add `Evidence/Final/` |

---

## Folder map

```text
Week8/                                   <- repository root
├── README.md                            <- you are here
├── PATTERN.md                           <- R6: the reusable pattern
├── ActionLogTaskList.md                 <- plan, phases, who does what
├── CS110-Week8-Team-Lead-Contract.txt   <- team and Team Lead contract
├── CS110-W8-MUSIC-Route Selection.pdf   <- route receipt (R7)
├── MusicReferenceHub-Release.zip        <- ready-to-run build (see "Run it")
├── Sample Music/                        <- one sample WAV for testing
├── Evidence/
│   ├── 8A/                              <- Action 8A records (R1, R2, R5)
│   │   ├── StoryBrief.md                   user, problem, result, limits, privacy, assumption
│   │   ├── Whiteboard 7a.jpg               UI sketch + Input/Event/Action/Data/Output/Failure chart
│   │   ├── Discord-Peer-Review.png         whiteboard and running app shared on Discord
│   │   └── CS110-W8-MUSIC-Route Selection.pdf   route receipt copy
│   └── 8B/                              <- Action 8B records (R5)
│       ├── AI Check Prompt.png             mandatory AI prompt and first reply
│       ├── AI Check.png                    AI answer: change, file, method, one test
│       ├── Evidence 8B.png                 whiteboard: classes/objects and what the team still needs
│       ├── Evidence 8B (2).png             whiteboard: class and method structure
│       ├── Evidence 8B (3).png             whiteboard: areas to improve / stuck on
│       ├── Evidence 8B (4).png             team whiteboard (Awayzo): reflections
│       └── Evidence 8B (5).png             lecture notes, Weeks 1 to 8
└── MusicReferenceHub/                   <- the Visual Studio project (source code only)
    ├── MusicReferenceHub.sln / .csproj
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml                     the interface
    ├── MainWindow.xaml.cs                  event handlers and methods
    ├── AudioEngine.cs                      playback, volume, equalizer, spectrum (NAudio)
    ├── WinampControls.cs                   custom controls (LED time display, etc.)
    ├── Evidence/README.txt                 note pointing to the evidence list
    └── Assets/
        ├── Media/                          course audio tracks (Kevin MacLeod)
        └── Images/                         cover art and button icon (add)
```

The main evidence lives in the **root `Evidence/` folder**. The `Evidence/` folder inside `MusicReferenceHub/` is only the starter's placeholder.

---

## Run it

**Needs:** Windows, Visual Studio 2022 with the ".NET desktop development" workload, .NET 8.

1. Open `MusicReferenceHub/MusicReferenceHub.sln` in Visual Studio.
2. Let it restore the NuGet package **NAudio 2.2.1** (automatic).
3. Press **F5**.

**No Visual Studio?** Unzip `MusicReferenceHub-Release.zip` and run `MusicReferenceHub.exe` (needs the .NET 8 Desktop Runtime).

**Try it in 20 seconds:** click **+ ADD**, pick a song from `MusicReferenceHub/Assets/Media/` (or `Sample Music/`), and it appears in the list. Double-click it to play.

---

## The user and the problem (R1 summary)

- **User:** someone who wants to play music without a confusing interface or extra features.
- **Problem:** finding and playing a song is harder than it should be.
- **Useful result:** select a song, play or pause it, and keep a few notes and links about it.
- **Limits:** basic playback and simple reference tools only; notes are plain text.
- **Privacy boundary:** the app does not collect personal information or track listening. It uses only the files, links and notes the user enters, and saves nothing unless the user exports it.
- **Assumption to test:** a new user can open the player and play a song with no instructions. Full text: `Evidence/8A/StoryBrief.md`.

---

## The three core functions (R3)

All code is in `MusicReferenceHub/MainWindow.xaml.cs`. Each function uses named event handlers, a collection, and the status line (`txtStatus`) at the bottom of the window.

| Core function | What the user does | Main handlers and methods | Collection |
|---|---|---|---|
| **1. Open and control a local audio file** | Add songs, play, pause, stop, next, previous, shuffle, repeat, seek, volume | `Load_Click`, `AddFiles`, `Play_Click`, `Pause_Click`, `Stop_Click`, `PlayIndex`, `PlayNext`, `PlayPrev`, `Shuffle_Click`, `Repeat_Click`, `Media_Opened`, `Media_Ended`, `Media_Failed` | `_playlist` (a `List<string>`) shown in `lstPlaylist` |
| **2. Store and open reference links** | Type a web address, add it, select it, open it in the browser | `AddLink_Click`, `OpenLink_Click` | `lstLinks` |
| **3. Export and reopen session notes** | Type notes, export to a `.txt` file, reopen it later | `Save_Click` (export), `Open_Click` (reopen) | notes box `txtNotes` |

Also included: artwork picker (`Image_Click`), drag-and-drop of audio files onto the list, keyboard shortcuts (Z previous, X play, C pause, V stop, B next, arrow keys seek and volume), skins (right-click menu), a 10-band equalizer, and a spectrum or oscilloscope display.

---

## Validation and recovery (R4)

| Situation | What happens | Where |
|---|---|---|
| Link box is blank | Rejected; status says "Enter a URL to add." | `AddLink_Click` |
| Link has no `http://` or `https://` | `https://` is added automatically | `AddLink_Click` |
| Open link with nothing selected | Status says "No link selected." | `OpenLink_Click` |
| File can't be decoded or sound device fails | Playback stops cleanly and the status line suggests MP3, WAV, WMA, AAC or M4A | `Media_Failed` |
| Press Play with no song loaded | Nothing happens; the app does not crash | `Play_Click` |
| Non-audio or missing files added | Skipped; status says "No audio files to add." | `AddFiles`, `Playlist_Drop` |
| Export or reopen dialog cancelled | Existing notes stay as they are; status says "Export cancelled." or "Reopen cancelled." | `Save_Click`, `Open_Click` |
| Any file, link or save error | Caught by `try/catch` and shown in the status line | every handler above |

**Export and reopen:** write notes, click **EXPORT NOTES**, save `notes.txt`, clear the box, click **REOPEN TEXT**, choose the file. The same text returns. Step-by-step test records belong in `TestNotes.md`.

---

## Class records (R5)

| Record | File |
|---|---|
| 8A whiteboard (UI sketch and logic chart) | `Evidence/8A/Whiteboard 7a.jpg` |
| 8A Discord record | `Evidence/8A/Discord-Peer-Review.png` |
| 8B whiteboards | `Evidence/8B/Evidence 8B.png`, `(2)`, `(3)`, `(4)` |
| Lecture notes | `Evidence/8B/Evidence 8B (5).png` |
| Mandatory AI test (prompt, answer, test) | `Evidence/8B/AI Check Prompt.png`, `Evidence/8B/AI Check.png` |
| Collaboration | `CS110-Week8-Team-Lead-Contract.txt`, `ActionLogTaskList.md` |
| Git checkpoints | Commit history on GitHub |

---

## The reusable pattern (R6)

**Timer-synced slider with guard flags.** A timer moves the seek slider as the song plays, and two flags stop the slider's own event from mistaking the timer's updates for the user dragging it. Where it appears, why it works and how to reuse it: see `PATTERN.md`.

---

## Known limits

- Notes are plain text only; nothing is saved automatically.
- The playlist and links are not kept after the app closes.
- Link checking is basic: blank entries are rejected and `https://` is added when missing, but the address is not otherwise verified. A bad address fails safely when opened and the error appears in the status line.
- Export writes whatever is in the notes box, including an empty box.
- Windows only (WPF).

## Next steps

- Check links with `Uri.TryCreate` and allow only `http` and `https`.
- Remember the playlist and links between sessions.
- Ask the notes export to confirm before saving an empty file.

## Credits

Audio in `MusicReferenceHub/Assets/Media/` and `Sample Music/` is by Kevin MacLeod. Keep the source and license for every audio file and image in `Evidence/image-sources.txt`. Audio playback uses the NAudio library.
