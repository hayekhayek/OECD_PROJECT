using System;
using Feri.Data.Factory;
using System.IO;
using System.Data.Common;
namespace OECD_SDMX
{
    class DataWriter : IDisposable
    {
        private readonly string currentDir = Directory.GetCurrentDirectory();
        private readonly string inputsFolder = Properties.Settings.Default.InputsFolder;
        private readonly string databaseName = Properties.Settings.Default.DatabaseName;
        private readonly string connectorFile = Properties.Settings.Default.ConnectorFile;
        public Connector connector()
        {
            string conName = Path.Combine(currentDir, inputsFolder, connectorFile);
            using (Connector con = new Connector())
            {
                if (!File.Exists(conName))
                {
                    con.Attach(new FileLocationInfo(con, Path.Combine(currentDir, inputsFolder, databaseName)));
                    con.Save(conName);             
                }
                else
                {
                    con.Load(conName, false);
                }
                return con;
            }
        }
        public Connector connector(string con_path, string db_path)
        {
            using (Connector con = new Connector())
            {

                con.Attach(new FileLocationInfo(con, db_path));
                con.Save(con_path);
                return con;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

    }


}
