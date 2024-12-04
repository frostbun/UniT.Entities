#nullable enable
namespace UniT.Entities.Controller
{
    public interface IController
    {
        public IComponent Component { set; }

        public void OnInstantiate();

        public void OnSpawn();

        public void OnRecycle();
    }
}