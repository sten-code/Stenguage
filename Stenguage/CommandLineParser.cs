using Stenguage.Json;
using System.Linq;
using System.Reflection;

namespace Stenguage
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

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

    public class ArgumentParser
    {
        public T Parse<T>(string[] args) where T : new()
        {
            return (T)Parse(typeof(T), args);
        }

        public object Parse(Type type, string[] args)
        {
            var options = Activator.CreateInstance(type);
            var argumentDictionary = GetArgumentProperties(type);
            var subcommandDictionary = GetSubCommandProperties(type);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg[0] == '-')
                {
                    string argName = arg.Substring(1);
                    if (arg[1] == '-') argName = argName.Substring(1);
                    if (argumentDictionary.TryGetValue(argName, out PropertyInfo property))
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
                        DisplayHelp(argumentDictionary);
                        return options;
                    }
                }
                else
                {
                    if (subcommandDictionary.TryGetValue(arg, out PropertyInfo property))
                    {
                        // Treat the argument as a sub-command
                        ArgumentParser parser = new ArgumentParser();
                        var result = parser.Parse(property.PropertyType, args.Skip(i + 1).ToArray());
                        property.SetValue(options, result);
                        break;
                    }
                    else
                    {
                        var positionalProperties = argumentDictionary.Values
                             .Where(prop => prop.GetCustomAttribute<ArgumentAttribute>().IsPositional)
                             .ToList();

                        if (positionalProperties.Count > 0)
                        {
                            PropertyInfo prop = positionalProperties.First();
                            prop.SetValue(options, Convert.ChangeType(arg, prop.PropertyType));
                        }
                        else
                        {
                            Console.WriteLine($"Unknown positional argument: {arg}");
                            DisplayHelp(argumentDictionary);
                            return options;
                        }
                    }
                }
            }

            // Check if required arguments are missing
            List<PropertyInfo> missingRequiredArguments = argumentDictionary.Values
                .Where(prop => prop.GetCustomAttribute<ArgumentAttribute>().IsRequired && prop.GetValue(options) == null)
                .ToList();

            // Remove duplicates
            List<ArgumentAttribute> missingRequiredArgumentsList = new List<ArgumentAttribute>();
            foreach (PropertyInfo argument in missingRequiredArguments)
            {
                ArgumentAttribute attr = argument.GetCustomAttribute<ArgumentAttribute>();
                if (!missingRequiredArgumentsList.Contains(attr))
                {
                    missingRequiredArgumentsList.Add(attr);
                }
            }

            if (missingRequiredArguments.Count > 0)
            {
                Console.WriteLine("Required argument(s) missing:");
                foreach (ArgumentAttribute missingArg in missingRequiredArgumentsList)
                {
                    Console.WriteLine($"  -{missingArg.ShortName}, --{missingArg.LongName}: {missingArg.Description}");
                }
            }

            return options;
        }

        private void DisplayHelp(Dictionary<string, PropertyInfo> propertyDictionary)
        {
            Console.WriteLine("Available Commands:");

            // Remove duplicates
            List<ArgumentAttribute> argumentsList = new List<ArgumentAttribute>();
            foreach (PropertyInfo argument in propertyDictionary.Values)
            {
                ArgumentAttribute attr = argument.GetCustomAttribute<ArgumentAttribute>();
                if (!argumentsList.Contains(attr))
                {
                    argumentsList.Add(attr);
                }
            }

            foreach (var attribute in argumentsList)
            {
                Console.WriteLine($"  -{attribute.ShortName}, --{attribute.LongName}: {attribute.Description}");
            }
        }

        private Dictionary<string, PropertyInfo> GetArgumentProperties(Type t)
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

        private Dictionary<ArgumentAttribute, PropertyInfo> GetRequiredArguments(Type t)
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyDictionary = new Dictionary<ArgumentAttribute, PropertyInfo>();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ArgumentAttribute>();
                if (attribute != null && attribute.IsRequired)
                {
                    propertyDictionary[attribute] = property;
                }
            }

            return propertyDictionary;
        }

        private Dictionary<string, PropertyInfo> GetSubCommandProperties(Type t)
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyDictionary = new Dictionary<string, PropertyInfo>();

            foreach (var property in properties)
            {
                var attribute = property.PropertyType.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    propertyDictionary[attribute.Name] = property;
                }
            }

            return propertyDictionary;
        }
    }
}
