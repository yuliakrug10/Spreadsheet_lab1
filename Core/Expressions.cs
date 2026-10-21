using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace Spreadsheet_lab1.Core
{
    public class Expressions
    {
        private readonly Dictionary<string, double> _variables;

        public Expressions(Dictionary<string, double> variables = null)
        {
            if (variables != null)
            {
                _variables = variables;
            }
            else
            {
                _variables = new Dictionary<string, double>();
            }
        }

        public bool BooleanExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            double result = EvaluateExpression(expression);
            return result != 0;
        }

        private double EvaluateExpression(string expression)
        {
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