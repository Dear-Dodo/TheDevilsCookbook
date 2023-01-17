using System.Collections.Generic;

namespace TDC.Items
{
    //this should be a dictionary
    public struct Query
    {
        public StorableObject ToFind;
        public int Quantity;

        public Query(StorableObject toFind, int quantity)
        {
            ToFind = toFind;
            Quantity = quantity;
        }

        public static Query[] MakeQuery(Item[] items)
        {
            List<Query> queries = new List<Query>();
            foreach (Item item in items)
            {
                Query query = new Query(item.Data, 1);
                queries.Add(query);
            }
            return queries.ToArray();
        }
        public static Query[] MakeQuery(StorableObject[] items)
        {
            List<Query> queries = new List<Query>();
            foreach (StorableObject item in items)
            {
                bool foundItem = false;
                for (int i = 0; i < queries.Count; i++)
                {
                    Query q = queries[i];
                    if (q.ToFind.Name == item.name)
                    {
                        q.Quantity++;
                        foundItem = true;
                    }
                }
                if (!foundItem)
                {
                    Query query = new Query(item, 1);
                    queries.Add(query);
                }
            }
            return queries.ToArray();
        }
    }
}
