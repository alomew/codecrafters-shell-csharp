namespace codecraftersshell.ast;

public class CommandNode(string command, IEnumerable<string> arguments)
    : IAstNode
{
    public string Command => command;
    public IEnumerable<string> Arguments => arguments;
}