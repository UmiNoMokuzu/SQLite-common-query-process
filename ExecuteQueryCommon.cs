/// <summary>
/// SQL文実行処理
/// </summary>
/// <param name="sqlStmt">実行対象のSQL文</param>
/// <param name="values">セットする値リスト</param>
/// <returns></returns>
public static bool ExecuteQueryCommon(string sqlStmt, object[] values)
{
    var isSuccess  = false;
    var matches    = Regex.Matches(sqlStmt, @"@\d{1,2}");
    var parameters = new List<string>();

    // パラメータ(@1, @2...@n)を検出してリスト化する
    foreach (Match match in matches)
        parameters.Add(match.Value);

    sqliteConnection = GetDbConnection();

    using (SQLiteCommand     command     = sqliteConnection.CreateCommand())    
    using (SQLiteTransaction transaction = sqliteConnection.BeginTransaction())
    {
        command.CommandText = sqlStmt;

        // パラメータ毎に値をバインドする
        for (int j = 0; j < parameters.Count; j++)
            command.Parameters.Add(
                new SQLiteParameter(parameters[j], values[j]));

        try
        {
            // 実行
            int affRowNum = command.ExecuteNonQuery();

            if (affRowNum > 1)
            {
                // 正常終了:コミット
                transaction.Commit();
                isSuccess = true;
            }
            else
            {
                // 異状時:ロールバック
                transaction.Rollback();
                isSuccess = false;
            }
        }

        // 更新時異状, 例外が発生したらロールバック
        catch (DbException e) { transaction.Rollback(); } 
        catch (Exception e)   { transaction.Rollback(); } 
    }
    return isSuccess;
}