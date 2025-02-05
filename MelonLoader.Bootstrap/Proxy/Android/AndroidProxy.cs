using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using JNISharp.NativeInterface;
using MelonLoader.Bootstrap.Logging;
using MelonLoader.Bootstrap.RuntimeHandlers.Il2Cpp;
using MelonLoader.Bootstrap.Utils;
using MelonLoader.Utils;


namespace MelonLoader.Bootstrap.Proxy.Android;

public static class AndroidProxy
{
    private static string PackageName;
    private static string DataDir;
    internal static string DotnetDir;
    
    
    [DllImport("liblog", EntryPoint = "__android_log_print")]
    public static extern int Log(int prio, string tag,  string text);

    public static void LogWith(string text) => Log(3, "MelonLoader", text);



    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "startup")]
    [RequiresDynamicCode("Calls MelonLoader.Bootstrap.Core.Init(nint)")]
    private static unsafe void Load()
    {

        if (!Il2CppHandler.TryInitialize())
        {
            MelonDebug.Log("Il2cppHandler.TryInitialize() failed");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "JNI_OnLoad")]
    [RequiresDynamicCode("")]
    public static unsafe int JNI_OnLoad(IntPtr vm, IntPtr dunno)
    {
        Core.LibraryHandle = System.Runtime.InteropServices.NativeLibrary.Load("libBootstrap.so");
        JNI.Initialize(vm);
        CacheDataDir();
        EnsurePerms();

        // var directoryInfo = new DirectoryInfo(Path.Combine(DataDir, "MelonLoader"));
        // if (!directoryInfo.Exists)
            // directoryInfo.Create();
            
        LoaderConfig.Current.Loader.BaseDirectory = DataDir;

        Core.InitConfig();

        MelonLogger.Init();

        MelonDebug.Log("JNI initialized!");
        APKAssetManager.Initialize();


        CopyMelonLoaderData(GetApkModificationDate());
        
        MelonDebug.Log("APK assets copied!");
        

        
        // Log(3, "MelonLoader", jMethodID.Handle.ToString());
        return 1;
    }

    public static void CacheDataDir()
    {
        JClass unityPlayer = JNI.FindClass("com/unity3d/player/UnityPlayer");
        JFieldID activityFieldId = JNI.GetStaticFieldID(unityPlayer, "currentActivity", "Landroid/app/Activity;");
        JObject currentActivityObj = JNI.GetStaticObjectField<JObject>(unityPlayer, activityFieldId);
        var callObjectMethod = JNI.CallObjectMethod<JString>(currentActivityObj, JNI.GetMethodID(JNI.GetObjectClass(currentActivityObj), "getPackageName", "()Ljava/lang/String;"));
        PackageName =  callObjectMethod.GetString();
        // JNI.DeleteLocalRef(callObjectMethod);

        JClass environment = JNI.FindClass("android/os/Environment");
        // LogWith((jMethodID.Handle == IntPtr.Zero).ToString());
        var getExtDir = JNI.CallStaticObjectMethod<JObject>(environment, JNI.GetStaticMethodID(environment, "getExternalStorageDirectory", "()Ljava/io/File;"));

        var jMethodID = JNI.GetMethodID(JNI.GetObjectClass(getExtDir), "toString", "()Ljava/lang/String;");
        var objectMethod = JNI.CallObjectMethod<JString>(getExtDir, jMethodID);
        
        // JNI.DeleteLocalRef(getExtDir);
        DataDir = Path.Combine(objectMethod.GetString(), "MelonLoader", PackageName);

    }

    public static bool EnsurePerms()
    {
        const int TRIES = 3;  // Number of attempts
        const int DELAY = 5000; // Delay in milliseconds
        for (int i = 0; i < TRIES; i++)
        {
            JClass unityPlayer = JNI.FindClass("com/unity3d/player/UnityPlayer");
            JFieldID activityFieldId = JNI.GetStaticFieldID(unityPlayer, "currentActivity", "Landroid/app/Activity;");
            JObject currentActivityObj = JNI.GetStaticObjectField<JObject>(unityPlayer, activityFieldId);

            JClass environment = JNI.FindClass("android/os/Environment");
            JClass uri = JNI.FindClass("android/net/Uri");
            JClass intent = JNI.FindClass("android/content/Intent");

        
            var callStaticMethod = JNI.CallStaticMethod<bool>(environment, environment.GetStaticMethodID("isExternalStorageManager", "()Z"));
            if (JNI.ExceptionCheck())
                return false;

            if (callStaticMethod)
                return true;


            var actionName = JNI.NewString("android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION");

            var packageName = JNI.NewString($"package:{PackageName}");

            var callStaticObjectMethod = JNI.CallStaticObjectMethod<JObject>(uri, uri.GetStaticMethodID("parse", "(Ljava/lang/String;)Landroid/net/Uri;"), packageName);
            JMethodID intentConstructor = JNI.GetMethodID(intent, "<init>", "(Ljava/lang/String;Landroid/net/Uri;)V");

            var newObject = JNI.NewObject<JObject>(intent, intentConstructor, actionName, callStaticObjectMethod);

            var activityClass = JNI.GetObjectClass(currentActivityObj);
            activityClass.CallVoidMethod(currentActivityObj, "startActivity", "(Landroid/content/Intent;)V", newObject);

            JNI.CheckExceptionAndThrow();
            
            System.Threading.Thread.Sleep(DELAY);
        }
        
        
        return true;
    }

    public static DateTimeOffset  GetApkModificationDate()
    { 
        var assetBytes = APKAssetManager.GetAssetBytes("lemon_patch_date.txt");
        string assetContent = Encoding.UTF8.GetString(assetBytes);

        // Now parse the string content into an RFC 3339 DateTime
        // RFC 3339 is essentially ISO 8601, so DateTime.Parse can handle it.
        DateTimeOffset date;
        if (DateTimeOffset.TryParse(assetContent, out date))
        {
            return date;
        }

        return default;
    }

    public static void CopyMelonLoaderData(DateTimeOffset date)
    {
        var combine = Path.Combine(DataDir);
        var combineMelon = Path.Combine(DataDir, "MelonLoader");
        var combineInternal= $"/data/data/{PackageName}/";
        var combineDotnet = $"/data/data/{PackageName}/dotnet";
        MelonDebug.Log(combineDotnet);
        if (Directory.Exists(combineMelon))
        {
            var fileModTime = Directory.GetLastWriteTimeUtc(combineMelon);
            if (fileModTime > date)
            {
                MelonDebug.Log("MelonLoader folder is already up-to-date");
            }
            else
            {
                APKAssetManager.SaveItemToDirectory("MelonLoader", combine, true);

            }
        }
        else
        {
            APKAssetManager.SaveItemToDirectory("MelonLoader", combine, true);

        }
        // if(Directory.Exists(combineDotnet))
        //     Directory.Delete(combineDotnet);
        if (Directory.Exists(combineDotnet))
        {
            var fileModTime = Directory.GetLastWriteTimeUtc(combineDotnet);
            if (fileModTime > date)
            {
                MelonDebug.Log("Dotnet folder is already up-to-date");
            }
            else
            {
                APKAssetManager.SaveItemToDirectory("dotnet", combineInternal, true);
            }
        }
        else
        {
            APKAssetManager.SaveItemToDirectory("dotnet", combineInternal, true);
        }

        DotnetDir = Path.Combine(combineDotnet);



    }
    
   
    
    
}