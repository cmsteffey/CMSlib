using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace CMSlib.Tables
{
    public class TableSection
    {
        internal Type Type;
        internal List<(TableColumn column, ValueGetter getter)> Columns;

        public TableSection(Type type, params TableColumn[] columns)
        {
            Columns = new();
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].Parent = this;
            }

            this.Type = type;
            foreach (TableColumn col in columns)
            {
                if (col.MemberName is null) Columns.Add((col, new()));
                else if (type.GetField(col.MemberName) is FieldInfo fieldInfo)
                {
                    Columns.Add((col, new(fieldInfo.GetValue)));
                }
                else if (type.GetProperty(col.MemberName) is PropertyInfo propInfo)
                {
                    Columns.Add((col, new(propInfo.GetGetMethod().Invoke)));
                }
                else if (type.GetMethod(col.MemberName, Array.Empty<Type>()) is MethodInfo methodInfo)
                {
                    Columns.Add((col, new(methodInfo.Invoke)));
                }
                else
                {
                    throw new NullReferenceException("Name was not a valid field, property, or parameterless method");
                }
            }
        }
    }


    public record TableColumn(string MemberName, int InnerWidth, string ColumnTitle = null, bool Ellipse = true,
        bool LeftPipe = false, bool RightPipe = false,
        ExtensionMethods.ColumnAdjust Adjust = ExtensionMethods.ColumnAdjust.Left,
        CustomStringFormatter CustomFormatter = null)

    {
        public TableSection Parent { get; internal set; }
    }

    public class ValueGetter
    {
        public readonly Func<object, object> FieldGetter;
        public readonly Func<object, object[], object> OtherGetter;
        public ValueGetter(Func<object, object[], object> other) => OtherGetter = other;
        public ValueGetter(Func<object, object> fieldGetter) => this.FieldGetter = fieldGetter;

        public ValueGetter()
        {
        }

        public object Invoke(object item)
        {
            return FieldGetter?.Invoke(item) ?? OtherGetter?.Invoke(item, null) ?? item;
        }
    }

    public delegate string CustomStringFormatter(object item);
}