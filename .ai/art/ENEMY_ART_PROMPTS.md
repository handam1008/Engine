# 적 전용 생성 프롬프트

2026-10-25 기준. `.ai/art/ENEMY_ART_RULES.md`가 우선한다.

아래 프롬프트는 **적(Enemy) 전용**이다. 아군 캐릭터·XP UI·도구·아이콘에는 사용하지 않는다. 주체를 `character`로 바꿔 재사용하지 않는다.

기존 아군 리소스를 첨부한다면 **픽셀 밀도·외곽선·색면 표현의 참고**라고 지정한다. 이전 적 이미지와 특정 게임의 적 이미지는 형태·얼굴 참고로 첨부하지 않는다.

## 1. 첫 기본 스프라이트 — 밀림덩이

```text
Create ONE original ENEMY UNIT called "Offset Blob" for a 2D side-scrolling game set inside a Windows XP computer.

SCOPE: This is exclusively an ENEMY CREATURE asset. Do not create or redesign any allied playable character, XP interface, app icon, or tool. The subject is a living corrupted-data organism with a simple readable face.

DESIGN: A low, thick, asymmetrical bean-shaped soft body facing SCREEN LEFT. Its upper body mass is visibly shifted a short distance toward the left relative to its broad lower body. This creates a rounded overhang at the front and ONE clear step at the rear. Keep both masses organically connected as one soft creature, not separate stacked blocks. The body shift itself is its ONE signature data error. Do not add square holes, floating fragments, texture patterns, or additional glitch effects.

FACE: Exactly two small solid dark dot eyes and one much smaller short horizontal line mouth on the left-front surface. Keep the face compact, plainly visible, and slightly off-center. A mildly blank, mischievous expression. No eye whites, separate pupils, reflections, detailed brows, teeth, or cat-like omega smile.

ANATOMY AND POSE: No external feet, arms, hands, cat ears, or weapons. The broad underside is its contact surface. A calm neutral idle pose with the upper body already offset. The body must look capable of squashing, sliding, and extending its front mass to strike an opponent on the left. Do not depict an attack or draw a second pose.

RENDERING: Crisp chunky pixel art with one consistent logical pixel grid. Thick dark slate contours matching the game's existing pixel density. Broad connected flat color planes: muted moss-green body with at most one lighter plane and one darker plane. Optional palette: outline/face #252B30, body #7FA650, light #A9C776, shade #536F3A. If allied game art is attached, use it only for pixel scale, outline thickness, and flat color treatment; do not copy the allied object's shape.

SIMPLICITY: Readable as a distinctive silhouette at small battle-screen size. Organic asymmetry and one shifted mass are the identity. No symmetrical white ball body, cat anatomy, standard paired stubby feet, human anatomy, robot construction, UI window body, mechanical armor, padlocks, chains, lettering, 0/1 patterns, gradients, glossy rendering, glow, noise, blur, or anti-aliased edges. Do not imitate an existing game's enemy design.

OUTPUT: One complete left-facing ENEMY UNIT, one neutral pose, genuinely transparent background, centered with safe margins on every side. No ground, cast shadow, checkerboard, black backdrop, text, labels, UI, border, effects, or sprite sheet. Output only the enemy sprite.
```

## 2. 대기 2프레임

기본 스프라이트가 결정된 다음에만 사용한다. 해당 기본 적 이미지를 반드시 첨부한다.

```text
Using the attached approved ENEMY UNIT sprite as the fixed design reference, make a TWO-FRAME idle loop for this exact same left-facing enemy.

Frame 1: the exact approved neutral pose.
Frame 2: a very small squash of the soft body, with its broad underside staying on the same baseline and the upper mass still offset left. The loop returns directly to frame 1.

Keep the two dot eyes, tiny line mouth, palette, outline thickness, logical pixel size, left-facing view, body structure, and apparent scale identical. Deform the existing soft body only. No new limbs, pieces, face details, effects, or redesign. No whole-sprite drift or camera movement.

Output one horizontal 2-column, 1-row sprite strip, equal-size cells, transparent background, identical baseline and reference origin in both cells, safe padding, no visible grid or text. These are two poses of ONE ENEMY UNIT, not two different enemies. Follow the approved enemy-only art rules.
```

