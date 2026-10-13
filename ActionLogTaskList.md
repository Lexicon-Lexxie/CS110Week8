### ⚠️ Critical Requirements & Rules
* **★ R3 (Build Core Hub)**, **★ R4 (Validate/Recover & Save)**, and **★ R7 (Submit Tested GitHub Link)** are **CRITICAL** (missing any drops the assignment to 1 point).
* **Prerequisite**: Both **Action 8A** and **Action 8B** records must be accepted on their respective class days.
* **Target Framework**: **.NET 8** WPF (Visual Studio 2022+).

---

### Folder & Deliverable Architecture
Following your team's standard structure (WPF source in its own folder, documentation and evidence at the root):

```text
Week8/                                  <-- REPOSITORY ROOT
├── README.md                           <-- User, purpose, setup, controls, limits, tests
├── PATTERN.md                          <-- Deep dive on 1 reusable architectural pattern
├── Context.md                          <-- User story, problem, boundaries, SEEK/AAA
├── TestNotes.md                        <-- Comprehensive test logs (normal, invalid, recovery)
├── .gitignore                          <-- Ignores .vs/, [Bb]in/, [Oo]bj/, *.user, *.suo
├── Evidence/
│   ├── 8A/                             (whiteboard photo, prototype, audio test screenshot, Discord)
│   ├── 8B/                             (whiteboard logic trace, AI check log, test screenshots, Discord)
│   ├── Final/                          (Week 8 route receipt, peer verification notes)
│   └── image-sources.txt               (credits for artwork and icons)
└── MusicReferenceHub/                  <-- VS / C# WPF PROJECT ONLY
    ├── MusicReferenceHub.csproj
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs
    └── Assets/
        ├── Images/                     (hub cover artwork, button icons)
        └── Media/                      (sample course track: e.g. Voidrunner, The Relay)
```

---

## 1. Phase-by-Phase Roadmap

### Phase 8A: Context, Prototype, Foundation & Audio Playback (Day 1)
* [ ] **01 / Pre-Start Context**: Define the intended user (student, sound designer, researcher), their friction point, boundaries, and assumptions in `Context.md`.
* [ ] **02 / Project Setup**: Extract starter into `MusicReferenceHub/`. Add `Assets/Images/`, `Assets/Media/`, and `Evidence/`. Verify initial build.
* [ ] **03–04 / XAML Interface**: Verify and customize `MainWindow.xaml` (Grid layout, MediaElement, list box, text inputs, buttons, and status bar).
* [ ] **05 / First Git Checkpoint**: Add `.gitignore` to repo root (`.vs/`, `bin/`, `obj/`). Commit: `"8A raw starter and layout"`.
* [ ] **06 / Data State**: In `MainWindow.xaml.cs`, add fields (`ObservableCollection<string> links`, `bool mediaReady`, `bool isPlaying`) and bind `lstLinks.ItemsSource = links` in constructor.
* [ ] **07 / First Working Action (Audio)**:
  * Implement `LoadLocalMedia()` (`OpenFileDialog`, filters: `*.mp3;*.wav;*.wma`).
  * Connect `Play_Click`, `Pause_Click`, `Stop_Click`, `Media_Opened`, `Media_Failed`, `Media_Ended`.
  * Guard all actions against unready media (`!mediaReady`) with status bar feedback.
* [ ] **8A In-Class Deliverables**: Whiteboard photo, visual prototype, test screenshots, and Discord peer proof saved in `Evidence/8A/`. Push checkpoint `"8A audio checkpoint"` and submit Action 8A.

---

### Phase 8B: Links, Session Notes, Artwork & AI Verification (Day 2)
* [ ] **08 / Reference Links (★ R3)**:
  * Implement `AddLink()` with `Uri.TryCreate` validation (allow only `http://` or `https://`).
  * Implement `OpenSelectedLink()` using `Process.Start` with `UseShellExecute = true` inside `try/catch`.
* [ ] **09 / Notes Export & Reopen (★ R3, ★ R4)**:
  * Implement `SaveText(string text)` using `SaveFileDialog` and `File.WriteAllText`.
  * Implement `Open_Click` using `OpenFileDialog` and `File.ReadAllText`.
  * Protect user work: cancellation must leave text intact; errors display in `txtStatus`.
* [ ] **10 / Custom Artwork Loader**:
  * Implement `Image_Click` with `OpenFileDialog` (`*.png;*.jpg;*.jpeg`).
  * Use `BitmapImage.BeginInit()`, `CacheOption = OnLoad`, `EndInit()` to load without file locking.
  * Update both `imgArtwork` and button `imgButton`.
* [ ] **11 / Testing & Mandatory AI Check (★ R4, R5)**:
  * Run boundary/cancellation suite (Play before load, cancel file dialog, invalid URL, blank notes).
  * Prompt AI using the required bounded prompt, run the test, and document Keep/Change/Reject in `Evidence/8B/`.
* [ ] **8B In-Class Deliverables**: Logic trace whiteboard, test logs in `TestNotes.md`, AI log, screenshots in `Evidence/8B/`. Push `"8B complete hub checkpoint"` and submit Action 8B.

