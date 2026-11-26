using ChatServer;

int port = 5000;
string historyPath = "history.jsonl";

if (args.Length > 0) port = int.Parse(args[0]);
if (args.Length > 1) historyPath = args[1];

var server = new ChatServer.ChatServer(port, historyPath);
server.Start();