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
        private List<ValueGetter> getters;
        public TableSection(Type type, params string[] memberNames)
        {
            foreach(string name in memberNames)
            {
                if (name is null) getters.Add(new());
                else if(type.GetField(name) is FieldInfo fieldInfo)
                {
                    getters.Add(new(fieldInfo.GetValue));
                    
                }
                else if(type.GetProperty(name) is PropertyInfo propInfo)
                {
                    getters.Add(new(propInfo.GetGetMethod().Invoke));
                    
                }
                else if(type.GetMethod(name, Array.Empty<Type>()) is MethodInfo methodInfo)
                {
                    getters.Add(new(methodInfo.Invoke));
                }
                else
                {
                    throw new NullReferenceException("Name was not a valid field, property, or parameterless method");
                }
            }
        }
    }
    public record TableColumn(ValueGetter Getter, int InnerWidth, bool Ellipse = true, bool LeftPipe = false, bool RightPipe = false)
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
