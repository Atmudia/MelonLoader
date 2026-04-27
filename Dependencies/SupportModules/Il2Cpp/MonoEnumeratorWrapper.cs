using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Runtime.InteropServices;
using Il2CppInterop.Common;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Boolean = Il2CppSystem.Boolean;
using Exception = System.Exception;
using NotSupportedException = System.NotSupportedException;
using NullReferenceException = System.NullReferenceException;

namespace MelonLoader.Support
{
    public class MonoEnumeratorWrapper : Il2CppSystem.Object, Il2CppSystem.Collections.IEnumerator, IIl2CppType<MonoEnumeratorWrapper>
    {
        public MonoEnumeratorWrapper(ObjectPointer ptr) : base(ptr) { }
        public MonoEnumeratorWrapper(IEnumerator _enumerator) : this(IL2CPP.NewObjectPointer<MonoEnumeratorWrapper>())
        {
            ReferencedEnumerator = _enumerator ?? throw new NullReferenceException("routine is null");

        }
        [Il2CppField]
        private Il2CppSystem.IntPtr EnumeratorHandleValue
        {
            get => FieldAccess.GetInstanceFieldValue<Il2CppSystem.IntPtr>(this, EnumeratorHandleFieldOffset);
            set => FieldAccess.SetInstanceFieldValue(this, EnumeratorHandleFieldOffset, value);
        }
        private GCHandle<IEnumerator> EnumeratorHandle
        {
            get => GCHandle<IEnumerator>.FromIntPtr(EnumeratorHandleValue);
            set => EnumeratorHandleValue = GCHandle<IEnumerator>.ToIntPtr(value);
        }
        public IEnumerator ReferencedEnumerator
        {
            get => EnumeratorHandle.Target;
            set
            {
                EnumeratorHandle.Dispose();
                EnumeratorHandle = value is not null ? new(value) : default;
            }
        }

        
        public Boolean MoveNext()
        {
            try
            {
                return ReferencedEnumerator.MoveNext();
            } catch(Exception e)
            {
                var melon = MelonUtils.GetMelonFromStackTrace(new System.Diagnostics.StackTrace(e), true);

                if (melon != null)
                    melon.LoggerInstance.Error("Unhandled exception in coroutine. It will not continue executing.", e);
                else
                    MelonLogger.Error("[Error: Could not identify source] Unhandled exception in coroutine. It will not continue executing.", e);

                return false;
            }
        }

        public void Reset() => ReferencedEnumerator.Reset();
        static readonly int EnumeratorHandleFieldOffset;

        public IObject Current
        {
            get => ReferencedEnumerator.Current switch
                 {
                     IEnumerator next => new MonoEnumeratorWrapper(next),
                     Il2CppSystem.Object il2cppObject => il2cppObject,
                     null => null,
                     _ => throw new NotSupportedException($"{ReferencedEnumerator.GetType()}: Unsupported type {ReferencedEnumerator.Current.GetType()}"),
                 };
        }
        
        public override void Il2CppFinalize()
        {
            try
            {
                ReferencedEnumerator = null!;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Exception in {nameof(MonoEnumeratorWrapper)}.{nameof(Il2CppFinalize)}: {{Exception}}", ex);
            }
            finally
            {
                base.Il2CppFinalize(); // Must call base method
            }        
        }
        
        public static void WriteToSpan(MonoEnumeratorWrapper value, System.Span<byte> span)
        {
            Il2CppType.WriteReference(value, span);
        }

        public static MonoEnumeratorWrapper ReadFromSpan(System.ReadOnlySpan<byte> span)
        {
            return Il2CppType.ReadReference<MonoEnumeratorWrapper>(span);
        }

        static int IIl2CppType<MonoEnumeratorWrapper>.Size => nint.Size;

        nint IIl2CppType.ObjectClass => Il2CppClassPointerStore<MonoEnumeratorWrapper>.NativeClassPointer;        
        static MonoEnumeratorWrapper()
        {
            TypeInjector.RegisterTypeInIl2Cpp<MonoEnumeratorWrapper>();
            EnumeratorHandleFieldOffset = (int)IL2CPP.il2cpp_field_get_offset(IL2CPP.GetIl2CppField(Il2CppClassPointerStore<MonoEnumeratorWrapper>.NativeClassPointer, nameof(EnumeratorHandleFieldOffset)));
            Il2CppObjectPool.RegisterInitializer(Il2CppClassPointerStore<MonoEnumeratorWrapper>.NativeClassPointer, ptr => new MonoEnumeratorWrapper(ptr));
        }
    }
}