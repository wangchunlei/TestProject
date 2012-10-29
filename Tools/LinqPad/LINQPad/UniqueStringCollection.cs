namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class UniqueStringCollection : KeyedCollection<string, string>
    {
        public UniqueStringCollection()
        {
        }

        public UniqueStringCollection(IEnumerable<string> existing)
        {
            this.AddRange(existing);
        }

        public UniqueStringCollection(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public UniqueStringCollection(IEnumerable<string> existing, IEqualityComparer<string> comparer) : base(comparer)
        {
            this.AddRange(existing);
        }

        public void AddRange(IEnumerable<string> items)
        {
            foreach (string str in items)
            {
                base.Add(str);
            }
        }

        protected override string GetKeyForItem(string item)
        {
            return item;
        }

        protected override void InsertItem(int index, string item)
        {
            if (!base.Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        public void RemoveRange(IEnumerable<string> toRemove)
        {
            foreach (string str in toRemove)
            {
                if (base.Contains(str))
                {
                    base.Remove(str);
                }
            }
        }
    }
}

