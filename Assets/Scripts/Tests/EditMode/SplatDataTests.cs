using System;
using NUnit.Framework;
using TeleportNative.Rendering;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class SplatDataTests
    {
        [Test]
        public void Parses_single_splat()
        {
            var b = new byte[32];
            Write(b, 0, 1f); Write(b, 4, 2f); Write(b, 8, 3f);       // position
            Write(b, 12, 0.1f); Write(b, 16, 0.1f); Write(b, 20, 0.1f); // scale
            b[24] = 255; b[25] = 128; b[26] = 0; b[27] = 255;        // rgba
            b[28] = 127; b[29] = 127; b[30] = 127; b[31] = 255;       // rot

            var d = SplatData.ParseSplat(b);
            Assert.IsNotNull(d);
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(1f, d.Positions[0].x, 0.001f);
            Assert.AreEqual(2f, d.Positions[0].y, 0.001f);
            Assert.AreEqual(3f, d.Positions[0].z, 0.001f);
        }

        [Test]
        public void Returns_null_for_too_few_bytes() =>
            Assert.IsNull(SplatData.ParseSplat(new byte[10]));

        private static void Write(byte[] b, int o, float f)
        {
            var a = BitConverter.GetBytes(f);
            for (int i = 0; i < 4; i++) b[o + i] = a[i];
        }
    }
}
