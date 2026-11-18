using System.Net;
using System.Net.Sockets;
using ChatCommon;

namespace ChatServer;

public class ChatServer
{
    private readonly TcpListener _listener;
    private readonly List<ClientHandler> _clients = new();
    private readonly object _gate = new();
    private readonly HistoryStore _history;

    public ChatServer(int port, string historyPath)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _history = new HistoryStore(historyPath);
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (true)
        {
            var tcp = _listener.AcceptTcpClient();
            var handler = new ClientHandler(this, tcp);
            new Thread(handler.Run) { IsBackground = true }.Start();
        }
    }

    public bool TryRegister(ClientHandler h, string nick)
    {
        lock (_gate)
        {
            if (_clients.Any(c => c.Nick == nick)) return false;
            h.Nick = nick;
            _clients.Add(h);
            return true;
        }
    }

    public void Unregister(ClientHandler h)
    {
        lock (_gate) { _clients.Remove(h); }

        if (h.Nick != null)
        {
            Broadcast(new ChatMessage(Protocol.System, "server", $"{h.Nick} вышел", DateTime.Now));
            BroadcastUsers();
        }
    }

    public void Broadcast(ChatMessage msg)
    {
        List<ClientHandler> snapshot;
        lock (_gate) { snapshot = _clients.ToList(); } // снимок под lock

        var line = msg.ToJson();
        foreach (var c in snapshot) c.SendRaw(line); // отправка вне lock
    }

    public void SendTo(string nick, ChatMessage msg)
    {
        ClientHandler? target;
        lock (_gate) { target = _clients.FirstOrDefault(c => c.Nick == nick); }
        target?.SendRaw(msg.ToJson());
    }

    public void PublishText(ChatMessage msg)
    {
        _history.Append(msg);
        Broadcast(msg);
    }

    public List<ChatMessage> GetHistory(int n) => _history.LastN(n);

    public void BroadcastUsers()
    {
        var users = new List<string>();
        lock (_gate) { users = _clients.Select(c => c.Nick!).ToList(); }

        // В протоколе users передается как JSON-массив строк в поле Text
        var jsonUsers = System.Text.Json.JsonSerializer.Serialize(users);
        Broadcast(new ChatMessage(Protocol.Users, "server", jsonUsers, DateTime.Now));
    }
}