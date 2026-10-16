### Folder & Deliverable Mapping
```text
Week8/                                  <-- REPOSITORY ROOT
├── README.md                           <-- User, problem, controls, event flow, setup, tests, limits, next step
├── PATTERN.md                          <-- Pattern, where it appears, why it works, how to adapt it
├── Context.md                          <-- User, need, limits, privacy boundary, assumptions, SEEK/AAA
├── TestNotes.md                        <-- Normal, invalid, recovery, and export/reopen test logs
├── .gitignore                          <-- Ignores .vs/, [Bb]in/, [Oo]bj/, *.user, *.suo
├── Evidence/
│   ├── 8A/                             (selection receipt, whiteboard photo, prototype, trade-seat Discord proof)
│   ├── 8B/                             (whiteboard trace, AI prompt/response log, peer usability issue & fix, Discord)
│   ├── Final/                          (Week 8 route receipt, peer verification record)
│   └── image-sources.txt               (credits/permissions for images and audio)
└── MusicReferenceHub/                  <-- VS / C# WPF PROJECT ONLY
    ├── MusicReferenceHub.csproj
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs
    └── Assets/
        ├── Images/                     (cover art, button icon)
        └── Media/                      (course track, e.g., Voidrunner, The Relay)
```

---

## 1. Phase-by-Phase Workflow Checklist

### Phase 00 & 8A: Pre-Start, Foundation & First Working Action (Day 1)
* [ ] **01 / Pre-Start Setup**: Verify WPF .NET 8 desktop environment; create repo or branch; save the downloaded selection receipt into `Evidence/8A/`.
* [ ] **02 / Define Context & Boundaries (R1)**: Write `Context.md` detailing the user (real or fictional), problem, useful result, limits, privacy boundary, assumptions, and Attention First **SEEK & AAA** (First action visible, clear feedback, safe recovery).
* [ ] **03 / Prototype & Logic Map (R2)**:
  * Whiteboard the event flow: `Input` $\rightarrow$ `Event` $\rightarrow$ `Method` $\rightarrow$ `Data` $\rightarrow$ `Output` $\rightarrow$ `Failure / Recovery`.
  * Create digital interface prototype (PowerPoint / photo editor) before touching XAML.
* [ ] **04 / Raw Starter Commit**: Place starter in `MusicReferenceHub/`, add `.gitignore` at root, run to confirm raw shell opens, and commit: `"8A raw starter and layout"`.
* [ ] **05 / Build Shell & First Action (Audio Playback - ★ R3)**:
  * Connect `ObservableCollection<string> links`, `mediaReady`, and `isPlaying`.
  * Implement `LoadLocalMedia()` (`OpenFileDialog`), `Play_Click`, `Pause_Click`, `Stop_Click`, and `MediaElement` events (`MediaOpened`, `MediaFailed`, `MediaEnded`).
  * Verify visible status updates (`txtStatus.Text`).
* [ ] **06 / Peer Seat Trade (Portal Step 05)**: Have a peer sit at the computer and attempt the first action (Load/Play audio) **without coaching**. Capture their observation and post it to Discord.
* [ ] **07 / 8A Checkpoint & Submission (R5)**: Save whiteboard photo, prototype, trade-seat Discord screenshot, and notes into `Evidence/8A/`. Push `"8A audio checkpoint"` to GitHub and submit Action 8A.

---

### Phase 8B: Core Workflows, Validation, Recovery & AI Check (Day 2)
* [ ] **01 / Reopen & List Unknowns (Portal Step 01)**: Reopen 8A checkpoint. In `TestNotes.md`, document any blockers, bugs, or unknowns discovered during Day 1.
* [ ] **02 / Reference Links (★ R3)**:
  * Implement `AddLink()` using `Uri.TryCreate` validation (strictly allow `http` and `https`; reject random text/schemes).
  * Implement `OpenSelectedLink()` using `Process.Start` with `UseShellExecute = true` wrapped in `try/catch`.
* [ ] **03 / Notes Export & Reopen (★ R3, ★ R4)**:
  * Implement `SaveText(string text)` using `SaveFileDialog` and `File.WriteAllText`.
  * Implement `Open_Click()` using `OpenFileDialog` and `File.ReadAllText`.
  * Ensure cancellation keeps existing notes intact without loss.
* [ ] **04 / Artwork Loader (Visual Polish)**: Implement `Image_Click` using `BitmapImage` with `CacheOption = OnLoad` so files are not left locked.
* [ ] **05 / Required AI Review (Portal Step 05)**:
  * Use the exact prompt:
    > *"I am building a beginner C# WPF utility. My selected route is Music. My current unknown is [ONE precise unknown]. Explain the smallest useful change, name the file and method where it belongs, and give me one test. Do not rewrite the whole project. I will test the response and record what I kept, changed, or rejected."*
  * Execute the proposed test and log Prompt, Response, Test, and Keep/Change/Reject in `Evidence/8B/ai_test_record.md`.
* [ ] **06 / Peer Usability Test & Revision (Portal Step 06)**: Run a peer usability session. Identify **one issue/friction point**, revise the code/UI to fix it, and document the change.
* [ ] **07 / 8B Checkpoint & Submission (R5)**: Save logic trace, test logs, AI report, revised issue notes, and Discord proof to `Evidence/8B/`. Push `"8B complete hub checkpoint"` and submit Action 8B.

---

