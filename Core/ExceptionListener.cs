using Antlr4.Runtime;
using System;
using System.IO;

namespace Spreadsheet_lab1.Core
{
    public class ExceptionListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException(FormatError(charPositionInLine, msg), e);
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException(FormatError(charPositionInLine, msg), e);
        }

        private string FormatError(int charPositionInLine, string msg)
        {
            return $"Синтаксична помилка (позиція {charPositionInLine + 1}): {msg}";
        }
    }
}
