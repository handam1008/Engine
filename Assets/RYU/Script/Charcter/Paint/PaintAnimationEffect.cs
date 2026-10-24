using System;
using UnityEngine;

namespace RYU.Script.Charcter.Paint
{
    public class PaintAnimationEffect : MonoBehaviour
    {
        [SerializeField] private Transform brushTip;
        [SerializeField] private GameObject[] dripPrefabs;

        public void Drip(int colorIndex)
        {
            Instantiate(dripPrefabs[colorIndex], brushTip.position, Quaternion.identity, brushTip);
        }
    }
}