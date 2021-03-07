using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Feri.Data.Access;

namespace OECD_SDMX
{

    public partial class Form1 : Form
    {
        //For working with excel file

        private List<catalog> cataloglist;
        private List<string> attributes_list;
        private string id;
        private readonly string currentDir = Directory.GetCurrentDirectory();
        private readonly string inputsFolder = Properties.Settings.Default.InputsFolder;
        private readonly string identificatorsFolder = Properties.Settings.Default.IdentificatorsFolder;
        private readonly string identificatorsFile = Properties.Settings.Default.IdentificatorsFile;
        private readonly string databaseName = Properties.Settings.Default.DatabaseName;
        private readonly string outputsFolder = Properties.Settings.Default.OutputsFolder;
        private readonly string connectorFile = Properties.Settings.Default.ConnectorFile;
        private string lastDB = "";
        helper help = new helper();

        public Form1()
        {
            InitializeComponent();
        }

        //Function (in brackets variables), void - no return. Return - function returns an element, no return - function changes the elements of the class but no return
        private void Form1_Load(object sender, EventArgs e)
        {
            cataloglist = help.GetXml();
            //if cataloglist is empty, then no XML was received due to no connection
            //then disable all elements to exclude any further actions
            //return nothing (since data type of function is void)
            if (cataloglist.Count == 0) {
                dataGridView1.Enabled = false;
                return;
            }

            List<catalog> chosen_IDs = new List<catalog>();
            chosen_IDs = help.GetSettings(Path.Combine(currentDir, inputsFolder, identificatorsFile), cataloglist);
            foreach (catalog c in chosen_IDs)
            {
                dataGridView1.Rows.Add(false, c.Name, c.Description);
            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            List<string> multdatabase = new List<string>();
            
           // multdatabase.Add()
            
            if (!(Convert.ToBoolean(dataGridView1.CurrentRow.Cells[0].Value)))
            {
                //Uncheck all options first, then assign corresponding dataset name
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    row.Cells[0].Value = false;
                }
                //check selected row and assign corresponding dataset name
                dataGridView1.CurrentRow.Cells[0].Value = true;

                //Collection<string> id = new Collection<string>();
               
                id = (String)dataGridView1.CurrentRow.Cells[1].Value;
            }
            else
            {
                //uncheck selected row and assign 'null' as dataset name
                dataGridView1.CurrentRow.Cells[0].Value = false;
                id = null;
                return;
            }
        }

