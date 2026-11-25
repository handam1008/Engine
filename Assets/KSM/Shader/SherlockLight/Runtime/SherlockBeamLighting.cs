using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace KSM.SherlockLight
{
    // One URP light per active beam. The 2D renderer switches every Sprite-Lit
    // layer from unlit to lit when the scene gains its first registered light.
    internal sealed class SherlockBeamLighting
    {
        private static Light2D whiteBase;
        private static int baseUsers;
        private readonly Light2D cone;
        private readonly float strength;
        private readonly bool usesWhiteBase;
        private float distance;

        public SherlockBeamLighting(Transform parent, float angle, float intensity)
        {
            usesWhiteBase = AcquireWhiteBase();
            strength = intensity;
            var lightObject = new GameObject("Lens illumination");
            lightObject.SetActive(false);
            lightObject.transform.SetParent(parent, false);
            cone = lightObject.AddComponent<Light2D>();
            cone.lightType = Light2D.LightType.Point;
            cone.blendStyleIndex = 1; // Renderer2D.asset's Additive style.
            cone.overlapOperation = Light2D.OverlapOperation.Additive;
            cone.color = new Color(1f, 0.84f, 0.30f, 1f);
            cone.intensity = 0f;
            cone.pointLightOuterAngle = angle;
            cone.pointLightInnerAngle = angle * 0.42f;
            cone.pointLightOuterRadius = 0.01f;
            cone.pointLightInnerRadius = 0f;
            cone.falloffIntensity = 0.5f;
            cone.targetSortingLayers = Array.ConvertAll(
                Array.FindAll(SortingLayer.layers, layer => layer.name != "UI"), layer => layer.id);
            lightObject.SetActive(true);
        }

        private static bool AcquireWhiteBase()
        {
            if (!whiteBase)
            {
                foreach (Light2D existing in Object.FindObjectsByType<Light2D>(FindObjectsInactive.Exclude))
                {
                    if (existing.isActiveAndEnabled)
                        return false;
                }

                var go = new GameObject("Sherlock beam | neutral scene light");
                go.SetActive(false);
                Object.DontDestroyOnLoad(go);
                whiteBase = go.AddComponent<Light2D>();
                whiteBase.lightType = Light2D.LightType.Global;
                whiteBase.blendStyleIndex = 0; // White Multiply reproduces the no-light baseline.
                whiteBase.color = Color.white;
                whiteBase.intensity = 1f;
                whiteBase.targetSortingLayers = Array.ConvertAll(SortingLayer.layers, layer => layer.id);
                baseUsers = 0;
                go.SetActive(true);
            }
            baseUsers++;
            return true;
        }

        public void SetPath(Vector3 start, Vector3 end)
        {
            Vector2 axis = end - start;
            distance = axis.magnitude;
            float rotation = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90f;
            cone.transform.SetPositionAndRotation(start, Quaternion.Euler(0f, 0f, rotation));
        }

        public void SetLevels(float progress, float level)
        {
            float radius = Mathf.Max(0.01f, distance * progress * 1.12f);
            cone.pointLightOuterRadius = radius;
            cone.pointLightInnerRadius = radius * 0.78f;
            cone.intensity = distance > 0f ? strength * level : 0f;
        }

        public void Dispose()
        {
            if (cone)
            {
                cone.enabled = false;
                Object.Destroy(cone.gameObject);
            }
            if (usesWhiteBase && whiteBase && --baseUsers == 0)
            {
                whiteBase.enabled = false;
                Object.Destroy(whiteBase.gameObject);
                whiteBase = null;
            }
        }
    }
}
