# 붓 물감 낙하 파티클

게임 리소스: `Assets/RYU/Sprite/Charcter/Paint/Effects/PaintDrip/`

- `PaintDrip_Red.prefab`
- `PaintDrip_Yellow.prefab`
- `PaintDrip_Blue.prefab`

붓끝에서 0.18초 동안 물감 방울 10개가 흘러나온다. 방울은 월드 좌표로 중력을 받아 내려오고, 바닥에 닿으면 납작한 물감 얼룩으로 바뀐다. 얼룩은 약 0.25~0.32초 후 사라진다. 반복하지 않으며 루트의 Stop Action은 Destroy다. 사용자 정의 스크립트는 없다.

## 연결

붓끝에 빈 자식 오브젝트를 놓고 생성 위치로 사용한다. 물감이 흘러야 하는 애니메이션 시점에 이번 붓 색상과 같은 프리팹을 그 위치에 한 번 생성한다. 적이 맞았을 때 생성하는 PaintHit과는 별도의 효과다. 현재 공격/애니메이션 이벤트 코드 및 씬에는 자동 연결하지 않았다.

방울은 월드 좌표로 움직여 붓이 이동해도 이미 나온 물감이 함께 끌려가지 않는다. 생성 회전은 Quaternion.identity를 사용하면 된다. 지속해서 흘리는 Loop 효과가 아니라 붓질 한 번에 생성하는 짧은 효과다.

충돌은 World / 2D이며 Default 레이어의 정적 Collider2D에만 반응한다. 현재 FightScene의 바닥 Square가 이 조건이다. Player / Enemy 레이어의 캐릭터는 무시한다. 다른 바닥 레이어를 사용할 경우 루트 Particle System의 Collision > Collides With를 그 레이어로 바꾼다. 최대 수명 약 1초 안에 닿을 수 있는 높이에서 사용한다.

기존 PaintHit의 픽셀 텍스처와 URP 2D Sprite Unlit 머티리얼을 공유하므로 PaintHit 폴더의 텍스처/머티리얼은 함께 유지한다.

## 확인 결과

FightScene의 실제 바닥 Collider2D를 대상으로 임시 복제본을 재생하고 0.02초씩 수동 시뮬레이션했다. 세 색상 모두 방울 10개가 바닥과 충돌했고, 착지 얼룩은 최대 10개였으며, Z 이동은 0이었다. 1.7초 시점 잔여 파티클은 0이었다. 임시 복제본은 확인 후 제거했다. 루트 자동 삭제는 Stop Action 설정을 확인했으며 실제 프레임 경과에 따른 삭제까지 검사한 것은 아니다.

미리보기는 분리된 Preview Scene에서 같은 높이의 충돌 평면을 사용한 렌더다. 표시된 붓과 회색 바닥 선은 미리보기 전용이고 프리팹에는 포함되지 않는다. 실제 게임용 프리팹의 충돌 방식은 World / 2D다.
