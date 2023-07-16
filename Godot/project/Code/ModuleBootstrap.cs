using System.Diagnostics;
using System.Runtime;
using Godot;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Super;
using QuadrumPrototype.Client;
using QuadrumPrototype.Code.GameModes;
using QuadrumPrototype.Code.Presentations;
using revghost;
using revghost.Module;
using revghost.Utility;

namespace QuadrumPrototype;

public partial class ModuleBootstrap : Node
{
	private GhostRunner _runner;
	
	public override void _Ready()
	{
		base._Ready();

		// HACK: Set default sync context
		SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

		Debug.Assert(GC.TryStartNoGCRegion(1));
		
		_runner = GhostInit.Launch(
			_ => {},
			scope => new EntryModule(scope)
		);
		
		GD.Print("hi");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		for (var i = 0; i < 2; i++)
		{
			_runner.Loop();
		}
	}

	public class EntryModule : HostModule
	{
		public EntryModule(HostRunnerScope scope) : base(scope)
		{
		}

		protected override void OnInit()
		{
			LoadModule(scope => new Quadrum.Game.Modules.Implementations.ImplBox2D.Box2DModule(scope));
			LoadModule(scope => new Quadrum.Game.Super.Module(scope));
			LoadModule(scope => new Quadrum.Modules.Abilities.Module(scope));
			GD.Print("OK 1");
			
			TrackDomain((SimulationDomain domain) =>
			{
				GD.Print("OK 2");
				
				Disposables.AddRange(new IDisposable[]
				{
					// Quadrum.Client
					new UpdatePresentationSystems(domain.Scope),
					
					new RegisterGameRhythmInput(domain.Scope),
					new RhythmEnginePresentation(domain.Scope),
					new UnitPresentation(domain.Scope),
					new ProjectilePresentation(domain.Scope),
					
					new BasicGameModeSystem(domain.Scope)
				});

				var gm = domain.GameWorld.CreateEntity();
				domain.GameWorld.AddBasicGameMode(gm);

				/*var ent = domain.GameWorld.CreateEntity();
				domain.GameWorld.AddRhythmEngineSettings(ent);
				domain.GameWorld.AddRhythmEngineState(ent);

				ent = domain.GameWorld.CreateEntity();
				domain.GameWorld.AddUnitDescription(ent);
				domain.GameWorld.AddPositionComponent(ent);*/
			});
		}
	}
}
