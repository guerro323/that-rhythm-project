using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace QuadrumPrototype.Code.Abilities;

public partial struct DefaultMarchAbility : ISparseComponent
{
    public class Provider : BaseRhythmAbilityProvider<DefaultMarchAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
        }

        protected override RhythmCommandAction[] CommandActions => new RhythmCommandAction[]
        {
            RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left), 
            RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left), 
            RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left), 
            RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right), 
        };

        public override void SetEntityData(ref UEntityHandle handle, CreateAbility data)
        {
            base.SetEntityData(ref handle, data);
            Simulation.AddMarchAbilityAspect(handle, new MarchAbilityAspect
            {
                AccelerationFactor = 1,
                Target = MarchAbilityAspect.ETarget.All
            });
        }
    }
}