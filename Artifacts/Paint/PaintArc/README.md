# Paint 곡선 물감 궤적

## 게임용 리소스

`Assets/RYU/Sprite/Charcter/Paint/Effects/PaintArc/`

| 색 | 프리팹 | 애니메이션 |
| --- | --- | --- |
| 빨강 | `PaintArc_Red.prefab` | `PaintArc_Red.anim` |
| 노랑 | `PaintArc_Yellow.prefab` | `PaintArc_Yellow.anim` |
| 파랑 | `PaintArc_Blue.prefab` | `PaintArc_Blue.anim` |

각 색상은 같은 형태의 5프레임이다. 프레임당 0.04초, 0.20초부터 Sprite가 비워지며 전체 클립 길이는 빈 종료 프레임을 포함해 0.24초다. 게임용 클립은 반복하지 않는다. 프리팹을 생성하면 기본 `Play` 상태가 재생된다. 별도 이벤트 수신용 C# 컴포넌트는 필요하지 않다.

프리팹에는 SpriteRenderer와 Animator가 들어 있다. 이미지가 사라져도 GameObject는 남으므로, 생성한 쪽에서 사용 중인 풀로 반납하거나 수명 처리를 해주면 된다. 풀에서 다시 쓸 때는 Animator의 `Play` 상태를 시간 0으로 재생한다.

## 캐릭터에 연결하는 위치

1. Paint의 붓이 실제로 휘둘러지는 구간에 물감 궤적 생성용 Animation Event를 둔다.
2. 그 이벤트를 받는 사용자 코드에서 이번 공격 색상의 프리팹을 캐릭터의 휘두르기 중심에 생성한다. 피격 순간까지 기다리면 궤적이 늦게 시작된다.
3. 기존 `Attack` 이벤트는 피해 판정에 사용한다. 이번 리소스에는 피격 물감 튀김은 포함하지 않았다.

세 프리팹은 안쪽 빈 공간의 중심을 같은 위치로 맞췄다. 이미 생성된 효과를 회전 중인 Brush의 자식으로 두면 효과 전체도 같이 회전하므로, 독립 오브젝트로 생성하는 방식이 기본이다. 곡선은 오른쪽이 열린 C 형태다. 원하는 휘두르기 방향에 맞춰 생성 회전 또는 Flip X/Y를 조정할 수 있다.

## 크기와 렌더링

- PNG: 512×512, 투명 배경, 부분 알파 없음, 색상당 4색.
- 원본 작업 크기: 128×128, 2×2 픽셀 묶음. 최근접 4배 출력으로 8px 단위 픽셀 묶음을 유지했다.
- Unity: PPU400, 중앙 피벗, Point 필터, Clamp, Mipmap 없음, 압축 없음.
- 프리팹 초기 Scale: 2. 캔버스 기준 크기 2.56×2.56 월드 유닛이며 실제 물감은 그 안쪽에 있다.
- Sorting Order: 20. 필요하면 게임의 기존 정렬 순서에 맞춘다.
- URP용 Sprite Unlit 머티리얼을 사용한다.

## 제작 및 검증

내장 `image_gen`으로 세 색상 원본을 생성했다. 원본은 `PaintArc-*-generated.png`, 프롬프트는 `prompts.json`에 있다. API/CLI 이미지 생성 대체 경로는 사용하지 않았다.

Aseprite에서 프레임을 분리하고 중심을 맞춘 뒤, 픽셀 격자·알파·팔레트를 정리했다. 세 색상은 빨강 원본에서 같은 형태와 음영 인덱스를 공유하므로 색상을 바꿔도 형상이 달라지지 않는다. 편집 가능한 원본은 `PaintArc_Red.aseprite`, `PaintArc_Yellow.aseprite`, `PaintArc_Blue.aseprite`다. `prepare-paint-arc.lua`로 동일하게 다시 내보낼 수 있다.

`PaintArc-preview.gif`는 보기 편하게 빈 구간을 두고 반복하는 미리보기다. 게임용 클립은 1회 재생이다.

검증 결과:

- PNG 15장의 8px 격자, 투명도, 팔레트, 테두리 잘림, 색상 간 형상 일치를 확인했다.
- Unity의 분리된 Preview Scene에서 3색 각각 5개 프레임의 순서와 0.20초 이후 숨김을 샘플링했다.
- 프리팹의 Animator/클립 연결과 반복 해제를 확인했다.
- 검증 파일: `pixel-verification.json`, `unity-verification.json`.
- 기존 캐릭터 코드·Animator Controller·작업 씬은 수정하지 않았다. 공격 이벤트의 생성 연결은 아직 적용하지 않았다.
