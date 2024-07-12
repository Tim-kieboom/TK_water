using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication1.data.ORM;

public static class ORM_SqLite
{
    public static ORM_Iterable<T> Remove<T>(SqliteConnection connection) where T : ORM_Table, new()
    {
        string tableName = GetTableName(typeof(T));

        ORM_Iterable<T> result = new(connection);
        result.SqlCommand.Append($"DELETE FROM {tableName} ");
        return result;
    }

    public static async Task<bool> Insert<T>(T row, SqliteConnection connection) where T : ORM_Table, new()
    {
        PropertyInfo[] props = GetProperties(typeof(T));
        string tableName = GetTableName(typeof(T));
        StringBuilder valueNames = new();
        StringBuilder values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            valueNames.Append(PropertyToName(prop));
            string value = prop.GetValue(row)?.ToString() ?? "null";
            if (prop.PropertyType == typeof(string))
            {
                values.Append($"\'{value}\'");
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

        string insert = $"INSERT INTO {tableName} ({valueNames})\n VALUES ({values})";
        SqliteCommand command = new(insert, connection);

        int rowsInserted;
        using (connection)
        {
            await connection.OpenAsync();
            rowsInserted = await command.ExecuteNonQueryAsync();
        }

        return (rowsInserted == 1);
    }

    public static ORM_Iterable<T> Update<T>(T row, SqliteConnection connection) where T : ORM_Table, new()
    {
        string tableName = GetTableName(typeof(T));
        PropertyInfo[] props = GetProperties(typeof(T));
        StringBuilder Values = new();

        foreach ((PropertyInfo prop, int index) in props.Select((value, index) => (value, index)))
        {
            Values.Append(PropertyToName(prop) + "=");
            string value = prop.GetValue(row)?.ToString() ?? "null";
            if (prop.PropertyType == typeof(string))
            {
                Values.Append($"\'{value}\'");
            }
            else
            {
                Values.Append(value);
            }


            if (index < props.Length - 1)
                Values.Append(", ");
        }

        string updateString = $"UPDATE {tableName} \nSET {Values}\n";
        ORM_Iterable<T> result = new(connection);
        result.SqlCommand.Append(updateString);

        return result;
    }

    public static ORM_Iterable<T> Select<T>(SqliteConnection connection) where T : ORM_Table, new()
    {
        string tableName = GetTableName(typeof(T));

        ORM_Iterable <T> result = new(connection);
        result.SqlCommand.Append($"SELECT * FROM {tableName}\n");
        return result;
    }

    public static ORM_Iterable<T> Count<T>(SqliteConnection connection) where T : ORM_Table, new()
    {
        string tableName = GetTableName(typeof(T));

        ORM_Iterable<T> result = new(connection);
        result.SqlCommand.Append($"SELECT COUNT(*) FROM {tableName}\n");
        return result;
    }

    public static LinkedList<T> GetAllResult<T>(SqliteDataReader reader) where T : ORM_Table, new()
    {
        LinkedList<T> result = [];

        while (reader.Read())
        {
            Dictionary<string, object> row = [];

            for (int i = 0; i < reader.FieldCount; i++)
                row.Add(reader.GetName(i), reader.GetValue(i));

            result.AddLast(GetResult<T>(row));
        }

        return result;
    }

    public static T GetResult<T>(Dictionary<string, object> row) where T : ORM_Table, new()
    {
        T obj = new();

        PropertyInfo[] props = typeof(T).GetProperties();

        foreach (PropertyInfo prop in props)
        {
            string dbName = PropertyToName(prop);
            prop.SetValue(obj, row[dbName]);
        }

        return obj;
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

