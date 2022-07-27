using System;
using System.Threading;
using revghost;
using revghost.Injection;
using revghost.Shared.Collections;

namespace Quadrum.Export.Desktop
{
    public class Program
    {
        public static void Main()
        {
            using var ghost = GhostInit.Launch(
                scope => {},
                scope => new EntryModule(scope)
            );
            
            /*ghost.Scope.Scheduler.Add(_ =>
            {
                return true;
                
                var list = new ValueList<IDependencyCollection>();

                var resolver = ghost.Scope.DependencyResolver as SchedulerDependencyResolver;
                resolver.GetQueuedCollections(ref list);
                if (list.Count == 0)
                    return false;

                Console.WriteLine($"======================================\nDependencies Collections Count: {list.Count}\n======================================");
                foreach (var collection in list)
                {
                    if (collection ==  null)
                        continue;

                    if (collection is DependencyCollection coll)
                        Console.WriteLine($"{coll.Source}");
                    else
                        Console.WriteLine(collection.ToString());
                
                    foreach (var dep in collection.Dependencies)
                    {
                        Console.WriteLine($"  " + dep);
                    }
                }
                
                list.Dispose();

                return false;
            }, 0, default);*/


            var prevMem = GC.GetTotalMemory(false);
            while (true)
            {
                ghost.Loop();
                var mem = GC.GetTotalMemory(false);
                var delta = mem - prevMem;
                prevMem = mem;
                if (delta != 0)
                {
                    Console.Write("Memory Delta: ");
                    Console.Write(delta);
                    Console.WriteLine("b");
                }

                Thread.Sleep(10);
            }
        }
    }
}