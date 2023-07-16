using Box2D.NetStandard.Collision.Shapes;
using DefaultEcs;
using Godot;
using PataNext.Game.Client.Core.Inputs;
using PataNext.Game.Modules.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Common;
using Quadrum.Game.Modules.Simulation.Common.GameMode;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.Teams;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Modules.Abilities.Providers;
using QuadrumPrototype.Code.Abilities;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;
using revghost.Domains.Time;
using revghost.Injection.Dependencies;
using Vector2 = System.Numerics.Vector2;

namespace QuadrumPrototype.Code.GameModes;

public partial struct BasicGameMode : ISparseComponent
{
    
}

public class BasicGameModeSystem :  GameModeSystemBase
{
    private World _world;
    private IManagedWorldTime worldTime;
    
    public BasicGameModeSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
        Dependencies.Add(() => ref worldTime);
        
        GD.Print("OK 4");
    }

    protected override void GetComponentTypes<TList>(TList all, TList none, TList or)
    {
        GD.Print("ZBOUE");
        
        all.Add(BasicGameMode.ToComponentType(Simulation));
    }

    private UEntityHandle _playerTeam;
    private UEntityHandle _enemyTeam;
    
    protected override async Task<bool> GetStateMachine(UEntitySafe gameModeEntity, CancellationToken token)
    {
        _playerTeam = Simulation.CreateEntity();
        Simulation.AddTeamDescription(_playerTeam);
        Simulation.AddTeamMovableArea(_playerTeam);
        Simulation.AddSimulationAuthority(_playerTeam);
        
        _enemyTeam = Simulation.CreateEntity();
        Simulation.AddTeamDescription(_enemyTeam);
        Simulation.AddTeamMovableArea(_enemyTeam);
        Simulation.AddSimulationAuthority(_enemyTeam);
        
        Simulation.AddTeamHostileDescriptionRelative(_playerTeam, _enemyTeam);
        Simulation.AddTeamHostileDescriptionRelative(_enemyTeam, _playerTeam);
        
        Console.WriteLine("yo");
        var hero = SpawnHero(0);
        var enemy = SpawnEnemy(5);

        Simulation.GetUnitEnemySeekingState(hero) = new UnitEnemySeekingState
        {
            Enemy = Simulation.Safe(enemy),
            RelativeDistance = 5,
            SelfDistance = 5
        };
        
        while (!token.IsCancellationRequested)
        {
            await Task.Yield();
        }
        
        return true;
    }

    protected override void OnCrash(UEntitySafe gameModeEntity, Exception exception)
    {
        Console.WriteLine("noooo the gamemode crashed :(");
    }
    
        private UEntityHandle SpawnHero(float positionX)
    {
        var player = Simulation.CreateEntity();
        Simulation.AddPlayerDescription(player);
        Simulation.AddGameRhythmInput(player);

        var cursor = Simulation.CreateEntity();
        Simulation.AddComponent(cursor, CursorLayout.ToComponentType(Simulation));
        Simulation.GetPositionComponent(cursor).X = positionX;
        Simulation.AddPlayerDescriptionRelative(cursor, player);

        var engine = Simulation.CreateEntity();
        Simulation.AddComponent(engine, RhythmEngineLayout.ToComponentType(Simulation));
        Simulation.GetRhythmEngineController(engine) = new RhythmEngineController
        {
            State = RhythmEngineController.EState.Playing,
            StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(1))
        };
        Simulation.GetRhythmEngineSettings(engine) = new RhythmEngineSettings
        {
            BeatInterval = TimeSpan.FromSeconds(0.5f),
            MaxBeats = 4
        };
        Simulation.GetGameComboSettings(engine) = new GameComboSettings
        {
            MaxComboToReachFever = 9,
            RequiredScoreStart = 4,
            RequiredScoreStep = 0.5f
        };
        Simulation.GetPowerGaugeState(engine) = new PowerGaugeState
        {
            MaxLevel = 4,
            MaxTick = 100
        };
        Simulation.AddPlayerDescriptionRelative(engine, player);

        var unit = Simulation.CreateEntity();
        Simulation.AddComponent(unit, PlayableUnitLayout.ToComponentType(Simulation));
        Simulation.AddPlayerDescriptionRelative(unit, player);
        Simulation.AddRhythmEngineDescriptionRelative(unit, engine);
        Simulation.AddCursorDescriptionRelative(unit, cursor);
        Simulation.AddOwnerActiveAbility(unit);

        Simulation.GetPositionComponent(unit).X = positionX;
        Simulation.GetUnitStatistics(unit) = new UnitStatistics
        {
            BaseWalkSpeed = 1f,
            FeverWalkSpeed = 1.2f,
            MovementAttackSpeed = 1.4f,
            Weight = 6,
            AttackSeekRange = 20,
        };
        Simulation.AddUnitDirection(unit, new UnitDirection(1));
        Simulation.AddCursorControlTag(unit);
        Simulation.AddSimulationAuthority(unit);
        
        Simulation.AddTeamDescriptionRelative(unit, _playerTeam);
        
        Simulation.GetContributeToTeamMovableArea(unit) = new ContributeToTeamMovableArea(0, 0.5f);

        void SpawnAbility<T>()
            where T : IRevolutionComponent
        {
            var ability = Simulation.CreateEntity();
            Simulation.AddSetSpawnAbility(ability, new SetSpawnAbility(T.ToComponentType(Simulation), Simulation.Safe(unit)));
        }
        
        SpawnAbility<DefaultMarchAbility>();
        SpawnAbility<DefaultAttackAbility>();
        SpawnAbility<DaggersThrowAbility>();
        SpawnAbility<DefaultJumpAbility>();
        SpawnAbility<DefaultRetreatAbility>();
        SpawnAbility<QuickRetreatAbility>();
        
        return unit;
    }
        
    private UEntityHandle SpawnEnemy(float positionX)
    {
        var cursor = Simulation.CreateEntity();
        Simulation.AddComponent(cursor, CursorLayout.ToComponentType(Simulation));
        Simulation.GetPositionComponent(cursor).X = positionX;
        
        var unit = Simulation.CreateEntity();
        Simulation.AddComponent(unit, PlayableUnitLayout.ToComponentType(Simulation));
        Simulation.AddCursorDescriptionRelative(unit, cursor);
        Simulation.AddOwnerActiveAbility(unit);

        Simulation.GetPositionComponent(unit).X = positionX;
        Simulation.GetUnitStatistics(unit) = new UnitStatistics
        {
            BaseWalkSpeed = 1f,
            FeverWalkSpeed = 1.2f,
            MovementAttackSpeed = 1.4f,
            Weight = 10,
            AttackSeekRange = 20,
        };
        Simulation.AddUnitDirection(unit, new UnitDirection(-1));
        Simulation.AddCursorControlTag(unit);
        Simulation.AddSimulationAuthority(unit);

        Simulation.GetContributeToTeamMovableArea(unit) = new ContributeToTeamMovableArea(0, 0.5f);
        
        Simulation.AddTeamDescriptionRelative(unit, _enemyTeam);
        
        Simulation.AddUnitIdentifier(unit, new UnitIdentifier {Value = "enemy"});

        using var collider = _world.CreateEntity();
        collider.Set<Shape>(new PolygonShape(1, 2, new Vector2(0, 1), 0));
        
        Simulation.GetPhysicsEngine()!.AssignCollider(unit, collider);
        
        return unit;
    }
}