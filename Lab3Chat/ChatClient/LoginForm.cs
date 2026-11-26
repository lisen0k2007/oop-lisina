using System.Net.Sockets;
using ChatCommon;

namespace ChatClient;

public sealed class LoginForm : Form
{
    private readonly TextBox _txtHost = new() { Text = "127.0.0.1", Width = 180 };
    private readonly NumericUpDown _numPort = new() { Minimum = 1, Maximum = 65535, Value = 5000, Width = 90 };
    private readonly TextBox _txtNick = new() { Width = 180, Text = Environment.UserName };
    private readonly Button _btnLogin = new() { Text = "Войти", Width = 100 };
    private readonly Label _lblStatus = new() { ForeColor = Color.DarkRed, AutoSize = true };

    public LoginForm()
    {
        Text = "Вход в чат";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 200);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Адрес:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtHost, 1, 0);
        layout.Controls.Add(new Label { Text = "Порт:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        layout.Controls.Add(_numPort, 1, 1);
        layout.Controls.Add(new Label { Text = "Ник:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
        layout.Controls.Add(_txtNick, 1, 2);
        layout.Controls.Add(_btnLogin, 1, 3);
        layout.Controls.Add(_lblStatus, 1, 4);

        Controls.Add(layout);
        AcceptButton = _btnLogin;
        _btnLogin.Click += async (_, _) => await DoLoginAsync();
    }

    private async Task DoLoginAsync()
    {
        _lblStatus.Text = "";
        _btnLogin.Enabled = false;
        try
        {
            var host = _txtHost.Text.Trim();
            var port = (int)_numPort.Value;
            var nick = _txtNick.Text.Trim();

            if (string.IsNullOrEmpty(host)) { _lblStatus.Text = "Введите адрес"; return; }
            if (string.IsNullOrEmpty(nick)) { _lblStatus.Text = "Введите ник"; return; }

            var client = new ChatClient();
            ChatMessage response;
            try
            {
                response = await client.ConnectAndLoginAsync(host, port, nick);
            }
            catch (SocketException ex)
            {
                client.Dispose();
                _lblStatus.Text = "Сервер недоступен: " + ex.Message;
                return;
            }
            catch (TimeoutException)
            {
                client.Dispose();
                _lblStatus.Text = "Сервер не ответил";
                return;
            }

            if (response.Type == Protocol.LoginFail)
            {
                client.Dispose();
                _lblStatus.Text = "Ник занят: " + response.Text;
                return;
            }

            var chat = new ChatForm(client);
            chat.FormClosed += (_, _) =>
            {
                if (!IsDisposed) { Show(); _btnLogin.Enabled = true; }
            };
            Hide();
            chat.Show();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Ошибка: " + ex.Message;
        }
        finally
        {
            if (Visible) _btnLogin.Enabled = true;
        }
    }
}