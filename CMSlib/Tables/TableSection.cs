using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CMSlib.Tables
{
    public class TableSection
    {
        internal Type type;
        internal List<(TableColumn column, ValueGetter getter)> Columns;
        public TableSection(Type type, params TableColumn[] columns)
        {
            Columns = new();
            for(int i = 0; i < columns.Length; i++)
            {
                columns[i].Parent = this;
            }
            this.type = type;
            foreach(TableColumn col in columns)
            {
                if (col.MemberName is null) Columns.Add((col, new()));
                else if(type.GetField(col.MemberName) is FieldInfo fieldInfo)
                {
                    Columns.Add((col, new(fieldInfo.GetValue)));
                }
                else if(type.GetProperty(col.MemberName) is PropertyInfo propInfo)
                {
                    Columns.Add((col, new(propInfo.GetGetMethod().Invoke)));
                }
                else if(type.GetMethod(col.MemberName, Array.Empty<Type>()) is MethodInfo methodInfo)
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
    
    public record TableColumn(string MemberName, int InnerWidth, string ColumnTitle = null, bool Ellipse = true, bool LeftPipe = false, bool RightPipe = false, ExtensionMethods.ColumnAdjust Adjust = ExtensionMethods.ColumnAdjust.Left)
    {
        public TableSection Parent { get; internal set; }
    }
    public class ValueGetter {
        public Func<object, object> fieldGetter = null;
        public Func<object, object[], object> otherGetter = null;
        public ValueGetter(Func<object, object[], object> other) => otherGetter = other;
        public ValueGetter(Func<object, object> fieldGetter) => this.fieldGetter = fieldGetter;
        public ValueGetter() { }
        public object Invoke(object item)
        {
            return fieldGetter?.Invoke(item) ?? otherGetter?.Invoke(item, null) ?? item;
        }
    }

}
