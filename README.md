# Shared

Common code shared across my Unity projects, packaged as a Unity package (`com.adrian.shared`).

## Use in a project

As an editable submodule (embedded package):

```bash
git submodule add https://github.com/Plapi/shared.git Packages/com.adrian.shared
```

Read-only, via the Package Manager (`Packages/manifest.json`):

```json
"com.adrian.shared": "https://github.com/Plapi/shared.git#v0.1.0"
```

## Layout

- `Runtime/` – game code (assembly `Shared`), one folder per module, namespace `com.Plapamaru.Shared.<Module>`
- `Editor/` – editor-only code (assembly `Shared.Editor`), same module folders and namespaces

## Modules

- `Singletons` – `Singleton<T>`, `MonoBehaviourSingleton<T>`, `ScriptableObjectSingleton<T>`
- `Utilities` – `Bezier`, `Chaikin`, `GeometryUtils`, `RandomUtils`, `EnumUtils`, `StringUtils`, `GizmoUtils`, `Timer`, `PosAndRot`, `SerializedDateTime`
- `Extensions` – `Transform`, `GameObject`, `RectTransform`, `MonoBehaviour` (coroutine helpers), `Vector` extensions
- `Pooling` – `PoolManager` (static `Get`/`Release` by prefab or registered id, prewarm, edit-mode fallback), `IPoolable` spawn/despawn callbacks
- `Debugging` – `GizmosController` (persistent gizmos by id, editor-only calls), `DebugFPS` on-screen counter
- `Haptics` – `HapticFeedback.VibrateHaptic(type)` for iOS (native plugin) and Android. Android needs `<uses-permission android:name="android.permission.VIBRATE"/>` in the project's manifest
- `MeshTools` – `MeshUtils` (combine by material, extract sub-mesh, re-pivot, bake materials into one), `MeshCombiner` and `MeshPivotChanger` components that save the result as mesh + prefab assets
- `Screenshots` – `ScreenshotUtility.Capture/CapturePng` (camera to texture, optional transparent background), `ScreenshotTaker` component (saves PNG assets with an optional import preset), Tools > Shared > Take Game View Screenshot (Cmd/Ctrl+K, saved to Desktop)
- `Audio` (assembly `Shared`) – `AudioManager` (singleton: pooled one-shot effect sources, looping music source, `GetVolume`/`SetVolume` on exposed mixer parameters in dB, saved to PlayerPrefs `Audio.<parameter>`), `AudioConfig` (mixer, Effects/Music groups, voices, volume parameters with defaults)
- `UI` (assembly `Shared.UI`, only compiled when uGUI is installed) – `UIUtils.IsPointerOverUI()` (Input System or legacy input), `Button.SetExclusiveListener`, `Graphic.SetAlpha`, `UIObject` (cached `RectTransform`), `IList<T>.SetItemCount` (grow a list from its first item and activate the first N), `UITextMeshProAutoWidth`, `UIPressScale` (drop on any button: scales it down while pressed, springs back on release; skips non-interactable Selectables, unscaled time) with `UIPressScaleSettings` (ScriptableObject: pressed scale, durations, eases; an empty Settings field uses `Resources/UIPressScaleDefault`, else built-in defaults), `UIButtonSound` (plays its clip on `onClick` through `AudioManager`), `UIEase`; editor-only `SelectableRegistryFix` (assembly `Shared.UI.Editor`) works around a uGUI bug, see CLAUDE.md
- `UI/Views` (assembly `Shared.UI.Views`, only compiled when uGUI and UniTask are installed) – `UIManager` (view registry `Get<T>()`, view stack, `HandleBack()`, `CanvasSize`), `UIView` / `UIView<TData>` (`Init(data)` → `Model`, awaitable `Show`/`Hide`, `WaitUntilHidden`, `Shown`/`Hidden` events, `OnShow/OnShown/OnHide/OnHidden` hooks, close buttons), transition components `UIFadeTransition` / `UIScaleTransition` / `UISlideTransition`, `UIFadeOverlay`, `UILoadingOverlay.Begin()`
- `GameStates` (assembly `Shared.GameStates`, only compiled when UniTask is installed via UPM) – `GameManager` (MonoBehaviour, not a singleton: owns the `GameStateMachine`, `EnterInitialState()` calls `Machine.Change`, `InitContext(Context)` can fill common fields of every context), `GameStateMachine` (`Change(state, context?)` / `Push(state, context?, ct)`: each state declares its context type, a plain class deriving from the abstract `GameStateBase.Context`, usually nested as `public new class Context : GameStateBase.Context`; the caller passes a filled one, else `new TContext()`, then the manager's `InitContext` runs; `Change(state)` replaces the whole stack from anywhere, `await Push(state)` runs a child state on top and returns when it exits, `Push<TResult>` returns its result, `HandleBack()`, `States`/`Current`/`StateChanged`), plain C# `GameState<TContext>` / `GameState<TContext, TResult>` (typed `_context` from `OnEnter` on, no casts; a context of the wrong type does not compile; named `_context` so a state's nested `Context` class compiles; `Machine` while running; `OnEnter`/`OnRun`/`OnExit`/`OnPause`/`OnResume`/`OnBack`; `OnExit` always runs except on machine teardown), `RunLoop(ct, params LoopHandler[])` (`LoopHandler(wait, handle)`: race the waits, run the winner's handle until one returns `LoopResult.Break`; a `GameState` can instead override the `Handlers` property, set once in its constructor, and the default `OnRun` loops over it), `WaitForBack(ct)` (a race branch completed by Back through the default `OnBack`; no waiter outside the race, so Back is never swallowed while another branch's handler runs)
- `Async` (assembly `Shared.UniTask`, only compiled when UniTask is installed via UPM) – `UniTaskRace.Race(entries, wait, ct)`: start `wait(entry)` for every entry, return the entry whose wait finished first, cancel the rest
- `Serialization` (assembly `Shared.Json`, only compiled when `com.unity.nuget.newtonsoft-json` is installed) – `AsJson`, `AsPrettyJson`, `AsModel<T>` (always invariant culture)
- `EditorTools` (editor only) – `EditorCoroutine` (nested routines, `WaitForSeconds`), `SceneCameraSaverAuto` (remembers the Scene view camera per scene; toggle in Tools > Shared), `SceneGameViewResolutionSettings` (Game view resolution per scene; create via Create > Shared), Tools > Shared > Restart Scene or Prefab (Cmd/Ctrl+T: in Play mode reloads the active scene in place; otherwise reloads the open prefab or scenes from disk, asking first if there are unsaved changes), Tools > Shared > Recompile Project (clears the Console, refreshes assets, requests a clean script compilation, then logs the Console's error/warning totals), main toolbar time scale (`TimeScaleToolbar`: `Time ×N` dropdown with presets and Custom..., plus a 0–10 slider; the value set in Edit mode is applied on entering Play mode, Play mode changes set `Time.timeScale` directly, and the Edit mode value is restored on exit)

Commit the `.meta` files Unity generates; other projects rely on their GUIDs.
