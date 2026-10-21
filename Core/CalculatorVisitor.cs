using System;
using System.Collections.Generic;

namespace Spreadsheet_lab1.Core
{
    public class CalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        private readonly Dictionary<string, double> _variables;

        public CalculatorVisitor(Dictionary<string, double> variables)
        {
            _variables = variables;
        }

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return double.Parse(context.GetText());
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            string name = context.GetText().ToUpper();
            if (_variables.ContainsKey(name))
            {
                return _variables[name];
            }
            throw new Exception("Посилання на неіснуючу клітинку: " + name);
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNotExpr(LabCalculatorParser.NotExprContext context)
        {
            var val = Visit(context.expression());
            if (val == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override double VisitMmaxExpr(LabCalculatorParser.MmaxExprContext context)
        {
            double max = double.NegativeInfinity;
            foreach (var child in context.paramlist().expression())
            {
                double val = Visit(child);
                if (val > max)
                {
                    max = val;
                }
            }
            return max;
        }

        public override double VisitMminExpr(LabCalculatorParser.MminExprContext context)
        {
            double min = double.PositiveInfinity;
            foreach (var child in context.paramlist().expression())
            {
                double val = Visit(child);
                if (val < min)
                {
                    min = val;
                }
            }
            return min;
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                return left * right;
            }
            if (context.operatorToken.Type == LabCalculatorLexer.DIVIDE)
            {
                if (right == 0)
                {
                    throw new Exception("Ділення на нуль!");
                }
                else
                {
                    return left / right;
                }
            }
            if (context.operatorToken.Type == LabCalculatorLexer.DIV)
            {
                if (right == 0)
                {
                    throw new Exception("Ділення на нуль!");
                }
                else
                {
                    return Math.Floor(left / right);
                }
            }
            if (context.operatorToken.Type == LabCalculatorLexer.MOD)
            {
                if (right == 0)
                {
                    throw new Exception("Ділення на нуль!");
                }
                else
                {
                    return left % right;
                }
            }

            return 0;
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                return left + right;
            }
            if (context.operatorToken.Type == LabCalculatorLexer.SUBTRACT)
            {
                return left - right;
            }

            return 0;
        }

        public override double VisitComparisonExpr(LabCalculatorParser.ComparisonExprContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.LTEQ)
            {
                if (left <= right)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            if (context.operatorToken.Type == LabCalculatorLexer.GTEQ)
            {
                if (left >= right)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            if (context.operatorToken.Type == LabCalculatorLexer.NEQ)
            {
                if (left != right)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }
    }
}