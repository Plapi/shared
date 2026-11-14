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

## Modules

- `Singletons` – `Singleton<T>`, `MonoBehaviourSingleton<T>`, `ScriptableObjectSingleton<T>`
- `Utilities` – `Bezier`, `Chaikin`, `GeometryUtils`, `RandomUtils`, `EnumUtils`, `StringUtils`, `GizmoUtils`, `Timer`, `PosAndRot`, `SerializedDateTime`

Commit the `.meta` files Unity generates; other projects rely on their GUIDs.
