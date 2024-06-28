#nullable enable
namespace UniT.Entities.Component
{
    using UniT.Entities.Entity;
    using UniT.Extensions;

    public abstract class Component : BetterMonoBehavior, IComponent
    {
        IEntityManager IComponent.Manager { get => this.Manager; set => this.Manager = value; }

        IEntity IComponent.Entity { get => this.Entity; set => this.Entity = value; }

        public IEntityManager Manager { get; private set; } = null!;

        public IEntity Entity { get; private set; } = null!;

        void IComponent.OnInstantiate() => this.OnInstantiate();

        void IComponent.OnSpawn() => this.OnSpawn();

        void IComponent.OnRecycle() => this.OnRecycle();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }
    }
}