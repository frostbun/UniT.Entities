#nullable enable
namespace UniT.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniT.DI;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.Pooling;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class EntityManager : IEntityManager
    {
        #region Constructor

        private readonly IDependencyContainer container;
        private readonly IObjectPoolManager   objectPoolManager;
        private readonly ILogger              logger;

        private readonly HashSet<object>                       trackingKeys            = new();
        private readonly HashSet<GameObject>                   trackingPrefabs         = new();
        private readonly Dictionary<GameObject, IEntity>       objToEntity             = new();
        private readonly Dictionary<IEntity, IComponent[]>     entityToComponents      = new();
        private readonly Dictionary<IComponent, Type[]>        componentToTypes        = new();
        private readonly Dictionary<Type, HashSet<IComponent>> typeToSpawnedComponents = new();

        [Preserve]
        public EntityManager(IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.container         = container;
            this.objectPoolManager = objectPoolManager;
            this.logger            = loggerManager.GetLogger(this);

            this.objectPoolManager.Instantiated += this.OnInstantiated;
            this.objectPoolManager.Spawned      += this.OnSpawned;
            this.objectPoolManager.Recycled     += this.OnRecycled;
            this.objectPoolManager.CleanedUp    += this.OnCleanedUp;

            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Instantiated { add => this.instantiated += value; remove => this.instantiated -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Spawned      { add => this.spawned += value;      remove => this.spawned -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Recycled     { add => this.recycled += value;     remove => this.recycled -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.CleanedUp    { add => this.cleanedUp += value;    remove => this.cleanedUp -= value; }

        void IEntityManager.Load(IEntity prefab, int count)
        {
            this.trackingPrefabs.Add(prefab.gameObject);
            this.objectPoolManager.Load(prefab.gameObject, count);
        }

        #if !UNITY_WEBGL
        void IEntityManager.Load(object key, int count)
        {
            this.trackingKeys.Add(key);
            this.objectPoolManager.Load(key, count);
        }
        #endif

        #if UNIT_UNITASK
        UniTask IEntityManager.LoadAsync(object key, int count, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        }
        #else
        IEnumerator IEntityManager.LoadAsync(object key, int count, Action? callback, IProgress<float>? progress)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, callback, progress);
        }
        #endif

        TEntity IEntityManager.Spawn<TEntity>(TEntity prefab, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn<TEntity>(prefab.gameObject, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TEntity>(prefab.gameObject, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity>(object key, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn<TEntity>(key, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(object key, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TEntity>(key, position, rotation, parent, spawnInWorldSpace);
        }

        void IEntityManager.Recycle(IEntity entity) => this.objectPoolManager.Recycle(entity.gameObject);

        void IEntityManager.RecycleAll(IEntity prefab) => this.objectPoolManager.RecycleAll(prefab.gameObject);

        void IEntityManager.RecycleAll(object key) => this.objectPoolManager.RecycleAll(key);

        void IEntityManager.Cleanup(IEntity prefab, int retainCount) => this.objectPoolManager.Cleanup(prefab.gameObject, retainCount);

        void IEntityManager.Cleanup(object key, int retainCount) => this.objectPoolManager.Cleanup(key, retainCount);

        void IEntityManager.Unload(IEntity prefab)
        {
            this.trackingPrefabs.Remove(prefab.gameObject);
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IEntityManager.Unload(object key)
        {
            this.trackingKeys.Remove(key);
            this.objectPoolManager.Unload(key);
        }

        IEnumerable<T> IEntityManager.Query<T>()
        {
            return this.typeToSpawnedComponents.GetOrDefault(typeof(T))?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        #endregion

        #region Private

        private Action<IEntity, IReadOnlyList<IComponent>>? instantiated;
        private Action<IEntity, IReadOnlyList<IComponent>>? spawned;
        private Action<IEntity, IReadOnlyList<IComponent>>? recycled;
        private Action<IEntity, IReadOnlyList<IComponent>>? cleanedUp;

        private object nextParams = null!;

        private void OnInstantiated(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            this.objToEntity.Add(instance, entity);
            var components = entity.GetComponentsInChildren<IComponent>();
            this.entityToComponents.Add(entity, components);
            foreach (var component in components.AsSpan())
            {
                this.componentToTypes.Add(
                    component,
                    component.GetType()
                        .GetInterfaces()
                        .Prepend(component.GetType())
                        .ToArray()
                );
                component.Container = this.container;
                component.Manager   = this;
                component.Entity    = entity;
            }
            foreach (var component in components.AsSpan()) component.OnInstantiate();
            this.instantiated?.Invoke(entity, components);
        }

        private void OnSpawned(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var entity)) return;
            if (entity is IEntityWithParams entityWithParams)
            {
                entityWithParams.Params = this.nextParams;
            }
            var components = this.entityToComponents[entity];
            foreach (var component in components.AsSpan())
            {
                foreach (var type in this.componentToTypes[component].AsSpan())
                {
                    this.typeToSpawnedComponents.GetOrAdd(type).Add(component);
                }
            }
            foreach (var component in components.AsSpan()) component.OnSpawn();
            this.spawned?.Invoke(entity, components);
        }

        private void OnRecycled(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var entity)) return;
            var components = this.entityToComponents[entity];
            foreach (var component in components.AsSpan())
            {
                foreach (var type in this.componentToTypes[component].AsSpan())
                {
                    this.typeToSpawnedComponents[type].Remove(component);
                }
            }
            foreach (var component in components.AsSpan()) component.OnRecycle();
            if (entity is IEntityWithParams entityWithParams)
            {
                entityWithParams.Params = null;
            }
            this.recycled?.Invoke(entity, components);
        }

        private void OnCleanedUp(GameObject instance)
        {
            if (!this.objToEntity.Remove(instance, out var entity)) return;
            this.entityToComponents.Remove(entity, out var components);
            this.componentToTypes.RemoveRange(components);
            foreach (var component in components.AsSpan()) component.OnCleanup();
            this.cleanedUp?.Invoke(entity, components);
        }

        #endregion

        #region Finalizer

        private void Dispose()
        {
            this.trackingKeys.Clear(this.objectPoolManager.Unload);
            this.trackingPrefabs.Clear(this.objectPoolManager.Unload);
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
            this.logger.Debug("Disposed");
        }

        ~EntityManager()
        {
            this.Dispose();
            this.logger.Debug("Finalized");
        }

        #endregion
    }
}