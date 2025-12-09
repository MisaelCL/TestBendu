using System.Runtime.CompilerServices;

namespace NetMAUI_Clase6_Crud_SQLLite.Helpers;

public class SQLLiteHelper<T> : SQLLiteBase where T : BaseModels, new()
{
    public SQLLiteHelper()
    {
        _connection.CreateTable<T>();
    }

    public List<T> GetAllData()
        => _connection.Table<T>().ToList();

    public int Add(T row)
    {
        _connection.Insert(row);
        return row.Id;
    }

    public int Update(T row)
        => _connection.Update(row);

    public int Delete(T row)
        => _connection.Delete(row);

    public T Get(int ID)
        => _connection.Table<T>().FirstOrDefault(w => w.Id == ID);
}

public class SQLLiteBase
{
    private readonly string _rutaDB;
    public SQLiteConnection _connection;

    public SQLLiteBase()
    {
        _rutaDB = FileAccessHelper.GetPathFile("pacientes.db3");
        if (_connection != null)
        {
            return;
        }

        _connection = new SQLiteConnection(_rutaDB);
    }
}