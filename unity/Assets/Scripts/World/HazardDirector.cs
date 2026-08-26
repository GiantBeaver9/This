using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Periodically launches environmental hazards across the lane to crank the
    /// chaos (AREAS.md §7c). Right now: cars barreling through on a random depth
    /// row from a random side — you have to watch the field AND the road.
    /// </summary>
    public sealed class HazardDirector : MonoBehaviour
    {
        public float MinInterval = 7f;
        public float MaxInterval = 13f;
        public float CarSpeed = 21f;

        private static readonly Color[] CarColors =
        {
            new(0.90f, 0.24f, 0.22f), new(0.25f, 0.45f, 0.85f),
            new(0.95f, 0.80f, 0.25f), new(0.85f, 0.85f, 0.90f),
        };

        private float _timer;

        private void Start() => _timer = Random.Range(MinInterval * 0.5f, MinInterval);

        private void Update()
        {
            if (!PlayerController.AnyAlive) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Random.Range(MinInterval, MaxInterval);

            bool fromLeft = Random.value < 0.5f;
            float edge = Tuning.ScreenWidthUnits * 0.5f + 3f;
            float x = PlayerController.MidX() + (fromLeft ? -edge : edge); // cross the shared frame
            float vel = (fromLeft ? 1f : -1f) * CarSpeed;
            float z = Random.Range(1f, Tuning.ZBandDepth - 1f);
            CarHazard.Spawn(x, z, vel, CarColors[Random.Range(0, CarColors.Length)]);
        }
    }
}
