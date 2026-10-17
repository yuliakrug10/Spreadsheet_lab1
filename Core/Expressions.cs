using System;
using System.Collections.Generic;
using System.Numerics;

namespace Core
{
    public class Expressions
    {
        private readonly Dictionary<string, BigInteger> _variables;

        public Expressions(Dictionary<string, BigInteger>? variables = null)
        {
           if (variables != null) {
                _variables = variables;
            } 
            else {
                _variables = new Dictionary<string, BigInteger>();
            }
        }

        public bool BooleanExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            
            return true; 
        }

        private BigInteger EvaluateArithmetic(string expression)
        {
            return BigInteger.Zero;
        }

        private List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            
            return tokens;

        }
    }
}