using System;
using System.Linq;
using System.Text;
using Feri.Data.Access;
using Feri.Data.Factory;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace OECD_SDMX
{
    public sealed class Updater
    {
        private static string getQuery(string tableName, Series[] seriesArray)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("select * from [");
            strBuilder.Append(tableName);
            strBuilder.Append("]");

            if (seriesArray.Length == 0)
            {
                strBuilder.Append(" where 1=0");
            }
            else
            {
                strBuilder.Append(" where SeriesName in (");
                bool First = true;
                foreach (Series series in seriesArray)
                {
                    if (First)
                    {
                        First = false;
                    }
                    else
                    {
                        strBuilder.Append(",");
                    }
                    strBuilder.Append("'" + series.Name + "'");
                }
                strBuilder.Append(")");
            }
            return strBuilder.ToString();

        }

        private static DataRow findRow(DataTable dt, String seriesName)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row["SeriesName"].ToString() == seriesName)
                    return row;
            }
            return null;
        }

        public static void Execute(Connector con, Series[] seriesArray, string tableName)
        {
            //MessageBox.Show();
            TableInfoDictionary tableInfoDic = con.TableInfoDictionary;
            TableInfo tableInfo = tableInfoDic[tableName];
            ConnectionBasedDatabaseInfo dataBaseInfo = (ConnectionBasedDatabaseInfo)tableInfo.DatabaseInfo;
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(dataBaseInfo.ProviderName);
            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = dataBaseInfo.ConnectionString;
                connection.Open();
                using (DbDataAdapter dataAdapter = providerFactory.CreateDataAdapter())
                {
                    dataAdapter.SelectCommand = connection.CreateCommand();
                    dataAdapter.SelectCommand.CommandText = getQuery(tableInfo.Name, seriesArray);

                    DbCommandBuilder commandBuilder = providerFactory.CreateCommandBuilder();
                    commandBuilder.DataAdapter = dataAdapter;
                    commandBuilder.QuotePrefix = "[";
                    commandBuilder.QuoteSuffix = "]";

                    using (DataTable dt = new DataTable())
                    {
                        dt.Locale = CultureInfo.CurrentCulture;
                        dataAdapter.Fill(dt);

                        foreach (Series series in seriesArray)
                        {
                            DataRow row = findRow(dt, series.Name);
                            bool newRow = (row == null);
                            if (newRow)
                            {
                                row = dt.NewRow();
                            }
                            series.Save(row);
                            if (newRow)
                            {
                                dt.Rows.Add(row);
                            }
                        }
                        dataAdapter.Update(dt);
                    }
                }
            }
        }

        public static void Execute(Connector connector, SeriesList seriesList, string tableName)
        {
            Execute(connector, seriesList.ToArray(), tableName);
        }

    }
}
