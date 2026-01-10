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
    InsideDoubleQuote,
    JustLeftQuote,
    JustAfterBackslash,
    JustAfterBackslashInDoubleQuote,
    Done,
    Failed
}

public enum SpecialChar
{
    Space,
    SingleQuote,
    DoubleQuote,
    Backslash
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
                    case ParserState.JustAfterBackslash:
                    case ParserState.JustAfterBackslashInDoubleQuote:
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
                    case ParserState.JustAfterBackslash:
                        JustAfterBackslashStep();
                        break;
                    case ParserState.JustAfterBackslashInDoubleQuote:
                        JustAfterBackslashInDoubleQuoteStep();
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
        var firstCharMatch = new Regex(@"\G\s*").Match(commandLine);

        _index += firstCharMatch.Length;
        _state = ParserState.Arg;
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

        var matches = new List<(SpecialChar Char, Match Match)>()
        {
            (SpecialChar.Space, new Regex(@"\G\S+").Match(commandLine, _index)),
            (SpecialChar.SingleQuote, new Regex(@"\G[^']+").Match(commandLine, _index)),
            (SpecialChar.DoubleQuote, new Regex(@"\G[^""]+").Match(commandLine, _index)),
            (SpecialChar.Backslash, new Regex(@"\G[^\\]+").Match(commandLine, _index))
        };

        var shortestMatch = matches.MinBy(x => x.Match.Length);

        // we see a quote first
        if (shortestMatch.Char == SpecialChar.SingleQuote)
        {
            _currentArg.Append(shortestMatch.Match.Value);
            _index += shortestMatch.Match.Length + 1;
            _state = ParserState.InsideSingleQuote;
        }
        else if (shortestMatch.Char == SpecialChar.DoubleQuote)
        {
            _currentArg.Append(shortestMatch.Match.Value);
            _index += shortestMatch.Match.Length + 1;
            _state = ParserState.InsideDoubleQuote;
        }
        else if (shortestMatch.Char == SpecialChar.Space)
        {
            _currentArg.Append(shortestMatch.Match.Value);
            _index += shortestMatch.Match.Length;
            _sofarArgs.Add(_currentArg.ToString());
            _currentArg.Clear();
            _state = ParserState.Spaces;
        }
        else if (shortestMatch.Char == SpecialChar.Backslash)
        {
            _currentArg.Append(shortestMatch.Match.Value);
            _index += shortestMatch.Match.Length + 1;
            _state = ParserState.JustAfterBackslash;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
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
        var untilDoubleQuoteMatch = new Regex(@"\G[^""]*").Match(commandLine, _index);
        var untilBackslashMatch = new Regex(@"\G[^\\]*").Match(commandLine, _index);

        if (untilBackslashMatch.Length < untilDoubleQuoteMatch.Length)
        {
            _currentArg.Append(untilBackslashMatch.Value);
            _index += untilBackslashMatch.Length + 1;
            _state = ParserState.JustAfterBackslashInDoubleQuote;
        }
        else
        {
            if (!untilDoubleQuoteMatch.Success 
                // below check will fail the parse if there isn't a closing quote
                || _index + untilDoubleQuoteMatch.Length == commandLine.Length
               )
            {
                _state = ParserState.Failed;
                return;
            }
        
            _currentArg.Append(untilDoubleQuoteMatch.Value);
            _index += untilDoubleQuoteMatch.Length + 1;
            _state = ParserState.JustLeftQuote;
        }
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

    private void JustAfterBackslashStep()
    {
        _currentArg.Append(commandLine[_index]);
        _index++;
        _state = ParserState.Arg;
    }

    private void JustAfterBackslashInDoubleQuoteStep()
    {
        var thisChar = commandLine[_index];
        
        if (thisChar is not '"' and not '\\')
        {
            _currentArg.Append('\\');
        }
        
        _currentArg.Append(thisChar);
        _index++;
        _state = ParserState.InsideDoubleQuote;
    }
}