#if UNIT_DI
#nullable enable
namespace UniT.Entities
{
    using UniT.DI;
    using UniT.Logging;
    using UniT.Pooling;

    public static class DIBinder
    {
        public static void AddEntityManager(this DependencyContainer container)
        {
            if (container.Contains<IEntityManager>()) return;
            container.AddLoggerManager();
            container.AddObjectPoolManager();
            container.AddInterfaces<EntityManager>();
        }
    }
}
#endif