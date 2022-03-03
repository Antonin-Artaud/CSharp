namespace ClientUnity
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using global::ClientUnity.Commands;
    using global::ClientUnity.Sockets;
    using MessagePack;
    using MessagePack.Resolvers;
    using ProtocolShared;
    using ProtocolShared.Commands;

    public class ClientUnity : MonoBehaviour
    {
        public static ClientUnity Instance { get; private set; }
        public TcpConnection Connection { get; private set; }
        public CommandManager CommandManager { get; private set; }
        public ConcurrentQueue<ICommand> ReceivedCommands { get; private set; }
        
        public uint UserId { get; private set; }
        
        public string Host = "127.0.0.1";
        public ushort Port = 8888;
        public static uint Id = 1;
        public bool AutoConnectOnStartup;

        public string GetId() { return ClientUnity.Id.ToString(); }

        private void Awake()
        {
            ClientUnity.Instance = this;
            this.Connection = new TcpConnection();
            this.CommandManager = new CommandManager(this);
            this.ReceivedCommands = new ConcurrentQueue<ICommand>();
            this.Connection.OnConnectionSuccessful += this.OnConnectionSuccessful;
            this.Connection.OnConnectionFailed += this.OnConnectionFailed;
            this.Connection.OnCommandReceived += this.OnCommandReceived;
            
            StaticCompositeResolver.Instance.Register(
                MessagePack.Unity.UnityResolver.Instance,
                MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver.Instance,
                GeneratedResolver.Instance,
                StandardResolver.Instance
            );

            MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }

        private void Start()
        {
            if (this.AutoConnectOnStartup)
            {
                this.Connect();
            }
        }

        private void OnCommandReceived(ICommand command)
        {
            this.ReceivedCommands.Enqueue(command);
        }

        private void OnConnectionFailed()
        {
            
        }

        private void OnConnectionSuccessful()
        {
            this.Login(ClientUnity.Id);
        }

        public void Connect()
        {
            this.Connection.Connect(this.Host, this.Port);
        }

        public void Login(uint userId)
        {
            this.UserId = userId;
            this.Connection.Send(new AuthenticationCommand
            {
                UserId = userId
            });
        }
    }
}