using System.Reflection;

namespace Stenguage
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class ArgumentAttribute : Attribute
    {
        public string ShortName { get; }
        public string LongName { get; }
        public string Description { get; }
        public bool IsRequired { get; }
        public bool IsPositional { get; }

        public ArgumentAttribute(string shortName, string longName, string description, bool isRequired = false, bool isPositional = false)
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
            IsRequired = isRequired;
            IsPositional = isPositional;
        }
    }

    class ArgumentParser
    {
        public T Parse<T>(string[] args) where T : new()
        {
            T options = new T();
            var propertyDictionary = GetArgumentProperties<T>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("-"))
                {
                    if (arg == "--help" || arg == "-h")
                    {
                        DisplayHelp(propertyDictionary);
                        return options;
                    }

                    string argName = arg.StartsWith("--") ? arg.Substring(2) : arg.Substring(1);
                    if (propertyDictionary.TryGetValue(argName, out PropertyInfo property))
                    {
                        if (property.PropertyType == typeof(bool))
                        {
                            property.SetValue(options, true);
                        }
                        else if (i + 1 < args.Length)
                        {
                            i++;
                            property.SetValue(options, Convert.ChangeType(args[i], property.PropertyType));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unknown argument: {arg}");
                        Environment.Exit(1);
                        return options;
                    }
                }
                else
                {
                    // Treat the argument as a positional argument
                    var positionalProperties = propertyDictionary.Values
                        .Where(prop => prop.GetCustomAttribute<ArgumentAttribute>().IsPositional)
                        .ToList();

                    if (positionalProperties.Count > 0)
                    {
                        PropertyInfo property = positionalProperties.First();
                        property.SetValue(options, Convert.ChangeType(arg, property.PropertyType));
                    }
                    else
                    {
                        Console.WriteLine($"Unknown positional argument: {arg}");
                        Environment.Exit(1);
                        return options;
                    }
                }
            }

            // Check if required arguments are missing
            var missingRequiredArguments = propertyDictionary.Values
                .Where(prop => prop.GetCustomAttribute<ArgumentAttribute>().IsRequired && prop.GetValue(options) == null)
                .ToList();

            if (missingRequiredArguments.Count > 0)
            {
                Console.WriteLine("Required argument(s) missing:");
                foreach (var missingArg in missingRequiredArguments)
                {
                    var attribute = missingArg.GetCustomAttribute<ArgumentAttribute>();
                    Console.WriteLine($"  -{attribute.ShortName} or --{attribute.LongName}: {attribute.Description}");
                }
                Environment.Exit(1);
            }

            return options;
        }

        private void DisplayHelp(Dictionary<string, PropertyInfo> propertyDictionary)
        {
            Console.WriteLine("Available Commands:");
            foreach (var kvp in propertyDictionary)
            {
                var attribute = kvp.Value.GetCustomAttribute<ArgumentAttribute>();
                Console.WriteLine($"  -{attribute.ShortName}, --{attribute.LongName}: {attribute.Description}");
            }
            Console.WriteLine("  -h, --help: Show this help message");
        }

        private Dictionary<string, PropertyInfo> GetArgumentProperties<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyDictionary = new Dictionary<string, PropertyInfo>();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ArgumentAttribute>();
                if (attribute != null)
                {
                    propertyDictionary[attribute.ShortName] = property;
                    propertyDictionary[attribute.LongName] = property;
                }
            }

            return propertyDictionary;
        }
    }

}
