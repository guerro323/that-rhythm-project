using System.Runtime.CompilerServices;

namespace Quadrum.Game.Utilities;

public static unsafe class UnsafeExtensions
{
    public static T* Ptr<T>(ref this T t) where T : unmanaged => (T*) Unsafe.AsPointer(ref t);
    public static ref T Ref<T>(ref this T t) where T : unmanaged => ref t;
}