using System.Reflection;
using System.Text;

namespace WebApplication1.data.ORM
{
    public enum SQLSpecialTypes
    {
        Serial
    }

    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public class SQLSpecialTypeAttribute : Attribute
    {
        public SQLSpecialTypes SqlType { get; set; }
        public int Size { get; set; }

        public SQLSpecialTypeAttribute(SQLSpecialTypes sqlType, int size = 0)
        {
            SqlType = sqlType;
            Size = size;
        }
    }

    public class ORM_Table
    {
        public void Print()
        {
            StringBuilder sb = new();

            PropertyInfo[] props = this.GetType().GetProperties();
            foreach ((PropertyInfo prop, int index) in props.Select((index, prop) => (index, prop))) 
            {
                sb.Append($"{prop.Name}: {prop.GetValue(this)?.ToString() ?? "null"}");

                if(index < props.Length-1)
                    sb.Append(", ");
            }

            Console.WriteLine($"rowOf({this.GetType().Name})"+" {"+sb.ToString()+"}");
        }
    }
}
