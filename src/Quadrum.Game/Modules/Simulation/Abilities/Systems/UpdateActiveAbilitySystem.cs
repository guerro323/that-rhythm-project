using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems;

// This is mostly unfinished (hero abilities and cooldown logic need to be done in other systems)
// Old logic: https://github.com/patanext-project/patanext/blob/master/PataNext.Simulation.Mixed/Game/Systems/GamePlay/Abilities/UpdateActiveAbilitySystem.cs
public partial class UpdateActiveAbilitySystem : SimulationSystem
{
    public UpdateActiveAbilitySystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b
                .SetGroup<AbilitySystemGroup>()
                // Update after all conditions were updated
                .AfterGroup<AbilityConditionSystemGroup>()
                // Update before abilities are executed
                .BeforeGroup<AbilityExecutionSystemGroup>()
        );
    }

    private Commands _cmd;
    private OwnerQuery _ownerQuery;

    protected override void OnInit()
    {
        _cmd = new Commands(Simulation);
        _ownerQuery = new OwnerQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        _ownerQuery.QueueAndComplete(Runner, static (state, entities) =>
        {
            foreach (var entity in entities)
                OnEntity(entity, state.Data);
        }, _cmd, true);

        static void OnEntity(OwnerQuery.Iteration entity, Commands cmd)
        {
            ref readonly var engineState = ref cmd.ReadRhythmEngineState(entity.rhythmRelative);
            ref readonly var engineSettings = ref cmd.ReadRhythmEngineSettings(entity.rhythmRelative);
            ref readonly var executingCommand = ref cmd.ReadRhythmEngineExecutingCommand(entity.rhythmRelative);
            ref readonly var comboState = ref cmd.ReadGameComboState(entity.rhythmRelative);
            ref readonly var commandState = ref cmd.ReadGameCommandState(entity.rhythmRelative);

            var isSimulationOwned = true;
            var isRhythmEngineOwned = true;

            var isNewIncomingCommand = commandState.StartTimeMs == entity.active.LastCommandActiveTime;
            var isActivation = entity.active.LastActivationTime == -1
                               && comboState.Count > 0
                               && commandState.StartTime <= engineState.Elapsed;

            ref var activeAbility = ref entity.active.Active;
            ref var incomingAbility = ref entity.active.Incoming;
            ref var previousActiveAbility = ref entity.active.PreviousActive;

            incomingAbility = default;
            {
                var priority = int.MinValue;
                foreach (var abilityHandle in entity.abilityBuffer)
                {
                    if (cmd.HasAbilityDiscardFromSelectionTag(abilityHandle))
                        continue;

                    var abilityPriority = cmd.ReadAbilityPriority(abilityHandle).Value;
                    if (priority > abilityPriority)
                        continue;

                    priority = abilityPriority;
                    incomingAbility = abilityHandle;
                }
            }

            // If we are not chaining anymore or if the chain is finished, terminate our current command.
            if ((!commandState.HasActivity(engineState, engineSettings) &&
                 commandState.ChainEndTimeMs < engineState.Elapsed.TotalMilliseconds
                 || comboState.Count <= 0)
                && activeAbility.Id != default)
            {
                // It might be possible that the entity got deleted... so we need to check if it got the component
                if (cmd.HasAbilityState(activeAbility))
                {
                    ref var state = ref cmd.UpdateAbilityState(activeAbility);
                    cmd.RemoveAbilityStateActiveTag(activeAbility);
                    // <
                    //   remove these too? (it should normally be removed by the buffer, but what if we detach the ability?)
                    cmd.RemoveAbilityStateChainingTag(activeAbility);
                    cmd.RemoveAbilityStateBeforeActivationTag(activeAbility);
                    // >

                    state.Combo = 0;
                }

                previousActiveAbility = activeAbility;
                activeAbility = default;
            }

            // Set next active ability, and reset imperfect data if active.
            if (!entity.active.Active.Equals(entity.active.Incoming))
            {
                if (isActivation)
                {
                    if (cmd.HasAbilityState(activeAbility))
                    {
                        ref var state = ref cmd.UpdateAbilityState(activeAbility);
                        state.Combo = 0;
                    }

                    previousActiveAbility = activeAbility;
                    activeAbility = incomingAbility;
                    // This is something that should be done by a system
                    // (get the previous value before this system is called, and then do something after by comparing this value)
                    /*if (cmd.HasAbilityState(activeAbility))
                    {
                        ref var state = ref cmd.UpdateAbilityState(activeAbility);
                        state.HeroModeImperfectCountWhileActive = 0;
                    }*/
                }
            }

            // Decrease cooldowns of abilities that have one when a command has been triggered.
            var engineElapsedMs = (int) (engineState.Elapsed.Ticks / TimeSpan.TicksPerMillisecond);
            if (isActivation)
            {
                entity.active.LastActivationTime = engineElapsedMs;

                // kept for historical purpose
                /*foreach (var abilityEntity in abilityToUpdateCooldown.Value)
                {
                    ref var cooldown = ref abilityStateAccessor[abilityEntity].CommandCooldown;
                    cooldown = Math.Max(0, cooldown - 1);
                }*/
            }

            // We update incoming state before active state (in case if it's the same ability...)
            if (!incomingAbility.Equals(default))
            {
                ref var incomingState = ref cmd.UpdateAbilityState(incomingAbility);
                // cmd.AddAbilityStateBeforeActivationTag(incomingAbility);
                if (isNewIncomingCommand)
                {
                    incomingState.UpdateVersion++;
                    incomingState.Combo++;
                    /*if (!isHeroModeActive)
                        incomingState.HeroModeImperfectCountWhileActive = 0;*/
                }
            }

            // Update data in the active ability
            if (!activeAbility.Equals(default))
            {
                ref var activeController = ref cmd.UpdateAbilityState(activeAbility);
                if (commandState.StartTime <= engineState.Elapsed)
                {
                    // Will be set after
                    //cmd.RemoveAbilityStateActiveTag(activeAbility);
                    //cmd.RemoveAbilityStateChainingTag(activeAbility);
                    //cmd.RemoveAbilityStateBeforeActivationTag(activeAbility);

                    if (isActivation)
                    {
                        if (activeController.ActivationVersion == 0)
                            activeController.ActivationVersion++;

                        activeController.ActivationVersion++;

                        // historical purpose
                        /*if (activeAbilityActivation.DefaultCooldownOnActivation > 0)
                            GetComponentData<AbilityState>(activeSelf.Active.Handle).CommandCooldown +=
                                activeAbilityActivation.DefaultCooldownOnActivation;

                        if (activeAbilityActivation.Type.HasFlag(EAbilityActivationType.HeroMode) && isRhythmEngineOwned
                            && HasComponent<RhythmSummonEnergy>(engineEntity))
                        {
                            GetComponentData<RhythmSummonEnergy>(engineEntity).Value += 75;
                        }*/
                    }
                }

                // historical purpose
                /*if (activeAbilityActivation.Type.HasFlag(EAbilityActivationType.HeroMode)
                    && activeController.Combo <= 1 // only do it if it's the first combo...
                    && activeSelf.Active ==
                    activeSelf.Incoming // only if the next command is the same as the current one...
                    && gameCommandState.StartTimeSpan + engineSettings.BeatInterval > engineState.Elapsed)
                {
// delay the command for the first frame
                    activeController.Phase |= EAbilityPhase.HeroActivation;
                }

                if ((activeController.Phase & EAbilityPhase.HeroActivation) == 0)
                    activeController.Phase |= gameCommandState.IsGamePlayActive(engineElapsedMs)
                        ? EAbilityPhase.Active
                        : EAbilityPhase.Chaining;*/

                /*if (commandState.IsGamePlayActive(engineElapsedMs))
                    cmd.AddAbilityStateActiveTag(activeAbility);
                else
                    cmd.AddAbilityStateChainingTag(activeAbility);*/
            }

            var shouldBeActive = commandState.IsGamePlayActive(engineElapsedMs);
            foreach (var ability in entity.abilityBuffer)
            {
                EnableIncoming(ability, ability.Equals(incomingAbility));
                EnableActive(ability, ability.Equals(activeAbility) && shouldBeActive);
                EnableChaining(ability, ability.Equals(activeAbility) && false == shouldBeActive);
                
                void EnableIncoming(UEntityHandle handle, bool value)
                {
                    if (value == cmd.HasAbilityStateBeforeActivationTag(handle))
                        return;
                    switch (value)
                    {
                        case true: cmd.AddAbilityStateBeforeActivationTag(handle); break;
                        case false: cmd.RemoveAbilityStateBeforeActivationTag(handle); break;
                    }
                }
                void EnableActive(UEntityHandle handle, bool value)
                {
                    if (value == cmd.HasAbilityStateActiveTag(handle))
                        return;
                    switch (value)
                    {
                        case true: cmd.AddAbilityStateActiveTag(handle); break;
                        case false: cmd.RemoveAbilityStateActiveTag(handle); break;
                    }
                }
                void EnableChaining(UEntityHandle handle, bool value)
                {
                    if (value == cmd.HasAbilityStateChainingTag(handle))
                        return;
                    switch (value)
                    {
                        case true: cmd.AddAbilityStateChainingTag(handle); break;
                        case false: cmd.RemoveAbilityStateChainingTag(handle); break;
                    }
                }
            }
        }
    }

    private partial record struct OwnerQuery : IQuery<(
        Read<AbilityOwnerDescription> abilityBuffer,
        Write<OwnerActiveAbility> active,
        
        Read<RhythmEngineDescription.Relative> rhythmRelative)>;

    private partial record struct Commands :
        RhythmEngineState.Cmd.IRead,
        RhythmEngineSettings.Cmd.IRead,
        RhythmEngineExecutingCommand.Cmd.IRead,
        GameComboState.Cmd.IRead,
        GameComboSettings.Cmd.IRead,
        GameCommandState.Cmd.IRead,

        AbilityState.Cmd.IWrite,
        AbilityStateBeforeActivationTag.Cmd.IAdmin,
        AbilityStateActiveTag.Cmd.IAdmin,
        AbilityStateChainingTag.Cmd.IAdmin,
        
        AbilityDiscardFromSelectionTag.Cmd.IRead,
        AbilityPriority.Cmd.IRead,

        AbilityRhythmEngineSet.Cmd.IAdmin;
}