#if UNIT_ZENJECT
#nullable enable
namespace UniT.Entities.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.Pooling.DI;
    using Zenject;

    public static class EntityManagerZenject
    {
        public static void BindEntityManager(this DiContainer container)
        {
            if (container.HasBinding<IEntityManager>()) return;
            container.BindDependencyContainer();
            container.BindLoggerManager();
            container.BindObjectPoolManager();
            container.BindInterfacesTo<EntityManager>().AsSingle();
        }
    }
}
#endif