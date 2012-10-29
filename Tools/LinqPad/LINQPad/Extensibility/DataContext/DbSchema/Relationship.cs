namespace LINQPad.Extensibility.DataContext.DbSchema
{
    using LINQPad;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Relationship
    {
        private bool _allowOneToOne;
        public readonly List<Column> ChildCols = new List<Column>();
        public readonly Table ChildTable;
        public readonly string Name;
        public readonly List<Column> ParentCols = new List<Column>();
        public readonly Table ParentTable;

        public Relationship(string name, Table parent, Table child, bool allowOneToOne)
        {
            this.Name = name;
            this.ParentTable = parent;
            this.ChildTable = child;
            this._allowOneToOne = allowOneToOne;
        }

        internal void AssignPropNames(Database db)
        {
            string str;
            if (this.IsOneToOne)
            {
                str = (this.ChildTable == this.ParentTable) ? "Child" : this.ChildTable.OriginalName;
            }
            else
            {
                str = (this.ChildTable == this.ParentTable) ? "Children" : this.ChildTable.PropertyName;
            }
            if (this.ParentCols.Count > 1)
            {
                this.PropNameForChild = this.ParentCols[0].ClrObjectName;
                this.PropNameForParent = str;
            }
            else
            {
                Column column = this.ChildCols[0];
                Column column2 = this.ParentCols[0];
                string str2 = (this.ParentTable.SingularName == null) ? this.ParentTable.DotNetName : this.ParentTable.SingularName;
                bool stripStem = true;
                this.PropNameForChild = StringUtil.StripTrailingKey(column.PropertyName, column2.PropertyName, true);
                if ((this.PropNameForChild.Length < 3) || (this.PropNameForChild.ToLowerInvariant() == "key"))
                {
                    this.PropNameForChild = str2;
                }
                if (this.PropNameForChild == column.PropertyName)
                {
                    this.PropNameForChild = this.PropNameForChild + (this.PropNameForChild.ToLowerInvariant().Contains(str2.ToLowerInvariant()) ? "Entity" : str2);
                }
                string identifier = StringUtil.GetDistinguishingIdentifier(column.PropertyName, this.ParentTable.DotNetName, column2.PropertyName, stripStem);
                identifier = db.TransformIdentifier(identifier);
                if (identifier.Length < 3)
                {
                    identifier = "";
                }
                else if ((identifier.ToLowerInvariant() == "parent") && (str.ToLowerInvariant() == "children"))
                {
                    identifier = "";
                }
                if ((identifier.Length > 4) && identifier.EndsWith("By", StringComparison.Ordinal))
                {
                    identifier = StringUtil.StripTrailingWord(identifier, 2);
                }
                if ((this._allowOneToOne && (identifier.Length > 0)) && str.StartsWith(identifier, StringComparison.OrdinalIgnoreCase))
                {
                    identifier = "";
                }
                this.PropNameForParent = identifier + str;
                if ((this.ChildTable != this.ParentTable) && this._allowOneToOne)
                {
                    if (this.PropNameForChild == this.ChildTable.DotNetName)
                    {
                        this.PropNameForChild = this.ParentTable.DotNetName;
                    }
                    if (this.PropNameForParent == this.ParentTable.DotNetName)
                    {
                        this.PropNameForParent = this.ChildTable.DotNetName;
                    }
                }
            }
        }

        internal string ChildFieldName
        {
            get
            {
                return ("_" + this.PropNameForChild);
            }
        }

        public bool IsOneToOne
        {
            get
            {
                if (this._allowOneToOne && this.ChildCols.All<Column>(cc => cc.IsKey))
                {
                }
                return ((CS$<>9__CachedAnonymousMethodDelegate3 == null) && (this.ChildTable.Columns.Values.Count<Column>(CS$<>9__CachedAnonymousMethodDelegate3) == this.ChildCols.Count));
            }
        }

        internal string ParentFieldName
        {
            get
            {
                return ("_" + this.PropNameForParent);
            }
        }

        public string PropNameForChild { get; private set; }

        public string PropNameForParent { get; private set; }
    }
}

