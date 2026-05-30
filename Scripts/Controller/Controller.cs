#nullable enable
namespace UniT.Entities.Controller
{
    public abstract class Controller<TComponent> : IController where TComponent : IComponentWithController
    {
        IComponent IController.Component { set => this.Component = (TComponent)value; }

        protected TComponent Component { get; private set; } = default!;

        protected IEntityManager Manager => this.Component.Manager;

        protected IEntity Entity => this.Component.Entity;

        void IComponentLifecycle.OnInstantiate() => this.OnInstantiate();

        void IComponentLifecycle.OnSpawn() => this.OnSpawn();

        void IComponentLifecycle.OnRecycle() => this.OnRecycle();

        void IComponentLifecycle.OnCleanup() => this.OnCleanup();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }

        protected virtual void OnCleanup() { }
    }
}