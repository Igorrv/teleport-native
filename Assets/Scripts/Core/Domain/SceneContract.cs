using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeleportNative.Core
{
    /// <summary>
    /// Contrato `scene` do HomeView (ver skill homeview-reference). O app nativo consome
    /// principalmente `splat_url`. Mantido compativel para reuso integral do backend.
    /// </summary>
    public enum SceneType { Walkthrough, Splat }

    public sealed class SceneContract
    {
        [JsonProperty("version")] public int Version = 1;
        [JsonProperty("type")] public string Type = "splat";
        [JsonProperty("provider")] public string Provider;
        [JsonProperty("splat_url")] public string SplatUrl;
        [JsonProperty("thumbnail_url")] public string ThumbnailUrl;

        [JsonProperty("capture")] public CaptureInfo Capture;
        [JsonProperty("points")] public List<ScenePoint> Points = new();
        [JsonProperty("links")] public List<SceneLink> Links = new();
        [JsonProperty("exports")] public SceneExports Exports;
    }

    public sealed class CaptureInfo
    {
        [JsonProperty("fov")] public float Fov = 60f;
        [JsonProperty("ultrawide")] public bool Ultrawide;
    }

    public sealed class ScenePoint
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("index")] public int Index;
        [JsonProperty("image_url")] public string ImageUrl;
        [JsonProperty("position")] public Vec3 Position;
        [JsonProperty("yaw")] public float Yaw;
    }

    public sealed class SceneLink
    {
        [JsonProperty("from")] public string From;
        [JsonProperty("to")] public string To;
    }

    public sealed class SceneExports
    {
        [JsonProperty("obj")] public string Obj;
        [JsonProperty("glb")] public string Glb;
        [JsonProperty("fbx")] public string Fbx;
    }

    public sealed class Vec3 { public float x, y, z; }
}
