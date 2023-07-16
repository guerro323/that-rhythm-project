using System.Runtime.CompilerServices;
using Godot;
using revecs.Core;
using revecs.Core.Components.Boards;
using revecs.Systems;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Collections.Concurrent;
using revghost.Threading.V2.Apps;
using revtask.Core;

namespace PataNext.Export.Godot.Presentation;

// async version!

public abstract class PresentationGodotBaseSystem : PresentationBaseSystem
{
    protected IJobRunner runner;

    private IReadOnlyDomainWorker _worker;
    
    protected PresentationGodotBaseSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref runner!);
        Dependencies.Add(() => ref _worker!);
    }

    private Dictionary<UEntitySafe, Node> entitiesToProxies = new();
    private ConcurrentList<(UEntitySafe entity, Node result)> jobQueue = new();

    protected ComponentType<EntityData> GenericType;

    protected override ComponentType CreateComponentType()
    {
        ComponentType systemType;
        if ((systemType = GameWorld.GetComponentType(SystemTypeName)).Equals(default))
        {
            systemType = GameWorld.RegisterComponent(
                SystemTypeName,
                new SparseSetComponentBoard(Unsafe.SizeOf<EntityData>(), GameWorld)
            );
        }

        return GenericType = GameWorld.AsGenericComponentType<EntityData>(systemType);
    }

    protected override void OnSetPresentation(in UEntitySafe entity)
    {
        if (!OnSetPresentation(entity, out var job))
        {
            if (job.Version != 0)
                throw new InvalidOperationException("proxy shouldn't have been created if it returned false");
            
            return;
        }
        
        GameWorld.AddComponent(entity.Handle, GenericType, new EntityData
        {
            Request = job,
            Node = default
        });
        
        /*entitiesToProxies.Add(entity, proxy);

        GameWorld.AddComponent(entity.Handle, GenericType, proxy);*/
    }

    protected override void OnRemovePresentation(in UEntitySafe entity)
    {
        var node = entitiesToProxies[entity];

        if (!OnRemovePresentation(entity, node))
            return;

        node.QueueFree();
        entitiesToProxies.Remove(entity);

        if (GameWorld.Exists(entity))
        {
            // ? it was here
            //GameWorld.GetComponentData(entity.Handle, GenericType).Dispose();
            GameWorld.RemoveComponent(entity.Handle, GenericType);
        }
    }

    protected abstract bool OnSetPresentation(in UEntitySafe entity, out JobRequest job);

    protected abstract bool OnRemovePresentation(in UEntitySafe entity, in Node node);

    protected bool TryGetNode(UEntityHandle handle, out Node node)
    {
        node = GameWorld.GetComponentData(handle, GenericType).Node;
        return node != null;
    }

    private int r = 0;
    protected override void OnPresentationLoop()
    {
        var root = ((SceneTree) Engine.GetMainLoop()).Root;
        
        while (jobQueue.Count > 0)
        {
            var (entity, result) = jobQueue[0];
            jobQueue.RemoveAt(0);

            if (result == null)
                throw new NullReferenceException();
            
            entitiesToProxies.Add(entity, result);
            GameWorld.GetComponentData(entity.Handle, GenericType).Node = result;

            Console.WriteLine("Added");
            
            root.AddChild(result);
        }
        
        base.OnPresentationLoop();
    }

    private JobRequest previousRequest;

    public JobRequest NewInstantiateJob(UEntitySafe caller, PackedScene packedScene, bool duplicate = true)
    {
        new InstantiateJob
        {
            System = this,
            Caller = caller,
            Scene = packedScene,
            Duplicate = duplicate
        }.Execute(runner, default);
        return default;
        return runner.Queue(new InstantiateJob
        {
            System = this,
            Caller = caller,
            Scene = packedScene,
            Duplicate = duplicate
        });
        return default;
    }

    private static SwapDependency GodotThreadingDependency = new SwapDependency();
    
    private struct InstantiateJob : IJob, IJobExecuteOnCondition
    {
        public PresentationGodotBaseSystem System;
        public UEntitySafe Caller;
        public PackedScene Scene;
        public bool Duplicate;

        public int SetupJob(JobSetupInfo info)
        {
            return 1;
        }

        public void Execute(IJobRunner runner, JobExecuteInfo info)
        {
            if (Duplicate)
                Scene = (PackedScene) Scene.Duplicate();

            Scene.Reference();
            
            var node = Scene.Instantiate();
            System.jobQueue.Add((Caller, node));
            Console.WriteLine("Instantiate");
        }

        public bool CanExecute(IJobRunner runner, JobExecuteInfo info)
        {
            using (SwapDependency.BeginContext())
            {
                return GodotThreadingDependency.TrySwap(runner, info.Request);
            }
        }
    }

    protected struct EntityData
    {
        public JobRequest Request;
        public Node Node;
    }
}
