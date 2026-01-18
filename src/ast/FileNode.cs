namespace codecraftersshell.ast;

public class FileNode(string filePath)
    : IAstNode
{
    public string FilePath => filePath;
}