### Final Phase: Pattern Extraction, Packaging & Release Hand-Off
* [ ] **01 / Grade Prerequisites Check (Portal Step 01)**: Confirm both Action 8A and Action 8B have been accepted.
* [ ] **02 / Reusable Pattern (`PATTERN.md` - R6)**: Author `PATTERN.md` explaining one reusable concept (e.g., *Safe Audio Playback with Asynchronous Readiness*, *URL Validation & Safe Browser Launch*, or *Non-Locking Bitmap Loading*):
  * Where it appears in the codebase.
  * Why it works (technical mechanics).
  * How another developer would adapt it.
* [ ] **03 / Project Readme (`README.md` - R6)**: Finish all sections: User, Problem, Controls, Event Flow, Setup, Tests, Known Limits (e.g., plain-text notes only, no serialized persistence), and Next Steps.
* [ ] **04 / Clean Repository & Hygiene**: Ensure `bin/`, `obj/`, `.vs/`, and any personal credentials are completely removed. Verify no files exceed 100 MB.
* [ ] **05 / Final Push & Verification (★ R7)**: Push final release to GitHub. Have an external peer verify accessibility. Place route receipt in `Evidence/Final/`, and submit the repository URL through Canvas before the deadline.

---

## 2. Member-by-Member Master Checklist

### 💻 Lexxie — Lead Developer & Git Manager
* [ ] **Pre-Start**: Initialize project structure in `MusicReferenceHub/` and configure root `.gitignore`.
* [ ] **8A**: Implement the audio playback engine in `MainWindow.xaml.cs` (`LoadLocalMedia`, `Play_Click`, `Pause_Click`, `Stop_Click`, `Media_Opened`, `Media_Failed`, `Media_Ended`).
* [ ] **8A**: Push the initial commit and the `"8A audio checkpoint"`.
* [ ] **8B**: Wire up `AddLink()`, `OpenSelectedLink()`, `SaveText()`, `Open_Click()`, and `Image_Click()`.
* [ ] **8B**: Implement the fix for the **revised usability issue** discovered during the 8B peer review.
* [ ] **8B**: Push the `"8B complete hub checkpoint"`.
* [ ] **Final**: Clean all build artifacts (`bin/`, `obj/`, `.vs/`), verify clean Release build, check instructor permissions, and submit final GitHub URL.

---

### 🎨 Niko — Logic Architect & Visual Designer
* [ ] **8A**: Co-design the 8A whiteboard event map (`Input` $\rightarrow$ `Event` $\rightarrow$ `Method` $\rightarrow$ `Data` $\rightarrow$ `Output` $\rightarrow$ `Failure`).
* [ ] **8A**: Build the digital UI prototype (PowerPoint / photo editor) showing player controls, link list, notes box, and status strip.
* [ ] **8A**: Style and refine `MainWindow.xaml` (colors, typography, margins, and minimum window size `900x780`).
* [ ] **8B**: Draw the 8B whiteboard logic trace focusing on Link Validation (`Uri.TryCreate`) and File I/O safety.
* [ ] **8B**: Format the artwork button in XAML using `StackPanel` containing both `Image` (`imgButton`) and `TextBlock`.
* [ ] **Final**: Create the architectural flow diagram illustrating the chosen pattern for `PATTERN.md`.
* [ ] **Final**: Verify visual contrast and readable layouts at runtime.

---

### 📝 Paulina — Narrative, Requirements & Documentation Lead
* [ ] **Pre-Start**: Download and place the route selection receipt into `Evidence/8A/`.
* [ ] **8A**: Draft `Context.md` covering the user persona, need, limits, privacy boundaries, assumptions, and SEEK/AAA principles.
* [ ] **8A**: Update contextual subtitles in `MainWindow.xaml` to match the chosen user scenario.
* [ ] **8B**: Record Day 8A/8B lecture notes and log Day 1 unknowns at the top of `TestNotes.md`.
* [ ] **8B**: Required AI Check.
* [ ] **Final**: Write `PATTERN.md` detailing where the pattern appears, why it works, and how to adapt it.
* [ ] **Final**: Complete root `README.md` (User, Problem, Controls table, Event Flow, Setup, Known Limits, Next Step, Contributions).
* [ ] **Final**: Verify the Week 8 route receipt is saved in `Evidence/Final/`.

---

### 🔍 Rudy — QA Tester, Media Assets & AI Lead
* [ ] **8A**: Source approved course audio tracks (Voidrunner/The Relay) in `Assets/Media/` and test cover art in `Assets/Images/`. Document sources in `Evidence/image-sources.txt`.
* [ ] **8A**: Conduct the **Peer Seat-Trade Test**: observe a peer navigating the audio controls without coaching, note findings, and post to Discord (screenshot saved in `Evidence/8A/`).
* [ ] **8B**: Execute the test matrix in `TestNotes.md`:
  * Audio: play before load, cancel file dialog, invalid file format.
  * Links: blank string, invalid URL scheme, valid link opening browser.
  * Notes: export notes, cancel save, reopen notes, invalid file recovery.
* [ ] **8B**: Execute the **Required AI Check** using the exact portal prompt; document prompt, output, and Keep/Change/Reject decision in `Evidence/8B/ai_test_record.md`.
* [ ] **8B**: Facilitate the **Peer Usability Test**, document the identified issue, and coordinate with Lexxie to verify the revision.
* [ ] **Final**: Coordinate independent peer verification of the repository and record their confirmation in `Evidence/Final/peer_verification.md`.
