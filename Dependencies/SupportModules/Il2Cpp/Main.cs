using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Startup;
using MelonLoader.Support.Preferences;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using Il2CppInterop.Common;
using Microsoft.Extensions.Logging;
using MelonLoader.Utils;
using System.IO;
using Il2CppInterop.HarmonySupport;
using MonoMod.Core;
using MonoMod.RuntimeDetour;

[assembly: MelonLoader.PatchShield]

namespace MelonLoader.Support
{
    internal static class Main
    {
        internal static ISupportModule_From Interface;
        internal static InteropInterface Interop;
        internal static GameObject obj = null;
        internal static SM_Component component = null;

        private static Assembly Il2Cppmscorlib = null;
        private static Type streamType = null;

        private static ISupportModule_To Initialize(ISupportModule_From interface_from)
        {
            Interface = interface_from; 

            foreach (var file in Directory.GetFiles(MelonEnvironment.Il2CppAssembliesDirectory, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(file);
                }
                catch { }
            }

            UnityMappers.RegisterMappers();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(typeof(Il2CppInteropRuntime).Assembly,
                    MacOsIl2CppInteropLibraryResolver);
            }

            DetourContext.SetGlobalContext(new DetourFactoryContext(new Il2CppInteropDetourFactory()));

            Il2CppInteropRuntime runtime = Il2CppInteropRuntime.Create(new()
            {
                DetourProvider = new MelonDetourProvider(),
                UnityVersion = new Version(
                    InternalUtils.UnityInformationHandler.EngineVersion.Major,
                    InternalUtils.UnityInformationHandler.EngineVersion.Minor,
                    InternalUtils.UnityInformationHandler.EngineVersion.Build)
            }).AddLogger(new InteropLogger());
            Interop = new InteropInterface();
            Interface.SetInteropSupportInterface(Interop);
            runtime.Start();


            Il2CppInterop.Initialization.Il2CppInitialization.Initialize();
            if (!LoaderConfig.Current.UnityEngine.DisableConsoleLogCleaner)
                ConsoleCleaner();

            GetSceneManagerMethods(out MethodInfo sceneLoaded,
                out MethodInfo sceneUnloaded);
            if (sceneLoaded == null)
            {
                MelonLogger.Warning("Failed to find Internal_SceneLoaded method");
                MelonLogger.Warning("Falling back to SupportModule Component Creation");
                SM_Component.Create();
            }
            else
                SceneHandler.Init(sceneLoaded, sceneUnloaded);

            return new SupportModule_To();
        }

        private static void GetSceneManagerMethods(out MethodInfo sceneLoaded,
            out MethodInfo sceneUnloaded)
        {
            sceneLoaded = null;
            sceneUnloaded = null;
            Type scenemanager = null;
            try
            {
                Assembly unityengine = Assembly.Load("UnityEngine.CoreModule");
                if (unityengine != null)
                    scenemanager = unityengine.GetType("UnityEngine.SceneManagement.SceneManager");

                if (scenemanager == null)
                {
                    unityengine = Assembly.Load("UnityEngine");
                    if (unityengine != null)
                        scenemanager = unityengine.GetType("UnityEngine.SceneManagement.SceneManager");
                }
            }
            catch { scenemanager = null; }
            if (scenemanager == null)
                return;

            sceneLoaded = scenemanager.GetMethod("Internal_SceneLoaded", BindingFlags.Public | BindingFlags.Static);
            sceneUnloaded = scenemanager.GetMethod("Internal_SceneUnloaded", BindingFlags.Public | BindingFlags.Static);
        }

