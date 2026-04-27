using System;
using UnityEngine;
using System.Reflection;
using Exception = System.Exception;

#if SM_Il2Cpp
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader.InternalUtils;

#else
using System.Linq;
#endif

namespace MelonLoader.Support
{
    internal static class ComponentSiblingFix
    {
        private static bool _failure;

#if SM_Il2Cpp
        private delegate void SetAsLastSiblingDelegate_Unity6(ByReference<Il2CppSystem.IntPtr> obj);
        private delegate void SetAsLastSiblingDelegate(ByReference<Transform> obj);
        private static SetAsLastSiblingDelegate_Unity6 _method_unity6;
#else
        private static MethodInfo _methodInfo;
        private delegate void SetAsLastSiblingDelegate(Transform obj);
        
#endif
        private static SetAsLastSiblingDelegate _method;


        internal static void SetAsLastSibling(Component obj)
        {
            if (_failure = !FindMethod())
                return;
            _failure = !InvokeMethod(obj);
        }

        private static void LogError(string cat, Exception ex)
        {
            MelonLogger.Warning($"Exception while {cat}: {ex}");
            MelonLogger.Warning("Melon Events might run before some MonoBehaviour Events");
        }

        private static bool FindMethod()
        {
            if (_failure)
                return false;

            try
            {
#if SM_Il2Cpp
                if (UnityInformationHandler.EngineVersion.Major >= 6000)
                {
                    _method_unity6 = IL2CPP.ResolveICall<SetAsLastSiblingDelegate_Unity6>("UnityEngine.Transform::SetAsLastSibling_Injected");
                }
                else
                {
                    _method = IL2CPP.ResolveICall<SetAsLastSiblingDelegate>("UnityEngine.Transform::SetAsLastSibling");

                }
                if (_method == null && _method_unity6 == null)
                    throw new Exception("Unable to find Internal Call for UnityEngine.Transform::SetAsLastSibling");
#else
                _methodInfo = typeof(Transform).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => (
                    (x.Name == nameof(SetAsLastSibling))
                    && (x.GetParameters().Count() == 0)));
                if (_methodInfo == null)
                    throw new Exception("Unable to find Method for UnityEngine.Transform::SetAsLastSibling");

                _method = (SetAsLastSiblingDelegate)Delegate.CreateDelegate(typeof(SetAsLastSiblingDelegate), _methodInfo);
#endif
            }
            catch (Exception ex)
            {
                LogError("Getting UnityEngine.Transform::SetAsLastSibling", ex);
                return false;
            }

            return true;
        }

        private static bool InvokeMethod(Component obj)
        {
            if (_failure || (_method == null))
                return false;

            try
            {
      
#if SM_Il2Cpp
                unsafe
                {

                    if (UnityInformationHandler.EngineVersion.Major >= 6000)
                    {
                        Il2CppSystem.IntPtr byref = obj.transform.m_CachedPtr;
                        _method_unity6((ByReference<Il2CppSystem.IntPtr>)(void*)&byref);
                        Il2CppSystem.IntPtr byref2 = obj.gameObject.transform.m_CachedPtr;
                        _method_unity6((ByReference<Il2CppSystem.IntPtr>)(void*)&byref2);
                    }
                    else
                    {
                        nint byref = obj.transform.Pointer;
                        _method((ByReference<Transform>)(void*)&byref);
                        nint byref2 = obj.gameObject.transform.Pointer;
                        _method((ByReference<Transform>)(void*)&byref2);
                    }

                    

                }
#else                
                _method(obj.transform);
                _method(obj.gameObject.transform);
#endif

            }
            catch (Exception ex)
            {
                LogError("Invoking UnityEngine.Transform::SetAsLastSibling", ex);
                return false;
            }

            return true;
        }
    }
}
