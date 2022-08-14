using System;
using System.Diagnostics;
using DefaultEcs;
using revecs.Core;
using revecs.Core.Boards;
using revecs.Systems;
using revghost;
using revghost.Domains;
using revghost.Domains.Time;
using revghost.Injection;
using revghost.Loop;
using revghost.Loop.EventSubscriber;
using revghost.Shared.Threading.Schedulers;
using revghost.Threading.V2;
using revghost.Threading.V2.Apps;
using revtask.Core;
using revtask.OpportunistJobRunner;

namespace Quadrum.Game.Modules.Simulation.Application;

// TODO: Move it into revecs.revghostExtension project
public class SimulationDomain : CommonDomainThreadListener
{
    public readonly SimulationScope Scope;

    public readonly World World;
    public readonly RevolutionWorld GameWorld;
    public readonly IJobRunner JobRunner;

    private readonly OpportunistJobRunner _jobRunner;

    public readonly IManagedWorldTime WorldTime;
    private readonly ManagedWorldTime _worldTime;

    public readonly IDomainUpdateLoopSubscriber UpdateLoop;
    private readonly DefaultDomainUpdateLoopSubscriber _updateLoop;

    public readonly ISimulationUpdateLoopSubscriber SimulationLoop;
    private readonly SimulationUpdateLoop _simulationLoop;

    private readonly Stopwatch _sleepTime = new();
    private readonly DomainWorker _worker;

    public TimeSpan? TargetFrequency
    {
        get => _targetFrequency;
        set => Scheduler.Add(args => args.t._targetFrequency = args.v, (t: this, v: value));
    }

    private TimeSpan? _targetFrequency;
    private FixedTimeStep _fts;

    private int _currentFrame;

    private UEntityHandle _timeEntity;

    public SimulationDomain(Scope scope, Entity domainEntity) : base(scope, domainEntity)
    {
        if (!scope.Context.TryGet(out _worker))
            _worker = new DomainWorker("Simulation Domain");

        Scope = new SimulationScope(DomainScope);
        {
            World = Scope.World;
            GameWorld = Scope.GameWorld;
            JobRunner = Scope.JobRunner;

            _jobRunner = (OpportunistJobRunner) JobRunner;

            Scope.Context.Register(WorldTime = _worldTime = new ManagedWorldTime());
            Scope.Context.Register(UpdateLoop = _updateLoop = new DefaultDomainUpdateLoopSubscriber(World));
            Scope.Context.Register(SimulationLoop = _simulationLoop = new SimulationUpdateLoop(World));
            Scope.Context.Register<IReadOnlyDomainWorker>(_worker);
            
            // GameWorld.AddBoard(nameof(BatchRunnerBoard), new BatchRunnerBoard(_jobRunner, GameWorld));
        }

        _targetFrequency = TimeSpan.FromMilliseconds(10);
        _timeEntity = GameWorld.CreateEntity();

        GameWorld.AddComponent(_timeEntity, GameTime.Type.GetOrCreate(GameWorld), default);
    }

    private JobRequest _previousLoopJob;
    
    public JobRequest CurrentJob => _previousLoopJob;

    protected override void DomainUpdate()
    {
        _jobRunner.CompleteBatch(_previousLoopJob, false);

        // future proof for a rollback system
        _worldTime.Total = _currentFrame * _worldTime.Delta;
        {
            GameWorld.GetComponentData(_timeEntity, GameTime.Type.GetOrCreate(GameWorld)) = new GameTime
            {
                Frame = _currentFrame,
                Total = _worldTime.Total,
                Delta = _worldTime.Delta
            };

            _jobRunner.StartPerformanceCriticalSection();
            try
            {
                _updateLoop.Invoke(_worldTime.Total, _worldTime.Delta);
            }
            finally
            {
                _jobRunner.StopPerformanceCriticalSection();
            }
        }

        _worldTime.Total += _worldTime.Delta;
        GameWorld.GetComponentData(_timeEntity, GameTime.Type.GetOrCreate(GameWorld)) = new GameTime
        {
            Frame = _currentFrame + 1,
            Total = _worldTime.Total,
            Delta = _worldTime.Delta
        };
        
        _previousLoopJob = _jobRunner.Queue(new JobExecuteLoop(_simulationLoop));
    }

    private readonly record struct JobExecuteLoop(SimulationUpdateLoop Loop) : IJob
    {
        public int SetupJob(JobSetupInfo info)
        {
            return 1;
        }

        public void Execute(IJobRunner runner, JobExecuteInfo info)
        {
            var sw = new Stopwatch();
            sw.Start();
            ((OpportunistJobRunner) runner).StartPerformanceCriticalSection();
            try
            {
                Loop.Invoke();
            }
            finally
            {
                ((OpportunistJobRunner) runner).StopPerformanceCriticalSection();
            }
            sw.Stop();
            Console.WriteLine($"Frame={sw.Elapsed.TotalMilliseconds:F3}ms");
        }
    }

    protected override ListenerUpdate OnUpdate()
    {
        if (IsDisposed || _disposalStartTask.Task.IsCompleted)
            return default;

        var delta = _worker.Delta + _sleepTime.Elapsed;

        var updateCount = 1;
        if (_targetFrequency is { } targetFrequency)
        {
            _fts.SetTargetFrameTime(targetFrequency);
            
            updateCount = Math.Clamp(_fts.GetUpdateCount(delta.TotalSeconds), 0, 3);
        }
        
        // If we don't have a target frequency, use the delta
        using (_worker.StartMonitoring(_targetFrequency ?? delta))
        {
            _worldTime.Delta = _targetFrequency ?? delta;

            using (CurrentUpdater.SynchronizeThread())
            {
                Scheduler.Run();
                TryExecuteScheduler();

                for (var tickAge = updateCount - 1; tickAge >= 0; --tickAge)
                {
                    _currentFrame++;
                    DomainUpdate();
                }
            }
        }

        var timeToSleep =
            TimeSpan.FromTicks(
                Math.Max(
                    (_targetFrequency ?? TimeSpan.FromMilliseconds(1)).Ticks - _worker.Delta.Ticks,
                    0
                )
            );

        _sleepTime.Restart();
        return new ListenerUpdate
        {
            TimeToSleep = timeToSleep
        };
    }
}