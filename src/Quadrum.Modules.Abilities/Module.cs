using PataNext.Game.Modules.Abilities.Storages;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Modules.Abilities.Providers;
using Quadrum.Modules.Abilities.Scripts;
using QuadrumPrototype.Code.Abilities;
using revghost;
using revghost.IO.Storage;
using revghost.Module;
using revghost.Utility;

namespace Quadrum.Modules.Abilities;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        ModuleScope.Context.Register(new AbilityDescriptionStorage(new MultiStorage
        {
            ModuleScope.DataStorage,
            ModuleScope.DllStorage
        }.GetSubStorage("Descriptions")));
        
        TrackDomain((SimulationDomain domain) =>
        {
            var scope = new FreeScope(new MultipleScopeContext
            {
                ModuleScope.Context,
                domain.Scope.Context
            });

            IDisposable register(IDisposable disposable)
            {
                scope.Context.Register(disposable);
                return disposable;
            }

            Disposables.AddRange(new IDisposable[]
            {
                register(new BasicProjectileProvider(scope)),
                new BasicProjectileSystem(scope),
                
                new DefaultMarchAbility.Provider(scope),
                new DefaultMarchScript(scope),
                
                new DefaultAttackAbility.Provider(scope),
                new DefaultAttackScript(scope),
                
                new DaggersThrowAbility.Provider(scope),
                new DaggersThrowScript(scope),
                
                new DefaultJumpAbility.Provider(scope),
                new DefaultJumpScript(scope),
                
                new DefaultRetreatAbility.Provider(scope),
                new DefaultRetreatScript(scope),
                
                new QuickRetreatAbility.Provider(scope),
                new QuickRetreatScript(scope)
            });
        });
    }
}