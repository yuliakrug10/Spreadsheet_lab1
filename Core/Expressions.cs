using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Spreadsheet_lab1.Core
{
    public class Expressions
    {
        private readonly Dictionary<string, object> _variables;

        public Expressions(Dictionary<string, object> variables = null)
        {
            if (variables != null)
            {
                _variables = variables;
            }
            else
            {
                _variables = new Dictionary<string, object>();
            }
        }

        public object EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return "";
            }

            if (expression.StartsWith("="))
            {
                expression = expression.Substring(1);
            }

            var inputStream = new AntlrInputStream(expression);
            var lexer = new LabCalculatorLexer(inputStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ExceptionListener());

            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new LabCalculatorParser(commonTokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ExceptionListener());

            var context = parser.compileUnit();
            var visitor = new CalculatorVisitor(_variables);

            return visitor.Visit(context);
        }
    }
}