        private static IntPtr MacOsIl2CppInteropLibraryResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == "GameAssembly")
            {
                string gameAssemblyPath = Path.Combine(MelonEnvironment.GameExecutablePath, "Contents", "Frameworks", $"{libraryName}.dylib");
                return System.Runtime.InteropServices.NativeLibrary.Load(gameAssemblyPath);
            }
            return IntPtr.Zero;
        }

        private static void ConsoleCleaner()
        {
            // Il2CppSystem.Console.SetOut(new Il2CppSystem.IO.StreamWriter(Il2CppSystem.IO.Stream.Null));
            try
            {
                Il2Cppmscorlib = Assembly.Load("Il2Cppmscorlib");
                if (Il2Cppmscorlib == null)
                    throw new Exception("Unable to Find Assembly Il2Cppmscorlib!");

                streamType = Il2Cppmscorlib.GetType("Il2CppSystem.IO.Stream");
                if (streamType == null)
                    throw new Exception("Unable to Find Type Il2CppSystem.IO.Stream!");

                PropertyInfo propertyInfo = streamType.GetProperty("Null", BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo == null)
                    throw new Exception("Unable to Find Property Il2CppSystem.IO.Stream.Null!");

                MethodInfo nullStreamField = propertyInfo.GetGetMethod();
                if (nullStreamField == null)
                    throw new Exception("Unable to Find Get Method of Property Il2CppSystem.IO.Stream.Null!");

                object nullStream = nullStreamField.Invoke(null, new object[0]);
                if (nullStream == null)
                    throw new Exception("Unable to Get Value of Property Il2CppSystem.IO.Stream.Null!");

                Type streamWriterType = Il2Cppmscorlib.GetType("Il2CppSystem.IO.StreamWriter");
                if (streamWriterType == null)
                    throw new Exception("Unable to Find Type Il2CppSystem.IO.StreamWriter!");

                object nullStreamWriter = null;
                ConstructorInfo[] constructors = streamWriterType.GetConstructors();
                foreach (var ctor in constructors)
                {
                    ParameterInfo[] parameters = ctor.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == streamType)
                    {
                        nullStreamWriter = ctor.Invoke(new[] { nullStream });
                        break;
                    }
                    else if (parameters.Length == 4 && parameters[0].ParameterType == streamType)
                    {
                        Type encodingType = Il2Cppmscorlib.GetType("Il2CppSystem.Text.Encoding");
                        if (encodingType == null)
                            throw new Exception("Unable to Find Type Il2CppSystem.Text.Encoding!");

                        MethodInfo getUtf8Method = encodingType.GetProperty("UTF8", BindingFlags.Static | BindingFlags.Public)?.GetGetMethod();
                        if (getUtf8Method == null)
                            throw new Exception("Unable to Find Method Il2CppSystem.Text.Encoding.get_UTF8!");

                        object utf8Encoding = getUtf8Method.Invoke(null, null);
                        if (utf8Encoding == null)
                            throw new Exception("Unable to Get Value of Il2CppSystem.Text.Encoding.UTF8!");

                        nullStreamWriter = ctor.Invoke(new[] { nullStream, utf8Encoding, 1024, false });
                        break;
                    }
                }

                if (nullStreamWriter == null)
                    throw new Exception("Unable to Find Suitable Constructor of Type Il2CppSystem.IO.StreamWriter!");

                Type consoleType = Il2Cppmscorlib.GetType("Il2CppSystem.Console");
                if (consoleType == null)
                    throw new Exception("Unable to Find Type Il2CppSystem.Console!");

                MethodInfo setOutMethod = consoleType.GetMethod("SetOut", BindingFlags.Static | BindingFlags.Public);
                if (setOutMethod == null)
                    throw new Exception("Unable to Find Method Il2CppSystem.Console.SetOut!");

                setOutMethod.Invoke(null, new[] { nullStreamWriter });
            }
            catch (Exception ex) { MelonLogger.Warning($"Console Cleaner Failed: {ex}"); }
        }
    }

    internal sealed class MelonDetourProvider : IDetourProvider
    {
        private static readonly List<object> s_keepAlive = new();
        public IDisposable Create<TDelegate>(nint original, TDelegate target, out TDelegate trampoline)
            where TDelegate : Delegate
        {
            var detour =
                DetourContext.CurrentFactory!.CreateNativeDetour(original, Marshal.GetFunctionPointerForDelegate(target));

            if (!detour.HasOrigEntrypoint)
            {
                throw new Exception("HasOrigEntrypoint has to be true");
            }

            trampoline = Marshal.GetDelegateForFunctionPointer<TDelegate>(detour.OrigEntrypoint);

            // monomod reorg undoes the hook on finalizer which is very stupid
            s_keepAlive.Add(detour);

            return detour;
        }
    }

    internal class InteropLogger
        : Microsoft.Extensions.Logging.ILogger
    {
        private MelonLogger.Instance _logger = new("Il2CppInterop");

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, System.Func<TState, Exception, string> formatter)
        {
            var logLine = state.ToString() ?? string.Empty;

            if (exception != null)
                logLine += $"\n{exception}";
            
            switch (logLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Trace:
                    MelonDebug.Msg(logLine);
                    break;

                case LogLevel.Critical:
                case LogLevel.Error:
                    _logger.Error(logLine);
                    break;

                case LogLevel.Warning:
                    _logger.Warning(logLine);
                    break;

                case LogLevel.Information:
                case LogLevel.None:
                default:
                    _logger.Msg(logLine);
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Debug or LogLevel.Trace => MelonDebug.IsEnabled(),
                _ => true
            };

        public IDisposable BeginScope<TState>(TState state)
            => throw new System.NotImplementedException();
    }
}
