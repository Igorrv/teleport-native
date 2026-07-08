using NUnit.Framework;
using TeleportNative.Capture;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class BlurDetectorTests
    {
        [Test]
        public void Uniform_image_is_low_sharpness()
        {
            int w = 8, h = 8;
            var g = new float[w * h];
            for (int i = 0; i < g.Length; i++) g[i] = 0.5f;
            Assert.Less(BlurDetector.Sharpness(w, h, g), 1f);
        }

        [Test]
        public void Checkerboard_is_higher_sharpness()
        {
            int w = 8, h = 8;
            var g = new float[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    g[y * w + x] = (x + y) % 2;
            Assert.Greater(BlurDetector.Sharpness(w, h, g), 1f);
        }
    }
}
