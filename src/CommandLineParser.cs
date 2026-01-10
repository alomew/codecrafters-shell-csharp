using System.Text;

namespace codecraftersshell;

using System.Text.RegularExpressions;

public enum ParserState
{
    Start,
    Command,
    BlankCommand,
    Spaces,
    Arg,
    InsideSingleQuote,
    JustLeftQuote,
    InsideDoubleQuote,
    Done,
    Failed
}

public class CommandLineParser(string commandLine)
{
    private int _index = 0;
    private ParserState _state = ParserState.Start;
    private List<string> _sofarArgs = [];
    private StringBuilder _currentArg = new();

    public List<string>? ParseArgs()
    {
        while (_state != ParserState.Done &&  _state != ParserState.Failed)
        {
            if (_index >= commandLine.Length)
            {
                switch (_state)
                {
                    case ParserState.Start:
                        _state = ParserState.Done;
                        break;
                    case ParserState.Command:
                    case ParserState.InsideSingleQuote:
                    case ParserState.InsideDoubleQuote:
                        _state = ParserState.Failed;
                        break;
                    case ParserState.BlankCommand:
                    case ParserState.Spaces:
                        _state = ParserState.Done;
                        break;
                    case ParserState.JustLeftQuote:
                    case ParserState.Arg:
                        if (_currentArg.Length > 0)
                        {
                            _sofarArgs.Add(_currentArg.ToString());
                        }
                        _state = ParserState.Done;
                        break;
                    case ParserState.Failed:
                    case ParserState.Done:
                        break;
                }
            }
            else
            {
                switch (_state)
                {
                    case ParserState.Start:
                    case ParserState.Command:
                        CommandStep();
                        break;
                    case ParserState.BlankCommand:
                        BlankCommandStep();
                        break;
                    case ParserState.Spaces:
                        SpaceStep();
                        break;
                    case ParserState.Arg:
                        ArgStep();
                        break;
                    case ParserState.InsideSingleQuote:
                        InsideSingleQuoteStep();
                        break;
                    case ParserState.InsideDoubleQuote:
                        InsideDoubleQuoteStep();
                        break;
                    case ParserState.JustLeftQuote:
                        JustLeftQuoteStep();
                        break;
                    case ParserState.Failed:
                        return null;
                    case ParserState.Done:
                        break;
                }
            }
        }

        return _state == ParserState.Done ? _sofarArgs : null;
    }

    private void BlankCommandStep()
    {
        var onlySpacesMatch = new Regex(@"^\s*$").Match(commandLine);
        if (!onlySpacesMatch.Success)
        {
            _state = ParserState.Failed;
        }
        else
        {
            _state = ParserState.Done;
        }
    }

    private void CommandStep()
    {
        var commandMatch = new Regex(@"\G\w*").Match(commandLine);

        if (!commandMatch.Success)
        {
            _state = ParserState.Failed;
            return;
        }

        if (commandMatch.Value == "")
        {
            _state = ParserState.BlankCommand;
            return;
        }
        
        _sofarArgs.Add(commandMatch.Value);
        _index += commandMatch.Length;
        _state = ParserState.Spaces;
    }

    private void SpaceStep()
    {
        var spacesMatch = new Regex(@"\G\s+").Match(commandLine, _index);

        if (!spacesMatch.Success)
        {
            _state = ParserState.Failed;
            return;
        }
        
        _index += spacesMatch.Length;
        _state = ParserState.Arg;
    }

    private void ArgStep()
    {
        /*
         * either next char is a quote, or it's non-whitespace and also not a quote
         * 
         */

        var untilSpaceMatch = new Regex(@"\G\S+").Match(commandLine, _index);
        var untilSingleQuoteMatch = new Regex(@"\G[^']+").Match(commandLine, _index);
        var untilDoubleQuoteMatch = new Regex(@"\G[^""]+").Match(commandLine, _index);

        // we see a quote first
        if (untilSingleQuoteMatch.Length < untilSpaceMatch.Length
            && untilSingleQuoteMatch.Length < untilDoubleQuoteMatch.Length)
        {
            _currentArg.Append(untilSingleQuoteMatch.Value);
            _index += untilSingleQuoteMatch.Length + 1;
            _state = ParserState.InsideSingleQuote;
        }
        else if (untilDoubleQuoteMatch.Length < untilSpaceMatch.Length)
        {
            _currentArg.Append(untilDoubleQuoteMatch.Value);
            _index += untilDoubleQuoteMatch.Length + 1;
            _state = ParserState.InsideDoubleQuote;
        }
        else
        {
            _currentArg.Append(untilSpaceMatch.Value);
            _index += untilSpaceMatch.Length;
            _sofarArgs.Add(_currentArg.ToString());
            _currentArg.Clear();
            _state = ParserState.Spaces;
        }
    }

    private void InsideSingleQuoteStep()
    {
        var untilNextQuoteMatch = new Regex(@"\G[^']*").Match(commandLine, _index);

        if (!untilNextQuoteMatch.Success 
            // below check will fail the parse if there isn't a closing quote
            || _index + untilNextQuoteMatch.Length == commandLine.Length
            )
        {
            _state = ParserState.Failed;
            return;
        }
        
        _currentArg.Append(untilNextQuoteMatch.Value);
        _index += untilNextQuoteMatch.Length + 1;
        _state = ParserState.JustLeftQuote;
    }
    
    private void InsideDoubleQuoteStep()
    {
        var untilNextQuoteMatch = new Regex(@"\G[^""]*").Match(commandLine, _index);

        if (!untilNextQuoteMatch.Success 
            // below check will fail the parse if there isn't a closing quote
            || _index + untilNextQuoteMatch.Length == commandLine.Length
           )
        {
            _state = ParserState.Failed;
            return;
        }
        
        _currentArg.Append(untilNextQuoteMatch.Value);
        _index += untilNextQuoteMatch.Length + 1;
        _state = ParserState.JustLeftQuote;
    }

    private void JustLeftQuoteStep()
    {
        var spaceMatch = new Regex(@"\G\s").Match(commandLine, _index);

        if (spaceMatch.Success)
        {
            _sofarArgs.Add(_currentArg.ToString());
            _currentArg.Clear();
            _state = ParserState.Spaces;
        }
        else
        {
            _state = ParserState.Arg;
        }
    }
}