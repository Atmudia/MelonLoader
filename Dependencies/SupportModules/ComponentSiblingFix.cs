using System;
using UnityEngine;
using System.Reflection;

#if SM_Il2Cpp
using Il2CppInterop.Runtime;
#else
using System.Linq;
#endif

namespace MelonLoader.Support
{
    internal static class ComponentSiblingFix
    {
        private static bool _failure;

#if SM_Il2Cpp
        // private delegate void SetAsLastSiblingDelegate(Transform obj);
#else
        private static MethodInfo _methodInfo;
#endif
        private delegate void SetAsLastSiblingDelegate(Transform obj);
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
                _method = IL2CPP.ResolveICall<SetAsLastSiblingDelegate>("UnityEngine.Transform::SetAsLastSibling");
                if (_method == null)
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
                _method(obj.transform);
                _method(obj.gameObject.transform);
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
