using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Versagen.Events.Commands
{
    public class ArgCommandExpressionBuilder
    {
        public Dictionary<RuntimeMethodHandle, IArgParser> parsers { get; }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateArgCommandParser(VersaArgsCommand.Builder b)
        {
            LambdaExpression GetInnerExpression<T>(LambdaArgParser<T> lambParse)
            {
                Expression expressiveness = lambParse.ParseExpression;
                while (expressiveness.CanReduce)
                    expressiveness = expressiveness.Reduce();
                LambdaExpression expression = Expression.Lambda(expressiveness, Expression.Parameter(typeof(string)));
                return expression;
            }

            //Adapted this from something I saw from John Skeet on StackOverflow. Thank you, John Skeet.
            LambdaExpression JankyTypeConversion(Func<LambdaArgParser<int>, Expression> theParse, IArgParser parser)
                =>(LambdaExpression)theParse.Method.GetGenericMethodDefinition().MakeGenericMethod(parser.TypeParsed).Invoke(theParse.Target, new[] { parser });
                



            IArgParser x = new LambdaArgParser<int>(s => int.Parse(s));
            Type type = x.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(LambdaArgParser<>))
                {
                    LambdaExpression express = JankyTypeConversion(GetInnerExpression, x);
                    break;
                }
                type = type.BaseType;
            }
        }

    }
}
