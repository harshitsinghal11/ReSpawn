## Development Phases
---
### Phase 1 - Project Setup

> Goal: A working, runnable WPF shell with no features — just the skeleton, configuration, and folder structure committed and verified.

**Environment & Tooling**

- [ ] Install Visual Studio 2022 (Community or higher) with the `.NET desktop development` workload
- [ ] Install .NET 8 SDK — verify with `dotnet --version`
- [ ] Install Git — configure user name and email
- [ ] Create GitHub repository: `ReSpawn` (public or private)
- [ ] Add `.gitignore` for Visual Studio / C# projects (`github/gitignore` → VisualStudio)
- [ ] Add `MIT LICENSE` file to repo root

**Solution & Project Bootstrap**

- [ ] Create new Visual Studio solution: `ReSpawn.sln`
- [ ] Add WPF App project: `ReSpawn`, target `net8.0-windows`, `OutputType = WinExe`
- [ ] Add xUnit test project: `ReSpawn.Tests`, target `net8.0`
- [ ] Add project reference: `ReSpawn.Tests` → `ReSpawn`
- [ ] Install NuGet package: `Hardcodet.NotifyIcon.Wpf` (1.1.0) into `ReSpawn`
- [ ] Install NuGet packages into `ReSpawn.Tests`: `xunit`, `xunit.runner.visualstudio`, `Moq`

**Folder Structure**

- [ ] Create all folders from the File Structure section: `Assets/`, `Models/`, `Services/`, `ViewModels/`, `Views/`, `Controls/`, `Helpers/`, `Tests/TestData/`
- [ ] Add placeholder `.gitkeep` files in empty folders so they commit to Git

**Constants & Configuration**

- [ ] Create `Constants.cs` with initial values:
  ```csharp
  public static class Constants
  {
      public const int    PollIntervalMs    = 5000;
      public const int    MinSessionSeconds = 60;
      public const int    MaxSessionHours   = 12;
      public const string AppDataFolder     = "ReSpawn";
      public const string GamesFileName     = "games.json";
      public const string IconsFolder       = "icons";
  }
  ```

**Assets**

- [ ] Add placeholder `ReSpawn.ico` (use any 256x256 icon for now)
- [ ] Add placeholder `tray-icon.ico` (16x16 minimum)
- [ ] Add `default-icon.png` (128x128 generic game controller placeholder)
- [ ] Set all assets' Build Action to `Resource` in project properties

**AppData Path Helper**

