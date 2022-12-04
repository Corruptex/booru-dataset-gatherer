using BooruSharp.Booru;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BooruDatasetGatherer.Factories
{
    internal class BooruFactory
    {
        public readonly string[] Boorus = new string[0];

        private readonly Dictionary<string, Type> _boorus = new Dictionary<string, Type>();

        public BooruFactory()
        {
            IEnumerable<Type>? types = typeof(ABooru).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ABooru)));

            if (types != null)
            {
                int i = 0;
                Boorus = new string[types.Count()];

                foreach (Type type in types)
                {
                    _boorus.Add(type.Name.ToLower(), type);
                    Boorus[i] = type.Name.ToLower();

                    i++;
                }
            }
        }

        public bool Contains(string name) => Boorus.Contains(name);

        public ABooru? GetBooru(string name)
        {
            if (_boorus.ContainsKey(name))
                return (ABooru)Activator.CreateInstance(_boorus[name])!;
            return null;
        }
    }
}
