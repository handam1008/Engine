using System.Collections.Generic;
using KSM._00.Scripts;
using UnityEngine;

namespace RYU.Script.Charcter.MousePointer
{
    public class MousePointerCharacter : AbstractUnit
    {
        [SerializeField] private SpriteRenderer selectionBox;
        [SerializeField] private Material selectionMaterial;

        private readonly List<(AbstractUnit Enemy, Rigidbody2D Body, Material Material)> _selected = new();
        private Vector2 _lastBoxPosition;
        private float _boxStartY;
        private bool _used;
        private bool _dragging;

        public void BeginDrag() => _used = true;

        private void SelectEnemies()
        {
            _dragging = true;   
            _lastBoxPosition = selectionBox.transform.position;
            _boxStartY = _lastBoxPosition.y;
            Bounds bounds = selectionBox.bounds;
            foreach (var collider in Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f, UnitSo.whatIsDetect))
            {
                var body = collider.attachedRigidbody;
                if (body is null || !body.TryGetComponent(out AbstractUnit enemy) || enemy.UnitSo is not EnemySO)
                    continue;

                _selected.Add((enemy, body, enemy.RenderCompo.sharedMaterial));
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
                enemy.RenderCompo.sharedMaterial = selectionMaterial;
            }
        }

        private void LateUpdate()
        {
            if (!_used)
                return;

            if (StateMachine.CurrentState != StateMachine.Dict[State.Attack])
            {
                Release(false);
                if (!HpCompo.Dead)
                    HpCompo.Damage(HpCompo.Health);
                return;
            }

            if (!_dragging)
            {
                if (selectionBox.enabled)
                    SelectEnemies();
                return;
            }

            Vector2 boxPosition = selectionBox.transform.position;
            foreach (var (_, body, _) in _selected)
            {
                if (!body) return;
                body.transform.position += (Vector3)(boxPosition - _lastBoxPosition);
            }
            _lastBoxPosition = boxPosition;

            if (selectionBox.enabled)
                return;

            Release(true);
            MoveCompo.RbCompo.simulated = false;
            transform.position = AnimCompo.transform.position;
            AnimCompo.transform.localPosition = Vector3.zero;
            HpCompo.Damage(HpCompo.Health);
        }

        private void OnDisable() => Release(false);

        private void Release(bool landed)
        {
            if (!_dragging)
                return;

            _dragging = false;
            foreach (var (enemy, body, material) in _selected)
            {
                if (!body)
                    continue;

                body.transform.position += Vector3.up * (_boxStartY - _lastBoxPosition.y);
                body.position = body.transform.position;
                body.linearVelocity = Vector2.zero;
                body.simulated = true;
                enemy.RenderCompo.sharedMaterial = material;
                if (landed)
                    enemy.HpCompo.Damage(UnitSo.attackDmg);
            }
            _selected.Clear();
        }
    }
}

