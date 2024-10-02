#if UNIT_VCONTAINER
#nullable enable
namespace UniT.Entities.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.Pooling.DI;
    using VContainer;

    public static class EntityManagerVContainer
    {
        public static void RegisterEntityManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IEntityManager), true)) return;
            builder.RegisterDependencyContainer();
            builder.RegisterLoggerManager();
            builder.RegisterObjectPoolManager();
            builder.Register<EntityManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif