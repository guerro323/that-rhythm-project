using Box2D.NetStandard.Dynamics.World;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Utilities;
using Quadrum.Modules.Abilities.Providers;
using revecs.Core;
using revghost;

namespace Quadrum.Modules.Abilities.Scripts;

public partial class QuickRetreatScript : AbilityScriptBase<QuickRetreatAbility>
{
	public QuickRetreatScript(Scope scope) : base(scope)
	{
	}

	private ExecuteCommands execCommands;
	
	protected override void OnInit()
	{
		base.OnInit();
		
		execCommands = new ExecuteCommands(Simulation);
	}

	const float walkbackTime = 1.25f;
	
	protected override void OnExecute(UEntityHandle owner, UEntityHandle self)
	{
		var dt = (float) GameTime.Delta.TotalSeconds;
		
        ref var ability = ref execCommands.UpdateQuickRetreatAbility(self);
        ref readonly var state = ref execCommands.ReadAbilityState(self);

		if (state.ActivationVersion != ability.LastActiveId)
		{
			ability.IsRetreating = false;
			ability.ActiveTime = 0;
			ability.LastActiveId = state.ActivationVersion;
		}

		ref readonly var translation = ref execCommands.ReadPositionComponent(owner).Value;
		ref var velocity = ref execCommands.UpdateVelocityComponent(owner).Value;
		if (!HasActiveOrChainingState(self))
		{
			if (MathUtils.Distance(ability.StartPosition, translation.X) > 2.5f
			    && ability.ActiveTime > 0.1f)
			{
				velocity.X = (ability.StartPosition - translation.X) * 3;
			}

			ability.ActiveTime = 0;
			ability.IsRetreating = false;
			return;
		}

		ref readonly var playState = ref execCommands.ReadUnitPlayState(owner);
		var unitDirection = execCommands.ReadUnitDirection(owner).Value;

		var wasRetreating = ability.IsRetreating;
		var retreatSpeed = playState.MovementAttackSpeed * 3.5f;

		ability.IsRetreating = ability.ActiveTime <= walkbackTime;
		ability.ActiveTime += dt;

		if (!wasRetreating && ability.IsRetreating)
		{
			ability.StartPosition = translation.X;
			velocity.X = -unitDirection * retreatSpeed;
			velocity.Y = 5f;
		}

		// there is a little stop when the character is stopping retreating
		if (ability.ActiveTime >= QuickRetreatAbility.StopTime && ability.ActiveTime <= walkbackTime)
		{
			// if he weight more, he will stop faster
			velocity.X = MathUtils.LerpNormalized(velocity.X, 0, playState.Weight * 0.25f * dt);
		}

		if (!ability.IsRetreating && ability.ActiveTime > walkbackTime)
		{
			// we add '2.8f' to boost the speed when backing up, so the unit can't chain retreat to go further
			if (wasRetreating)
				ability.BackVelocity = Math.Abs(ability.StartPosition - translation.X) * 1.5f;

			var newPosX = MathUtils.MoveTowards(translation.X, ability.StartPosition, ability.BackVelocity * dt);
			velocity.X = (newPosX - translation.X) / dt;
		}

		ref var unitController = ref execCommands.UpdateUnitControllerState(owner);
		unitController.ControlOverVelocityX = true;
	}
	
	private partial record struct ExecuteCommands :
		QuickRetreatAbility.Cmd.IWrite,
		AbilityState.Cmd.IRead,
        
		UnitControllerState.Cmd.IWrite,
		UnitPlayState.Cmd.IRead,
		UnitDirection.Cmd.IRead,
		PositionComponent.Cmd.IRead,
		VelocityComponent.Cmd.IWrite;
}