        //Button "Download"
        private void Start_click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true && checkBox2.Checked == true)
            {
                bool overwrite_json = true;
                if (overwrite.Checked == true)
                {
                    overwrite_json = true;
                }
                else
                {
                    overwrite_json = false;
                }

                attributes_list = help.GetMandatoryNodes(id);

                if (attributes_list.Count == 0)
                {
                    MessageBox.Show("Choose database first and then press 'Download and Write' button");
                    return;
                }

                //If directory with name <Dataset name> does not exist, create it
                if (!Directory.Exists(Path.Combine(currentDir, identificatorsFolder, id)))
                {
                    Directory.CreateDirectory(Path.Combine(currentDir, identificatorsFolder, id));
                }

                List<string> allAttrElements = help.GetAttrElements(id, attributes_list[0]);
                progressBar1.Value = 0;
                progressBar1.Maximum = allAttrElements.Count;
                progressBar1.Step = 1;

                int numOfElementsInAttr = attributes_list.Count;

                for (int i = 0; i < allAttrElements.Count; i++)
                {
                    progressBar1.PerformStep();
                    help.GetQuery(id, allAttrElements[i], numOfElementsInAttr, overwrite_json);
                }

                //    MessageBox.Show("Database is successfully downloaded");
                //    return;
                //}

                ////Button "Upload"
                //private void Upload_click(object sender, EventArgs e)
                //{
                string currentTime = DateTime.Now.ToString("HHmmss_ddMMyyyy");
                if (update.Checked && lastDB == "")
                {
                    MessageBox.Show("No MDB-file was chosen");
                    return;
                }
                //If no dataset is chosen, throw a warning message
                if (id == null)
                {
                    MessageBox.Show(String.Format("Choose dataset first, then press 'Execute'", id));
                    return;
                }

                //If directory with name <Dataset name> does not exist, throw a warning message
                if (!Directory.Exists(Path.Combine(currentDir, identificatorsFolder, id)))
                {
                    MessageBox.Show(String.Format("No folder by given dataset ('{0}') was found.\nPress 'Download OECD-Database' in order to download the chosen identificator and then press 'Upload'", id));
                    return;
                }

                // Choose all the files in a given folder (by dataset name)
                DirectoryInfo d = new DirectoryInfo(Path.Combine(currentDir, identificatorsFolder, id));
                FileInfo[] Files = d.GetFiles("*.json");
                SeriesList DataSeriesSet = null;
                DataWriter db_feri_connect = new DataWriter();

                progressBar1.Value = 0;
                progressBar1.Maximum = Files.Length;
                progressBar1.Step = 1;


                string unique_databaseName = string.Format("{0}_{1}.{2}", databaseName.Split('.')[0], id, databaseName.Split('.')[1]);
                string unique_connectorName = string.Format("{0}_{1}.{2}", connectorFile.Split('.')[0], currentTime, connectorFile.Split('.')[1]);
                string dbI = Path.Combine(currentDir, inputsFolder, databaseName);
                string dbO = Path.Combine(currentDir, outputsFolder, unique_databaseName);
                string connectorI = Path.Combine(currentDir, inputsFolder, connectorFile);
                string connectorO = Path.Combine(currentDir, outputsFolder, unique_connectorName);
                if (!update.Checked)
                {
                    // Copy empty MDB and Connector.xml template from Inputs-folder to Outputs-folder
                    // if exists add records there, if not - create and then add records
                    if (Directory.Exists(Path.Combine(currentDir, inputsFolder)))
                    {
                        if (!File.Exists(dbO))
                        {
                            File.Copy(dbI, dbO);
                        }
                        if (!File.Exists(connectorO))
                        {
                            File.Copy(connectorI, connectorO);
                        }
                    }
                }
                foreach (FileInfo file in Files)
                {
                    progressBar1.PerformStep();
                    DataSeriesSet = help.SendData(Path.Combine(currentDir, identificatorsFolder, id, file.ToString()), DataSeriesSet);
                    if (update.Checked && lastDB != "")
                    {
                        Updater.Execute(db_feri_connect.connector(connectorO, lastDB), DataSeriesSet, Path.GetFileNameWithoutExtension(databaseName));
                    }
                    else
                    {
                        Updater.Execute(db_feri_connect.connector(connectorO, dbO), DataSeriesSet, Path.GetFileNameWithoutExtension(databaseName));
                    }
                    DataSeriesSet = null;
                }
                if (Directory.Exists(Path.Combine(currentDir, outputsFolder)))
                {
                    d = new DirectoryInfo(Path.Combine(currentDir, outputsFolder));
                    Files = d.GetFiles("*.xml");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        Files[i].Delete();
                    }
                }

