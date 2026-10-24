# 그림판 물감 피격 파티클

게임용 폴더: `Assets/RYU/Sprite/Charcter/Paint/Effects/PaintHit/`

- `PaintHit_Red.prefab`: 중앙 물감 번짐 + 밝은 부분 + 방울 8개.
- `PaintHit_Yellow.prefab`: 같은 구성, 방울 10개.
- `PaintHit_Blue.prefab`: 같은 구성, 방울 12개.

모든 프리팹은 생성 시 자동으로 한 번 재생된다. 중앙 번짐은 약 0.19초, 방울은 약 0.22~0.39초 동안 보인다. 루트 Particle System은 Loop를 끄고 Duration 0.5초, Stop Action Destroy를 설정해 종료 후 루트와 자식을 정리한다. 사용자 정의 C# 컴포넌트는 없다.

방울은 오른쪽 전방으로 퍼지고 중력으로 내려온다. 왼쪽을 향하게 할 때는 생성 회전을 Z축 180도로 주어 방향을 바꿀 수 있다. 크기는 프리팹의 Transform Scale로 조절한다. 16×16의 단순한 픽셀 마스크, Point 필터, Mipmap/압축 없음, 기존 URP 2D Sprite Unlit 셰이더를 사용한다.

연결 위치는 PaintAttackSO의 실제 피해 처리 직후, 타격한 적의 위치다. 큰 곡선 궤적은 공격당 한 번 생성하고, 이 피격 파티클은 실제로 피해를 준 적마다 생성한다. 색상은 이번 공격에 선택한 붓의 색상과 동일하게 선택해야 한다. 이번에는 리소스만 만들었고 기존 공격 코드 및 씬에 자동 연결하지 않았다.

Unity의 분리된 미리보기에서 각 색상의 방울 개수, XY 평면 유지, 전방 이동, 0.65초 시점 잔여 파티클 0개를 확인했다. 자동 삭제는 루트의 네이티브 Stop Action 설정을 확인했다. 실제 게임의 공격 연결 및 Play 중 자동 삭제 호출까지 검증한 것은 아니다. 검증 결과는 verification.json에 있다.

PaintHit-preview.gif는 보기 편하게 반복하는 시뮬레이션 미리보기다. 왼쪽부터 빨강, 노랑, 파랑이다. 게임용 프리팹은 반복하지 않는다.