## 3. 이동 4프레임

```text
Using the attached approved ENEMY UNIT sprite as the fixed design reference, make a FOUR-FRAME in-place sliding movement loop for this exact same left-facing enemy.

Frame 1: approved neutral offset-body pose.
Frame 2: the front/left mass lowers and presses forward slightly while the rear body follows.
Frame 3: the lower body compresses and catches up; retain the distinctive left-offset upper mass.
Frame 4: the body restores toward the neutral pose and joins smoothly back to frame 1.

Animate soft-body compression and restoration, not a biped walk or bouncing mascot gait. Keep the reference origin fixed; world travel is handled by game code. Preserve the face, palette, pixel grid, outline thickness, size, perspective, and one signature body error. No new feet, arms, detached pieces, particles, motion trails, or shadows.

Output one horizontal 4-column, 1-row sprite strip with equal-size cells, transparent background, consistent reference origin and contact baseline, and enough identical padding in every cell. No grid, text, or labels. One ENEMY UNIT repeated in four ordered poses.
```

## 4. 근접 공격 4프레임

```text
Using the attached approved ENEMY UNIT sprite as the fixed design reference, make a FOUR-FRAME non-looping melee attack for this exact same left-facing enemy.

Frame 1 — ANTICIPATION: compress the soft body slightly toward SCREEN RIGHT, drawing its front mass back. Maintain the planted lower body and the enemy's recognizable asymmetry.
Frame 2 — CONTACT / HIT: thrust and extend the existing upper body mass toward SCREEN LEFT as one connected soft shape. This is the clear physical strike/contact pose. Keep the small dot face on the front mass. No projectile and no newly grown arm or weapon.
Frame 3 — RECOIL: retract the striking mass with a short compressed rebound.
Frame 4 — RECOVERY: return to the approved neutral pose so the enemy can resume idle.

The hit occurs in FRAME 2, not on the last frame. Use the same body, two dot eyes, tiny line mouth, flat palette, logical pixel grid, contour thickness, and left-facing view in every frame. Keep the reference origin fixed. Reserve enough canvas space for the leftward extension; do not shrink the whole enemy to fit one frame. Do not add extra glitches, detached blocks, limbs, weapons, effects, camera movement, or motion blur.

Output one horizontal 4-column, 1-row transparent sprite strip with equal-size cells and consistent reference origin. No grid lines, text, labels, ground, or shadow. These are four poses of ONE ENEMY UNIT.
```

## 5. 피격 1자세

```text
Edit the attached approved ENEMY UNIT into ONE brief hit-reaction pose. Squash and lean the existing soft body slightly toward SCREEN RIGHT as if struck from the left. Keep its broad underside as the grounding reference. Preserve the same left-facing face, palette, pixel grid, outlines, size, and left-offset upper mass. No new anatomy, pieces, symbols, effects, or perspective change. One complete enemy pose on the same-size transparent canvas with the same reference origin and safe margins. This is an ENEMY UNIT only.
```

## 6. 사망 2프레임

```text
Using the attached approved ENEMY UNIT as the fixed design reference, make a TWO-FRAME non-looping death sequence for the same left-facing enemy.

Frame 1: the enemy's soft body loses support and collapses low against its existing contact baseline. Its simple face remains readable.
Frame 2: the same body loosens into only two or three simple soft chunks close to the collapsed mass. Keep the same green palette and pixel treatment; no explosion, particles, smoke, gore, text, loot, or additional creatures. The game will remove the enemy after this sequence, so do not add a new full pose or a separate effects animation.

Output one horizontal 2-column, 1-row sprite strip, equal-size cells, genuinely transparent background, unchanged reference origin and logical pixel scale, safe margins, no grid or labels. ENEMY UNIT assets only.
```

## 사용 순서

- 먼저 「1. 첫 기본 스프라이트」만 사용한다. 기본 외형과 여러 동작을 한꺼번에 요청하지 않는다.
- 적 공통 규칙을 전달하고, 확정한 기본 적 이미지와 필요한 동작 하나만 함께 요청한다.
- 결과물의 실제 접촉 자세를 확인한 뒤 Unity 공격 이벤트를 맞춘다. 여기서는 스크립트를 작성·수정하지 않는다.
