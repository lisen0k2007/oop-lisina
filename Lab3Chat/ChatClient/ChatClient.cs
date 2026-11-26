using System.Net.Sockets;
using System.Text;
using ChatCommon;

namespace ChatClient;

public sealed class ChatClient : IDisposable
{
    public event Action<ChatMessage>? MessageReceived;
    public event Action<string>? Error;
    public event Action? Disconnected;

    private TcpClient? _tcp;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private Thread? _readThread;
    private readonly object _writeLock = new();
    private readonly object _stateLock = new();
    private readonly object _bufferLock = new();
    private readonly List<ChatMessage> _buffer = new();
    private volatile bool _started;
    private volatile bool _closed;
    private TaskCompletionSource<ChatMessage>? _pendingLogin;

    public string? Nick { get; private set; }
    public bool IsConnected => !_closed && _tcp is { Connected: true };

    public async Task<ChatMessage> ConnectAndLoginAsync(string host, int port, string nick, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Пустой адрес");
        if (port is <= 0 or > 65535) throw new ArgumentException("Неверный порт");
        if (string.IsNullOrWhiteSpace(nick)) throw new ArgumentException("Пустой ник");

        var tcp = new TcpClient();
        try
        {
            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));
            await tcp.ConnectAsync(host, port, cts.Token);
        }
        catch
        {
            tcp.Dispose();
            throw;
        }

        _tcp = tcp;
        Nick = nick;
        _closed = false;

        var stream = tcp.GetStream();
        _reader = new StreamReader(stream, new UTF8Encoding(false));
        _writer = new StreamWriter(stream, new UTF8Encoding(false))
        {
            NewLine = "\n",
            AutoFlush = true
        };

        _pendingLogin = new TaskCompletionSource<ChatMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        _readThread = new Thread(ReadLoop) { IsBackground = true, Name = "chat-reader" };
        _readThread.Start();

        SendRaw(new ChatMessage(Protocol.Login, nick, "", DateTime.Now));

        var waitLogin = _pendingLogin.Task;
        var timeoutTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(10));
        if (await Task.WhenAny(waitLogin, timeoutTask) != waitLogin)
        {
            Dispose();
            throw new TimeoutException("Сервер не ответил на вход");
        }
        return await waitLogin;
    }

    public void StartProcessing()
    {
        ChatMessage[] pending;
        lock (_bufferLock)
        {
            _started = true;
            pending = _buffer.ToArray();
            _buffer.Clear();
        }
        foreach (var m in pending) MessageReceived?.Invoke(m);
    }

    public void SendText(string text)
    {
        if (!IsConnected) { Error?.Invoke("Нет соединения с сервером"); return; }
        if (string.IsNullOrEmpty(text)) return;
        SendRaw(new ChatMessage(Protocol.Text, Nick ?? "", text, DateTime.Now));
    }

    public void SendPrivate(string to, string text)
    {
        if (!IsConnected) { Error?.Invoke("Нет соединения с сервером"); return; }
        if (string.IsNullOrWhiteSpace(to)) { Error?.Invoke("Не выбран адресат"); return; }
        if (string.IsNullOrEmpty(text)) return;
        SendRaw(new ChatMessage(Protocol.Private, Nick ?? "", text, DateTime.Now, to));
    }

    private void SendRaw(ChatMessage msg)
    {
        try
        {
            var json = msg.ToJson();
            lock (_writeLock) { _writer?.WriteLine(json); }
        }
        catch (Exception ex)
        {
            Error?.Invoke("Ошибка отправки: " + ex.Message);
            HandleDisconnect();
        }
    }

    private void ReadLoop()
    {
        try
        {
            while (!_closed)
            {
                var line = _reader?.ReadLine();
                if (line == null) break;

                ChatMessage? msg;
                try { msg = ChatMessage.FromJson(line); }
                catch (Exception ex)
                {
                    Error?.Invoke("Некорректное сообщение: " + ex.Message);
                    continue;
                }

                var pending = _pendingLogin;
                if (pending != null && (msg.Type == Protocol.LoginOk || msg.Type == Protocol.LoginFail))
                {
                    _pendingLogin = null;
                    pending.TrySetResult(msg);
                    continue;
                }

                if (!_started)
                {
                    bool buffered = false;
                    lock (_bufferLock)
                    {
                        if (!_started) { _buffer.Add(msg); buffered = true; }
                    }
                    if (buffered) continue;
                }

                if (msg.Type == Protocol.Error) Error?.Invoke(msg.Text);
                MessageReceived?.Invoke(msg);
            }
        }
        catch (Exception ex)
        {
            if (!_closed) Error?.Invoke("Соединение разорвано: " + ex.Message);
        }
        finally
        {
            HandleDisconnect();
        }
    }

    private void HandleDisconnect()
    {
        TaskCompletionSource<ChatMessage>? pending;
        lock (_stateLock)
        {
            if (_closed) return;
            _closed = true;
            pending = _pendingLogin;
            _pendingLogin = null;
        }
        pending?.TrySetException(new IOException("Соединение закрыто"));

        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _tcp?.Dispose(); } catch { }

        Disconnected?.Invoke();
    }

    public void Dispose()
    {
        lock (_stateLock)
        {
            if (_closed) return;
            _closed = true;
        }
        _pendingLogin?.TrySetCanceled();
        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _tcp?.Dispose(); } catch { }
    }
}