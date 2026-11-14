# Shared package (`com.adrian.shared`)

Reusable Unity code shared across my projects. Lives in its own repo (github.com/Plapi/shared) and is added to each project as a git submodule at `Packages/com.adrian.shared` (embedded package).

## Module rules

- One folder per module: `Runtime/<Module>/`, namespace `com.Plapamaru.Shared.<Module>`.
- Core assembly `Shared` (`Runtime/Shared.asmdef`) has **no** third-party or optional-package references. Anything needing UniTask, Newtonsoft, DOTween, uGUI or the Input System gets its own small asmdef (e.g. `Shared.UniTask`) inside its module folder.
- Editor-only code goes under `Editor/<Module>/` (assembly `Shared.Editor`) and uses the module's namespace — never a `...Shared.Editor` namespace, it would shadow `UnityEditor.Editor`. Editor-only members of runtime components stay in the runtime file under `#if UNITY_EDITOR` so game code can still call them.
- Commit the `.meta` files Unity generates; projects rely on their GUIDs.
- Components that old projects have on scenes/prefabs: reuse the old script GUID in the new `.meta` (check `grep guid <old>.cs.meta`) and keep renamed fields readable with `[FormerlySerializedAs("oldName")]`, so references and values survive migration.
- Update the module list in `README.md` when adding a module.

## Code style

- Allman braces, 4-space indent, `_camelCase` private fields, file ends with a newline, no BOM.
- No comments or doc summaries unless truly necessary.
- Prefer `IReadOnlyList<T>` / `IList<T>` parameters over concrete arrays/lists.

## Importing a module from an old project

Sources are the sibling repos in the same GitHub folder: `townbuilder/Assets/Plugins` and `lanes/Assets/Plugins`. Where both have a module, townbuilder's copy is newer (namespaced, asmdefs) and is the base.

1. Read both copies; check what other code uses (`grep` usages in both projects) before renaming anything.
2. Port into `Runtime/<Module>/`, change the namespace, clean up, fix obvious bugs, drop what Unity already provides. Renames are fine — old projects migrate when they adopt the package.
3. Verify in the host project with the editor open: `unity recompile .` (no rows = no errors). For logic changes, sanity-check with `unity cmd script eval --file <check.cs>`.
4. Commit inside the submodule (message = module name), then the host project commits the new submodule pointer.

## Import status

- [x] Singletons
- [x] Utilities (from `Utils/`; `Utils.cs` split into Random/Enum/String/Gizmo/GeometryUtils)
- [x] Extensions (core only; `ToVector2/ToVector3` became `ToXZ/XZToVector3`, `IterateAllChildren` became `ForEachInHierarchy`)
- [x] Pooling (merged townbuilder `ObjectPoolingSystem` id registry + lanes `ObjectPoolManager` prefab-keyed lazy pools into `PoolManager`; no interface needed to be poolable)
- [x] Debugging (`GizmosController` draw calls are `[Conditional("UNITY_EDITOR")]`, stripped from builds)
- [x] UI (`Runtime/UI/Shared.UI.asmdef`: compiled only when `com.unity.ugui` is installed; uses the Input System when present (`SHARED_INPUT_SYSTEM`), else legacy `Input`. lanes-rush has no uGUI, so verify with Unity's `csc` against townbuilder's `Library/ScriptAssemblies/UnityEngine.UI.dll` + references from `Shared.csproj`)
- [x] Haptics (native symbol renamed `PlapamaruHaptics_Impact` so it can't clash with an old project's own `.mm`; `.mm` is iOS-only in its `.meta`; host project must declare the Android `VIBRATE` permission; not yet tested on a device)
- [x] Mesh tools (`MeshCombiner`/`MeshPivotChanger` keep their old script GUIDs — lanes has `MeshCombiner` on 29 prefabs/scenes; MantisLOD "Vertices Reduction" dropped, paid asset)
- [x] Editor tools (`Editor/EditorTools/`; `SceneGameViewResolutionSettings.cs` keeps the old script GUID so townbuilder's settings asset still loads; Game view resolution uses Unity internals via reflection — verified on 6000.6, failures only log a warning)
- [x] Screenshots (base is townbuilder `Scripts/ScreenshotTaker/ScreenshotTaker.cs`, keeps its GUID; the duplicated `DebugWindow` "Take Screenshot %k" menu moved here; lanes `DriversScreenshotTaker` stays in lanes — game-specific)
- [ ] Optional add-ons — `UniTaskUtils`, `JsonExtensions`

Never import: paid Asset Store code (OSA, MantisLOD), vendored third-party libs (DOTween, UniTask, JsonFX, ParticleEffectForUGUI — use UPM/Asset Store per project), per-game platform files (Android templates, Tenjin SDK).
