using UnityEngine;

namespace TeleportNative.Capture
{
    /// <summary>
    /// Deteccao de blur por variancia do Laplaciano (proxy de nitidez), em escala de cinza
    /// downscaled. Logica pura sobre um array de luminancia (testavel sem Texture2D).
    /// Score alto = nitido. Em GPU/Job no futuro (M2++) para 0 custo no main.
    /// </summary>
    public static class BlurDetector
    {
        /// <summary>Variancia da resposta do Laplaciano [0 1 0;1 -4 1;0 1 0].</summary>
        public static float Sharpness(int w, int h, float[] gray)
        {
            if (w < 3 || h < 3 || gray == null || gray.Length < w * h) return float.MaxValue;

            double sum = 0, sumSq = 0, n = 0;
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int i = y * w + x;
                    float lap = gray[i - w] + gray[i + w] + gray[i - 1] + gray[i + 1] - 4f * gray[i];
                    sum += lap; sumSq += (double)lap * lap; n++;
                }
            }
            if (n <= 0) return 0f;
            double mean = sum / n;
            return (float)((sumSq / n) - mean * mean);
        }

        /// <summary>Downscale (caixa) para ~targetW e computa sharpness.USA GetPixels (main).</summary>
        public static float SharpnessFromTexture(Texture2D tex, int targetW = 96)
        {
            if (tex == null) return float.MaxValue;
            int srcW = tex.width, srcH = tex.height;
            if (srcW < 3 || srcH < 3) return float.MaxValue;

            int dstW = targetW, dstH = Mathf.RoundToInt(targetW * srcH / (float)srcW);
            Color[] px = tex.GetPixels();
            float[] gray = DownscaleToGray(px, srcW, srcH, dstW, dstH);
            return Sharpness(dstW, dstH, gray);
        }

        private static float[] DownscaleToGray(Color[] src, int srcW, int srcH, int dstW, int dstH)
        {
            float[] dst = new float[dstW * dstH];
            float sx = (float)srcW / dstW, sy = (float)srcH / dstH;
            for (int y = 0; y < dstH; y++)
            {
                for (int x = 0; x < dstW; x++)
                {
                    int sx0 = Mathf.Min(srcW - 1, Mathf.FloorToInt(x * sx));
                    int sy0 = Mathf.Min(srcH - 1, Mathf.FloorToInt(y * sy));
                    Color c = src[sy0 * srcW + sx0];
                    dst[y * dstW + x] = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                }
            }
            return dst;
        }
    }
}
