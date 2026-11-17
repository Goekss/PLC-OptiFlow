using Opc.Ua;
using Opc.Ua.Server;
using OptiCHmiApp.Backend.Services;

namespace OptiCHmiApp.Backend.OpcUa;

// StandardServer, der den KesselNodeManager als einzigen NodeManager registriert.
public class KesselServer : StandardServer
{
    private readonly IKesselQuelle _quelle;

    public KesselServer(IKesselQuelle quelle)
    {
        _quelle = quelle;
    }

    protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
    {
        var nodeManagers = new List<INodeManager>
        {
            new KesselNodeManager(server, configuration, _quelle)
        };

        return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
    }

    protected override ServerProperties LoadServerProperties()
    {
        return new ServerProperties
        {
            ManufacturerName = "OptiCHmiApp",
            ProductName = "OptiCHmiApp Kessel OPC UA Server",
            ProductUri = "urn:OptiCHmiApp:Kessel",
            SoftwareVersion = Utils.GetAssemblySoftwareVersion(),
            BuildNumber = Utils.GetAssemblyBuildNumber(),
            BuildDate = Utils.GetAssemblyTimestamp()
        };
    }
}
