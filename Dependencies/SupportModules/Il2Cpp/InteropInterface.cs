using Il2CppInterop.Common;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using System.Reflection;
using Il2CppSystem;
using IntPtr = System.IntPtr;
using Type = System.Type;

namespace MelonLoader.Support
{
    internal class InteropInterface : InteropSupport.Interface
    {
        public IntPtr CopyMethodInfoStruct(IntPtr ptr)
            => ptr;
            //=> UnityVersionHandler.CopyMethodInfoStruct(ptr);

        public int? GetIl2CppMethodCallerCount(MethodBase method)
        {
            CallerCountAttribute att = method.GetCustomAttribute<CallerCountAttribute>();
            if (att == null)
                return null;
            return att.Count;
        }

        public FieldInfo MethodBaseToIl2CppFieldInfo(MethodBase method)
            => Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method);

        public void RegisterTypeInIl2CppDomain(Type type, bool logSuccess)
        {
        }

        public void RegisterTypeInIl2CppDomainWithInterfaces(Type type, Type[] interfaces, bool logSuccess)
        {
            
        }

        public bool IsInheritedFromIl2CppObjectBase(Type type)
            => (type != null) && typeof(IObject).IsAssignableFrom(type);

        public bool IsInjectedType(Type type)
        {
            IntPtr ptr = GetClassPointerForType(type);
            return ptr != IntPtr.Zero;
            // return ptr != IntPtr.Zero && RuntimeSpecificsStore.IsInjected(ptr);
        }

        public IntPtr GetClassPointerForType(Type type)
        {
            if (type == typeof(void)) return Il2CppClassPointerStore<Il2CppSystem.Void>.NativeClassPointer;
            return (IntPtr)typeof(Il2CppClassPointerStore<>).MakeGenericType(type)
                  .GetField(nameof(Il2CppClassPointerStore<int>.NativeClassPointer)).GetValue(null);
        }
    }
}