---

### Phase Final: Documentation, Reusable Pattern & Submission (Week 8 Release)
* [ ] **Pattern Documentation (R6)**: Write `PATTERN.md` explaining one reusable concept from the app (e.g., *Safe Asynchronous Audio Playback*, *URL Validation & Shell Execution*, or *Non-Locking Bitmap Loading*).
* [ ] **Project Readme (R6)**: Finalize `README.md` with user persona, setup instructions, controls, code map, test results, limitations, and credits.
* [ ] **Peer Verification**: Have a classmate clone/extract, follow `README.md`, run all 3 core features, and record the test in `Evidence/Final/peer_verification.md`.
* [ ] **Canvas Hand-off (★ R7)**: Add Week 8 route receipt into `Evidence/Final/`, verify repository access permissions, push final commit, and submit GitHub link to Canvas.

---

## 2. Member-by-Member Master Checklist

### 💻 Lexxie — Lead Developer & Git Architecture
* **8A**: Initialize solution in `MusicReferenceHub/`, verify .NET 8 WPF targets, and ensure `.gitignore` excludes `bin/`, `obj/`, and `.vs/`.
* **8A**: Wire up `MainWindow.xaml.cs` fields (`links`, `mediaReady`, `isPlaying`) and the audio playback methods (`LoadLocalMedia`, `Play`, `Pause`, `Stop`, and media events).
* **8A**: Commit and push the **"8A audio checkpoint"** to GitHub.
* **8B**: Implement `AddLink()`, `OpenSelectedLink()`, `SaveText()`, and `Open_Click()` inside `MainWindow.xaml.cs`.
* **8B**: Implement the non-locking `BitmapImage` artwork loading routine in `Image_Click`.
* **8B**: Commit and push the **"8B complete hub checkpoint"** to GitHub.
* **Final**: Perform a clean **Release build** check and verify that Git does not track files >100 MB.
* **Final**: Test instructor repository access permissions and submit the repository URL.

---

### 🎨 Niko — Logic Architect & Interface Designer
* **8A**: Co-design the 8A user flow whiteboard: *Input $\rightarrow$ Event $\rightarrow$ Method $\rightarrow$ Data $\rightarrow$ Output $\rightarrow$ Status*.
* **8A**: Create digital UI prototype in PowerPoint/Figma reflecting player layout, reference list, and session notes box.
* **8A**: Customize `MainWindow.xaml` colors, margins, titles, and layout to match the chosen user theme.
* **8B**: Draw the 8B Whiteboard Logic Trace focusing on Link Validation (`Uri.TryCreate`) and Text Export (`SaveFileDialog` $\rightarrow$ `WriteAllText`).
* **8B**: Design and format the button layout for the image-picker button (`StackPanel` with `Image` + `TextBlock`).
* **Final**: Diagram the architecture flowchart for `PATTERN.md` visualizing the chosen reusable pattern.
* **Final**: Assist in testing UI readability and contrast under minimum window dimensions (900x780).

---

### 📝 Paulina — Narrative & Documentation Lead
* **8A**: Write `Context.md`: define the user persona, need, assumptions, SEEK principles (first action visible), and AAA recovery rules.
* **8A**: Write the app title and contextual subtitle in `MainWindow.xaml`.
* **8B**: Record Day 8A/8B lecture notes and organize screenshots into `Evidence/8A/` and `Evidence/8B/`.
* **Final**: Author the master `README.md` (User, Problem, How to run, Controls table, Limitations, Member contributions).
* **Final**: Write `PATTERN.md` detailing the mechanics, code snippet, and reuse steps for one selected feature pattern.
* **Final**: Verify the Week 8 route receipt is properly archived in `Evidence/Final/`.

---

### 🔍 Rudy — QA Tester, Media Assets & AI Lead
* **8A**: Acquire and optimize local audio tracks (Voidrunner/The Relay or approved audio) in `MusicReferenceHub/Assets/Media/` and test cover art in `Assets/Images/`.
* **8A**: Conduct 8A boundary tests (Play before loading, cancel file selection, Pause/Stop behavior) and log results.
* **8B**: Execute the full 8B test matrix:
  * Link tests: blank URL, invalid scheme (ftp/file), valid `http`/`https`, open without selecting.
  * Notes tests: save note, cancel save dialog, reopen note, attempt corrupt file read.
  * Artwork tests: cancel image picker, unsupported format recovery.
* **8B**: Execute the **Required AI Verification Prompt**:
  > *"I am building Music Reference Hub in C# WPF. My unknown is [chosen question, e.g., handling non-standard URL schemes in Process.Start]. Explain the smallest useful change, the exact file and method, and one test..."*
  * Document Prompt, Response, Test, and Keep/Change/Reject decision in `Evidence/8B/ai_test_record.md`.
* **Final**: Document all media and image permissions in `Evidence/image-sources.txt`.
* **Final**: Coordinate with an external classmate for the independent peer verification review.
