namespace ChatCommon;

public static class Protocol
{
    // клиент → сервер
    public const string Login = "login";
    // сервер → клиент
    public const string LoginOk = "login_ok";
    public const string LoginFail = "login_fail";
    // обе стороны
    public const string Text = "text";
    public const string Private = "private";   // ★
    // сервер → клиент
    public const string System = "system";
    public const string Users = "users";
    public const string History = "history";   // ★★
    public const string Error = "error";
}