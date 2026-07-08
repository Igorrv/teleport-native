using UnityEngine;

namespace TeleportNative.Rendering
{
    /// <summary>
    /// Parser do formato .splat simples (antimatter/PlayCanvas): 32 bytes/splat sem header.
    ///   pos(3f) + scale(3f) + rgba(4b) + rot(4b, quaternion 0..255). Logica pura (testavel).
    /// </summary>
    public sealed class SplatData
    {
        public int Count;
        public Vector3[] Positions;
        public Vector3[] Scales;
        public Quaternion[] Rotations;
        public Color32[] Colors;

        public static SplatData ParseSplat(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 32) return null;
            int n = bytes.Length / 32;
            var d = new SplatData
            {
                Count = n,
                Positions = new Vector3[n],
                Scales = new Vector3[n],
                Rotations = new Quaternion[n],
                Colors = new Color32[n]
            };
            for (int i = 0; i < n; i++)
            {
                int o = i * 32;
                d.Positions[i] = new Vector3(ToFloat(bytes, o), ToFloat(bytes, o + 4), ToFloat(bytes, o + 8));
                d.Scales[i] = new Vector3(ToFloat(bytes, o + 12), ToFloat(bytes, o + 16), ToFloat(bytes, o + 20));
                d.Colors[i] = new Color32(bytes[o + 24], bytes[o + 25], bytes[o + 26], bytes[o + 27]);
                d.Rotations[i] = NormalizeQuat(bytes[o + 28], bytes[o + 29], bytes[o + 30], bytes[o + 31]);
            }
            return d;
        }

        private static float ToFloat(byte[] b, int o)
        {
            // .splat e little-endian; reverte se a plataforma for big-endian.
            if (System.BitConverter.IsLittleEndian) return System.BitConverter.ToSingle(b, o);
            return System.BitConverter.ToSingle(new byte[] { b[o + 3], b[o + 2], b[o + 1], b[o] }, 0);
        }

        private static Quaternion NormalizeQuat(byte x, byte y, byte z, byte w)
        {
            var q = new Quaternion(
                (x / 127.5f) - 1f, (y / 127.5f) - 1f, (z / 127.5f) - 1f, (w / 127.5f) - 1f);
            return QuaternionNormalize(q);
        }

        private static Quaternion QuaternionNormalize(Quaternion q)
        {
            float mag = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (mag < 1e-6f) return Quaternion.identity;
            mag = 1f / mag;
            return new Quaternion(q.x * mag, q.y * mag, q.z * mag, q.w * mag);
        }
    }
}
