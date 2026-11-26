using System.Text.Json;
using ChatCommon;

namespace ChatServer;

public class HistoryStore
{
    private readonly string _path;
    private readonly object _lock = new();

    public HistoryStore(string path)
    {
        _path = path;
        if (!File.Exists(_path)) File.Create(_path).Close();
    }

    // Добавляем сообщение в файл (только text)
    public void Append(ChatMessage msg)
    {
        if (msg.Type != Protocol.Text) return; // system и private не пишем
        lock (_lock)
        {
            File.AppendAllText(_path, msg.ToJson() + "\n");
        }
    }

    // Читаем последние N сообщений
    public List<ChatMessage> LastN(int n)
    {
        var result = new List<ChatMessage>();
        lock (_lock)
        {
            if (!File.Exists(_path)) return result;

            var lines = File.ReadAllLines(_path);
            // Берем последние n строк
            var start = Math.Max(0, lines.Length - n);

            for (int i = start; i < lines.Length; i++)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                        result.Add(ChatMessage.FromJson(lines[i]));
                }
                catch { /* пропускаем битые строки */ }
            }
        }
        return result;
    }
}