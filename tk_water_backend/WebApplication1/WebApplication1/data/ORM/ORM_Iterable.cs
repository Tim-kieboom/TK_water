using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace WebApplication1.data.ORM;

public static class Extensions
{
    public static void Append(this SqliteCommand cmd, string sqlLine)
    {
        cmd.CommandText += sqlLine;
    }

    public static void AddParameter(this SqliteCommand cmd, string value)
    {
        int index = cmd.Parameters.Count+1;
        string parameterName = $"@parameter{index}";
        cmd.Append(parameterName);
        cmd.Parameters.AddWithValue(parameterName, value);
    }

}

public class ORM_Iterable<T> where T : ORM_Table, new()
{
    public SqliteConnection Connection { get; set; }
    public SqliteCommand SqlCommand { get; set; }

    public ORM_Iterable(SqliteConnection connection)
    {
        Connection = connection;
        SqlCommand = new() {Connection = connection};
    }

    public ORM_Iterable<T> Where(Expression<Func<T, bool>> lambda)
    {
        SqlCommand.Append("WHERE ");
        BuildWhereClause(lambda.Body);
        return this;
    }

    public async Task<LinkedList<T>> GetResult()
    {
        Console.WriteLine(SqlCommand.CommandText);

        LinkedList<T> result;

        using (Connection)
        {
            await Connection.OpenAsync();

            using SqliteDataReader reader = await SqlCommand.ExecuteReaderAsync();
            if (reader == null)
            {
                Connection.Close();
                return [];
            }

            result = ORM_SqLite.GetAllResult<T>(reader);
        }

        return result;
    }

    public async Task<long> GetAfflictedCount()
    {
        Console.WriteLine(SqlCommand.CommandText);

        object? affectedRows;
        using (Connection)
        {
            await Connection.OpenAsync();
            affectedRows = await SqlCommand.ExecuteScalarAsync();
        }

        return Convert.ToInt64(affectedRows ?? 0);
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
            string value = GetComparisonValue(rightMember)?.ToString() ?? "";
            SqlCommand.AddParameter(value);
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

            var instance = Expression.Lambda(memberExpression.Expression).Compile().DynamicInvoke();
            var propertyInfo = memberExpression.Member as PropertyInfo;
            return propertyInfo?.GetValue(instance);
        }
        else
        {
            throw new NotSupportedException("Only constant values and property values are supported for comparison.");
        }
    }
}

