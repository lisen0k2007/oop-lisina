using System.Net.Sockets;
using System.Text;
using ChatCommon;

namespace ChatServer;

public class ClientHandler
{
    private readonly ChatServer _server;
    private readonly TcpClient _tcp;
    private StreamReader _reader = null!;
    private StreamWriter _writer = null!;
    private readonly object _writeLock = new();

    public string? Nick { get; set; }

    public ClientHandler(ChatServer server, TcpClient tcp)
    {
        _server = server;
        _tcp = tcp;
    }

    public void Run()
    {
        try
        {
            var stream = _tcp.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // 1. Ждем login
            var firstLine = _reader.ReadLine();
            if (firstLine == null) return;

            var loginMsg = ChatMessage.FromJson(firstLine);
            if (loginMsg.Type != Protocol.Login) return;

            // 2. Проверяем ник
            if (!_server.TryRegister(this, loginMsg.Nick))
            {
                SendRaw(new ChatMessage(Protocol.LoginFail, "server", "Ник занят", DateTime.Now).ToJson());
                return;
            }

            // 3. Успешный вход
            SendRaw(new ChatMessage(Protocol.LoginOk, Nick!, "", DateTime.Now).ToJson());

            // Отправляем историю (★★)
            foreach (var histMsg in _server.GetHistory(20))
                SendRaw(histMsg.ToJson());

            // Оповещаем всех
            _server.Broadcast(new ChatMessage(Protocol.System, "server", $"{Nick} вошел", DateTime.Now));
            _server.BroadcastUsers();

            // 4. Цикл чтения сообщений
            string? line;
            while ((line = _reader.ReadLine()) != null)
            {
                try
                {
                    var msg = ChatMessage.FromJson(line);
                    msg = msg with { Nick = Nick! }; // Принудительно ставим правильный ник

                    if (msg.Type == Protocol.Text)
                    {
                        _server.PublishText(msg);
                    }
                    else if (msg.Type == Protocol.Private && msg.To != null)
                    {
                        // ★ Личные сообщения
                        _server.SendTo(msg.To, msg);
                        SendRaw(msg.ToJson()); // Отправляем копию автору
                    }
                }
                catch { /* игнорируем битые сообщения */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка клиента: {ex.Message}");
        }
        finally
        {
            _server.Unregister(this);
            _tcp.Close();
        }
    }

    public void SendRaw(string json)
    {
        lock (_writeLock)
        {
            try { _writer.WriteLine(json); } catch { }
        }
    }
}