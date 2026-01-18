namespace codecraftersshell.ast;

public class RedirectNode(CommandNode commandNode, FileNode fileNode)
    : IAstNode
{
    public CommandNode CommandNode => commandNode;
    public FileNode FileNode => fileNode;
}