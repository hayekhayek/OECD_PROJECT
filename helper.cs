using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Feri.Data.Access;
using System.Data;
using System.Globalization;

namespace OECD_SDMX
{
    public class helper
    {
        private readonly string currentDir = Directory.GetCurrentDirectory();
        private readonly string inputsFolder = Properties.Settings.Default.InputsFolder;
        private readonly string identificatorsFolder = Properties.Settings.Default.IdentificatorsFolder;
        private readonly string identificatorsFile = Properties.Settings.Default.IdentificatorsFile;
        private readonly string schemaURL = Properties.Settings.Default.schemaURL;
        private readonly string jsonData = Properties.Settings.Default.JSONdataURL;
        private readonly string datastructure = Properties.Settings.Default.dataStructureURL;
        private readonly string subjectAttribute = Properties.Settings.Default.SubjectAttribute;
        private readonly string measureAttribute = Properties.Settings.Default.MeasureAttribute;
        private readonly string frequencyAttribute = Properties.Settings.Default.FrequencyAttribute;
        private readonly string countryAttrList = Properties.Settings.Default.CountryAttrList;
        private string dateType = String.Empty;
        // This function is used only when there is no connection to the oecd-website
        // in order to get the data
        public List<catalog> GetSettings(String path)
        {
            StreamReader file;
            // If path is not correct, throw an exception and return all ID's
            try
            {
                file = new StreamReader(path);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(string.Format("No internet connection.\n No file with user settings was found({0})", identificatorsFile));
                return new List<catalog>();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(string.Format("No internet connection.\n No 'Inputs'-folder was found({0})", inputsFolder));
                return new List<catalog>();
            }
            // Read data from txt and add to list
            string line;
            List<string> chosen_IDs = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                chosen_IDs.Add(line);
            }
            file.Close();
            // If Identificators.txt is empty
            // then return all ID's
            if (chosen_IDs.Count == 0)
            {
                MessageBox.Show(string.Format("No internet connection.\n " +
                    "File with user settings was empty({0})\n" +
                    "Please, fill the file with identificators and launch the program again", identificatorsFile));
                return new List<catalog>();
            }
            // Filter all ID's by using REGEX
            var id_regex = new Regex("[a-zA-Z0-9_]+");
            var temp = id_regex.Match(chosen_IDs[0]);
            List<string> filtered_ids = new List<string>();
            foreach (string ID in chosen_IDs)
            {
                filtered_ids.Add(id_regex.Match(ID).Value);
            }
            List<catalog> new_catalog_IDs = new List<catalog>(filtered_ids.Count);
            foreach (string id in filtered_ids)
            {
                new_catalog_IDs.Add(new catalog() { Name = id, Description = "" });
            }
            return new_catalog_IDs;
        }

        // Get only ID's that were specified by user
        // These ID's are read from "~/Settings/Identificators.txt" file
        public List<catalog> GetSettings(String path, List<catalog> all_identificators)
        {
            StreamReader file;
            // If path is not correct, throw an exception and return all ID's
            try
            {
                file = new StreamReader(path);
            }
            catch (FileNotFoundException)
            {
                return all_identificators;
            }
            catch (DirectoryNotFoundException)
            {
                return all_identificators;
            }

            // Read data from txt and add to list
            string line;
            List<string> chosen_IDs = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                chosen_IDs.Add(line);
            }
            file.Close();

            // If Identificators.txt is empty
            // then return all ID's
            if (chosen_IDs.Count == 0)
            {
                return all_identificators;
            }

            // Filter all ID's by using REGEX
            var id_regex = new Regex("[a-zA-Z0-9_]+");
            var temp = id_regex.Match(chosen_IDs[0]);
            List<string> filtered_ids = new List<string>();
            foreach(string ID in chosen_IDs)
            {
                filtered_ids.Add(id_regex.Match(ID).Value);
            }

            List<string> string_all_identificators = new List<string>();
            foreach(catalog ID in all_identificators)
            {
                string_all_identificators.Add(ID.Name);
            }

