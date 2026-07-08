using System.Linq;
using System.Reflection;
using System.Threading;
using TeleportNative.Core;
using UnityEngine;

namespace TeleportNative.Rendering
{
    /// <summary>
    /// ISplatRenderer sobre o UnityGaussianSplatting (Aras-p) via REFLECTION. Nao referencia o
    /// pacote em compile-time -> o projeto compila antes do pacote resolver. Suporta:
    ///  - Editor asset (M1, 60fps): asset ja importado atribuido ao renderer.
    ///  - Runtime .splat (M3): parseia (SplatData) e tenta criar GaussianSplatAsset via reflection.
    /// O pacote oficial NAO importa .ply/.splat em runtime (issue #157); runtime load exige
    /// importador/fork. Ver BUILD.md §Runtime Splat Loader p/ o ajuste contra a versao instalada.
    /// </summary>
    public sealed class UnityGaussianSplatAdapter : ISplatRenderer
    {
        private readonly Transform _host;
        private Component _renderer;
        private System.Type _rendererType;
        private System.Type _assetType;

        public UnityGaussianSplatAdapter(Transform host) => _host = host;

        public bool IsAvailable => ResolveRenderer();
        public bool HasAsset => IsAvailable && GetAsset() != null;

        public void ApplyEditorAsset(Object asset)
        {
            if (!ResolveRenderer() || asset == null) return;
            SetProp(_renderer, "asset", asset);
            TryInvoke(_renderer, "InitDataFromSplatAsset");
            TryInvoke(_renderer, "OnValidate");
        }

        public async Awaitable<Result<bool>> LoadFromPathAsync(string localPath)
        {
            if (!ResolveRenderer())
                return Result<bool>.Fail("GaussianSplatRenderer nao encontrado na cena. Adicione-o ao host.");

            if (string.IsNullOrEmpty(localPath) || !System.IO.File.Exists(localPath))
                return Result<bool>.Fail("arquivo de splat ausente: " + localPath);

            byte[] bytes;
            try { bytes = System.IO.File.ReadAllBytes(localPath); }
            catch (System.Exception e) { return Result<bool>.Fail("leitura: " + e.Message); }

            // So o formato .splat cru e suportado aqui; .ksplat/.ply exigem importador dedicado.
            var data = SplatData.ParseSplat(bytes);
            if (data == null) return Result<bool>.Fail(".splat invalido ou vazio");

            var asset = TryCreateRuntimeAsset(data);
            if (asset == null)
                return Result<bool>.Fail(
                    "runtime import indisponivel nesta versao do pacote. Converta para .ksplat no " +
                    "editor (M1) ou plugue um importador/fork (ver BUILD.md).");

            ApplyEditorAsset(asset);
            await Awaitable.EndOfFrameAsync(default);
            return Result<bool>.Ok(true);
        }

        public void SetBudgetFraction(float fraction)
        {
            if (!ResolveRenderer()) return;
            // Tenta as propriedades de cutout/opacity conhecidas do renderer do Aras.
            TrySetFloat(_renderer, "cutout", 1f - Mathf.Clamp01(fraction));
            TrySetFloat(_renderer, "globalOpacity", Mathf.Clamp01(fraction));
        }

        public void SetPaused(bool paused)
        {
            if (!ResolveRenderer()) return;
            _renderer.gameObject.SetActive(!paused);
        }

        // --- reflexao sobre o pacote ---

        private bool ResolveRenderer()
        {
            if (_renderer != null) return true;
            _assetType = System.Type.GetType("GaussianSplatting.GaussianSplatAsset, GaussianSplatting");
            _rendererType = System.Type.GetType("GaussianSplatting.GaussianSplatRenderer, GaussianSplatting");
            if (_host == null) return false;
            _renderer = _host.GetComponents<Component>().FirstOrDefault(c => c.GetType().Name == "GaussianSplatRenderer");
            return _renderer != null;
        }

        private Object GetAsset() => ResolveRenderer() ? GetProp(_renderer, "asset") as Object : null;

        private Object TryCreateRuntimeAsset(SplatData data)
        {
            if (_assetType == null) return null;
            // Procura uma fabrica estatica CreateAsset(...) no pacote. Assinatura varia; tentamos
            // a mais comum e deixamos o ajuste documentado. Retorna null se nao casar.
            var methods = _assetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "CreateAsset");
            foreach (var m in methods)
            {
                var p = m.GetParameters();
                // Exemplo conhecido: CreateAsset(int count, NativeArray<Vector3> pos, ...)
                // Encaminhamos os arrays como NativeArray; se a assinatura nao casar, ignora.
                if (p.Length >= 4 && p[0].ParameterType == typeof(int))
                {
                    try
                    {
                        var posNa = ToNativeVector3(data.Positions);
                        var scaleNa = ToNativeVector3(data.Scales);
                        var rotNa = ToNativeQuaternion(data.Rotations);
                        var colNa = ToNativeColor32(data.Colors);
                        var args = BuildCreateArgs(m, data.Count, posNa, scaleNa, rotNa, colNa);
                        if (args == null) continue;
                        var result = m.Invoke(null, args);
                        return result as Object;
                    }
                    catch { /* assinatura diferente: tenta proxima */ }
                }
            }
            return null;
        }

        private static object[] BuildCreateArgs(MethodInfo m, int count,
            object pos, object scale, object rot, object col)
        {
            var pars = m.GetParameters();
            var args = new object[pars.Length];
            for (int i = 0; i < pars.Length; i++)
            {
                string n = pars[i].Name.ToLowerInvariant();
                args[i] = n switch
                {
                    var s when s.Contains("count") => count,
                    var s when s.Contains("pos") => pos,
                    var s when s.Contains("scale") => scale,
                    var s when s.Contains("rot") => rot,
                    var s when s.Contains("col") => col,
                    _ => pars[i].ParameterType.IsValueType ? System.Activator.CreateInstance(pars[i].ParameterType) : null
                };
            }
            return args;
        }

        private static object ToNativeVector3(Vector3[] a) => MakeNative(a, typeof(Vector3));
        private static object ToNativeQuaternion(Quaternion[] a) => MakeNative(a, typeof(Quaternion));
        private static object ToNativeColor32(Color32[] a) => MakeNative(a, typeof(Color32));

        private static object MakeNative(System.Array src, System.Type elemType)
        {
            var naType = typeof(Unity.Collections.NativeArray<>).MakeGenericType(elemType);
            var ctor = naType.GetConstructor(new[] { src.GetType(), typeof(Unity.Collections.Allocator) });
            return ctor?.Invoke(new object[] { src, Unity.Collections.Allocator.Persistent });
        }

        private static object GetProp(object o, string name)
            => o.GetType().GetProperty(name)?.GetValue(o);

        private static void SetProp(object o, string name, object value)
        { o.GetType().GetProperty(name)?.SetValue(o, value); }

        private static void TryInvoke(object o, string name)
        { o.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(o, null); }

        private static void TrySetFloat(object o, string name, float value)
        {
            var p = o.GetType().GetProperty(name);
            if (p != null && p.PropertyType == typeof(float)) p.SetValue(o, value);
        }
    }
}
