using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A STATIC blocker the player has to walk around — parked cars, mall kiosks, crates
    /// (creator: "objects in the way"). Has a rectangular footprint in the X-Z plane; the
    /// player's movement is pushed out of it (see <see cref="Resolve"/>). Generic placeholders
    /// for now; real per-area props can swap in via the sprite passed to <see cref="Spawn"/>.
    /// </summary>
    public sealed class Obstacle : MonoBehaviour
    {
        public static readonly List<Obstacle> All = new();

        public float X, Z, HalfX, HalfZ;
        private SpriteRenderer _sr;

        public static Obstacle Spawn(Sprite spr, float x, float z, float halfX, float halfZ, Vector2 scale)
        {
            var go = new GameObject("obstacle");
            var o = go.AddComponent<Obstacle>();
            o.X = x; o.Z = z; o.HalfX = halfX; o.HalfZ = halfZ;
            o._sr = go.AddComponent<SpriteRenderer>();
            o._sr.sprite = spr;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            return o;
        }

        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        private void LateUpdate()
        {
            Playfield.Place(transform, X, Z, _sr);
            if (_sr != null) _sr.sortingOrder = Playfield.SortingOrder(Z); // depth-sorted like an actor on its row
        }

        /// <summary>True if (x,z) sits inside any obstacle footprint — used to absorb projectiles so
        /// parked cars / crates act as cover from gunfire.</summary>
        public static bool Blocks(float x, float z)
        {
            foreach (var o in All)
            {
                if (o == null) continue;
                if (x > o.X - o.HalfX && x < o.X + o.HalfX && z > o.Z - o.HalfZ && z < o.Z + o.HalfZ) return true;
            }
            return false;
        }

        /// <summary>Push (<paramref name="x"/>,<paramref name="z"/>) out of any obstacle footprint
        /// (expanded by the mover's <paramref name="rad"/>), along whichever axis it's least inside —
        /// so you slide along the object instead of walking through it.</summary>
        public static void Resolve(ref float x, ref float z, float rad)
        {
            foreach (var o in All)
            {
                if (o == null) continue;
                float minX = o.X - o.HalfX - rad, maxX = o.X + o.HalfX + rad;
                float minZ = o.Z - o.HalfZ - rad, maxZ = o.Z + o.HalfZ + rad;
                if (x <= minX || x >= maxX || z <= minZ || z >= maxZ) continue; // fully outside → no hit
                float overlapX = Mathf.Min(x - minX, maxX - x);
                float overlapZ = Mathf.Min(z - minZ, maxZ - z);
                if (overlapX < overlapZ) x = (x > o.X) ? maxX : minX;          // eject in X
                else                     z = (z > o.Z) ? maxZ : minZ;          // eject in Z
            }
        }
    }
}
