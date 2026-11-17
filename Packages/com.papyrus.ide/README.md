# PAPyrus Integration (com.papyrus.ide)

Unity 2021.3 이상.

1. PAPyrus에서 프로젝트를 열고 메뉴 → **Unity 패키지 설치/업데이트**를 누르거나, 이 폴더를 `<프로젝트>/Packages/com.papyrus.ide`에 복사합니다.
2. Unity에서 **Edit > Preferences > External Tools > External Script Editor**를 **PAPyrus**로 바꿉니다.
   목록에 없으면 PAPyrus를 먼저 실행하거나 **Browse...**로 실행 파일을 선택하세요. 개발 빌드에서는 `npm run build`가 만든 `node_modules/.cache/papyrus/PAPyrus.exe`를 선택하세요.
3. 스크립트를 더블클릭하면 PAPyrus에서 열립니다. Unity가 켜져 있으면 IDE 상단에 연결 상태와 Play/Stop 버튼이 활성화됩니다.

- 프로젝트 파일은 SDK 스타일 `.csproj`로 생성되며, obj 파일은 `Temp/obj` 아래에 생깁니다.
- 같은 Preferences 화면의 패키지 체크(Embedded/Local/Registry/Git/Built-in)는 소스까지 프로젝트로 만들 패키지를 고릅니다. 체크하지 않은 패키지(DOTween, UniTask 등)도 Unity가 컴파일한 DLL로 참조되어 자동완성이 됩니다. 바꾸면 바로 다시 생성되고 PAPyrus가 언어 서버를 자동으로 다시 불러옵니다.
- 연동 포트는 `Temp/PAPyrus/bridge.json`에 기록됩니다 (localhost 전용).
- Unity의 기본 외부 편집기 등록으로도 스크립트를 열 수 있습니다. 이 패키지는 환경설정 목록 등록, 프로젝트 파일 생성과 실시간 연동에 사용합니다.
