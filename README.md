# UGUI-Screen-Point

English | [한국어](README.ko.md)

Make a uGUI element follow a world-space `Transform`.

Two tiny `MonoBehaviour`s, one per Canvas render mode. Drop one on the UI object you want pinned to a 3D object (nameplate, damage number, quest marker, HUD indicator) and it tracks the target every `LateUpdate`.

- [`UIScreenOverlayPoint.cs`](UIScreenOverlayPoint.cs) — for **Screen Space - Overlay** canvases
- [`UIScreenCameraPoint.cs`](UIScreenCameraPoint.cs) — for **Screen Space - Camera** canvases

## Install

Copy the `.cs` file you need into your Unity project. No packages, no dependencies beyond `UnityEngine`.

Both classes live in the global namespace — wrap them in your own namespace if that matters to you.

## Which one do I use?

| Canvas Render Mode | Component | Extra setup |
| --- | --- | --- |
| Screen Space - Overlay | `UIScreenOverlayPoint` | none |
| Screen Space - Camera | `UIScreenCameraPoint` | must set the UI camera |
| World Space | neither | parent the UI to the target directly |

## Usage

### Inspector

Add the component to the UI `GameObject` and fill in the fields:

| Field | Meaning |
| --- | --- |
| `_followTransform` | world-space target to follow |
| `_worldCamera` | camera that renders the target. Falls back to `Camera.main` when left empty |
| `_uiCamera` | *(`UIScreenCameraPoint` only)* the camera assigned to the Canvas |
| `_offset` | world-space offset added to the target position, e.g. `(0, 2, 0)` to sit above the head |

### From code

```csharp
// Screen Space - Overlay
UIScreenOverlayPoint.Require(nameplate.gameObject)
    .SetTarget(character.transform);

// Screen Space - Camera
UIScreenCameraPoint.Require(nameplate.gameObject)
    .SetTarget(character.transform)
    .SetUICamera(uiCanvas.worldCamera);
```

`Require(GameObject)` returns the existing component or adds one if it is missing, so it is safe to call repeatedly. It returns `null` when the passed `GameObject` is `null`.

Lift the anchor above the target's pivot:

```csharp
point.SetOffset(new Vector3(0f, 2f, 0f));
```

Stop following:

```csharp
point.SetTarget(null);
```

`LateUpdate` skips all work while the target is `null`, so this costs nothing.

## How it works

`UIScreenOverlayPoint` projects the target to screen space and writes it straight into `transform.position`, which is what an Overlay canvas expects — its rect coordinates *are* pixel coordinates:

```csharp
var screenPoint = WorldCamera.WorldToScreenPoint(targetPosition);
transform.position = screenPoint;
```

`UIScreenCameraPoint` does the same projection, then converts the screen point back to the UI camera's world space, because a Screen Space - Camera canvas lives in front of its own camera:

```csharp
var screenPoint = RectTransformUtility.WorldToScreenPoint(WorldCamera, targetPosition);
transform.position = _uiCamera.ScreenToWorldPoint(screenPoint);
```

Both run in `LateUpdate`, after the target has already moved this frame, so the UI never lags a frame behind.

## Notes & caveats

- **API differences are intentional.** `UIScreenCameraPoint.SetTarget` / `SetUICamera` return `this` for chaining; `UIScreenOverlayPoint.SetTarget` returns `void`.
- **`_uiCamera` is required for `UIScreenCameraPoint`.** It is not defaulted, and `LateUpdate` will throw a `NullReferenceException` if the target is set but the UI camera is not. Assign it in the Inspector or via `SetUICamera`.
- **`Camera.main` fallback is cached.** The first `WorldCamera` access stores the result. If the main camera is replaced at runtime, assign `_worldCamera` yourself.
- **Targets behind the camera still get positioned.** `WorldToScreenPoint` returns a mirrored point with negative `z` when the target is behind the world camera. Hide the UI yourself if that matters:
  ```csharp
  var toTarget = target.position - cam.transform.position;
  bool visible = Vector3.Dot(cam.transform.forward, toTarget) > 0f;
  ```
- **No clamping to screen edges.** Off-screen targets produce off-screen UI. Clamp the result yourself if you need edge indicators.
- **One target per component.** For many followers, one component per UI object is fine, but a single manager iterating a list will be cheaper at high counts.

## License

MIT. See [LICENSE](LICENSE).
