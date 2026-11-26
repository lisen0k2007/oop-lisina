using System.Text.Json;
using ChatCommon;

namespace ChatClient;

public sealed class ChatForm : Form
{
    private readonly ChatClient _client;
    private readonly RichTextBox _feed = new() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, WordWrap = true, Font = new Font("Segoe UI", 10) };
    private readonly ListBox _users = new() { Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Segoe UI", 10) };
    private readonly TextBox _input = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
    private readonly Button _send = new() { Text = "Отправить", Width = 110 };
    private readonly Label _hint = new() { Text = "Выберите участника — отправить лично. Esc — снять выбор.", Dock = DockStyle.Bottom, Height = 32, ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleLeft };
    private bool _historyShown;

    public ChatForm(ChatClient client)
    {
        _client = client;
        Text = "Чат — " + (client.Nick ?? "?");
        Width = 900; Height = 580;
        StartPosition = FormStartPosition.CenterScreen;

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 660 };
        split.Panel1.Controls.Add(_feed);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(6) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(new Label { Text = "Онлайн:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 0);
        right.Controls.Add(_users, 0, 1);
        split.Panel2.Controls.Add(right);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 44, ColumnCount = 2, Padding = new Padding(8, 4, 8, 4) };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        bottom.Controls.Add(_input, 0, 0);
        bottom.Controls.Add(_send, 1, 0);

        Controls.Add(split);
        Controls.Add(bottom);
        Controls.Add(_hint);

        _client.MessageReceived += OnMessage;
        _client.Error += OnError;
        _client.Disconnected += OnDisconnected;
        this.Load += (_, _) => _client.StartProcessing();

        _send.Click += (_, _) => SendCurrent();
        _input.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendCurrent(); }
        };
        _users.SelectedIndexChanged += (_, _) => UpdateMode();

        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) { _users.ClearSelected(); UpdateMode(); }
        };

        FormClosed += (_, _) => _client.Dispose();
    }

    private void SendCurrent()
    {
        var text = _input.Text;
        if (string.IsNullOrWhiteSpace(text)) return;
        _input.Clear();

        var selected = _users.SelectedItem as string;
        var myNick = _client.Nick ?? "";
        if (!string.IsNullOrEmpty(selected) && selected != myNick)
            _client.SendPrivate(selected, text);
        else
            _client.SendText(text);
    }

    private void UpdateMode()
    {
        var selected = _users.SelectedItem as string;
        var myNick = _client.Nick ?? "";
        if (!string.IsNullOrEmpty(selected) && selected != myNick)
        {
            _send.Text = "→ " + selected;
            _send.ForeColor = Color.DarkMagenta;
        }
        else
        {
            _send.Text = "Отправить";
            _send.ForeColor = SystemColors.ControlText;
        }
    }

    private void OnMessage(ChatMessage msg)
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        if (InvokeRequired) { BeginInvoke(new Action(() => OnMessage(msg))); return; }

        switch (msg.Type)
        {
            case Protocol.Text:
            case Protocol.Private:
            case Protocol.System:
                AppendMessage(msg);
                break;
            case Protocol.Users:
                UpdateUsers(msg.Text);
                break;
            case Protocol.History:
                ShowHistory(msg.Text);
                break;
            case Protocol.Error:
                AppendLine($"[!] {msg.Text}", Color.DarkRed);
                break;
        }
    }

    private void AppendMessage(ChatMessage msg)
    {
        switch (msg.Type)
        {
            case Protocol.Text:
                AppendLine($"[{msg.Time:HH:mm}] {msg.Nick}: {msg.Text}", Color.Black);
                break;
            case Protocol.Private:
                var arrow = msg.Nick == _client.Nick ? $"я → {msg.To}" : $"{msg.Nick} → {msg.To}";
                AppendLine($"[{msg.Time:HH:mm}] {arrow} (лично): {msg.Text}", Color.DarkMagenta);
                break;
            case Protocol.System:
                AppendLine($"[*] {msg.Text}", Color.DarkSlateGray);
                break;
        }
    }

    private void ShowHistory(string json)
    {
        if (_historyShown) return;
        _historyShown = true;

        ChatMessage[] history;
        try { history = JsonSerializer.Deserialize<ChatMessage[]>(json) ?? Array.Empty<ChatMessage>(); }
        catch { return; }
        if (history.Length == 0) return;

        AppendLine("──── история ────", Color.Gray);
        foreach (var m in history) AppendMessage(m);
        AppendLine("──── конец истории ────", Color.Gray);
    }

    private void UpdateUsers(string json)
    {
        string[] users;
        try { users = JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>(); }
        catch { return; }

        var current = _users.SelectedItem as string;
        _users.BeginUpdate();
        _users.Items.Clear();
        foreach (var u in users) _users.Items.Add(u);
        if (current != null && users.Contains(current)) _users.SelectedItem = current;
        _users.EndUpdate();

        Text = $"Чат — {_client.Nick}  (онлайн: {users.Length})";
        UpdateMode();
    }

    private void AppendLine(string text, Color color)
    {
        _feed.SelectionStart = _feed.TextLength;
        _feed.SelectionLength = 0;
        _feed.SelectionColor = color;
        _feed.AppendText(text + Environment.NewLine);
        _feed.SelectionColor = _feed.ForeColor;
        _feed.ScrollToCaret();
    }

    private void OnError(string message)
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        if (InvokeRequired) { BeginInvoke(new Action(() => OnError(message))); return; }
        AppendLine($"[!] {message}", Color.DarkRed);
    }

    private void OnDisconnected()
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        if (InvokeRequired) { BeginInvoke(new Action(OnDisconnected)); return; }
        AppendLine("[!] Соединение с сервером потеряно", Color.DarkRed);
        _send.Enabled = false;
        _input.Enabled = false;
        Text = "Чат — соединение потеряно";
    }
}