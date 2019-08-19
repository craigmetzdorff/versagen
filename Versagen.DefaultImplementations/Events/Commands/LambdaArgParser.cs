using System;
using System.Linq.Expressions;

namespace Versagen.Events.Commands
{
    public class LambdaArgParser<T> : IArgParser<T>
    {
        /// <summary>
        /// Stored as an expression because we want to ensure that anything put in here isn't some uber long function.
        /// Each command will be parsed using an optimized function that runs as few things as possible for these. We don't want anything that depends on a longer class in here.
        /// </summary>
        protected Expression<Func<string, T>> parseFunc { get; }
        public Type TypeParsed => typeof(T);
        public T ParseThis(string input) => parseFunc.Compile().Invoke(input);

        public Expression<Func<string, T>> ParseExpression { get { return parseFunc; } }

        object IArgParser.ParseThis(string token) => parseFunc.Compile().Invoke(token);

        public LambdaArgParser(Expression<Func<string, T>> parseExpression)
        {
            parseFunc = parseExpression;
        }
    }
}
