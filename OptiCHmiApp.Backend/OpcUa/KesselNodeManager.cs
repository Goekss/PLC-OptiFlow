using Opc.Ua;
using Opc.Ua.Server;
using OptiCHmiApp.Backend.Services;

namespace OptiCHmiApp.Backend.OpcUa;

// Stellt 4 schreibgeschützte Variablen (Temperatur, Heizung, Pumpe, Sollwert) bereit.
// Değerler IKesselQuelle'den okunur (Simulation oder ADS).
public class KesselNodeManager : CustomNodeManager2
{
    private readonly IKesselQuelle _quelle;
    private ushort _namespaceIndex;

    public KesselNodeManager(IServerInternal server, ApplicationConfiguration configuration, IKesselQuelle quelle)
        : base(server, configuration, "urn:OptiCHmiApp:Kessel")
    {
        _quelle = quelle;
        SystemContext.NodeIdFactory = this;
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        lock (Lock)
        {
            _namespaceIndex = NamespaceIndexes[0];

            var folder = new FolderState(null)
            {
                SymbolicName = "Kessel",
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId("Kessel", _namespaceIndex),
                BrowseName = new QualifiedName("Kessel", _namespaceIndex),
                DisplayName = new LocalizedText("Kessel"),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            folder.AddChild(CreateVariable("Temperatur", DataTypeIds.Double));
            folder.AddChild(CreateVariable("Heizung", DataTypeIds.Boolean));
            folder.AddChild(CreateVariable("Pumpe", DataTypeIds.Boolean));
            folder.AddChild(CreateVariable("Sollwert", DataTypeIds.Double));

            AddPredefinedNode(SystemContext, folder);
        }
    }

    protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
    {
        return new NodeStateCollection();
    }

    protected override NodeState AddBehaviourToPredefinedNode(ISystemContext context, NodeState predefinedNode)
    {
        if (predefinedNode is BaseVariableState variable)
        {
            variable.OnReadValue = OnReadValue;
        }

        return predefinedNode;
    }

    private BaseVariableState CreateVariable(string name, NodeId dataType)
    {
        return new BaseDataVariableState(null)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId(name, _namespaceIndex),
            BrowseName = new QualifiedName(name, _namespaceIndex),
            DisplayName = new LocalizedText(name),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            DataType = dataType,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = AccessLevels.CurrentRead,
            UserAccessLevel = AccessLevels.CurrentRead,
            Historizing = false,
            StatusCode = Opc.Ua.StatusCodes.Good,
            Timestamp = DateTime.UtcNow
        };
    }

    private ServiceResult OnReadValue(
        ISystemContext context,
        NodeState node,
        NumericRange indexRange,
        QualifiedName dataEncoding,
        ref object value,
        ref StatusCode statusCode,
        ref DateTime timestamp)
    {
        var zustand = _quelle.LeseZustand();

        switch (node.SymbolicName)
        {
            case "Temperatur":
                value = zustand.Temperatur;
                break;
            case "Heizung":
                value = zustand.Heizung;
                break;
            case "Pumpe":
                value = zustand.Pumpe;
                break;
            case "Sollwert":
                value = zustand.Sollwert;
                break;
            default:
                return Opc.Ua.StatusCodes.BadNodeIdUnknown;
        }

        timestamp = DateTime.UtcNow;
        statusCode = Opc.Ua.StatusCodes.Good;
        return ServiceResult.Good;
    }
}
