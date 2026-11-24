using System.Collections;
using KSM._00.Scripts.EnemyAttack;
using UnityEngine;

namespace KSM.SherlockLight
{
    [CreateAssetMenu(menuName = "AttackSO/Sherlock Lens Light")]
    public sealed class SherlockLensAttack : EnemyBeam
    {
        [SerializeField] private Material lightMaterial = null;
        [Tooltip("Width of the visible beam at a distance of seven world units.")]
        [SerializeField, Min(0.05f)] private float width = 1.9f;
        [SerializeField, Range(0f, 2f)] private float lightIntensity = 0.65f;
        [SerializeField, Range(5f, 80f)] private float lightAngle = 24f;
        [SerializeField, Min(0.01f)] private float revealSeconds = 0.12f;
        [SerializeField, Min(0.01f)] private float releaseSeconds = 0.12f;
        [Tooltip("Path from the animated Visual object to the magnifier.")]
        [SerializeField] private string magnifierPath = "Sherlock-Magnifier_0";
        [Tooltip("Optical centre inside the sprite rectangle, measured from its bottom left.")]
        [SerializeField] private Vector2 lensUV = new Vector2(0.364f, 0.635f);

        public override void Attacking(AbstractUnit unit)
        {
            BeamEnemy beam = Instantiate(skillPrefab, unit.transform.position, Quaternion.identity);
            LineRenderer line = beam.GetComponentInChildren<LineRenderer>(true);
            if (!line || !lightMaterial)
            {
                beam.StartCoroutine(beam.Beam(unit));
                return;
            }

            SpriteRenderer lens = null;
            if (unit.AnimCompo && !string.IsNullOrEmpty(magnifierPath))
            {
                Transform mount = unit.AnimCompo.transform.Find(magnifierPath);
                if (mount)
                    lens = mount.GetComponent<SpriteRenderer>();
            }

            var visual = line.gameObject.AddComponent<SherlockLensBeamVisual>();
            visual.Begin(line, unit, lens, lensUV, lightMaterial, width, lightIntensity, lightAngle,
                revealSeconds, releaseSeconds);
            beam.StartCoroutine(RunBeam(beam, unit, visual));
        }

        private static IEnumerator RunBeam(BeamEnemy beam, AbstractUnit unit, SherlockLensBeamVisual visual)
        {
            // The original coroutine still owns the first tick, target, damage and AnimEnd.
            IEnumerator attack = beam.Beam(unit);
            while (attack.MoveNext())
                yield return attack.Current;

            if (visual)
                visual.Release(beam.transform);
        }
    }
}
