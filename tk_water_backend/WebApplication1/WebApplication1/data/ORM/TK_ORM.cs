using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.data.ORM;

public static class Extensions
{
    public static bool IsGenericTypeOf(this PropertyInfo prop, Type type)
    {
        return prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == type;
    }

    public static void AddParameters(this DbCommand command, LinkedList<(string name, object? value)> parameters)
    {
        foreach ((string parameterName, object? value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }

    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((el, index) => (el, index));
    }
}

public class TK_ORM
{
    public Func<DbConnection> GetConnection { get; set; }

    public TK_ORM(Func<DbConnection> getConnection)
    {
        GetConnection = getConnection;
    }

    public async Task<int> ExecuteSqlQuery(string sqlQuery)
    {
        ORM_Iterable<ORM_Table> iterator = new(GetConnection);
        iterator.Query.Append(sqlQuery);

        return await iterator.Execute();
    }

    public async Task<LinkedList<T>> GetResultFrom_SqlQuery<T>(string sqlQuery) where T : ORM_Table, new()
    {
        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append(sqlQuery);

        return await iterator.GetResult();
    }

    public async Task<long> GetAfflictedCountFrom_SqlQuery(string sqlQuery)
    {
        ORM_Iterable<ORM_Table> iterator = new(GetConnection);
        iterator.Query.Append(sqlQuery);

        return await iterator.GetAfflictedCount();
    }

    public ORM_Iterable<T> Remove<T>() where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"DELETE FROM {tableName} ");
        return iterator;
    }

    public async Task<bool> Insert<T>(T row) where T : ORM_Table, new()
    {
        PropertyInfo[] props = typeof(T).GetProperties();
        string tableName = typeof(T).Name;
        StringBuilder valueNames = new();
        StringBuilder values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            if (IsSerial(prop))
                continue;
            
            string value = prop.GetValue(row)?.ToString() ?? "null";
            valueNames.Append(prop.Name);

            if (prop.PropertyType == typeof(string))
            {
                values.Append($"\'{value}\'");
            }
            else if(prop.PropertyType == typeof(DateTime))
            {
                string sqlDateTime = "\'" + DateTime.Parse(value).ToString("yyyy-MM-dd HH:mm:ss.fff") + '\'';
                values.Append(sqlDateTime);
            }
            else
            {
                values.Append(value);
            }


            if (index < props.Length - 1)
            {
                valueNames.Append(", ");
                values.Append(", ");
            }
        }

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"INSERT INTO {tableName} ({valueNames})\n VALUES ({values})");
        
        return (await iterator.GetAfflictedCount() == 0);
    }

    public ORM_Iterable<T> Update<T>(T row) where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;
        PropertyInfo[] props = typeof(T).GetProperties();
        StringBuilder Values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            Values.Append(prop.Name + "=");
            string value = prop.GetValue(row)?.ToString() ?? "null";

            if (prop.PropertyType == typeof(string))
            {
                Values.Append($"\'{value}\'");
            }
            else if (prop.PropertyType == typeof(DateTime))
            {
                string sqlDateTime = "\'" + DateTime.Parse(value).ToString("yyyy-MM-dd HH:mm:ss.fff") + '\'';
                Values.Append(sqlDateTime);
            }
            else
            {
                Values.Append(value);
            }


            if (index < props.Length - 1)
                Values.Append(", ");
        }

        string updateString = $"UPDATE {tableName} \nSET {Values}\n";
        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append(updateString);

        return iterator;
    }

    public ORM_Iterable<T> Select<T>() where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"SELECT * FROM {tableName}\n");
        return iterator;
    }

    public ORM_Iterable<T> SelectMax<T>(Expression<Func<T, object>> selectColumn) where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;
        string columnName;

        if (selectColumn.Body is MemberExpression member)
        {
            columnName = member.Member.Name;
        }
        else if(selectColumn.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
        {
            columnName = unaryMember.Member.Name;
        }
        else
        {
            throw new ArgumentException("!!\"Expression<Func<T, object>> selectColumn\" can only be an memberExpression!!");
        }

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"SELECT MAX({columnName}) FROM {tableName}\n");
        return iterator;
    }

    public ORM_Iterable<T> SelectMin<T>(Expression<Func<T, object>> selectColumn) where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;
        string columnName;

        if (selectColumn.Body is MemberExpression member)
        {
            columnName = member.Member.Name;
        }
        else if (selectColumn.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
        {
            columnName = unaryMember.Member.Name;
        }
        else
        {
            throw new ArgumentException("!!\"Expression<Func<T, object>> selectColumn\" can only be an memberExpression!!");
        }

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"SELECT MIN({columnName}) FROM {tableName}\n");
        return iterator;
    }

    public ORM_Iterable<T> Count<T>() where T : ORM_Table, new()
    {
        string tableName = typeof(T).Name;

        ORM_Iterable<T> iterator = new(GetConnection);
        iterator.Query.Append($"SELECT COUNT(*) FROM {tableName}\n");
        return iterator;
    }

    public static bool IsSerial(PropertyInfo prop)
    {
        SQLSpecialTypeAttribute? attribute = prop.GetCustomAttribute<SQLSpecialTypeAttribute>();
        return attribute != null && attribute.SqlType == SQLSpecialTypes.Serial;
    }
}

