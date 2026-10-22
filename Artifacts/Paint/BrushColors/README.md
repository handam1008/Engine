# 붓 끝 색상 리소스

`Assets/RYU/Sprite/Charcter/Paint/BrushColors/`

- `Brush_Red_01~03.png`: 빨간 물감을 묻힌 붓.
- `Brush_Yellow_01~03.png`: 노란 물감을 묻힌 붓.
- `Brush_Blue_01~03.png`: 파란 물감을 묻힌 붓.
- `Brush_TipMask_01~03.png`: 붓털만 남긴 회색조 오버레이. 별도 SpriteRenderer에 올리고 color를 바꾸면 붓털만 착색할 수 있다.

각 번호는 기존 `Brush_01~03.png`와 일치한다. 전체 붓 이미지는 SpriteRenderer.color를 흰색으로 두고 사용한다. 마스크를 쓴다면 기존 붓과 같은 위치·회전·스케일에 겹치고, sortingOrder를 하나 높인다. 붓이 흔들릴 때 마스크도 같은 번호로 전환해야 한다.

모든 PNG는 512×512, 투명 배경, PPU400, 중앙 피벗, Point 필터, 압축 없음으로 가져온다. 기존 붓의 크기·외곽선·손잡이·금속 부분은 유지하고 붓털의 세 가지 색상만 바꿨다. 빨강·노랑·파랑은 기존 팔레트 물감의 색상과 같다.

편집 원본: `BrushColors.aseprite` (128×128, 색상별 레이어, 3프레임 80/80/90ms). `create-brush-colors.lua`로 기존 PNG에서 다시 내보낼 수 있다.

`BrushColors-AI-reference.png`는 내장 이미지 생성 도구로 만든 색상 참고 이미지다. 실제 게임용 PNG는 기존 픽셀 모양과 알파를 보존하도록 Aseprite 원본 작업 방식으로 만들었다. API/CLI 이미지 생성 대체 경로는 사용하지 않았다. 기존 애니메이션·씬·게임플레이 코드는 수정하지 않았다.
