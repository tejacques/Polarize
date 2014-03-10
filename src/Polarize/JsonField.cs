﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    public class JsonField
    {
        public string Name;
        public JsonField[] Fields;

        public JsonField(string name, string[][] fields, int depth)
        {
            Name = name;

            if (null != fields && fields.Length > 0)
            {
                Fields = MakeFields(fields, depth + 1);
            }
        }

        public static JsonField[] MakeFields(string[] fields)
        {
            // Ensures the fields are sorted, this is crucial to the ordering.
            Array.Sort(fields);

            string[][] splitFields = new string[fields.Length][];

            for (int i = 0; i < fields.Length; i++)
            {
                splitFields[i] = fields[i].Split(StringSplits.Period);
            }

            return MakeFields(splitFields, 0);
        }

        public static JsonField[] MakeFields(string[][] fields, int depth)
        {
            var groups = fields
                .Where(splits => splits.Length > depth)
                .GroupBy(splits => splits[depth])
                .Select(group => group.ToArray()).ToArray();

            if (0 == groups.Length)
            {
                return null;
            }

            JsonField[] results = new JsonField[groups.Length];

            for (int i = 0; i < results.Length; i++)
            {
                var group = groups[i];
                results[i] = new JsonField(group[0][depth], group, depth);
            }

            return results;
        }
    }
}