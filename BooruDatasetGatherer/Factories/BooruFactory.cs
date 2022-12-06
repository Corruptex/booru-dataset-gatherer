using BooruDatasetGatherer.Data;
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

        public ABooru? GetBooru(BooruProfile profile)
        {
            if (_boorus.ContainsKey(profile.Source))
            {
                ABooru booru = (ABooru)Activator.CreateInstance(_boorus[profile.Source])!;
                if (profile.HasAuth)
                    booru.Auth = new BooruAuth(profile.Username, profile.Password);
                return booru;
            }
            return null;
        }
    }
}
