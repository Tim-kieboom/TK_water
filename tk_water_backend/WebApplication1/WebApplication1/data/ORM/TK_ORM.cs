using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.data.ORM;

public static class Extensions
{
    public static bool isGenericTypeOf(this PropertyInfo prop, Type type)
    {
        return prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == type;
    }
}

public class TK_ORM
{
    public DbConnection Connection { get; set; }
    public DbCommand SQLCommand { get; set; }

    public TK_ORM(DbConnection connection)
    {
        Connection = connection;
        SQLCommand = Connection.CreateCommand();
    }

    ~TK_ORM()
    {
        Connection.Close();
    }

    public async Task<int> ExecuteSqlQuery(string sqlQuery)
    {
        ResetSqlCommand();
        SQLCommand.Append(sqlQuery);

        int result;

            if(Connection.State != ConnectionState.Open) 
                await Connection.OpenAsync();
            
            result = await SQLCommand.ExecuteNonQueryAsync();

        return result;
    }

    public async Task<LinkedList<T>> GetResultFrom_SqlQuery<T>(string sqlQuery) where T : ORM_Table, new()
    {
        ResetSqlCommand();
        SQLCommand.Append(sqlQuery);

        LinkedList<T> result;

            if(Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();

            using IDataReader reader = await SQLCommand.ExecuteReaderAsync();
            if (reader == null)
            {
                Connection.Close();
                return [];
            }

            result = await GetAllResult<T>(reader);

        return result;
    }

    public async Task<long> GetAfflictedCountFrom_SqlQuery(string sqlQuery)
    {
        ResetSqlCommand();
        SQLCommand.Append(sqlQuery);

        object? affectedRows;

            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();

            affectedRows = await SQLCommand.ExecuteScalarAsync();

        return Convert.ToInt64(affectedRows ?? 0);
    }

    public ORM_Iterable<T> Remove<T>() where T : ORM_Table, new()
    {
        ResetSqlCommand();

        string tableName = GetTableName(typeof(T));

        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"DELETE FROM {tableName} ");
        return result;
    }

    public async Task<bool> Insert<T>(T row) where T : ORM_Table, new()
    {
        ResetSqlCommand();

        PropertyInfo[] props = GetProperties(typeof(T));
        string tableName = GetTableName(typeof(T));
        StringBuilder valueNames = new();
        StringBuilder values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            if(prop.isGenericTypeOf(typeof(SqlSerial<>)) && prop.GetValue(row) == null)
                    continue;

            string value = prop.GetValue(row)?.ToString() ?? "null";
            valueNames.Append(PropertyToName(prop));

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

        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"INSERT INTO {tableName} ({valueNames})\n VALUES ({values})");
        
        return (await result.GetAfflictedCount() == 0);
    }

    public ORM_Iterable<T> Update<T>(T row) where T : ORM_Table, new()
    {
        ResetSqlCommand(); 

        string tableName = GetTableName(typeof(T));
        PropertyInfo[] props = GetProperties(typeof(T));
        StringBuilder Values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            if (prop.isGenericTypeOf(typeof(SqlSerial<>)) && prop.GetValue(row) == null)
                continue;

            Values.Append(PropertyToName(prop) + "=");
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
        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append(updateString);

        return result;
    }

    public ORM_Iterable<T> Select<T>() where T : ORM_Table, new()
    {
        ResetSqlCommand();

        string tableName = GetTableName(typeof(T));

        ORM_Iterable <T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"SELECT * FROM {tableName}\n");
        return result;
    }

    public ORM_Iterable<T> SelectMax<T>(Expression<Func<T, object>> selectColumn) where T : ORM_Table, new()
    {
        ResetSqlCommand();

        string tableName = GetTableName(typeof(T));
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

        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"SELECT MAX({columnName}) FROM {tableName}\n");
        return result;
    }

    public ORM_Iterable<T> SelectMin<T>(Expression<Func<T, object>> selectColumn) where T : ORM_Table, new()
    {
        ResetSqlCommand();

        string tableName = GetTableName(typeof(T));
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

        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"SELECT MIN({columnName}) FROM {tableName}\n");
        return result;
    }

    public ORM_Iterable<T> Count<T>() where T : ORM_Table, new()
    {
        ResetSqlCommand();

        string tableName = GetTableName(typeof(T));

        ORM_Iterable<T> result = new(Connection, SQLCommand);
        result.SqlCommand.Append($"SELECT COUNT(*) FROM {tableName}\n");
        return result;
    }

    public static async Task<LinkedList<T>> GetAllResult<T>(IDataReader reader) where T : ORM_Table, new()
    {
        LinkedList<T> result = [];

        while (await reader.ReadAsync())
        {
            Dictionary<string, object?> row = [];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i).ToLower();
                object rowValue = reader.GetValue(i);
                row.Add(columnName, rowValue);
            }

            T value = GetResult<T>(row);

            if (value == null)
                continue;

            result.AddLast(value);
        }
        return result;
    }

    public static T GetResult<T>(Dictionary<string, object?> row) where T : ORM_Table, new()
    {
        T obj = new();

        PropertyInfo[] props = typeof(T).GetProperties();

        foreach (PropertyInfo prop in props)
        {
            string dbName = PropertyToName(prop).ToLower();
            row.TryGetValue(dbName, out object? value);

            if(Nullable.GetUnderlyingType(prop.PropertyType) != null && value == null)
                continue;

            if ( prop.isGenericTypeOf(typeof(SqlSerial<>)) )
            {
                var serialValue = Activator.CreateInstance(prop.PropertyType) 
                    ?? throw new Exception($"!!serialValue is null at GetResult propertyInfo: {prop}!!");

                PropertyInfo sqlSerialProp = serialValue.GetType().GetProperty("Key")
                    ?? throw new Exception($"!!serialValue is does not have property Key at GetResult propertyInfo: {prop}!!");

                sqlSerialProp.SetValue(serialValue, value);
                prop.SetValue(obj, serialValue);
                continue;
            }

            prop.SetValue(obj, value);
        }

        return obj;
    }

    private void ResetSqlCommand()
    {
        SQLCommand.CommandText = "";
        SQLCommand.Parameters.Clear();
    }
    private static string NameToPropertyName(string propName)
    {
        return char.ToUpper(propName[0]) + propName[1..];
    }

    private static string PropertyToName(PropertyInfo prop)
    {
        return char.ToLower(prop.Name[0]) + prop.Name[1..];
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties();
    }

    private static string GetTableName(Type type)
    {
        string className = type.Name;
        return char.ToLower(className[0]) + className[1..];
    }
}

