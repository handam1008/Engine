using DevLib.ServiceLocator;
using RYU.Script.Service.StorageService;
using UnityEngine;

namespace RYU.Script.Tower
{
    public class TowerHealth : MonoBehaviour
    {
        [SerializeField] private Transform virusFill;
        [SerializeField] private Transform storageFill;
        [SerializeField] private int barPixels = 80;
        [SerializeField] private float pixelSize = 0.01f;
        [SerializeField] private float pixelsPerSecond = 60f;

        private HealthSystem _health;
        private IStorageService _storage;
        private float _virusShown;
        private float _storageShown;
        private float _originX;

        public float VirusRatio => _health._maxHealth > 0 ? 1f - (float)_health.Health / _health._maxHealth : 0f;

        private void Awake()
        {
            _health = GetComponent<HealthSystem>();
            _originX = virusFill.localPosition.x;
        } 

        private void Start()
        {
            _storage = ServiceLocator.Get<IStorageService>();
        }

        private void OnEnable()
        {
            _virusShown = 0f;
            _storageShown = 0f;
        }

        private void Update()
        {
            int virusTarget = Mathf.RoundToInt(Mathf.Clamp01(VirusRatio) * barPixels);
            float storageRatio = (float)_storage.CurrentStorage / _storage.MaxStorage;
            int storageTarget = Mathf.RoundToInt(Mathf.Clamp01(storageRatio) * barPixels);

            float step = pixelsPerSecond * Time.deltaTime;
            _virusShown = Mathf.MoveTowards(_virusShown, virusTarget, step);
            _storageShown = Mathf.MoveTowards(_storageShown, storageTarget, step);

            int virusPixels = Mathf.RoundToInt(_virusShown);
            int storagePixels = Mathf.Min(Mathf.RoundToInt(_storageShown), barPixels - virusPixels);
            SetFill(virusFill, 0, virusPixels);
            SetFill(storageFill, virusPixels, storagePixels);
        }

        private void SetFill(Transform fill, int startPixel, int pixels)
        {
            fill.gameObject.SetActive(pixels > 0);

            Vector3 pos = fill.localPosition;
            pos.x = _originX + startPixel * pixelSize;
            fill.localPosition = pos;

            Vector3 scale = fill.localScale;
            scale.x = pixels;
            fill.localScale = scale;
        }
    }
}