            List<catalog> new_catalog_IDs = new List<catalog>();
            catalog config;
            foreach (string ID in filtered_ids)
            {
                config = all_identificators.Find(item => item.Name == ID.ToUpper());
                if (config == null)
                {
                    MessageBox.Show("ID " + ID + " does not exist.\nThis ID is skipped.");
                }
                else
                {
                    new_catalog_IDs.Add(config);
                }
            }
            return new_catalog_IDs;
        }

        // Get all mandatory attributes of ID
        public List<string> GetMandatoryNodes(string id)
        {
            
           
            //String query = schema + id;
            if (id == null)
            {
                return new List<string>();
            }
            string query = Path.Combine(schemaURL, id);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException)
            {
                MessageBox.Show("No dataset with such name was found");
                return new List<string>();
            }
            
            Stream stream = response.GetResponseStream();
            if (stream != null)
            {
                XmlTextReader xmlReader = new XmlTextReader(stream);
                xmlReader.Namespaces = false;
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xmlReader);

                XmlNodeList allComplexTypeNodes = xdoc.SelectNodes("//*[local-name()='xs:complexType']");
                XmlNode dataSetTypeNode = allComplexTypeNodes[0];
                int counter = 0;
                foreach (XmlNode singleNode in allComplexTypeNodes)
                {
                    if (singleNode.OuterXml.ToString().Contains("DataSetType"))
                    {
                        dataSetTypeNode = allComplexTypeNodes[counter];
                        break;
                    }
                    counter++;
                }
                // Go down through xmlTree
                // "xs:complexContent" => ChildNodes[0]
                // "xs:extension" => ChildNodes[0]
                // "xs:attribute" => ChildNodes
                List<string> IDattributes = new List<string>();
                if (dataSetTypeNode!=null)
                {
                    foreach (XmlNode item in dataSetTypeNode.ChildNodes[0].ChildNodes[0].ChildNodes)
                    {
                        // Special exception because except xs:attributes 
                        // there is xs:choice in xs:extension as well
                        try
                        {
                            IDattributes.Add((string)item.Attributes["name"].Value);
                        }
                        catch(NullReferenceException)
                        {
                            continue;
                        }
                    }
                    return IDattributes;
                }
                return new List<string>();
            }
            return new List<string>();
        }

        public List<catalog> GetXml()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = false;


            // Check if no internet connection
            XmlReader reader = null;
            try
            {
                reader = XmlReader.Create(datastructure, settings);
            }
            catch (WebException)
            {
                MessageBox.Show("No internet connection");
                //return GetSettings(Path.Combine(currentDir, settings_path));
                return new List<catalog>();
            }
            if (reader == null)
            {
                return new List<catalog>();
            }

            List<catalog> allCatalogs = new List<catalog>();
            catalog cat;
            while (!reader.EOF)
            {
                if (reader.Name != "KeyFamily")
                {
                    reader.ReadToFollowing("KeyFamily");
                }
                if (!reader.EOF)
                {
                    XElement keyFamily = (XElement)XElement.ReadFrom(reader);
                    List<string> columns = new List<string>();
                    columns.Add((string)keyFamily.Attribute("id"));

                    columns.AddRange(keyFamily.Elements().Where(x => (x.Name.LocalName == "Name") && ((string)x.Attributes().First() == "en")).Select(x => (string)x));
                    cat = new catalog();
                    cat.Name = columns[0];
                    cat.Description = columns[1];
                    allCatalogs.Add(cat);
                }
            }
            // Returns list with all ID's
            return allCatalogs;
        }

        public List<string> GetAttrElements(String id, String attr, bool withWindow=true)
        {
            List<catalog> allCountries = new List<catalog>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = false;
            String query = Path.Combine(schemaURL, id);

            if (id == null)
            {
                return new List<string>();
            }
                
            var request = (HttpWebRequest)WebRequest.Create(query);
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            List<string> columns = new List<string>();

            if (stream != null)
            {
                var xmlReader = new XmlTextReader(stream);
                xmlReader.Namespaces = false;
                var xdoc = new XmlDocument();
                xdoc.Load(xmlReader);

                XmlNodeList allNodes = xdoc.SelectNodes("//*[local-name()='xs:simpleType']"); // match every element
                foreach (XmlNode singleNode in allNodes)
                {
                    if(singleNode.Attributes["name"].Value.Contains(attr))
                    {
                        foreach (XmlNode item in singleNode.ChildNodes[0].ChildNodes)
                        {
                            string item_value = (string)item.Attributes["value"].Value;
                            if (item_value != null)
                            {
                                columns.Add((string)item.Attributes["value"].Value);
                            }
                        }
                        return columns;
                    }
                }
            }
            else
            {
                MessageBox.Show("Bad request\n");
                return new List<string>();
            }
            if (withWindow)
            {
                MessageBox.Show("No countries were found for \n" + id);
            }
            return new List<string>();
        }

        public void GetQuery(String id, String elAttr, int amountOfAttributes, bool overwrite_json)
        {
            string filename = id + "_" + elAttr;
            string json_file = Path.Combine(currentDir, identificatorsFolder, id, filename + ".json");
            if (!overwrite_json)
            {
                // If json-file already exists, skip it
                if (File.Exists(json_file))
                {
                    return;
                }
            }


            WebClient client = new WebClient();
            

            // Query with only first attribute by priority
            string points = String.Concat(Enumerable.Repeat(".", amountOfAttributes-1));
            String query = Path.Combine(jsonData,id,elAttr+points);
            string json_string = "";

            try
            {
                json_string = client.DownloadString(query);
            }
            catch (WebException)
            {
                return;
            }

            JObject json_object = JObject.Parse(json_string);

            // Save as JSON
            using (StreamWriter file = File.CreateText(json_file))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                json_object.WriteTo(writer);
            }
        }

        public Obslist GetObslist(List<string> observations_list, List<JToken> observations)
        {
            //string dateType = String.Empty;
            Obslist obsList = new Obslist();
            foreach (JToken observation in observations)
            {
                JProperty prop = observation.ToObject<JProperty>();
                string name = prop.Name;
                string value = prop.ToList()[0][0].ToString();
                string date = observations_list[int.Parse(name)];
                // Period formats of SDMX
                // -------------------------------------------------
                // The following solution is done only to comply with 
                // FERI class requirements of 'obsDate' member variable from class 'Obs' 
                // since it requires date to be of type 'YYYYDDMM'
                // It does not influence the data anyhow and 
                // its aggregated relationship within observations 
                // but could affect the visualization significantly.
                // -------------------------------------------------

                // Daily/Business YYYY-MM-DD
                // -------------------------------------------------
                // In this case one needs only to rearrange 
                // the date in an appropriate format
                // -------------------------------------------------
                if (Regex.Match(date, @"(^[0-9]{4}-(0[1-9]|1[0-2])-[0-9]{2}$)").Groups[1].Value != "")
                {
                    string[] dateArray = date.Split('-');
                    date = string.Join("", new string[] { dateArray[0], dateArray[2], dateArray[1] });
                    dateType = "D";
                }
                // Monthly YYYY-MM
                // -------------------------------------------------
                // Add the first date of the given month
                // -------------------------------------------------
                else if (Regex.Match(date, @"(^[0-9]{4}-(0[1-9]|1[0-2])$)").Groups[1].Value != "")
                {
                    date = date.Split('-')[0] + ("01" + date.Split('-')[1]);
                    dateType = "M";
                }
                // Quarterly YYYY-Q[1-4]
                // -------------------------------------------------
                // if year is separated into quarters (Q1, Q2, Q3, Q4)
                // then make the first date of the beginning of each quarter
                // as if it is the whole quarter and assign the observation for this "period".
                // -------------------------------------------------
                else if (Regex.Match(date, @"(^[0-9]{4}-Q[1-4]{1}$)").Groups[1].Value != "")
                {
                    string month = "0" + (int.Parse(date[date.Length - 1].ToString()) * 3 - 2).ToString();
                    if (month.Length > 2)
                    {
                        month = month.Substring(1);
                    }
                    date = date.Split('-')[0] + ("01" + month);
                    dateType = "Q";
                }
                // Weekly YYYY-W[01-53]
                // -------------------------------------------------
                // The first day of the given is added and the date converted according to requirements
                // -------------------------------------------------
                else if (Regex.Match(date, @"(^[0-9]{4}-W[0-9]{2}$)").Groups[1].Value != "")
                {
                    int weekNumber = int.Parse(date.Split('W')[1]);
                    int year = int.Parse(date.Split('-')[0]);
                    date = GetDateByWeekNumber(year, weekNumber);
                    dateType = "W";
                }
                // Semi-annual YYYY-S[1-2]
                // -------------------------------------------------
                // if date is separated into quarters (S1, S2)
                // then make the first date of the beginning of each semi-annual
                // as if it is the whole semi-annual and assign the observation for this "period".
                // -------------------------------------------------
                else if (Regex.Match(date, @"(^[0-9]{4}-S[1-2]{1}$)").Groups[1].Value != "")
                {
                    string month = "0" + (int.Parse(date[date.Length - 1].ToString()) * 6 - 5).ToString();
                    if (month.Length > 2)
                    {
                        month = month.Substring(1);
                    }
                    date = date.Split('-')[0] + ("01" + month);
                    dateType = "S";
                }
                // Annual YYYY
                // -------------------------------------------------
                // if year is not separated into quarters
                // then the first date of the year "represents" the whole year
                // -------------------------------------------------
                else if (Regex.Match(date, @"(^[0-9]{4}$)").Groups[1].Value != "")
                {
                    date += "0101";
                    dateType = "A";
                }
                // Nothing matches
                // must be as impossible situation but anyway if it happens it
                //  must be skipped to prevent further mistakes
                else
                {
                    continue;
                }

                Obs obs = new Obs();
                string obsState = string.Empty;
                obs.obsDate = date;
                obs.obsValue = value;
                obsList.Add(obs);
            }
            obsList.ObsListAscendingOrder();
            obsList = CheckConsistency(dateType, obsList);
            return obsList;
        }

        private string GetDateByWeekNumber(int year, int weekNumber)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Tuesday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);
            Calendar clndr = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = clndr.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (firstWeek <= 1)
            {
                weekNumber -= 1;
            }
            int dayOfWeek = 0;
            DateTime result = firstMonday.AddDays(weekNumber * 7 + dayOfWeek - 1);
            return result.ToString("yyyyddMM");
        }
        public int GetWeeksInYear(int year)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(year, 12, 31);
            Calendar clndr = dfi.Calendar;
            return clndr.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        private Obslist CheckConsistency(string dateType, Obslist obsList)
        {
            List<string> oddDates = new List<string>();
            int firstYear = int.Parse(obsList.itemDate(0).Substring(0,4));
            int lastYear = int.Parse(obsList.itemDate(obsList.Count - 1).Substring(0, 4));
            int diff = lastYear - firstYear;

            if (dateType == "M")
            {
                for (int y = 0; y < diff + 1; y++)
                {
                    for (int m = 1; m < 13; m++)
                    {
                        oddDates.Add(new DateTime(firstYear + y, m, 1).ToString("yyyyddMM"));
                    }
                }
            }
            else if(dateType == "Q")
            {
                for (int y = 0; y < diff + 1; y++)
                {
                    for (int q = 1; q < 5; q++)
                    {
                        oddDates.Add(new DateTime(firstYear + y, q*3-2, 1).ToString("yyyyddMM"));
                    }
                }
            }
            else if (dateType == "W")
            {
                // -------------------------------------------------
                // -------- For possible future usage --------------
                //Since there is no option for weeks (Frequency.Weekly)
                //this chunk is commented and algorithm for Frequency.Daily is used insted
                //for (int y = 0; y < diff + 1; y++)
                //{
                //    int weeksInGivenYear = GetWeeksInYear(firstYear + y);
                //    for (int w = 0; w < weeksInGivenYear; w++)
                //    {
                //        oddDates.Add(GetDateByWeekNumber(firstYear + y, w));
                //    }
                //}
                // -------------------------------------------------
                Calendar clndr = CultureInfo.CurrentCulture.Calendar;
                for (int y = 0; y < diff + 1; y++)
                {
                    for (int m = 1; m < 13; m++)
                    {
                        int daysInGivenMonthOfYear = int.Parse(clndr.GetDaysInMonth(firstYear + y, m).ToString());
                        for (int d = 1; d < daysInGivenMonthOfYear + 1; d++)
                        {
                            oddDates.Add(new DateTime(firstYear + y, m, d).ToString("yyyyddMM"));
                        }
                    }
                }
                dateType = "D";
            }
            else if (dateType == "S")
            {
                for (int y = 0; y < diff + 1; y++)
                {
                    for (int s = 1; s < 3; s++)
                    {
                        oddDates.Add(new DateTime(firstYear + y, s * 6 - 5, 1).ToString("yyyyddMM"));
                    }
                }
            }
            else if (dateType == "D")
            {
                Calendar clndr = CultureInfo.CurrentCulture.Calendar;
                for (int y = 0; y < diff + 1; y++)
                {
                    for (int m = 1; m < 13; m++)
                    {
                        int daysInGivenMonthOfYear = int.Parse(clndr.GetDaysInMonth(firstYear + y, m).ToString());
                        for (int d = 1; d < daysInGivenMonthOfYear+1; d++)
                        {
                            oddDates.Add(new DateTime(firstYear + y, m, d).ToString("yyyyddMM"));
                        }
                    }
                }
            }
            else if (dateType == "A")
            {
                for (int y = 0; y < diff + 1; y++)
                {
                    oddDates.Add(new DateTime(firstYear + y, 1, 1).ToString("yyyyddMM"));
                }
            }
            // Remove items that already exist in the list of observations
            for(int o = 0; o<obsList.Count; o++)
            {
                oddDates.RemoveAll(r => r == obsList.itemDate(o));
            }
            foreach(string o in oddDates)
            {
                Obs obs = new Obs();
                string obsState = string.Empty;
                obs.obsDate = o;
                obs.obsValue = null;
                obsList.Add(obs);
            }
            obsList.ObsListAscendingOrder();
            return obsList;
        }
        public Series GetDataSeries(DataSeries oecdSeries, Obslist obsList)
        {

            Series dataSeries = new Series(oecdSeries.Frequency, oecdSeries.StartDate, obsList.Count, Aggregation.Average, Disaggregation.Even);
            dataSeries.Source = oecdSeries.Source;
            dataSeries.Name = oecdSeries.SeriesId;
            dataSeries.Description = oecdSeries.Description;
            if (dataSeries.Description.Length > 255)
            {
                dataSeries.Description = dataSeries.Description.Substring(0, 254);
            }
            dataSeries.LastExp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            dataSeries.LastRev = dataSeries.LastExp;
            dataSeries.HistoricalEnd = dataSeries.EndDate;

            for (int i = 0; i <= obsList.Count - 1; i++)
            {
                if (obsList.isValue(i))
                    dataSeries.set_Value(i, obsList.itemValue(i));
                else
                    dataSeries.set_State(i, DataState.NA);
            }
            return dataSeries;
        }

        public SeriesList CheckDataSeriesSet(SeriesList DataSeriesSet, Series dataSeries)
        {
            foreach (Series seriesSet in DataSeriesSet)
            {
                if (seriesSet.Name.Equals(dataSeries.Name))
                {
                    if (seriesSet.GetEndPeriod < dataSeries.GetEndPeriod)
                    {
                        DataSeriesSet.Remove(seriesSet);
                        break;
                    }
                    else if (seriesSet.GetEndPeriod > dataSeries.GetEndPeriod)
                    {
                        DataSeriesSet.Remove(dataSeries);
                        break;
                    }
                    else if (seriesSet.GetEndPeriod == dataSeries.GetEndPeriod)
                    {
                        DataSeriesSet.Remove(seriesSet);
                        DataSeriesSet.Add(dataSeries);
                        break;
                    }
                }
            }
            return DataSeriesSet;
        }

        public string GetUniqueName(string[] splitted_series, Dictionary<string, List<string>> structure)
        {
            // -------------------------------------------------
            // Get unique name of series keys (unique combination of keys) 
            // of format "LOCATION.SUBJECT.MEASURE.FREQUENCY"
            // For ex.: 
            // AUS.B1_GA.LNBQR.A
            // -------------------------------------------------
            string unique_name = "";
            for (int i = 0; i < splitted_series.Count(); i++)
            {
                unique_name += String.Format("{0}.", structure[i.ToString()][int.Parse(splitted_series[i])].ToString());
            }
            unique_name = unique_name.Substring(0, unique_name.Length - 1);
            return unique_name;
        }

        public DataSeries GetOECDseries(string unique_name, string unique_fullname, Dictionary<string, string> keys_and_attrs, Dictionary<string, List<string>> structure, string[] splitted_series)
        {
            DataSeries oecdSeries = new DataSeries();
            if (keys_and_attrs.Values.ToList().Contains(frequencyAttribute))
            {
                int freq_num = keys_and_attrs.Values.ToList().IndexOf(frequencyAttribute);
                oecdSeries.Freq = structure[freq_num.ToString()][int.Parse(splitted_series[freq_num])].ToString();
            }
            else
            {
                oecdSeries.Freq = "N";
            }
            oecdSeries.Source = "OECD";
            oecdSeries.SeriesId = unique_name;
            string[] splitted_unique_fullname = unique_fullname.Split('.');
            string subjectAttr = "";
            string measureAttr = "";
            List<string> seriesKeys = keys_and_attrs.Values.ToList();

            string[] countryAttrArr = countryAttrList.Split(',');
            foreach (string couAttr in countryAttrArr)
            {
                seriesKeys.Remove(couAttr);
            }
            //if (seriesKeys.Contains(measureAttribute))
            //{
            //    measureAttr = splitted_unique_fullname[keys_and_attrs.Values.ToList().IndexOf(measureAttribute)];
            //}
            //if (seriesKeys.Contains(subjectAttribute))
            //{
            //    subjectAttr = splitted_unique_fullname[keys_and_attrs.Values.ToList().IndexOf(subjectAttribute)];
            //}
            //if(subjectAttr != "" || measureAttr != "")
            //{
            //    oecdSeries.Description = string.Format("{0};{1};;", subjectAttr, measureAttr);
            //}
            string descr = "";
            foreach(string seriesKey in seriesKeys)
            {
                descr += splitted_unique_fullname[keys_and_attrs.Values.ToList().IndexOf(seriesKey)] + ";";
            }
            descr += ";";
            oecdSeries.Description = descr;

            return oecdSeries;
        }

        public List<string> GetAllUniqueNames(string json)
        {
            // -------------------------------------------------
            // Final results are packed in 'DataSeriesSet' of class 'SeriesList'
            // and sent to FERI via method 'Execute' of class 'Updater' 
            // with using class 'Connector' from Feri.Data.Factory
            // -------------------------------------------------
            StreamReader r = new StreamReader(json);
            string json_string = r.ReadToEnd();
            dynamic json_array = JsonConvert.DeserializeObject(json_string);
            dynamic json_structure = json_array["structure"]["dimensions"]["series"];
            dynamic json_observations = json_array["structure"]["dimensions"]["observation"][0]["values"];
            dynamic json_data = json_array["dataSets"][0]["series"];

            // Pack encoded observations into a C#-List
            List<string> observations_list = new List<string>();
            foreach (var item in json_observations)
            {
                observations_list.Add(item["id"].ToString());
            }

            // Pack the structure from JSON into C#-dictionary
            List<List<string>> seriesKeys = new List<List<string>>();
            List<string> values = new List<string>();
            List<string> keyPositions = new List<string>();
            List<string> attributes = new List<string>();
            foreach (var substructure in json_structure)
            {
                keyPositions.Add(substructure["keyPosition"].ToString());
                attributes.Add(substructure["id"].ToString());
                foreach (var value in substructure["values"])
                {
                    values.Add(value["id"].ToString());

                }
                seriesKeys.Add(values.ToList());
                values.Clear();
            }

            Dictionary<string, string> keys_and_attrs = keyPositions.Zip(attributes, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            Dictionary<string, List<string>> structure = keys_and_attrs.Keys.Zip(seriesKeys, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            List<string> all_unique_names = new List<string>();

            foreach (var item in json_data)
            {
                string[] splitted_series = item.Name.ToString().Split(':');
                string unique_name = GetUniqueName(splitted_series, structure);
                all_unique_names.Add(unique_name);
            }

            return all_unique_names;
        }

        public SeriesList SendData(string json, SeriesList DataSeriesSet)
        {
            // -------------------------------------------------
            // Final results are packed in 'DataSeriesSet' of class 'SeriesList'
            // and sent to FERI via method 'Execute' of class 'Updater' 
            // with using class 'Connector' from Feri.Data.Factory
            // -------------------------------------------------
            StreamReader r = new StreamReader(json);
            string json_string = r.ReadToEnd();
            if (json_string == "")
            {
                return new SeriesList();
            }
            dynamic json_array = JsonConvert.DeserializeObject(json_string);
            dynamic json_structure = json_array["structure"]["dimensions"]["series"];
            if (json_structure == null)
            {
                return new SeriesList();
            }
            dynamic json_observations = json_array["structure"]["dimensions"]["observation"][0]["values"];
            dynamic json_data = json_array["dataSets"][0]["series"];



            // Pack encoded observations into a C#-List
            List<string> observations_list = new List<string>();
            foreach (var item in json_observations)
            {
                observations_list.Add(item["id"].ToString());
            }

            // Pack the structure from JSON into C#-dictionary
            List<List<string>> seriesKeys = new List<List<string>>();
            List<List<string>> seriesKeys_for_fullnames = new List<List<string>>();
            List<string> values = new List<string>();
            List<string> fullnames = new List<string>();
            List<string> keyPositions = new List<string>();
            List<string> attributes = new List<string>();
            foreach (var substructure in json_structure)
            {
                keyPositions.Add(substructure["keyPosition"].ToString());
                attributes.Add(substructure["id"].ToString());
                foreach (var value in substructure["values"])
                {
                    values.Add(value["id"].ToString());
                    fullnames.Add(value["name"].ToString());
                }
                seriesKeys.Add(values.ToList());
                values.Clear();
                seriesKeys_for_fullnames.Add(fullnames.ToList());
                fullnames.Clear();
            }

            Dictionary<string, string> keys_and_attrs = keyPositions.Zip(attributes, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            Dictionary<string, List<string>> structure = keys_and_attrs.Keys.Zip(seriesKeys, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            Dictionary<string, List<string>> structure_with_fullnames = keys_and_attrs.Keys.Zip(seriesKeys_for_fullnames, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            List<string> all_unique_names = GetAllUniqueNames(json);

            int impCounter = 0;
            foreach (var item in json_data)
            {
                string[] splitted_series = item.Name.ToString().Split(':');
                string unique_name = GetUniqueName(splitted_series, structure);
                string unique_fullname = GetUniqueName(splitted_series, structure_with_fullnames);
                DataSeries oecdSeries = GetOECDseries(unique_name, unique_fullname, keys_and_attrs, structure, splitted_series);

                foreach (JObject obj in item)
                {
                    List<JToken> observations = obj["observations"].Select(obs => obs).ToList();
                    Obslist obsList = GetObslist(observations_list, observations);
                    oecdSeries.Start = obsList.itemDate(0);
                    oecdSeries.Freq = dateType;
                    Series dataSeries = GetDataSeries(oecdSeries, obsList);

                    if (DataSeriesSet == null)
                    {
                        DataSeriesSet = new SeriesList();
                    }
                            
                    DataSeriesSet.Add(dataSeries);
                    DataSeriesSet = CheckDataSeriesSet(DataSeriesSet, dataSeries);

                    if (DataSeriesSet != null)
                    {
                        impCounter = DataSeriesSet.Count;
                    }
                            
                    if (impCounter >= 5000)
                    {
                        // Here the DataSeriesSet is ready (the output of this program and the input for Feri)
                        return DataSeriesSet;
                    }
                }
            }
            if (impCounter > 0)
            {
                // Here the DataSeriesSet is ready (the output of this program and the input for Feri)
                return DataSeriesSet;
            }
            return DataSeriesSet;
        }
    }
}

