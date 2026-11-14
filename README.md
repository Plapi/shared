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
- `UI` (assembly `Shared.UI`, only compiled when uGUI is installed) – `UIUtils.IsPointerOverUI()` (Input System or legacy input), `Button.SetExclusiveListener`, `Graphic.SetAlpha`
- `EditorTools` (editor only) – `EditorCoroutine` (nested routines, `WaitForSeconds`), `SceneCameraSaverAuto` (remembers the Scene view camera per scene; toggle in Tools > Shared), `SceneGameViewResolutionSettings` (Game view resolution per scene; create via Create > Shared)

Commit the `.meta` files Unity generates; other projects rely on their GUIDs.
