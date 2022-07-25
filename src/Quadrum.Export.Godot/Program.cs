using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using GDNative;
using GodotCLR;
using GodotCLR.HighLevel;
using Microsoft.VisualBasic;
using revghost;
using revghost.Domains;
using revghost.IO.Storage;
using revghost.Module.Storage;

namespace Quadrum.Export.Godot;

/*public unsafe struct GDNativeInterface
{
    public uint version_major;
    public uint version_minor;
    public uint version_patch;
    public char* version_string;

    public delegate*<uint, void*> mem_alloc;
    public delegate*<void*, uint, void*> mem_realloc;
    public delegate*<void*, void> mem_free;
    public delegate*<char*, char*, char*, int, void> print_error;
}*/

[StructLayout(LayoutKind.Explicit)]
unsafe struct ObjData
{
    [FieldOffset(0)]
    public long id;
    [FieldOffset(sizeof(long))]
    public void* obj;
}

public unsafe class Program
{
    private static GDNativeInterface* _interface;
    private static void* _library;
    
    [UnmanagedCallersOnly(EntryPoint = "my_lib_init", CallConvs = new []{typeof(CallConvCdecl)})]
    public static byte GodotLoad(GDNativeInterface* gdInterface, void* gdLibrary, GDNativeInitialization* gdInit)
    {
        Console.WriteLine("\n\n\nomg it's C# !\n\n\n");

        gdInit->initialize = &InitializeLevel;
        gdInit->deinitialize = &DeinitializeLevel;
        gdInit->minimum_initialization_level = GDNativeInitializationLevel.GDNATIVE_INITIALIZATION_SCENE;

        _interface = gdInterface;
        _library = gdLibrary;
        
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvCdecl)})]
    private static void InitializeLevel(void* data, GDNativeInitializationLevel level)
    {
        Console.WriteLine($"Init(level={level})");

        if (level == GDNativeInitializationLevel.GDNATIVE_INITIALIZATION_SCENE)
        {
            Native.Load(_interface, _library);
            UtilityFunctions.Print($"Hello from C# level={level}");

            GD.RegisterClass<int>("MyClass", "Node3D");
            GD.AddMethod<int>("MyClass", "PrintText",
                (ref byte methodData, ref int instance, VariantMethodArgs args) =>
                {
                    UtilityFunctions.Print($"Text from GDScript: {args[0]}");
                    return default;
                },
                new (Variant.EType, string)[]
                {
                    (Variant.EType.STRING, "text")
                }, Variant.EType.NIL,
                GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_STATIC
            );
            GD.AddMethod<int>("MyClass", "PrintTextNonStatic",
                (ref byte methodData, ref int instance, VariantMethodArgs args) =>
                {
                    UtilityFunctions.Print($"Text from GDScript: {args[0]}");
                    return default;
                },
                new (Variant.EType, string)[]
                {
                    (Variant.EType.STRING, "text")
                }, Variant.EType.NIL,
                GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_NORMAL
            );
            GD.AddMethod<int>("MyClass", "update",
                (ref byte methodData, ref int instance, VariantMethodArgs args) =>
                {
                    UtilityFunctions.Print($"update {args[0].Float}");
                    return default;
                },
                new (Variant.EType, string)[]
                {
                    (Variant.EType.FLOAT, "dt")
                }, Variant.EType.NIL,
                GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_NORMAL);

            Task.Delay(1000).ContinueWith(_ =>
            {
                var mainLoop = GD.Engine.GetMainLoop();
                var scene = GD.SceneTree.GetCurrentScene(mainLoop);
                var node = GD.ResourceLoader.Load("res://hello.tscn")
                    .To<GD.PackedScene>()
                    .Duplicate()
                    .Instantiate();

                var root = new GD.Node(scene);
                root.AddChild(node);
                node.CallDeferred("say_hi", default);
                node.CallDeferred("set_text", stackalloc[]
                {
                    new Variant("Text from C#!")
                });
            });
        }
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvCdecl)})]
    private static void DeinitializeLevel(void* data, GDNativeInitializationLevel level)
    {
        Console.WriteLine($"Deinit(level={level})");
    }
    
    public static unsafe int Load(IntPtr ptr, int args)
    {
        return 0;
        
        /*GodotHL.Load(ptr, OnUpdate, OnClean, OnExchange);

        var directory = Native.GetDirectory();
        for (var i = 0; i < directory.Length; i++)
        {
            if (directory[i] == '\0')
            {
                directory = directory[..i];
                break;
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
        {
            Console.WriteLine($"trying to resolve: {eventArgs.Name} (requested by {eventArgs.RequestingAssembly})");
                
            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == eventArgs.Name);
            if (assembly != null)
                return assembly;
                
            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = eventArgs.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(directory,filename);
            try
            {
                return AssemblyLoadContext.GetLoadContext(eventArgs.RequestingAssembly)
                    .LoadFromAssemblyPath(asmFile);
                //return Assembly.LoadFrom(asmFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION ON RESOLVE: " + ex);
                return null;
            }
        };
            
        OnInit(directory);*/
        return 0;
    }

    private static void OnExchange(Variant subject, VariantMethodArgs args, VariantArgBuilder ret)
    {
        // start bootstraps from this
    }

    private static GhostRunner runner;
    private static bool runnerLoopResult;
    
    private static void OnInit(string directory)
    {
        Console.WriteLine("Program - OnInit");
        
        runner = GhostInit.Launch(sc =>
        {
            // ...
            sc.ExecutiveStorage = new ExecutiveStorage(new LocalStorage(directory));
            sc.UserStorage = new LocalStorage(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            ).GetSubStorage("GameHostExp");
            sc.ModuleStorage = new ModuleCollectionStorage(new MultiStorage
            {
                sc.ExecutiveStorage.GetSubStorage("Modules"),
                sc.UserStorage.GetSubStorage("Modules")
            });
        }, sc => new EntryModule(sc));
        runnerLoopResult = true;
    }

    public static void Main()
    {
        throw new InvalidOperationException("Quadrum.Export.Godot shouldn't be called via command line");
    }

    private static void OnUpdate()
    {
        runnerLoopResult = runner.Loop();
    }

    private static void OnClean()
    {
        runnerLoopResult = false;
        runner.Dispose();
    }
}