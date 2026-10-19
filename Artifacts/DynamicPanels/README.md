# Dynamic Panels 적용

YouTube와 Character 창에 Dynamic Panels 1.3.5를 적용했다. 기존 XP 그림과 내용은 그대로 재사용한다.

- 원본: https://github.com/yasirkula/UnityDynamicPanels
- 원본 revision: 11036028b90df09212e2bc238c13983a4cfcd105
- 설치: Assets/Plugins/DynamicPanels (MIT, LICENSE.txt 포함)
- 적용 씬: Assets/RYU/Scene/MainScene.unity
- 별도 적용본: Assets/RYU/Scene/MainScene - DynamicPanels.unity
- 공통 프리팹: Assets/RYU/Prefab/Panel - YouTube.prefab

## 편집 구조와 실행 구조

Panel - YouTube는 화면에서 작업표시줄 60을 제외한 영역의 DynamicPanelsCanvas다. 기존 PanelService의 YoutubePanel 참조는 같은 오브젝트를 유지한다.

편집 중에는 YouTube Content 아래에 기존 UI가 있다. Play 후 창을 처음 열면 패키지가 DynamicPanel을 만들고, YouTube Content를 그 Content 아래로 옮긴다. 실제 크기 조절 대상은 이 실행 중 생성된 DynamicPanels.Panel이다.

Inspector의 DynamicPanelsCanvas에서 Panel Resizable Area Length는 10, Free Panels의 첫 탭 Minimum Size는 (500, 350)이다. 제목줄 드래그와 8방향 크기 조절은 원본 패키지에서 처리한다. 도킹 영역은 0으로 설정했다. 바탕화면 입력이 막히지 않도록 전체 컨테이너에 배경 Image를 두지 않았다.

기존 XP 창의 파란 테두리/제목줄/웹페이지/상태줄은 보존했다. 패키지 기본 제목/탭은 숨겼고 제목줄 버튼을 제외한 부분에 투명 드래그 영역을 배치했다. 기존 UIDragePanel, UIResizeHandle 영역은 이 창에서 비활성화했다.

## 버튼과 사용자 코드 연결

닫기와 최소화는 기존 PanelService.SetYoutubePanelActive(false)를 호출해 컨테이너 전체를 숨긴다. 최소화는 작업표시줄 상태를 추적하는 구현이 아니라 창 숨김이다. 바탕화면 바로가기로 다시 열 수 있다.

최대화 버튼은 사용자가 작성한 UIFullScreenPanel.FullScreen을 각 Content의 컴포넌트에 연결했다. 이 컴포넌트는 실행 중 부모 DynamicPanels.Panel을 찾아 최대화하고 원래 위치와 크기로 복원한다. C# 파일은 변경하지 않았다.

나중에 실제 패널을 찾을 때는 YouTube Content의 연결된 탭(PanelUtils.GetAssociatedTab)에서 Panel을 가져오면 된다. 컨테이너에서 첫 Panel을 찾는 방법은 DummyPanel을 선택할 수 있어 피한다.

Assets의 사용자 C#과 외부 패키지 C#은 수정하지 않았다. 수정한 패키지 자산은 Resources의 네이티브 프리팹 3개뿐이며, 이전 Unity UI GUID를 현재 GUID로 바꾸고 XP 콘텐츠를 가리지 않도록 배치했다.

Backups에는 적용 전 씬/프리팹 파일을 텍스트로 보관했다. Setup.json, Validation.json에 연결과 검사 결과가 있다.

## Character 창 적용 (2026-10-19)

MainScene - DynamicPanels 씬의 Panel - Character를 같은 구조로 연결했다. Character Content가 기존 UI를 담으며, PanelService.CharcterPanel과 닫기 버튼 참조는 유지했다. 최소 크기는 800×450이고 YouTube는 500×350이다. 두 창은 Free Panels에 유효 탭을 하나씩 등록하고 빈 Free Panel을 제거했다. Resizable Area는 10, 두 Anchor Zone은 0, Leave Free Space는 해제했다.

Play 모드에서 캐릭터 창 생성, 드래그, 우측 크기 조절, 최소 크기 제한, 최대화/복원, 닫기/재열기를 확인했다. YouTube 최대화/복원도 확인했으며 새 실행 오류는 없었다.
