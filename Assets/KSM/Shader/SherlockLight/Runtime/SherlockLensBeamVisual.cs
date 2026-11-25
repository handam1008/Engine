using UnityEngine;
using UnityEngine.Rendering;

namespace KSM.SherlockLight
{
    [DisallowMultipleComponent]
    public sealed class SherlockLensBeamVisual : MonoBehaviour
    {
        private static readonly int Progress = Shader.PropertyToID("_Progress");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int StartFeather = Shader.PropertyToID("_StartFeather");
        private static readonly int ApertureRatio = Shader.PropertyToID("_ApertureRatio");
        // Sherlock's 362 px sprite rect contains a 260 px diameter lens and the handle.
        private const float LensRadiusFraction = 130f / 362f;

        private LineRenderer line;
        private AbstractUnit owner;
        private SpriteRenderer lens;
        private MaterialPropertyBlock properties;
        private SherlockBeamLighting lighting;
        private float endWidth;
        private Vector2 lensUV;
        private float rise;
        private float fall;
        private float peak;
        private float startFeather;
        private float start;
        private float releaseStart;
        private float releaseStrength;
        private float level;
        private float progress;
        private bool releasing;
        private Transform lastTarget;

        public void Begin(LineRenderer renderer, AbstractUnit attacker, SpriteRenderer glass, Vector2 opticalCenter,
            Material material, float width, float lightIntensity, float lightAngle,
            float revealSeconds, float releaseSeconds)
        {
            line = renderer;
            owner = attacker;
            lens = glass;
            lensUV = opticalCenter;
            rise = Mathf.Max(0.01f, revealSeconds);
            fall = Mathf.Max(0.01f, releaseSeconds);
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.textureMode = LineTextureMode.Stretch;
            line.textureScale = Vector2.one;
            endWidth = Mathf.Max(0.05f, width);
            line.widthMultiplier = endWidth;
            // A rectangular ribbon interpolates the transverse UV uniformly across both triangles.
            // The graph supplies the taper around the aim axis.
            line.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            line.numCapVertices = 0;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            if (lens)
            {
                line.sortingLayerID = lens.sortingLayerID;
                line.sortingOrder = lens.sortingOrder + 1;
            }
            peak = material.GetFloat(Intensity);
            startFeather = material.GetFloat(StartFeather);
            properties = new MaterialPropertyBlock();
            line.GetPropertyBlock(properties);
            if (lightIntensity > 0f)
                lighting = new SherlockBeamLighting(transform, lightAngle, lightIntensity);
            start = Time.time;
            ApplyLight(0f, 0f);
        }

        private void LateUpdate()
        {
            if (!line || line.positionCount < 2)
                return;

            if (releasing)
            {
                float elapsed = Mathf.Clamp01((Time.time - releaseStart) / fall);
                ApplyLight(progress, releaseStrength * (1f - Mathf.SmoothStep(0f, 1f, elapsed)));
                if (elapsed >= 1f)
                    Destroy(gameObject);
                return;
            }

            if (owner && owner.target && owner.target != lastTarget)
            {
                if (!ReferenceEquals(lastTarget, null))
                    start = Time.time;
                lastTarget = owner.target;
            }

            Vector3 target = line.GetPosition(line.positionCount - 1);
            bool hasLens = lens && lens.sprite;
            Vector3 center = hasLens ? OpticalCenter() : line.GetPosition(0);
            Vector2 axis = target - center;
            float centerDistance = axis.magnitude;
            if (centerDistance < 0.0001f)
            {
                ApplyLight(0f, 0f);
                return;
            }

            Vector2 direction = axis / centerDistance;
            float rim = hasLens ? LensRadius(direction) : 0f;
            if (centerDistance <= rim + 0.0001f)
            {
                ApplyLight(0f, 0f);
                return;
            }

            // Draw the visible beam from the optical center. Scene illumination starts past the glass.
            line.SetPosition(0, center);
            float length = centerDistance;
            float lightOffset = 1.07f * rim;
            Vector3 lightSource = lightOffset < centerDistance ? center + (Vector3)direction * lightOffset : target;
            lighting?.SetPath(lightSource, target);

            float aperture = hasLens
                ? Mathf.Min(endWidth, 0.78f * LensRadius(new Vector2(-direction.y, direction.x)))
                : endWidth * 0.3f;
            float width = Mathf.Max(0.001f, Mathf.LerpUnclamped(aperture, endWidth, length / 7f));
            line.widthMultiplier = width;
            properties.SetFloat(ApertureRatio, Mathf.Clamp01(aperture / width));
            properties.SetFloat(StartFeather, Mathf.Min(startFeather, 0.06f / length));
            float phase = Mathf.Clamp01((Time.time - start) / rise);
            ApplyLight(Mathf.SmoothStep(0f, 1f, phase), peak * Mathf.SmoothStep(0f, 1f, phase * 2f));
        }

        private float LensRadius(Vector2 worldDirection)
        {
            Sprite sprite = lens.sprite;
            Vector2 radii = sprite.rect.size * (LensRadiusFraction / sprite.pixelsPerUnit);
            Vector3 localDirection = lens.transform.InverseTransformVector(worldDirection);
            float x = localDirection.x / radii.x;
            float y = localDirection.y / radii.y;
            float inverseRadius = Mathf.Sqrt(x * x + y * y);
            return inverseRadius > 0.0001f ? 1f / inverseRadius : 0f;
        }

        private Vector3 OpticalCenter()
        {
            Sprite sprite = lens.sprite;
            Vector2 point = (Vector2.Scale(sprite.rect.size, lensUV) - sprite.pivot) / sprite.pixelsPerUnit;
            if (lens.flipX) point.x = -point.x;
            if (lens.flipY) point.y = -point.y;
            return lens.transform.TransformPoint(point);
        }

        public void Release(Transform beamRoot)
        {
            if (level <= 0f)
            {
                line.enabled = false;
                enabled = false;
                return;
            }
            if (transform == beamRoot)
                return;
            transform.SetParent(null, true);
            releasing = true;
            releaseStart = Time.time;
            releaseStrength = level;
        }

        private void ApplyLight(float newProgress, float brightness)
        {
            progress = newProgress;
            level = brightness;
            properties.SetFloat(Progress, progress);
            properties.SetFloat(Intensity, level);
            line.SetPropertyBlock(properties);
            lighting?.SetLevels(progress, level);
        }

        private void OnDisable()
        {
            lighting?.Dispose();
            lighting = null;
        }
    }
}
