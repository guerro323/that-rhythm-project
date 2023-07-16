using System.Linq;
using System.Runtime.InteropServices;
using Collections.Pooled;
using PataNext.Game.Modules.Abilities.Components;
using PataNext.Game.Modules.Abilities.Storages;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Conditions;
using Quadrum.Game.Modules.Simulation.Common.SystemBase;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revecs.Querying;
using revecs.Utility;
using revghost;
using revghost.Injection;
using revghost.Injection.Dependencies;

namespace PataNext.Game.Modules.Abilities.SystemBase;

public record struct CreateAbility(UEntitySafe Target);

public abstract class BaseRhythmAbilityProvider : BaseProvider<CreateAbility>
{
    public virtual bool UseStatsModification => true;

    protected BaseRhythmAbilityProvider(Scope scope) : base(scope)
    {
        if (!scope.Context.TryGet(out abilityStorage))
            throw new InvalidOperationException(
                $"Module has created an ability provider, but do not provide an AbilityDescriptionStorage, this is not allowed."
            );
    }

    protected AbilityDescriptionStorage abilityStorage;
    protected string configuration;

    protected abstract void GetComboCommands<TList>(TList componentTypes) where TList : IList<UEntitySafe>;

    public string GetConfigurationData()
    {
        return configuration;
    }
}

public abstract class BaseRhythmAbilityProvider<TAbility> : BaseRhythmAbilityProvider
    where TAbility : IRevolutionComponent
{
    protected virtual string FilePathPrefix => string.Empty;

    protected virtual string FolderPath
    {
        get
        {
            var folder = "{0}";
            if (!string.IsNullOrEmpty(FilePathPrefix))
                folder = string.Format(folder, FilePathPrefix + "\\{0}");
            
            // kinda useless, but it will automatically create folders that don't exist, so it's a bit useful for lazy persons (eg: guerro)
            try
            {
                abilityStorage.GetSubStorage(folder);
            }
            catch
            {
                // ignored (DllStorage will throw an exception if it does not exist)
            }

            return folder;
        }
    }

    protected virtual string FilePath => $"{FolderPath}\\{typeof(TAbility).Name.Replace("Ability", string.Empty)}";

    protected abstract RhythmCommandAction[] CommandActions { get; }
    protected virtual int CommandDuration => 4;

    private AbilitySpawner _spawner;

    protected BaseRhythmAbilityProvider(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _spawner);
    }
    
    private static ReadOnlySpan<byte> Utf8Bom => new byte[] {0xEF, 0xBB, 0xBF};

    private UEntitySafe[] _comboComponentTypes;
    private ComponentType _abilityType;

    private UEntityHandle _commandEntity;

    protected override void OnInit()
    {
        _commandEntity = Simulation.CreateEntity();
        Simulation.AddCommandDuration(_commandEntity, new CommandDuration(CommandDuration));
        Simulation.AddCommandActions(_commandEntity, MemoryMarshal.Cast<RhythmCommandAction, CommandActions>(CommandActions));
        
        using var list = new PooledList<UEntitySafe>();
        list.Add(Simulation.Safe(_commandEntity));
        GetComboCommands(list);
        _comboComponentTypes = list.ToArray();

        _abilityType = TAbility.ToComponentType(Simulation);

        _spawner.Register(_abilityType, this);
    }

    public override void SetEntityData(ref UEntityHandle handle, CreateAbility data)
    {
        if (!Simulation.Exists(data.Target))
            throw new InvalidOperationException($"Simulation Entity '{data.Target}' does not exist");
        
        Simulation.AddComponent(handle, AbilityLayout.ToComponentType(Simulation));
        Simulation.AddComponent(handle, _abilityType);
        Simulation.AddAbilityType(handle, new AbilityType(_abilityType));
        // don't pass data.Target directly (since Relative components only accept UEntityHandle and not UEntitySafe)
        // TODO: in future Relative component should also add extension methods to RevolutionWorld to not have this problem
        Simulation.AddComponent(handle, AbilityOwnerDescription.Relative.ToComponentType(Simulation), data.Target.Handle);
        
        Simulation.AddAbilityControlVelocity(handle);

        if (_comboComponentTypes.Length > 0)
        {
            Simulation.AddComboAbilityCondition(
                handle,
                _comboComponentTypes.AsSpan().UnsafeCast<UEntitySafe, ComboAbilityCondition>()
            );
        }

        if (UseStatsModification)
        {
            var component = new AbilityModifyStatsOnChaining();
            
            var stats = new Dictionary<string, StatisticModifier>();
            StatisticModifierJson.FromMap(stats, GetConfigurationData());

            void TryGet(string val, out StatisticModifier modifier)
            {
                if (!stats.TryGetValue(val, out modifier))
                    modifier = StatisticModifier.Default;
            }

            TryGet("active", out component.ActiveModifier);
            TryGet("fever", out component.FeverModifier);
            TryGet("perfect", out component.PerfectModifier);
            TryGet("charge", out component.ChargeModifier);
            
            Simulation.AddComponent(handle, AbilityModifyStatsOnChaining.ToComponentType(Simulation), component);
        }
    }
}