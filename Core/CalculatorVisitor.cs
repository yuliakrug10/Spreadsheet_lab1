using System;
using System.Collections.Generic;
using System.Numerics;

namespace Spreadsheet_lab1.Core
{
    public class CalculatorVisitor : LabCalculatorBaseVisitor<object>
    {
        private readonly Dictionary<string, object> _variables;

        public CalculatorVisitor(Dictionary<string, object> variables)
        {
            _variables = variables;
        }

        public override object VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return BigInteger.Parse(context.GetText());
        }

        public override object VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            string name = context.GetText().ToUpper();
            if (_variables.ContainsKey(name))
            {
                return _variables[name];
            }
            throw new Exception("Посилання на неіснуючу клітинку: " + name);
        }

        public override object VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitNotExpr(LabCalculatorParser.NotExprContext context)
        {
            bool val = (bool)Visit(context.expression());
            if (val == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override object VisitMmaxExpr(LabCalculatorParser.MmaxExprContext context)
        {
            BigInteger max = (BigInteger)Visit(context.paramlist().expression(0));
            for (int i = 1; i < context.paramlist().expression().Length; i++)
            {
                BigInteger val = (BigInteger)Visit(context.paramlist().expression(i));
                if (val > max)
                {
                    max = val;
                }
            }
            return max;
        }

        public override object VisitMminExpr(LabCalculatorParser.MminExprContext context)
        {
            BigInteger min = (BigInteger)Visit(context.paramlist().expression(0));

            for (int i = 1; i < context.paramlist().expression().Length; i++)
            {
                BigInteger val = (BigInteger)Visit(context.paramlist().expression(i));
                if (val < min)
                {
                    min = val;
                }
            }
            return min;
        }

        public override object VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            BigInteger left = (BigInteger)Visit(context.expression(0));
            BigInteger right = (BigInteger)Visit(context.expression(1));

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
                    return left / right;
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

            return new BigInteger(0);
        }

        public override object VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            BigInteger left = (BigInteger)Visit(context.expression(0));
            BigInteger right = (BigInteger)Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                return left + right;
            }
            if (context.operatorToken.Type == LabCalculatorLexer.SUBTRACT)
            {
                return left - right;
            }

            return new BigInteger(0);
        }

        public override object VisitComparisonExpr(LabCalculatorParser.ComparisonExprContext context)
        {
            BigInteger left = (BigInteger)Visit(context.expression(0));
            BigInteger right = (BigInteger)Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.LTEQ)
            {
                return left <= right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.GTEQ)
            {
                return left >= right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.NEQ)
            {
                return left != right;
            }
            else
            {
                return false;
            }
        }
    }
}