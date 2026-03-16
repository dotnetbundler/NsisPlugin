using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable CheckNamespace
// ReSharper disable JoinDeclarationAndInitializer

namespace SourceGenerators;

/// <summary>
/// <seealso href="https://github.com/dotnet/runtime/blob/281d01cfd353f1ce15208c80b2f148b0d6ce106f/src/libraries/Common/src/SourceGenerators/SourceWriter.cs">to source</seealso>
/// </summary>
internal sealed class SourceWriter
{
    private const char IndentationChar = ' ';
    private const int CharsPerIndentation = 4;

    private readonly StringBuilder _sb = new();
    private int _indentation;

    public int Indentation
    {
        get => _indentation;
        set
        {
            if (value < 0)
            {
                Throw();

                static void Throw() => throw new ArgumentOutOfRangeException(nameof(value));
            }

            _indentation = value;
        }
    }

    public void WriteLine(char value)
    {
        AddIndentation();
        _sb.Append(value);
        _sb.AppendLine();
    }

    public void WriteLine(string text)
    {
        if (_indentation == 0)
        {
            _sb.AppendLine(text);
            return;
        }

        bool isFinalLine;
        var remainingText = text.AsSpan();
        do
        {
            var nextLine = GetNextLine(ref remainingText, out isFinalLine);

            AddIndentation();
            AppendSpan(_sb, nextLine);
            _sb.AppendLine();
        } while (!isFinalLine);
    }

    public void WriteLine() => _sb.AppendLine();

    public SourceText ToSourceText()
    {
        Debug.Assert(_indentation == 0 && _sb.Length > 0);
        return SourceText.From(_sb.ToString(), Encoding.UTF8);
    }

    public void Reset()
    {
        _sb.Clear();
        _indentation = 0;
    }

    private void AddIndentation()
        => _sb.Append(IndentationChar, CharsPerIndentation * _indentation);

    private static ReadOnlySpan<char> GetNextLine(ref ReadOnlySpan<char> remainingText, out bool isFinalLine)
    {
        if (remainingText.IsEmpty)
        {
            isFinalLine = true;
            return default;
        }

        ReadOnlySpan<char> next;
        ReadOnlySpan<char> rest;

        var lineLength = remainingText.IndexOf('\n');
        if (lineLength == -1)
        {
            lineLength = remainingText.Length;
            isFinalLine = true;
            rest = default;
        }
        else
        {
            rest = remainingText.Slice(lineLength + 1);
            isFinalLine = false;
        }

        if ((uint)lineLength > 0 && remainingText[lineLength - 1] == '\r')
        {
            lineLength--;
        }

        next = remainingText.Slice(0, lineLength);
        remainingText = rest;
        return next;
    }

    private static unsafe void AppendSpan(StringBuilder builder, ReadOnlySpan<char> span)
    {
        // There is no StringBuilder.Append(ReadOnlySpan<char>) overload in the NS2.0
        fixed (char* ptr = span)
        {
            builder.Append(ptr, span.Length);
        }
    }
}
