using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ActionProtocols
{
    public enum Actions
    {
        List,
        Enter,
        Move,
        Hurt,
        Attack,
        Die
    }

    public static string GetProtocolScript(Actions action, ClientState cs)
    {
        var sendStr = "";

        if (action == Actions.List)
        {
            sendStr = $"List|";
        }
        else if (action == Actions.Enter)
        {
            // protocol type: <Command>|<RemoteIPEndPoint>,<Location.x>,<Location.y>,<Location.z>,<Rotation.y>
            sendStr = $"Enter|{NetManager.GetDesc()},{cs.x},{cs.y},{cs.z},{cs.y}";
        }
        else if (action == Actions.Move)
        {
            // protocol type: <Command>|<RemoteIPEndPoint>,<Location.x>,<Location.y>,<Location.z>
            sendStr = $"Move|{NetManager.GetDesc()},{cs.x:F4},{cs.y:F4},{cs.z:F4}";
        }
        else if (action == Actions.Attack)
        {
            // protocol type: <Command>|<RemoteIPEndPoint>,<Rotation.y>
            sendStr = $"Attack|{NetManager.GetDesc()},{cs.eulY:F4}";
        }
        else if (action == Actions.Hurt)
        {
            // protocol type: <Command>|<RemoteIPEndPoint>,<Target.IPEndPoint>,<Hurt Damage>
            sendStr = $"Hurt|{NetManager.GetDesc()},{cs.desc},1";
        }

        return sendStr;
    }
}
