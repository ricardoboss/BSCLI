using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BSCLI
{
    public static class Extensions
    {
        internal static string Arg(this IReadOnlyList<string> args, int idx, bool optional = false)
        {
            if (args.Count > idx)
                return args[idx];

            if (optional)
                return null;
            
            Console.Error.WriteLine("Invalid number of arguments: " + args.Count);
            
            Commands.CommandHelp();
                
            System.Environment.Exit(-1);
            
            return null;
        }

        internal static string Val(this JToken obj, string property)
        {
            var val = obj.Value<string>(property);

            return string.IsNullOrWhiteSpace(val) ? null : val;
        }
    }
}