using System;

public class Table<T> 
{
    private T[,] tableData;
    public Table(int columns, int rows)
    {
        tableData = new T[columns,rows];
    }
    public T this[int column, int row]
    {
        get{
            return tableData[column,row];
        }set{
            tableData[column,row] = value;
        }
    }
    public void ForEach(Action<T, int, int> action){
        for(int column = 0; column < tableData.GetLength(0);column++)
        {
            for(int row = 0; row < tableData.GetLength(1);row++)
            {
                action(tableData[column,row],column,row);
            }
        }
    }
}