- [ ] Create `Helpers/AppDataHelper.cs`:
  - [ ] `GetAppDataPath()` → `%AppData%\ReSpawn\`
  - [ ] `GetIconsPath()` → `%AppData%\ReSpawn\icons\`
  - [ ] `GetGamesFilePath()` → `%AppData%\ReSpawn\games.json`
  - [ ] `EnsureDirectoriesExist()` — creates all folders if missing (call on startup)

**Base Window**

- [ ] Build `MainWindow.xaml` — basic frameless window (no native chrome):
  - [ ] `WindowStyle="None"` + `AllowsTransparency="False"` + custom title bar strip
  - [ ] Drag region: `MouseLeftButtonDown` → `DragMove()`
  - [ ] Custom minimize `[—]`, maximize `[□]`, close `[×]` buttons
  - [ ] Override `OnClosing` → hide to tray (logic wired in Phase 6)

**Verify**

- [ ] Run the application — blank window opens, no crash
- [ ] Run the test project — 0 tests, 0 failures
- [ ] Commit: `"chore: initial project scaffold"`

---

### Phase 2 - Data Layer

> Goal: A fully tested `GameService` that reads, writes, and manages `games.json` with all edge cases handled. No UI involved.

**Models**

- [ ] Create `Models/GameSession.cs`:
  ```csharp
  public class GameSession
  {
      public string Id                { get; set; } = Guid.NewGuid().ToString();
      public DateTime Start           { get; set; }
      public DateTime End             { get; set; }
      public long DurationSeconds     { get; set; }
  }
  ```
- [ ] Create `Models/Game.cs`:
  ```csharp
  public class Game
  {
      public string           Id                   { get; set; } = Guid.NewGuid().ToString();
      public string           Name                 { get; set; } = string.Empty;
      public string           ExePath              { get; set; } = string.Empty;
      public string           ProcessName          { get; set; } = string.Empty;
      public string           IconPath             { get; set; } = string.Empty;
      public long             TotalPlaytimeSeconds { get; set; } = 0;
      public DateTime?        LastPlayed           { get; set; }
      public bool             IsPlaying            { get; set; } = false;
      public List<GameSession> Sessions            { get; set; } = new();
  }
  ```

**Helpers**

- [ ] Create `Helpers/AtomicFileWriter.cs`:
  - [ ] `WriteAllText(string path, string content)` — writes to `path.tmp`, then `File.Replace(path.tmp, path, path.bak)` for atomic swap
- [ ] Create `Helpers/TimeFormatter.cs`:
  - [ ] `FormatPlaytime(long totalSeconds)` → `"0h 05m"`, `"1h 00m"`, `"45h 30m"`
  - [ ] `FormatLastPlayed(DateTime? lastPlayed)` → `"Never"`, `"Today"`, `"Yesterday"`, `"3 days ago"`, `"Dec 15, 2025"` (beyond 7 days)
  - [ ] All methods are `static` and pure (no side effects)

**GameService**

- [ ] Create `Services/GameService.cs`:
  - [ ] `List<Game> LoadGames()` — reads `games.json`; if missing, returns empty list; if corrupt, backs up and returns empty list with a flag
  - [ ] `void SaveGames(List<Game> games)` — serializes with `JsonSerializerOptions { WriteIndented = true }`, calls `AtomicFileWriter.WriteAllText()`
  - [ ] `void AddGame(Game game)` — assigns `Guid.NewGuid()` if `game.Id` is empty, loads, appends, saves
  - [ ] `void UpdateGame(string id, Action<Game> mutate)` — loads, finds by `Id`, applies `mutate`, saves; throws `KeyNotFoundException` if not found
  - [ ] `void RemoveGame(string id)` — loads, removes matching entry, deletes `iconPath` file if it exists, saves
  - [ ] `void AppendSession(string gameId, GameSession session)` — loads, finds game, appends session, increments `TotalPlaytimeSeconds`, updates `LastPlayed`, sets `IsPlaying = false`, saves

**Unit Tests — GameService**

- [ ] `LoadGames_WhenFileNotFound_ReturnsEmptyList()`
- [ ] `LoadGames_WhenFileIsCorrupt_ReturnsEmptyListAndCreatesBackup()`
- [ ] `AddGame_AssignsGuidAndPersists()`
- [ ] `AddGame_DoesNotOverwriteExistingGuid()`
- [ ] `RemoveGame_RemovesCorrectEntry()`
- [ ] `RemoveGame_WithUnknownId_ThrowsKeyNotFoundException()`
- [ ] `AppendSession_UpdatesTotalPlaytimeAndLastPlayed()`
- [ ] `AppendSession_AppendsToSessionsArray()`
- [ ] `SaveGames_WritesValidJson()`
- [ ] `SaveGames_IsAtomic_DoesNotLeavePartialFile()`

**Unit Tests — TimeFormatter**

- [ ] `FormatPlaytime_ZeroSeconds_ReturnsZeroHoursZeroMinutes()`
- [ ] `FormatPlaytime_90Seconds_ReturnsZeroHoursOneMinute()`
- [ ] `FormatPlaytime_LargeValue_ReturnsCorrectHoursAndMinutes()`
- [ ] `FormatLastPlayed_Null_ReturnsNever()`
- [ ] `FormatLastPlayed_Today_ReturnsToday()`
- [ ] `FormatLastPlayed_Yesterday_ReturnsYesterday()`
- [ ] `FormatLastPlayed_OldDate_ReturnsFormattedDate()`

**Verify**

- [ ] All unit tests pass
- [ ] Manually inspect `games.json` written to `%AppData%\ReSpawn\` — confirm valid JSON
- [ ] Commit: `"feat: data layer — GameService, models, formatters, atomic writes"`

---

### Phase 3 - Game Launcher UI

> Goal: Users can see their game library, add games, launch them, edit, and remove them. No tracking yet.

**ViewModels**

- [ ] Create `Helpers/RelayCommand.cs` — standard `ICommand` wrapper:
  - [ ] Constructor: `RelayCommand(Action execute, Func<bool>? canExecute = null)`
  - [ ] `RelayCommand<T>` generic variant for parameterized commands
- [ ] Create `ViewModels/GameTileViewModel.cs`:
  - [ ] Exposes: `Id`, `Name`, `IconPath`, `FormattedPlaytime`, `FormattedLastPlayed`, `IsPlaying`
  - [ ] Implements `INotifyPropertyChanged`
  - [ ] `IsPlaying` setter triggers `OnPropertyChanged("StatusText")` and `OnPropertyChanged("BadgeVisible")`
- [ ] Create `ViewModels/MainViewModel.cs`:
  - [ ] `ObservableCollection<GameTileViewModel> Games` — bound to the tile grid
  - [ ] `ICommand AddGameCommand` — opens `AddGameDialog`
  - [ ] `ICommand LaunchGameCommand` — `Process.Start(game.ExePath)`; shows error if file missing
  - [ ] `ICommand EditGameCommand` — opens `EditGameDialog`
  - [ ] `ICommand RemoveGameCommand` — shows confirmation, calls `GameService.RemoveGame()`
  - [ ] `bool IsEmpty` — `true` when `Games.Count == 0`, drives empty state visibility
  - [ ] `LoadGamesFromDisk()` — called on startup; populates `Games` from `GameService.LoadGames()`
- [ ] Create `ViewModels/AddGameViewModel.cs`:
  - [ ] `ICommand BrowseCommand` — opens `OpenFileDialog(Filter = "Executables|*.exe")`
  - [ ] After file selection: auto-populate `Name`, `ProcessName`, trigger icon extraction preview
  - [ ] `ICommand ConfirmCommand` — validates fields, calls `GameService.AddGame()`, closes dialog
  - [ ] `ICommand CancelCommand` — discards, closes dialog
  - [ ] Input validation: `Name` and `ProcessName` must be non-empty; `ExePath` must exist on disk
- [ ] Create `ViewModels/EditGameViewModel.cs`:
  - [ ] Same shape as `AddGameViewModel` but pre-populated with existing `Game` data
  - [ ] `ConfirmCommand` calls `GameService.UpdateGame()` instead of `AddGame()`

**Controls**

- [ ] Build `Controls/GameTileControl.xaml`:
  - [ ] 160×200px card layout
  - [ ] Top 60%: game icon centered (fallback to `default-icon.png`)
  - [ ] Bottom 40%: name (bold, truncated with ellipsis), playtime, last played / "Now Playing" badge
  - [ ] Hover state: subtle scale transform (1.0 → 1.03) + drop shadow elevation — WPF `Trigger` on `IsMouseOver`
  - [ ] `IsPlaying = true` state: green pulsing dot `●` with a WPF `DoubleAnimation` on opacity (1.0 ↔ 0.3, 1s infinite)
  - [ ] Context menu: Launch, Edit, Remove (with separator before Remove)
  - [ ] Double-click fires `LaunchGameCommand`

**Views**

- [ ] Build `Views/HomeView.xaml`:
  - [ ] Header bar: "My Library" title + "+ Add Game" button (right-aligned)
  - [ ] `WrapPanel` inside `ScrollViewer` — tiles wrap naturally as window resizes
  - [ ] Empty state overlay: icon + "No games yet" message + "Add your first game" button
  - [ ] Empty state visibility bound to `MainViewModel.IsEmpty`
- [ ] Build `Views/AddGameDialog.xaml`:
  - [ ] Browse button + selected path display (read-only text box)
  - [ ] Extracted icon preview (64×64)
  - [ ] Editable `Name` field
  - [ ] Editable `ProcessName` field with helper tooltip: "This is the process name Windows uses. Usually the filename without .exe."
  - [ ] Add / Cancel buttons; Add disabled until validation passes
- [ ] Build `Views/EditGameDialog.xaml` — identical structure to Add, pre-populated

**Launch Error Handling**

- [ ] `Helpers/PathValidator.cs` — `bool Exists(string exePath)` wraps `File.Exists()`
- [ ] In `MainViewModel.LaunchGameCommand`: if path invalid, show `MessageBox` with "Game file not found at [path]. Would you like to re-link it?" — Yes opens `EditGameDialog`

**Verify**

- [ ] Add a game → tile appears on home screen
- [ ] Double-click tile → game process launches
- [ ] Right-click → Remove → game disappears
- [ ] Right-click → Edit → save new name → tile updates
- [ ] 0 games → empty state shown
- [ ] Commit: `"feat: game launcher UI — home screen, add/edit/remove, launch"`

---

### Phase 4 - Background Process Tracker

> Goal: Games are tracked automatically the moment their process is detected, regardless of launch source.

**ProcessMonitor Service**

- [ ] Create `Services/ProcessMonitor.cs`:
  - [ ] Constructor: accepts `GameService` instance (injected)
  - [ ] `void Start()` — initializes `System.Threading.Timer` with `Constants.PollIntervalMs`
  - [ ] `void Stop()` — disposes timer, saves any in-flight sessions cleanly
  - [ ] `Dictionary<string, DateTime> _activeSessions` — key: `processName`, value: `sessionStart` UTC
  - [ ] `void OnTick(object? state)` — the poll callback:
    1. Call `Process.GetProcesses()` — get array of all running process names (lowercased)
    2. Load current games list from `GameService.LoadGames()`
    3. For each game: compare `game.ProcessName.ToLower()` against the process name set
    4. **IDLE → PLAYING**: process found, game not in `_activeSessions` → add to dict, call `GameService.UpdateGame()` to set `IsPlaying = true`, raise `GameStarted` event
    5. **PLAYING → IDLE**: game in `_activeSessions`, process not found → compute duration, call `HandleSessionEnd()`
  - [ ] `void HandleSessionEnd(Game game, DateTime sessionStart)`:
    - [ ] `durationSeconds = (DateTime.UtcNow - sessionStart).TotalSeconds`
    - [ ] Remove from `_activeSessions`
    - [ ] If `durationSeconds < Constants.MinSessionSeconds` → log discard, call `GameService.UpdateGame()` to clear `IsPlaying`, return
    - [ ] If `durationSeconds > Constants.MaxSessionHours * 3600` → cap to max, log warning
    - [ ] Build `GameSession` with `Guid.NewGuid()`, start, end, duration
    - [ ] Call `GameService.AppendSession(game.Id, session)`
    - [ ] Raise `GameStopped` event with the completed session
  - [ ] `event EventHandler<GameStatusEventArgs> GameStarted`
  - [ ] `event EventHandler<GameStatusEventArgs> GameStopped`

**Orphan Cleanup**

- [ ] In `ProcessMonitor.Start()`, before the timer begins:
  - [ ] Load all games
  - [ ] Find any with `IsPlaying == true`
  - [ ] For each: call `GameService.UpdateGame(id, g => g.IsPlaying = false)`
  - [ ] Log each cleanup: `[Startup] Cleared orphaned session for {game.Name}`

**ViewModel Integration**

- [ ] In `MainViewModel`: subscribe to `ProcessMonitor.GameStarted` and `ProcessMonitor.GameStopped`
- [ ] On `GameStarted`: find matching `GameTileViewModel` by game ID, set `IsPlaying = true` (dispatched to UI thread via `Application.Current.Dispatcher.Invoke`)
- [ ] On `GameStopped`: set `IsPlaying = false`, update `FormattedPlaytime` and `FormattedLastPlayed`

**Unit Tests — ProcessMonitor**

- [ ] `OnTick_WhenProcessFound_StartsSession()`
- [ ] `OnTick_WhenProcessAlreadyTracked_DoesNotDuplicate()`
- [ ] `OnTick_WhenProcessDisappears_SavesSession()`
- [ ] `OnTick_WhenSessionUnder60s_DiscardsSession()`
- [ ] `OnTick_WhenSessionOverMaxHours_CapsAtMaxHours()`
- [ ] `Start_CleansOrphanedSessions()`
- [ ] `HandleSessionEnd_RaisesGameStoppedEvent()`

**Verify**

- [ ] Start a registered game from Steam → ReSpawn tile shows "Now Playing"
- [ ] Close the game → tile returns to normal, `games.json` contains a new session entry
- [ ] Start and immediately close game (< 60s) → no session saved, playtime unchanged
- [ ] Force-kill ReSpawn while game is running → reopen → `isPlaying` is `false`, no bad session
- [ ] Commit: `"feat: background process tracker with session recording and orphan cleanup"`

---

### Phase 5 - Icon Extraction Service

> Goal: Every game shows its real icon automatically. The Add Game flow never blocks or fails due to icon issues.

- [ ] Create `Services/IconExtractor.cs`:
  - [ ] `string Extract(string exePath, string gameId)`:
    1. Build output path: `Path.Combine(AppDataHelper.GetIconsPath(), $"{gameId}.png")`
    2. Call `Icon.ExtractAssociatedIcon(exePath)` — requires `using System.Drawing`
    3. Call `icon.ToBitmap()` → `bitmap.Save(outputPath, ImageFormat.Png)`
    4. Return `outputPath` (relative to AppData)
    5. Wrap in `try/catch(Exception)` — on any failure, return `"Assets/default-icon.png"` fallback path
    6. Log extraction failure to `Debug.WriteLine`
  - [ ] `void DeleteIcon(string iconPath)` — safe delete (catch if file not found)
  - [ ] `bool IconExists(string iconPath)` — wraps `File.Exists()`
- [ ] Wire `IconExtractor` into `AddGameViewModel.BrowseCommand` — extract preview icon immediately after `.exe` selection, before user confirms
- [ ] Wire `IconExtractor` into `GameService.RemoveGame()` — delete icon file when game is removed
- [ ] In `GameTileControl.xaml`: bind `Image.Source` to `IconPath`; add value converter `StringToImageSourceConverter` that falls back to `default-icon.png` if the file path resolves to nothing
- [ ] Create `Helpers/StringToImageSourceConverter.cs` — `IValueConverter` for WPF image binding with fallback

**Verify**

- [ ] Add PUBG → tile shows PUBG's real icon
- [ ] Add a game with no embedded icon → tile shows `default-icon.png`, no crash
- [ ] Remove a game → confirm its icon file is deleted from `%AppData%\ReSpawn\icons\`
- [ ] Commit: `"feat: icon extraction service with fallback and cleanup"`

---

### Phase 6 - System Tray Integration

> Goal: The app runs silently in the background. Window close hides to tray; tracker keeps running.

- [ ] Add `<tb:TaskbarIcon>` resource to `App.xaml` using `Hardcodet.NotifyIcon.Wpf`
- [ ] Set tray icon source to `Assets/tray-icon.ico`
- [ ] Build tray context menu in XAML with `WPF MenuItem` bindings:
  - [ ] **Open ReSpawn** → `Application.Current.MainWindow.Show(); MainWindow.Activate();`
  - [ ] Separator
  - [ ] **[Status Line]** — bound to `MainViewModel.TrayStatusText`:
    - [ ] `"● Now Playing: PUBG"` when a game is active
    - [ ] `"No game running"` when idle
    - [ ] This item is non-clickable (used as a display label)
  - [ ] Separator
  - [ ] **Exit** → `Application.Current.Shutdown()`
- [ ] Override `MainWindow.OnClosing`:
  - [ ] Set `e.Cancel = true` to prevent actual close
  - [ ] Call `this.Hide()`
  - [ ] Show a one-time balloon tip: "ReSpawn is still running. Right-click the tray icon to exit." (show only on first minimize, controlled by a `bool _shownTrayHint` flag)
- [ ] Wire tray icon `TrayMouseDoubleClick` → `MainWindow.Show(); Activate()`
- [ ] Add `MainViewModel.TrayStatusText` property:
  - [ ] Subscribe to `ProcessMonitor.GameStarted` → update to `"● Now Playing: {gameName}"`
  - [ ] Subscribe to `ProcessMonitor.GameStopped` → update to `"No game running"`
- [ ] Ensure `ProcessMonitor` is **not stopped** when window hides — it must keep running
- [ ] On `Application.Exit` (triggered by tray "Exit"): call `ProcessMonitor.Stop()` before shutdown to flush any in-flight session

**Verify**

- [ ] Click window close → window hides, tray icon appears, balloon shows once
- [ ] Double-click tray icon → window restores
- [ ] Launch a game → tray icon tooltip and menu status line updates to "Now Playing"
- [ ] Click tray Exit → app fully closes, session saved if game was running
- [ ] Commit: `"feat: system tray integration — hide to tray, live status, clean shutdown"`

---

### Phase 7 - Polish, Edge Cases & Release

> Goal: Production-quality build. All edge cases handled. Installer ready. Zero known crashes.

**Edge Case Hardening**

- [ ] **Broken exe path**: `PathValidator` check in `MainViewModel`; tile shows warning badge + "File not found" tooltip; right-click offers "Re-link executable"
- [ ] **Duplicate processName**: In `AddGameViewModel.ConfirmCommand` — check if any existing game shares `ProcessName` (case-insensitive); show warning dialog, require explicit confirmation to proceed
- [ ] **Corrupt `games.json`**: Covered in Phase 2; add integration test using `TestData/corrupt_games.json`
- [ ] **Hibernate/sleep cap**: In `HandleSessionEnd`, `durationSeconds > Constants.MaxSessionHours * 3600` → cap and log
- [ ] **Multiple instances of same process**: `Process.GetProcessesByName()` returns an array — presence of ≥ 1 instance counts as running; quantity does not affect session count
- [ ] **Icon file deleted manually**: `StringToImageSourceConverter` falls back to `default-icon.png` at render time
- [ ] **`games.json` write fails** (disk full, permissions): `AtomicFileWriter` wraps in `try/catch`, logs error with details, shows non-blocking error toast

**UI Polish**

- [ ] Smooth tile entrance animation: tiles fade in on home screen load (`OpacityAnimation` 0→1 staggered per tile)
- [ ] Hover elevation: scale 1.0→1.03 + shadow depth 2→6 on `GameTileControl` hover
- [ ] Session saved toast notification: `Hardcodet.NotifyIcon.Wpf` balloon → `"PUBG — 1h 35m recorded"`
- [ ] Loading skeleton: show 6 placeholder grey shimmer tiles while `games.json` loads on startup
- [ ] Window min-size constraint: `MinWidth="800" MinHeight="500"` — tiles always visible

**End-to-End Testing Checklist**

- [ ] Add game from Steam install dir → tile appears with correct icon, name, process name
- [ ] Launch game from ReSpawn → session tracked
- [ ] Launch same game from Steam → session still tracked by ReSpawn
- [ ] Close game → session appears in `games.json`, playtime on tile updates
- [ ] Short session (< 60s) → no session entry added
- [ ] Force-kill ReSpawn mid-game → reopen → tile shows correct playtime, no phantom session
- [ ] Remove game → tile gone, session history gone, icon file deleted
- [ ] Re-add same game → starts fresh with 0 playtime
- [ ] Corrupt `games.json` manually → reopen → backup created, fresh empty library shown
- [ ] Move game `.exe` to different path → tile shows "File not found" warning
- [ ] Open app with 50+ games → startup under 1s, no lag scrolling the grid

**Performance Profiling**

- [ ] Profile memory at idle (tray only): confirm < 30 MB
- [ ] Profile poll tick duration: confirm < 50ms for 50 registered games
- [ ] Profile `games.json` load: confirm < 100ms for 500 games

**Build & Distribution**

- [ ] Update `AssemblyInfo.cs` or `.csproj` with version `0.1.0`, product name `ReSpawn`, copyright
- [ ] Test self-contained single-file publish:
  ```
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ```
- [ ] Verify published `.exe` runs on a clean Windows machine with no .NET installed
- [ ] Write NSIS `.nsi` installer script:
  - [ ] Install to `%ProgramFiles%\ReSpawn\`
  - [ ] Add Start Menu shortcut
  - [ ] Add Desktop shortcut (optional, prompted)
  - [ ] Register Windows startup entry (optional, prompted)
  - [ ] Uninstaller that removes `%ProgramFiles%\ReSpawn\` (but not `%AppData%\ReSpawn\` — preserve user data)
- [ ] Build installer → test install and uninstall on a clean VM
- [ ] Tag release `v0.1.0` in Git
- [ ] Create GitHub Release with attached installer `.exe` and changelog

**Commit:** `"release: v0.1.0 — MVP production build"`
