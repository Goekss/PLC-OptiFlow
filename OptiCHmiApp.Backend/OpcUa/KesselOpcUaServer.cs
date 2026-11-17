using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using OptiCHmiApp.Backend.Services;

namespace OptiCHmiApp.Backend.OpcUa;

// Minimaler OPC-UA-Server (Demo): SecurityPolicy None, Zertifikate werden
// automatisch akzeptiert. Läuft auf Port 4840, sofern appsettings.json
// "OpcUa:Enabled" = true ist.
public sealed class KesselOpcUaServer
{
    private ApplicationInstance? _application;
    private KesselServer? _server;

    public async Task StartAsync(IKesselQuelle quelle, int port = 4840)
    {
        var config = BuildConfiguration(port);
        await config.ValidateAsync(ApplicationType.Server).ConfigureAwait(false);

        _application = new ApplicationInstance(config)
        {
            ApplicationName = "OptiCHmiApp Kessel OPC UA",
            ApplicationType = ApplicationType.Server
        };

        var certOk = await _application.CheckApplicationInstanceCertificatesAsync(false, 0).ConfigureAwait(false);
        if (!certOk)
        {
            throw new InvalidOperationException("OPC UA uygulama sertifikası oluşturulamadı/doğrulanamadı.");
        }

        _server = new KesselServer(quelle);
        await _application.StartAsync(_server).ConfigureAwait(false);
    }

    public void Stop()
    {
        _application?.Stop();
        _server?.Dispose();
    }

    private static ApplicationConfiguration BuildConfiguration(int port)
    {
        var hostName = Utils.GetHostName();

        return new ApplicationConfiguration
        {
            ApplicationName = "OptiCHmiApp Kessel OPC UA",
            ApplicationUri = $"urn:{hostName}:OptiCHmiApp:Kessel",
            ApplicationType = ApplicationType.Server,

            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/own",
                    SubjectName = $"CN=OptiCHmiApp Kessel OPC UA, DC={hostName}"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/trusted"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/issuer"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/rejected"
                },
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = true
            },

            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },

            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = { $"opc.tcp://localhost:{port}" },
                SecurityPolicies =
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = SecurityPolicies.None
                    }
                },
                MinRequestThreadCount = 2,
                MaxRequestThreadCount = 10,
                MaxQueuedRequestCount = 200
            }
        };
    }
}
