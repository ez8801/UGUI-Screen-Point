# UGUI-Screen-Point

[English](README.md) | 한국어

uGUI 요소가 월드 공간의 `Transform`을 따라다니게 만듭니다.

Canvas 렌더 모드별로 하나씩, 두 개의 작은 `MonoBehaviour`로 구성됩니다. 3D 오브젝트에 고정하고 싶은 UI 오브젝트(네임플레이트, 데미지 숫자, 퀘스트 마커, HUD 인디케이터)에 붙이면 매 `LateUpdate`마다 대상을 추적합니다.

- [`UIScreenOverlayPoint.cs`](UIScreenOverlayPoint.cs) — **Screen Space - Overlay** 캔버스용
- [`UIScreenCameraPoint.cs`](UIScreenCameraPoint.cs) — **Screen Space - Camera** 캔버스용

## 설치

필요한 `.cs` 파일을 Unity 프로젝트에 복사하면 됩니다. 패키지 의존성 없이 `UnityEngine`만 사용합니다.

두 클래스 모두 전역 네임스페이스에 있습니다. 필요하면 직접 네임스페이스로 감싸서 쓰세요.

## 어느 쪽을 써야 하나

| Canvas 렌더 모드 | 컴포넌트 | 추가 설정 |
| --- | --- | --- |
| Screen Space - Overlay | `UIScreenOverlayPoint` | 없음 |
| Screen Space - Camera | `UIScreenCameraPoint` | UI 카메라 지정 필수 |
| World Space | 해당 없음 | UI를 대상에 직접 자식으로 붙이면 됨 |

## 사용법

### 인스펙터

UI `GameObject`에 컴포넌트를 추가하고 필드를 채웁니다.

| 필드 | 의미 |
| --- | --- |
| `_followTransform` | 따라갈 월드 공간 대상 |
| `_worldCamera` | 대상을 렌더링하는 카메라. 비워두면 `Camera.main`으로 대체됨 |
| `_uiCamera` | *(`UIScreenCameraPoint` 전용)* Canvas에 지정된 카메라 |
| `_offset` | 대상 위치에 더해지는 월드 공간 오프셋. 예: 머리 위에 띄우려면 `(0, 2, 0)` |

### 코드에서

```csharp
// Screen Space - Overlay
UIScreenOverlayPoint.Require(nameplate.gameObject)
    .SetTarget(character.transform);

// Screen Space - Camera
UIScreenCameraPoint.Require(nameplate.gameObject)
    .SetTarget(character.transform)
    .SetUICamera(uiCanvas.worldCamera);
```

`Require(GameObject)`는 기존 컴포넌트를 반환하고, 없으면 새로 추가합니다. 반복 호출해도 안전합니다. 넘긴 `GameObject`가 `null`이면 `null`을 반환합니다.

대상의 피벗보다 위로 앵커 올리기:

```csharp
point.SetOffset(new Vector3(0f, 2f, 0f));
```

추적 중지:

```csharp
point.SetTarget(null);
```

대상이 `null`이면 `LateUpdate`가 아무 일도 하지 않으므로 비용이 없습니다.

## 동작 원리

`UIScreenOverlayPoint`는 대상을 스크린 공간으로 투영한 뒤 그 값을 `transform.position`에 그대로 씁니다. Overlay 캔버스는 rect 좌표가 곧 픽셀 좌표이기 때문입니다.

```csharp
var screenPoint = WorldCamera.WorldToScreenPoint(targetPosition);
transform.position = screenPoint;
```

`UIScreenCameraPoint`는 같은 방식으로 투영한 뒤, 스크린 좌표를 UI 카메라의 월드 공간으로 다시 변환합니다. Screen Space - Camera 캔버스는 자기 카메라 앞의 월드 공간에 존재하기 때문입니다.

```csharp
var screenPoint = RectTransformUtility.WorldToScreenPoint(WorldCamera, targetPosition);
transform.position = _uiCamera.ScreenToWorldPoint(screenPoint);
```

둘 다 `LateUpdate`에서 실행됩니다. 대상이 그 프레임에 이미 이동한 뒤이므로 UI가 한 프레임 늦게 따라가지 않습니다.

## 참고 사항 및 주의점

- **두 클래스의 API 차이는 의도된 것입니다.** `UIScreenCameraPoint.SetTarget` / `SetUICamera`는 체이닝을 위해 `this`를 반환하지만, `UIScreenOverlayPoint.SetTarget`은 `void`를 반환합니다.
- **`UIScreenCameraPoint`는 `_uiCamera`가 필수입니다.** 기본값이 없어서, 대상만 지정하고 UI 카메라를 지정하지 않으면 `LateUpdate`에서 `NullReferenceException`이 발생합니다. 인스펙터나 `SetUICamera`로 반드시 지정하세요.
- **`Camera.main` 대체값은 캐시됩니다.** `WorldCamera`를 처음 접근할 때 결과가 저장됩니다. 런타임에 메인 카메라가 교체된다면 `_worldCamera`를 직접 지정해야 합니다.
- **카메라 뒤에 있는 대상도 위치가 계산됩니다.** 대상이 월드 카메라 뒤에 있으면 `WorldToScreenPoint`는 `z`가 음수인 반전된 좌표를 반환합니다. 필요하면 직접 UI를 숨기세요.
  ```csharp
  var toTarget = target.position - cam.transform.position;
  bool visible = Vector3.Dot(cam.transform.forward, toTarget) > 0f;
  ```
- **화면 가장자리 클램핑은 없습니다.** 화면 밖 대상은 화면 밖 UI가 됩니다. 가장자리 인디케이터가 필요하면 결과를 직접 클램핑하세요.
- **컴포넌트 하나당 대상 하나입니다.** 추적 대상이 적으면 UI 오브젝트마다 컴포넌트를 붙여도 되지만, 개수가 많아지면 매니저 하나가 리스트를 순회하는 편이 더 저렴합니다.

## 라이선스

MIT. [LICENSE](LICENSE) 참고.
