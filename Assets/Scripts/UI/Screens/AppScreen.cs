using UnityEngine;

namespace TeleportNative.UI
{
    /// <summary>Tela base. Cada tela constroi sua UI em codigo (Build) e reage a Show/Hide.</summary>
    public abstract class AppScreen : MonoBehaviour
    {
        protected AppContext Ctx;
        public RectTransform Root => (RectTransform)transform;

        public void Init(AppContext ctx)
        {
            Ctx = ctx;
            UIFactory.Stretch(Root);
            Build();
        }

        protected abstract void Build();
        public virtual void OnShow() { }
        public virtual void OnHide() { }

        public void Show() { gameObject.SetActive(true); OnShow(); }
        public void Hide() { OnHide(); gameObject.SetActive(false); }

        protected static void Stretch(RectTransform rt) => UIFactory.Stretch(rt);
    }
}