                MessageBox.Show("Files is Downloaded & Database is uploaded successfully!");
            }
            else if (checkBox1.Checked == true)
            {
                bool overwrite_json = true;
                if (overwrite.Checked == true)
                {
                    overwrite_json = true;
                }
                else
                {
                    overwrite_json = false;
                }

                attributes_list = help.GetMandatoryNodes(id);

                if (attributes_list.Count == 0)
                {
                    MessageBox.Show("Choose database first and then press 'Execute' button");
                    return;
                }

                //If directory with name <Dataset name> does not exist, create it
                if (!Directory.Exists(Path.Combine(currentDir, identificatorsFolder, id)))
                {
                    Directory.CreateDirectory(Path.Combine(currentDir, identificatorsFolder, id));
                }

                List<string> allAttrElements = help.GetAttrElements(id, attributes_list[0]);
                progressBar1.Value = 0;
                progressBar1.Maximum = allAttrElements.Count;
                progressBar1.Step = 1;

                int numOfElementsInAttr = attributes_list.Count;

                for (int i = 0; i < allAttrElements.Count; i++)
                {
                    progressBar1.PerformStep();
                    help.GetQuery(id, allAttrElements[i], numOfElementsInAttr, overwrite_json);
                }

                MessageBox.Show("Database is successfully downloaded!");
            }
            else if (checkBox2.Checked == true)
            {
                string currentTime = DateTime.Now.ToString("HHmmss_ddMMyyyy");
                if (update.Checked && lastDB == "")
                {
                    MessageBox.Show("No MDB-file was chosen");
                    return;
                }
                //If no dataset is chosen, throw a warning message
                if (id == null)
                {
                    MessageBox.Show(String.Format("Choose dataset first, then press 'Execute'", id));
                    return;
                }

                //If directory with name <Dataset name> does not exist, throw a warning message
                if (!Directory.Exists(Path.Combine(currentDir, identificatorsFolder, id)))
                {
                    MessageBox.Show(String.Format("No folder by given dataset ('{0}') was found.\nPress 'Execute' in order to download the chosen identificator and then press 'Execute'", id));
                    return;
                }

                // Choose all the files in a given folder (by dataset name)
                DirectoryInfo d = new DirectoryInfo(Path.Combine(currentDir, identificatorsFolder, id));
                FileInfo[] Files = d.GetFiles("*.json");
                SeriesList DataSeriesSet = null;
                DataWriter db_feri_connect = new DataWriter();

                progressBar1.Value = 0;
                progressBar1.Maximum = Files.Length;
                progressBar1.Step = 1;


                string unique_databaseName = string.Format("{0}_{1}.{2}", databaseName.Split('.')[0], id, databaseName.Split('.')[1]);
                string unique_connectorName = string.Format("{0}_{1}.{2}", connectorFile.Split('.')[0], currentTime, connectorFile.Split('.')[1]);
                string dbI = Path.Combine(currentDir, inputsFolder, databaseName);
                string dbO = Path.Combine(currentDir, outputsFolder, unique_databaseName);
                string connectorI = Path.Combine(currentDir, inputsFolder, connectorFile);
                string connectorO = Path.Combine(currentDir, outputsFolder, unique_connectorName);
                if (!update.Checked)
                {
                    // Copy empty MDB and Connector.xml template from Inputs-folder to Outputs-folder
                    // if exists add records there, if not - create and then add records

                    if (Directory.Exists(Path.Combine(currentDir, inputsFolder)))
                    {
                        if (!File.Exists(dbO))
                        {
                            File.Copy(dbI, dbO);
                        }
                        if (!File.Exists(connectorO))
                        {
                            File.Copy(connectorI, connectorO);
                        }
                    }
                }
                foreach (FileInfo file in Files)
                {
                    progressBar1.PerformStep();
                    DataSeriesSet = help.SendData(Path.Combine(currentDir, identificatorsFolder, id, file.ToString()), DataSeriesSet);
                    if (update.Checked && lastDB != "")
                    {
                        Updater.Execute(db_feri_connect.connector(connectorO, lastDB), DataSeriesSet, Path.GetFileNameWithoutExtension(databaseName));
                    }
                    else
                    {
                        Updater.Execute(db_feri_connect.connector(connectorO, dbO), DataSeriesSet, Path.GetFileNameWithoutExtension(databaseName));
                    }
                    DataSeriesSet = null;
                }
                if (Directory.Exists(Path.Combine(currentDir, outputsFolder)))
                {
                    d = new DirectoryInfo(Path.Combine(currentDir, outputsFolder));
                    Files = d.GetFiles("*.xml");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        Files[i].Delete();
                    }
                }
                MessageBox.Show("Database is uploaded successfully");

            }
            else if (checkBox1.Checked==false && checkBox2.Checked == false)
            {
                MessageBox.Show("Choose Download json files only or Write to Database only or both!");
            }
        }

        private void Update_CheckedChanged(object sender, EventArgs e)
        {
            if (update.Checked)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Textdateien (*.mdb)|*.mdb|Alle Dateien (*.*)|*.*";
                dialog.InitialDirectory = Path.Combine(currentDir, outputsFolder);
                dialog.Title = "Select a MDB-file";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    lastDB = dialog.FileName;
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
        //Download CheckBox
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        //Write to Database Checkbox
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void edit_Click(object sender, EventArgs e)
        {
            if (edit_click.Text == "Edit")
            {
                edit_click.Text = "Save";
                dataGridView1.AllowUserToAddRows = true;
                dataGridView1.AllowUserToDeleteRows = true;
                contextMenuStrip1.Enabled = true;
                contextMenuStrip1.Visible = true;
            }
            else
            {
                string cName = string.Empty;
                string cKey = string.Empty;
                string cNewsFeed = string.Empty;
                string cDataflow = string.Empty;
                string sDate = string.Empty;
                int startD = 0;
                int endD = 0;
                string eDate = string.Empty;
                string saveAs = string.Empty;

                dataGridView1.ClearSelection();
                List<string> lines = new List<string>();
                Regex r = new Regex(@"(\d+_\d+)");

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) break;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.ColumnIndex == 0) continue;
                        if (cell.Value == null && !((cell.ColumnIndex == 5) || cell.ColumnIndex == 6))
                        {                           
                            dataGridView1.CurrentCell = cell;
                            dataGridView1.BeginEdit(true);
                            MessageBox.Show("Leere Zellen sind nicht erlaubt!");
                            return;
                        }

                        switch (cell.ColumnIndex)
                        {
                            case 1:
                                if (cell.Value.ToString().Length > 200)
                                {
                                    dataGridView1.CurrentCell = cell;
                                    dataGridView1.BeginEdit(true);
                                    MessageBox.Show("Zu viele Zeichen verwendet! Name wurde gekürzt.");
                                    cName = cell.Value.ToString().Substring(0, 199);
                                }
                                else
                                    cName = cell.Value.ToString();
                                break;
                            case 2:
                                if (!cell.Value.ToString().Contains("."))
                                {
                                    dataGridView1.CurrentCell = cell;
                                    dataGridView1.BeginEdit(true);
                                    MessageBox.Show("Key muss mit einem \".\" getrennt werden!");
                                    return;
                                }
                                else
                                    cKey = cell.Value.ToString();
                                break;
                            case 3:
                                if (cell.Value.ToString().Length < 2)
                                {
                                    dataGridView1.CurrentCell = cell;
                                    dataGridView1.BeginEdit(true);
                                    MessageBox.Show("Zu wenige Zeichen verwendet.");
                                }
                                else
                                    cNewsFeed = cell.Value.ToString();
                                break;
                            case 4:
                                if (cell.Value.ToString().Contains("_") && r.Match(cell.Value.ToString()).Success)
                                {
                                    cDataflow = cell.Value.ToString();
                                }
                                else
                                {
                                    dataGridView1.CurrentCell = cell;
                                    dataGridView1.BeginEdit(true);
                                    MessageBox.Show("DataFlow muss aus Zahlen bestehen und mit einem \"_\" getrennt werden! \nBeispiel: 168_45");
                                    return;
                                }
                                break;
                            case 5:
                                if (cell.Value != null && cell.Value.ToString() != "")
                                {
                                    if (int.TryParse(cell.Value.ToString(), out startD) && startD >= 1900)
                                    {
                                        sDate = cell.Value.ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.CurrentCell = cell;
                                        dataGridView1.BeginEdit(true);
                                        MessageBox.Show("Start Datum muss eine Zahl größer als 1900 sein!");
                                        return;
                                    }
                                }
                                else
                                    sDate = "";
                                break;
                            case 6:
                                if (cell.Value != null && cell.Value.ToString() != "")
                                {
                                    if (int.TryParse(cell.Value.ToString(), out endD) && endD >= startD)
                                    {
                                        eDate = cell.Value.ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.CurrentCell = cell;
                                        dataGridView1.BeginEdit(true);
                                        MessageBox.Show("End Datum muss eine Zahl größer oder gleich dem Start Datum sein!");
                                        return;
                                    }
                                }
                                else
                                    eDate = "";
                                break;
                            case 7:
                                if (cell.Value.ToString().Length > 80)
                                {
                                    dataGridView1.CurrentCell = cell;
                                    dataGridView1.BeginEdit(true);
                                    MessageBox.Show("Zu viele Zeichen verwendet! Dateiname wurde gekürzt.");
                                    saveAs = cell.Value.ToString().Substring(0, 79);
                                }
                                else
                                    saveAs = cell.Value.ToString();
                                break;
                        }
                    }
                    string rowString = string.Join("|", cName, cKey, cNewsFeed, cDataflow, sDate, eDate, saveAs);
                    lines.Add(rowString);
                }

                using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.IdentificatorsFile))
                {
                    sw.WriteLine(";ID|Description");
                    foreach(string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
                edit_click.Text = "Edit";
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                contextMenuStrip1.Enabled = false;
                contextMenuStrip1.Visible = false;
            }
        }
    }
}