# 그림판 파츠 작업 상태

`Paint-Parts-v1.png`는 **사용자가 그림체를 거절한 중간 결과**다. 최종 승인본이나 Unity 적용 대상으로 사용하지 않는다.

1024×512 출력, 두 파츠 분리, 빨강·노랑·파랑 물감 3개, 투명도·격자 검사는 통과했다. 그러나 물감의 촘촘한 타원 계단과 겹치는 광택 띠, 유색 테두리가 기존 메모장·휴지통의 큰 명암 면과 맞지 않는다. 이전 시각 검수 PASS를 철회했다. 출력 규격 검사가 그림체 검수를 대신하지 않는다.

현재 색상 수정본은 `Paint-Parts-v4.png`와 편집 가능한 `Paint-Parts-v4.aseprite`다. 사용자가 제공한 `D:/SteamLibrary/steamapps/common/Aseprite/Aseprite.exe`(실제 실행 버전1.3.18.6)에서 Lua로 픽셀 좌표와 단색 면을 직접 지정해 다시 그렸다. 생성된 고해상도 이미지를 축소하거나 색상을 자동 양자화한 결과가 아니다.

- 원본256×128, 두128×128칸에 팔레트·붓을 별도 레이어로 배치.
- 주된 픽셀 덩어리2×2, 투명 포함24색. 큰 명암 면을 직접 배치하고 반투명·안티앨리어싱·그라데이션을 사용하지 않음.
- 최종1024×512 PNG는 원본의 정확한 최근접4배. 왼쪽/오른쪽512칸으로 자를 수 있음.
- `v2`는 Aseprite 재작성 첫 중간본이다. v3에서 팔레트의 큰 그림자 면과 붓 연결부 비례를 추가 수정했다.
- v4는 v3 Aseprite 원본의 색상만 수정했다. 나무는 따뜻한 꿀색, 빨강·노랑·파랑 물감과 파란 손잡이는 더 선명하게 조정했다. 색상16개만 일대일 치환했고 픽셀 배치·투명도·외곽선·중립 금속과 붓털은 그대로인 것을 실제 PNG 비교로 확인했다. `recolor-native-v4.lua`가 이 수정 과정을 재현한다.
- `palette-native-data.lua`, `brush-native-data.lua`는 정수 좌표 그림 데이터이고 `render-native-parts.lua`가 Aseprite에서 원본과 PNG를 만든다. `verify-native-parts.cjs`는 이미지 수정 없이 출력 검사만 한다.

사용자의 v4 Unity 적용 요청에 따라 `Assets/RYU/Sprite/Charcter/Paint/`에 팔레트·붓 각각 정지 PNG와3프레임 위글리 PNG를 넣었다. 모두512×512, PPU400, Single, Full Rect, Point, 무압축, Mipmap 없음이다. 승인된 색과4×4정수픽셀격자를 보존했고 각 파츠의 내부 명암도 국소적으로 움직인다.

`Palette.controller`와 `Brush.controller`는 각 파츠 단독 사용용이다. `Paint.controller`와 `Paint_Wiggle.anim`은 루트 팔레트와 `Brush` 자식 파츠를 함께 재생한다. 클립은 SpriteRenderer와 uGUI Image 바인딩을 함께 제공하고 실제 Animator에서 두 방식의 프레임 전환을 확인했다. 키0/.08/.16/.25초, stopTime.25, 3프레임80/80/90ms 반복이다. Unity의 raw clip.length는 마지막 PPtr키 뒤 한 프레임을 포함해.26이지만 실제 AnimatorState 길이.25를 확인했다.

`FightScene/Paint/Visual`의 계산기 임시 외형을 팔레트로 교체하고 `Visual/Brush`를 별도 자식으로 연결했다. 두 파츠가 겹치지 않도록 Brush의 상대x위치는1.28이며 원래 캐릭터와 Visual의 위치·스케일은 보존했다. `Paint.asset`은 sprite/controller 두 참조만 수정했다. 기존 공격 코드와 수치는 변경하지 않았고, 이번 작업은 외형과 위글리 대기 애니메이션 적용이다.

픽셀 검사는 `Unity/wiggle-verification.json`, 실제 Unity 적용 검사는 `Unity/unity-verification.json`에 기록했다. Aseprite 애니메이션 원본은 `Unity/Paint_Wiggle.aseprite`에 있다.
