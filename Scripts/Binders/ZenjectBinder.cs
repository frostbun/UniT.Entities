#if UNIT_ZENJECT
#nullable enable
namespace UniT.Entities
{
    using UniT.Logging;
    using UniT.Pooling;
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindEntityManager(this DiContainer container)
        {
            if (container.HasBinding<IEntityManager>()) return;
            container.BindLoggerManager();
            container.BindObjectPoolManager();
            container.BindInterfacesTo<EntityManager>().AsSingle();
        }
    }
}
#endif