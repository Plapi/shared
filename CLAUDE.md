# Shared package (`com.adrian.shared`)

Reusable Unity code shared across my projects. Lives in its own repo (github.com/Plapi/shared) and is added to each project as a git submodule at `Packages/com.adrian.shared` (embedded package).

## Module rules

- One folder per module: `Runtime/<Module>/`, namespace `com.Plapamaru.Shared.<Module>`.
- Core assembly `Shared` (`Runtime/Shared.asmdef`) has **no** third-party or optional-package references. Anything needing UniTask, Newtonsoft, DOTween, uGUI or the Input System gets its own small asmdef (e.g. `Shared.UniTask`) inside its module folder.
- Editor-only code goes under `Editor/` (assembly `Shared.Editor`).
- Commit the `.meta` files Unity generates; projects rely on their GUIDs.
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
- [ ] Debugging — `GizmosController`, `DebugFPS`
- [ ] UI — `MouseIsOverUI` (held back from Utilities: needs uGUI + Input System), `ButtonExtensions.SetExclusiveListener`, `ImageExtensions.SetAlpha` (widen to `Graphic`). Own asmdef `Shared.UI`, compiled only when `com.unity.ugui` is installed (`versionDefines` + `defineConstraints`)
- [ ] Haptics — `HapticFeedback.cs` + `iOS/HapticFeedback.mm`
- [ ] Mesh tools — townbuilder `MeshUtils/` (`MeshUtils.cs` depends on paid MantisLOD → split it out)
- [ ] Editor tools — `EditorCoroutine`, `SceneCameraSaverAuto`, `SceneGameViewResolution`
- [ ] Screenshots — lanes `ScreenshotTaker/`
- [ ] Optional add-ons — `UniTaskUtils`, `JsonExtensions`

Never import: paid Asset Store code (OSA, MantisLOD), vendored third-party libs (DOTween, UniTask, JsonFX, ParticleEffectForUGUI — use UPM/Asset Store per project), per-game platform files (Android templates, Tenjin SDK).
