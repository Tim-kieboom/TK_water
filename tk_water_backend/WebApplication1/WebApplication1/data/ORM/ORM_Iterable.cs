using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Mysqlx.Datatypes;
using MySqlX.XDevAPI.Common;
using Npgsql;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace WebApplication1.data.ORM;

using ParameterList = LinkedList<(string parameter, object? value)>;

public static class MyExtensions
{
    public static void SetQuery(this IDbCommand cmd, StringBuilder Query)
    {
        cmd.CommandText = Query.ToString();
    }

    public static string AddParameter(this ParameterList parameters, object? value)
    {
        int index = parameters.Count + 1;
        string parameterName = $"@parameter{index}";

        parameters.AddLast((parameterName, value));
        return parameterName;
    }

    public static async Task<T?> AsyncFirstOrDefault<T>(this Task<LinkedList<T>> listTask)
    {
        LinkedList<T> list = await listTask;
        return list.FirstOrDefault();
    }

}

public class ORM_Iterable<T> where T : ORM_Table, new()
{
    public Func<DbConnection> GetConnection { get; set; }
    public StringBuilder Query { get; set; } = new();
    public ParameterList Parameters { get; set; } = new();

    public ORM_Iterable(Func<DbConnection> getConnection)
    {
        GetConnection = getConnection;
    }

    public ORM_Iterable<T> Where(Expression<Func<T, bool>> lambda)
    {
        if(Query.ToString().Contains("WHERE"))
            Query.Append("AND ");
        else
            Query.Append("WHERE ");

        BuildWhereClause(lambda.Body);
        Query.Append('\n');
        return this;
    }

    public async Task<LinkedList<T>> GetResult()
    {
        Console.WriteLine(Query.ToString());
        
        return await GenericExecute<T, LinkedList<T>>
        (
            async (command) =>
            {
                using IDataReader reader = await command.ExecuteReaderAsync();

                if (reader == null)
                    return [];

                return await GetAllResult(reader);
            }
        );
    }

    public async Task<long> GetAfflictedCount()
    {
        Console.WriteLine(Query.ToString());

        return await GenericExecute<T, long>
        (
            async (command) => 
            { 
                return Convert.ToInt64(await command.ExecuteScalarAsync() ?? 0); 
            }
        );
    }

    public async Task<int> Execute()
    {
        Console.WriteLine(Query.ToString());

        return await GenericExecute<T, int>
        (
            async (command) =>
            {
                return await command.ExecuteNonQueryAsync();
            }
        );
    }

    public async Task<LinkedList<T>> GetResultAndPrint()
    {
        Console.WriteLine(Query.ToString());

        LinkedList<T> list = await GetResult();
        foreach (T item in list)
            item.Print();

        return list;
    }

    private async Task<R> GenericExecute<V, R>(Func<DbCommand, Task<R>> sqlGenericExecute) where V : ORM_Table, new()
    {
        R result;

        using DbConnection dbConnection = GetConnection();
        using (DbCommand dbCommand = dbConnection.CreateCommand())
        {

            dbCommand.SetQuery(Query);
            dbCommand.AddParameters(Parameters);

            await dbConnection.OpenAsync();

            try
            {
                result = await sqlGenericExecute(dbCommand);
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
        }
        return result;
    }

    private void BuildWhereClause(Expression expression)
    {
        if (expression is not BinaryExpression body)
            throw new ArgumentException();

        if (body.Left is MemberExpression leftMember)
        {
            string columnName = leftMember.Member.Name;

            Query.Append(columnName);
        }
        else if (body.Left is BinaryExpression leftCompare)
        {
            BuildWhereClause(leftCompare);
        }
        else
        {
            throw new ArgumentException();
        }

        string comparisonOperator = GetComparisonOperator(expression.NodeType);
        Query.Append($" {comparisonOperator} ");

        if (body.Right is MemberExpression rightMember)
        {
            object? value = GetComparisonValue(rightMember);

            PropertyInfo? prop = rightMember.Member as PropertyInfo;
            Type type = (prop != null) ? prop.PropertyType : ((FieldInfo)rightMember.Member).FieldType;

            if (type == typeof(DateTime))
            {
                string paramStr = Parameters.AddParameter(DateTime.Parse(value?.ToString() ?? ""));
                Query.Append(paramStr);
            }
            else if(type == typeof(string))
            {
                string paramStr = Parameters.AddParameter(value?.ToString() ?? "");
                Query.Append(paramStr);
            }
            else
            {
                string paramStr = Parameters.AddParameter(value ?? Expression.Default(type));
                Query.Append(paramStr);
            }
        }
        else if (body.Right is ConstantExpression rightConst)
        {
            object value = rightConst?.Value ?? Expression.Default(rightConst?.Type!);

            string paramStr = Parameters.AddParameter(value);
            Query.Append(paramStr);
        }
        else if (body.Right is BinaryExpression rightCompare)
        {
            BuildWhereClause(rightCompare);
        }
        else
        {
            throw new ArgumentException();
        }

    }

    private static string GetComparisonOperator(ExpressionType nodeType)
    {
        switch (nodeType)
        {
            case ExpressionType.Equal:
                return "=";
            case ExpressionType.NotEqual:
                return "!=";
            case ExpressionType.LessThan:
                return "<";
            case ExpressionType.LessThanOrEqual:
                return "<=";
            case ExpressionType.GreaterThan:
                return ">";
            case ExpressionType.GreaterThanOrEqual:
                return ">=";
            case ExpressionType.AndAlso:
                return "AND";
            case ExpressionType.OrElse:
                return "OR";
            default:
                throw new NotSupportedException($"Comparison operator for {nodeType} is not supported.");
        }
    }

    private static object? GetComparisonValue(Expression expression)
    {
        if (expression is ConstantExpression constantExpression)
        {
            return constantExpression.Value;
        }
        else if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null)
                throw new NullReferenceException();

            object? instance = Expression.Lambda(memberExpression.Expression).Compile().DynamicInvoke();
            var propertyInfo = memberExpression.Member as PropertyInfo;

            if(propertyInfo is null)
                return ((FieldInfo)memberExpression.Member).GetValue(instance);

            return propertyInfo?.GetValue(instance);
        }
        else
        {
            throw new NotSupportedException("Only constant values and property values are supported for comparison.");
        }
    }

    private static async Task<LinkedList<T>> GetAllResult(IDataReader reader)
    {
        LinkedList<T> result = [];

        while (await reader.ReadAsync())
        {
            Dictionary<string, object?> row = [];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i).ToLower(culture: CultureInfo.InvariantCulture);
                object rowValue = reader.GetValue(i);
                row.Add(columnName, rowValue);
            }

            T value = GetResult(row);

            if (value == null)
                continue;

            result.AddLast(value);
        }

        return result;
    }

    private static T GetResult(Dictionary<string, object?> row)
    {
        T obj = new();

        PropertyInfo[] props = typeof(T).GetProperties();

        foreach (PropertyInfo prop in props)
        {
            string dbName = prop.Name.ToLower(culture: CultureInfo.InvariantCulture);
            row.TryGetValue(dbName, out object? value);

            if (Nullable.GetUnderlyingType(prop.PropertyType) != null && value == null)
                continue;

            prop.SetValue(obj, value);
        }

        return obj;
    }
}

