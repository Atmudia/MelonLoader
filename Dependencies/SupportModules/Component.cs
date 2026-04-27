using UnityEngine;

#if SM_Il2Cpp
using System;
using Il2CppInterop.Common;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
#endif

namespace MelonLoader.Support
{
    internal class SM_Component : MonoBehaviour
#if SM_Il2Cpp
        , IIl2CppType<SM_Component>
#endif
    {
#if SM_Il2Cpp
        [Il2CppField]
        private Il2CppSystem.Boolean isQuitting
        {
            get => FieldAccess.GetInstanceFieldValue<Il2CppSystem.Boolean>(this, IsQuitingFieldOffset);
            set => FieldAccess.SetInstanceFieldValue(this, IsQuitingFieldOffset, value);
        }
#else
    private bool isQuitting;
#endif

#if SM_Il2Cpp
        public SM_Component(ObjectPointer value) : base(value) { }
#endif

        internal static void Create()
        {
            if (Main.component != null)
                return;

            MelonCoroutines._hasProcessed = false;

            Main.obj = new GameObject();
            DontDestroyOnLoad(Main.obj);
            Main.obj.hideFlags = HideFlags.DontSave;

#if SM_Il2Cpp
            Main.component = Main.obj.AddComponent<SM_Component>();
#else
            Main.component = (SM_Component)Main.obj.AddComponent(typeof(SM_Component));
#endif

            ComponentSiblingFix.SetAsLastSibling(Main.obj.transform);
        }

        private void ProcessCoroutineQueue()
        {
            MelonCoroutines._hasProcessed = true;

            if (MelonCoroutines._queue.Count <= 0)
                return;

            foreach (var queuedCoroutine in MelonCoroutines._queue)
#if SM_Il2Cpp
                StartCoroutine(new MonoEnumeratorWrapper(queuedCoroutine));
#else
                StartCoroutine(queuedCoroutine);
#endif

            MelonCoroutines._queue.Clear();
        }

        void Start()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            ComponentSiblingFix.SetAsLastSibling(transform);
            Main.Interface.OnApplicationLateStart();
        }

        void Awake()
        {
            ProcessCoroutineQueue();
        }

        void Update()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            isQuitting = false;
            ComponentSiblingFix.SetAsLastSibling(transform);

            SceneHandler.OnUpdate();
            Main.Interface.Update();
        }

        void OnDestroy()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            if (!isQuitting)
            {
                Create();
                return;
            }

            OnApplicationDefiniteQuit();
        }

        void OnApplicationQuit()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            isQuitting = true;
            Main.Interface.Quit();
        }

        void OnApplicationDefiniteQuit()
        {
            Main.Interface.DefiniteQuit();
        }

        void FixedUpdate()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            Main.Interface.FixedUpdate();
        }

        void LateUpdate()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            Main.Interface.LateUpdate();
        }

        void OnGUI()
        {
            if ((Main.component == null) || (Main.component != this))
                return;

            Main.Interface.OnGUI();
        }

#if SM_Il2Cpp
        public static void WriteToSpan(SM_Component value, Span<byte> span)
        {
            Il2CppType.WriteReference(value, span);
        }

        public static SM_Component ReadFromSpan(ReadOnlySpan<byte> span)
        {
            return  Il2CppType.ReadReference<SM_Component>(span);;   
        }
        
        static int IIl2CppType<SM_Component>.Size => nint.Size;

        nint IIl2CppType.ObjectClass => Il2CppClassPointerStore<SM_Component>.NativeClassPointer;
        static readonly int IsQuitingFieldOffset;

        static SM_Component()
        {
            TypeInjector.RegisterTypeInIl2Cpp<SM_Component>();
            IsQuitingFieldOffset = (int)IL2CPP.il2cpp_field_get_offset(IL2CPP.GetIl2CppField(Il2CppClassPointerStore<SM_Component>.NativeClassPointer, nameof(isQuitting)));
            Il2CppObjectPool.RegisterInitializer(Il2CppClassPointerStore<SM_Component>.NativeClassPointer, ptr => new SM_Component(ptr));
        }
        
#endif
    }
    
}