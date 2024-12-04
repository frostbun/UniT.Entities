#nullable enable
namespace UniT.Entities.Controller
{
    public abstract class Controller<TComponent> : IController where TComponent : IComponent
    {
        IComponent IController.Component { set => this.Component = (TComponent)value; }

        void IController.OnInstantiate() => this.OnInstantiate();

        void IController.OnSpawn() => this.OnSpawn();

        void IController.OnRecycle() => this.OnRecycle();

        protected TComponent Component { get; private set; } = default!;

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }
    }
}