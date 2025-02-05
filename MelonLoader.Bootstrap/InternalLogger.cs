using MelonLoader.Bootstrap.Logging;
using System.Drawing;
using MelonLoader.Bootstrap.Proxy.Android;

namespace MelonLoader.Bootstrap;

internal class InternalLogger(ColorRGB sectionColor, string sectionName)
{
    public void Msg(string msg)
    {
        MelonLogger.Log(Color.LightGray, msg, sectionColor, sectionName);
        
    }

    public void Error(string msg)
    {
        MelonLogger.LogError(msg, sectionName);

    }

    public void Warning(string msg)
    {
        MelonLogger.LogWarning(msg, sectionName);
    }
}
