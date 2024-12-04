#nullable enable
namespace UniT.Entities
{
    using System.Diagnostics.CodeAnalysis;
    using UniT.DI;
    using UniT.Extensions;

    public abstract class Component : BetterMonoBehavior, IComponent
    {
        IDependencyContainer IComponent.Container { set => this.Container = value; }

        IEntityManager IComponent.Manager { get => this.Manager; set => this.Manager = value; }

        IEntity IComponent.Entity { get => this.Entity; set => this.Entity = value; }

        protected IDependencyContainer Container { get; private set; } = null!;

        public IEntityManager Manager { get; private set; } = null!;

        public IEntity Entity { get; private set; } = null!;

        void IComponent.OnInstantiate() => this.OnInstantiate();

        void IComponent.OnSpawn() => this.OnSpawn();

        void IComponent.OnRecycle() => this.OnRecycle();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }

        #region Extensions

        #region Self

        public T? GetComponentOrDefault<T>() => base.GetComponent<T>();

        public new T GetComponent<T>() => this.GetComponentOrThrow<T>();

        public bool HasComponent<T>() => UnityExtensions.HasComponent<T>(this);

        #endregion

        #region Children

        public T? GetComponentInChildrenOrDefault<T>(bool includeInactive = false) => base.GetComponentInChildren<T>(includeInactive);

        public new T GetComponentInChildren<T>(bool includeInactive = false) => this.GetComponentInChildrenOrThrow<T>(includeInactive);

        public bool HasComponentInChildren<T>(bool includeInactive = false) => UnityExtensions.HasComponentInChildren<T>(this, includeInactive);

        public bool TryGetComponentInChildren<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false) => UnityExtensions.TryGetComponentInChildren(this, out component, includeInactive);

        #endregion

        #region Parent

        public T? GetComponentInParentOrDefault<T>(bool includeInactive = false) => base.GetComponentInParent<T>(includeInactive);

        public new T GetComponentInParent<T>(bool includeInactive = false) => this.GetComponentInParentOrThrow<T>(includeInactive);

        public bool HasComponentInParent<T>(bool includeInactive = false) => UnityExtensions.HasComponentInParent<T>(this, includeInactive);

        public bool TryGetComponentInParent<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false) => UnityExtensions.TryGetComponentInParent(this, out component, includeInactive);

        #endregion

        #endregion
    }
}