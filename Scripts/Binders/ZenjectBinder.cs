#if UNIT_ZENJECT
#nullable enable
namespace UniT.Entities
{
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindEntityManager(this DiContainer container)
        {
            container.BindInterfacesTo<EntityManager>().AsSingle();
        }
    }
}
#endif