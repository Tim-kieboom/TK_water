using System.Reflection;
using System.Text;

namespace WebApplication1.data.ORM
{
    public class PrimaryKey<T>
    {
        public T? Key { get; set; }
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
