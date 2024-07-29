using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace WebApplication1.data.ORM;

public static class Extensions
{
    public static void Append(this IDbCommand cmd, string sqlLine)
    {
        cmd.CommandText += sqlLine;
    }

    public static void AddParameter(this IDbCommand cmd, DateTime value)
    {
        IDbDataParameter parameter = cmd.CreateParameter();

        int index = cmd.Parameters.Count + 1;
        parameter.ParameterName = $"@parameter{index}";
        parameter.Value = value.ToString("yyyy-MM-dd HH:mm:ss");

        cmd.Append($"DATETIME({parameter.ParameterName})");
        cmd.Parameters.Add(parameter);
    }

    public static void AddParameter(this IDbCommand cmd, object value)
    {
        IDbDataParameter parameter = cmd.CreateParameter();

        int index = cmd.Parameters.Count + 1;
        parameter.ParameterName = $"@parameter{index}";
        parameter.Value = value;

        cmd.Append(parameter.ParameterName);
        cmd.Parameters.Add(parameter);
    }

    public static async Task<T?> AsyncFirstOrDefault<T>(this Task<LinkedList<T>> listTask)
    {
        LinkedList<T> list = await listTask;
        return list.FirstOrDefault();
    }

}

public class ORM_Iterable<T> where T : ORM_Table, new()
{
    public DbConnection Connection { get; set; }
    public DbCommand SqlCommand { get; set; }

    public ORM_Iterable(DbConnection connection, DbCommand command)
    {
        Connection = connection;
        SqlCommand = command;
    }

    public ORM_Iterable<T> Where(Expression<Func<T, bool>> lambda)
    {
        if(SqlCommand.CommandText.Contains("WHERE"))
            SqlCommand.Append("AND ");
        else
            SqlCommand.Append("WHERE ");

        BuildWhereClause(lambda.Body);
        SqlCommand.Append("\n");
        return this;
    }

    public async Task<LinkedList<T>> GetResult()
    {
        Console.WriteLine(SqlCommand.CommandText);

        LinkedList<T> result;

            if(Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();

            using IDataReader reader = await SqlCommand.ExecuteReaderAsync();
            if (reader == null)
            {
                Connection.Close();
                return [];
            }

            result = await TK_ORM.GetAllResult<T>(reader);

        return result;
    }

    public async Task<long> GetAfflictedCount()
    {
        Console.WriteLine(SqlCommand.CommandText);

        object? affectedRows;

            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();
            
            affectedRows = await SqlCommand.ExecuteScalarAsync();

        return Convert.ToInt64(affectedRows ?? 0);
    }

    public async Task<int> Execute()
    {
        Console.WriteLine(SqlCommand.CommandText);

        int result;

            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();

            result = await SqlCommand.ExecuteNonQueryAsync();

        return result;
    }

    public async Task<LinkedList<T>> GetResultAndPrint()
    {
        Console.WriteLine(SqlCommand.CommandText);

        LinkedList<T> list = await GetResult();
        foreach (T item in list)
            item.Print();

        return list;
    }

    private void BuildWhereClause(Expression expression)
    {
        if (expression is not BinaryExpression body)
            throw new ArgumentException();

        if (body.Left is MemberExpression leftMember)
        {
            string columnName = leftMember.Member.Name;

            var prop = (PropertyInfo)leftMember.Member;
            if (prop.PropertyType == typeof(DateTime))
            {
                columnName = "DATETIME(" + columnName + ")";
            }

            SqlCommand.Append(columnName);
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
        SqlCommand.Append($" {comparisonOperator} ");

        if (body.Right is MemberExpression rightMember)
        {
            object? value = GetComparisonValue(rightMember);

            PropertyInfo? prop = rightMember.Member as PropertyInfo;
            Type type = (prop != null) ? prop.PropertyType : ((FieldInfo)rightMember.Member).FieldType;

            if (type == typeof(DateTime))
            {
                SqlCommand.AddParameter(DateTime.Parse(value?.ToString() ?? ""));
            }
            else if(type == typeof(string))
            {
                SqlCommand.AddParameter(value?.ToString() ?? "");
            }
            else
            {
                SqlCommand.AddParameter(value ?? Expression.Default(type));
            }
        }
        else if (body.Right is ConstantExpression rightConst)
        {
            string value = rightConst?.Value?.ToString() ?? "";

            SqlCommand.AddParameter(value);
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
}

