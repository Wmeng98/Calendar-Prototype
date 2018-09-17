// C# Project - SMTP Dashboard
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
// ^^^ path to c# compiler
// Workaround only have 1 view, retreive/read daily csv file when user clicks on the appropriate date in the calendar...
// For Directory.GetFiles and Directory.GetDirectories
// For File.Exists, Directory.Exists
using System;
using System.ComponentModel;
using System.Data; // datatable
using System.IO; 
using System.Security;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions; // regex
using System.Drawing;
using System.Windows.Forms;
namespace recurseFileProcessor
{
                public class fileProcessor
                {
                                private List<string> dailyCSVList = new List<string>();
                                public bool validMainPath(string path) {
                                                if (!Directory.Exists(path))
                                                {
                                                                return true;
                                                }
                                                return false;
                                }
                                // returns an array of string paths to all of the daily generated smtp csv files
                                public List<string> getDailyPaths(string path)
                                {
                                                if(File.Exists(path))
                                                {
                                                                // This path is a file
                                                                ProcessFile(path);
                                                }              
                                                else if(Directory.Exists(path))
                                                {
                                                                // This path is a directory
                                                                ProcessDirectory(path);
                                                }
                                                else
                                                {
                                                                // Don't append the string array
                                                                Console.WriteLine("------------------------------------");
                                                                Console.WriteLine("{0} is not a valid file or directory.", path);
                                                                Console.WriteLine("------------------------------------");
                                                }
                                                return dailyCSVList;
                                }
                                // Process all files in the directory passed in, recurse on any directories
                                // that are found, and process the files they contain.
                                public void ProcessDirectory(string targetDirectory)
                                {
                                                // Process the list of files found in the directory.
                                                string [] fileEntries = Directory.GetFiles(targetDirectory);
                                                foreach(string fileName in fileEntries)
                                                                ProcessFile(fileName);
                                                // Recurse into subdirectories of this directory.
                                                string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                                                foreach(string subdirectory in subdirectoryEntries)
                                                                ProcessDirectory(subdirectory);
                                }
                                // Insert logic for processing found files here.
                                public void ProcessFile(string path)
                                {
                                                dailyCSVList.Add(path);   
                                }
                }             
}

namespace SMTPData {
                public class SmtpData {
                                private string rawDataPath;
                                private List<string> dailyCSVPaths;
                                private IDictionary<string, string> smtpDataDict;
                               
                                private void listCSV()
                                {
                                                foreach (string csv in dailyCSVPaths)
                                                {
                                                                Console.WriteLine("------------------------------------");
                                                                Console.WriteLine("Found Daily csv file:\n{0}", csv);
                                                                Console.WriteLine("------------------------------------");
                                                }
                                }
                                private void initializeDict()
                                {
                                                foreach (string csvPath in dailyCSVPaths)
                                                {
                                                                string[] separators = {"\\"};
                                                                string [] csvPathArr = csvPath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                                                // retrieve the daily csv name without the path & add to smtpDict...
                                                                string dailyCSVFileName = csvPathArr[csvPathArr.Length - 1];
                                                                if (!smtpDataDict.ContainsKey(dailyCSVFileName))
                                                                {
                                                                                smtpDataDict.Add(dailyCSVFileName, csvPath);
                                                                                Console.WriteLine("************************************");
                                                                                Console.WriteLine("Added key:\n{0}\nvalue:\n{1}\npair to smtpDataDict", dailyCSVFileName, csvPath);
                                                                                Console.WriteLine("************************************");
                                                                }
                                                }
                                }
                                public SmtpData(string path)
                                {
                                                rawDataPath = path;
                                                recurseFileProcessor.fileProcessor processObj = new recurseFileProcessor.fileProcessor();
                                                dailyCSVPaths = processObj.getDailyPaths(rawDataPath);
                                                this.listCSV();
                                                // first get a list of all daily data smtp csv file paths by calling getDailyPaths method in fileProcessor object
                                                smtpDataDict = new Dictionary<string, string>();
                                                Console.WriteLine("\nDone SmtpData Object Construction...");
                                                this.initializeDict();
                                                Console.WriteLine("\nDone SmtpData Dictionary Initialization...");
                                }
                                public List<string> getDailyCSVData(string date) { // in the format "yyyyMMdd"
                                                string csvFile = "smtp_" + date + ".csv";
                                                 List <string>csvData = new List<string>();
                                                try {
                                                                if (smtpDataDict.ContainsKey(csvFile)) {
                                                                    // Create an instance of StreamReader to read from a file.
                                                                    // The using statement also closes the StreamReader.
                                                                    using (StreamReader sr = new StreamReader(smtpDataDict[csvFile])) 
                                                                    {
                                                                        string line;
                                                                        // Read and display lines from the file until the end of 
                                                                        // the file is reached.
                                                                        while ((line = sr.ReadLine()) != null) 
                                                                        {
                                                                            csvData.Add(line);
                                                                        }
                                                                    }
                                                                        //csvData = File.ReadAllLines(smtpDataDict[csvFile]);
                                                                }
                                                                else 
                                                                {
                                                                        csvData.Add("\"Csv data not found (deleted or not created yet) for date: " + date + "\"");
                                                                }
                                                }
                                                catch (SecurityException) {
                                                                csvData.Add("Permission not granted to read csv data for this date");
                                                                return csvData;
                                                }
                                                catch {
                                                                // default error message
                                                                csvData.Add("Error, csv data may be corrupted");
                                                                return csvData;
                                                }

                                                return csvData;
                                }
                }
}

// Windows Forms Calendar...
public class Form1 : System.Windows.Forms.Form
{
                private SMTPData.SmtpData smtpObj;
                private System.Windows.Forms.MonthCalendar monthCalendar1;

                // properties for DataGridView
                private System.Windows.Forms.DataGridView dataGridView1;
                private System.Windows.Forms.BindingSource bindingSource1;
                private System.Windows.Forms.TextBox textBox1;

                [STAThread]
                static void Main(string[] args)
                {
                                // Backend config...
                                string mainPath;
                                // Assume first argument is the path to smtp raw data folder...
                                if (args.Length == 0)
                                {
                                                MessageBox.Show("Please specify a valid path to smtp data folder in the run_dashboard batch file.");
                                                return;
                                }
                                else
                                {
                                                // grab the first CL argument & use as the mainPath to raw data
                                                mainPath = args[0];
                                                // test if directory is valid
                                                if (!Directory.Exists(mainPath)) {
                                                                MessageBox.Show("Please specify a valid path to smtp data folder.");
                                                                return;
                                                }
                                }
                                Application.Run(new Form1(mainPath));
                }

                public DataTable getDataTable(string inputDate)
                {
                                List<string> printOut;
                                printOut = smtpObj.getDailyCSVData(inputDate);
                                // Construct data table...
                                DataTable dt = new DataTable();

                                for(int i = 0; i < printOut.Count; i++)
                                {             
                                        if (printOut[i] != "") // exclude empty lines...
                                        {
                                                // table
                                                string pattern = @"""\s*,\s*""";
                                                string[] digits = Regex.Split(printOut[i].Substring(1, printOut[i].Length - 2), pattern);

                                                //Console.WriteLine(">>> " + digits[0] + " <<<");

                                                //Construct the number of columns in the data table
                                                if (digits.Length > 0 && i == 0)
                                                {
                                                                //construct columns first
                                                                foreach (string col in digits)
                                                                {
                                                                                dt.Columns.Add(col, typeof(string));
                                                                }
                                                }
                                                //construct data row & append to datatable
                                                if (i != 0)
                                                {
                                                                

                                                                DataRow row = dt.NewRow();
                                                                for(int el = 0; el < digits.Length; el++)
                                                                {
                                                                                row[el] = digits[el];
                                                                }
                                                                dt.Rows.Add(row);                                                                                                         
                                                }   
                                        }

                                }
                                return dt;
                }

                public void consoleDataTable(string dateStr)
                {
                                DataTable dt = new DataTable();
                                dt = getDataTable(dateStr);

                                //Console.WriteLine(">>" + dt.Rows[0].ItemArray[0]);

                                foreach (DataRow dataRow in dt.Rows)
                                        {
                                        Console.WriteLine("New Row ***********");
                                        foreach (var item in dataRow.ItemArray)
                                        {
                                                Console.WriteLine(item);
                                        }
                                  }

                }

                private void InitializeDataGridView(string dateStr)
                {
                        try
                        {
                            // Automatically generate the DataGridView columns.
                            dataGridView1.AutoGenerateColumns = true;

                            // Set up the data source.
                            bindingSource1.DataSource = getDataTable(dateStr);
                            dataGridView1.DataSource = bindingSource1;

                            // resize & distribute the columns in DataGridView

                            // distribute size (roughyl) of datagrid by no. of columns
                            int perColSize;
                            if (dataGridView1.Columns.Count != 0)
                            {
                                perColSize = (dataGridView1.Width / dataGridView1.Columns.Count) - 1;
                            }
                            else // size of the whole datagridview control
                            {
                                perColSize = dataGridView1.Width;
                            }
                            
                            foreach (DataGridViewColumn col in dataGridView1.Columns)
                            {
                                col.Width = perColSize;
                            }

                            // // Automatically resize the visible rows.
                            // dataGridView1.AutoSizeRowsMode =
                            //     DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

                            // Set the DataGridView control's border.
                            dataGridView1.BorderStyle = BorderStyle.Fixed3D;

                            // Put the cells in edit mode when user enters them.
                            dataGridView1.ReadOnly = true;
                            dataGridView1.AllowUserToAddRows = false;
                            dataGridView1.ClearSelection();

                            this.colorCodeGrid(0, @"august", @"september");
                        }
                        catch
                        {
                            Console.WriteLine("Error with DataGridView Initialization...");
                        }
                }

                // constructor
                public Form1(string mainPath)
                {
                                smtpObj = new SMTPData.SmtpData(mainPath);

                                // creating the calendar
                                this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
                                // set calendar location

                                this.monthCalendar1.Anchor = System.Windows.Forms.AnchorStyles.None;
                                this.monthCalendar1.Location = new System.Drawing.Point(50,3);
                                // change the colour
                                this.monthCalendar1.BackColor = System.Drawing.SystemColors.Info;
                                this.monthCalendar1.TitleBackColor = System.Drawing.Color.DarkOrange;
                                this.monthCalendar1.TitleForeColor = System.Drawing.Color.DarkBlue;
                                this.monthCalendar1.TrailingForeColor = System.Drawing.Color.Orange;
                                this.monthCalendar1.ForeColor = System.Drawing.Color.Blue;

                                // Add dates to the AnnuallyBoldedDatesArray
                                // More bold...
                                // Configure calendar to display 1 row by 4 columns of months
                                this.monthCalendar1.CalendarDimensions = new System.Drawing.Size(4,1);
                                // Set week to begin on Monday...
                                this.monthCalendar1.FirstDayOfWeek = System.Windows.Forms.Day.Monday;
                                // Only allow 1 day to be selected at the same time.
                                this.monthCalendar1.MaxSelectionCount = 1;
                                // Set calendar to move one month at a time when navigating using the arrow keys
                                this.monthCalendar1.ScrollChange = 1;
                                // circle today's dates
                                this.monthCalendar1.ShowTodayCircle = true;
                                //...
                                Console.WriteLine("monthCalendarHeight...>>>{0}", this.monthCalendar1.Location.Y);
                                // Read up on delegates & events...
                                // Add event handlers for DateSelected & DateChanged events
                                this.monthCalendar1.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateSelected);
                                this.monthCalendar1.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateChanged);
                                
                                // ******************************
                                // configure DataGridView Control

                                // config caption for data grid
                                this.textBox1 = new System.Windows.Forms.TextBox();
                                this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                                this.textBox1.Location = new System.Drawing.Point(50,this.monthCalendar1.Location.Y + 175);
                                
                                this.textBox1.Multiline = false;
                                this.textBox1.ReadOnly = true;
                                
                                this.textBox1.Size = new System.Drawing.Size(800,32);
                                this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
                                this.textBox1.TextAlign = HorizontalAlignment.Center;
                                this.textBox1.BackColor = System.Drawing.Color.LightGray;

                                this.textBox1.ForeColor = System.Drawing.Color.DarkBlue;
                                this.textBox1.Text = "Simple Mail Transfer Protocol Report for the date: " + DateTime.Now.ToString("yyyy-MM-dd");

                                this.dataGridView1 = new System.Windows.Forms.DataGridView();
                                this.bindingSource1 = new System.Windows.Forms.BindingSource();

                                // this.monthCalendar1.Location.Y + 100
                                
                                this.dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.None;
                                // config size of the DataGridView
                                this.dataGridView1.Location = new System.Drawing.Point(50, this.monthCalendar1.Location.Y + 200);
                                // Set fixed width & height of the DataGridView Table
                                this.dataGridView1.RowHeadersVisible  = false;
                                this.dataGridView1.Width = 800; // width of each column should be proportionally distributed
                                this.dataGridView1.Height = 450;
                                
                                // WORD WRAP
                                // Set defaultcellstyle of column and set autosizerow for dgv
                                
                                // Column / Row formatting
                                this.dataGridView1.AllowUserToResizeColumns = true;
                                this.dataGridView1.AllowUserToResizeRows = true;

                                // Add event handler to deal with auto-highlight when any sorting operation occurs
                                //  On the DataGridView
                                this.dataGridView1.Sorted += new EventHandler(this.dataGrid_ClearSelection);



                                // Setting up how the form should be displayed and add the controls to the form...
                                this.ClientSize = new System.Drawing.Size(920, 700);


                                this.Controls.AddRange(new System.Windows.Forms.Control[] {this.monthCalendar1, this.textBox1, this.dataGridView1});
                                this.Text = "Simple Mail Transfer Protocol (SMTP) Dashboard";

                                InitializeDataGridView(DateTime.Now.ToString("yyyyMMdd"));
                }

                private void monthCalendar1_DateSelected(object sender, System.Windows.Forms.DateRangeEventArgs e)
                {
                                // show start & end dates in the text box
                                Console.WriteLine("**********");
                                Console.WriteLine(e.Start.ToShortDateString().Replace("-","")); 

                                consoleDataTable(e.Start.ToShortDateString().Replace("-",""));

                                Console.WriteLine("**********");

                                textBox1.Text = "Simple Mail Transfer Protocol Report for the date: " + e.Start.ToShortDateString();
                                InitializeDataGridView(e.Start.ToShortDateString().Replace("-",""));
                }
                private void monthCalendar1_DateChanged(object sender, System.Windows.Forms.DateRangeEventArgs e)
                {
                                // show start & end dates in the text box
                                Console.WriteLine("**********");
                                Console.WriteLine(e.Start.ToShortDateString().Replace("-","")); 

                                consoleDataTable(e.Start.ToShortDateString().Replace("-",""));

                                Console.WriteLine("**********");

                                textBox1.Text = "Simple Mail Transfer Protocol Report for the date: " + e.Start.ToShortDateString();
                                InitializeDataGridView(e.Start.ToShortDateString().Replace("-",""));
                }
                private void dataGrid_ClearSelection(object sender, EventArgs e)
                {
                      this.dataGridView1.ClearSelection();
                      this.colorCodeGrid(0, @"august", @"september");        
                }

                // Color code a column of the DataGridView based off text values of a given cell
                // 
                // Check if the column index exists in current DGV
                private void colorCodeGrid(int colNum, string passRegex, string failRegex)
                {
                    if (colNum >= 0 && colNum < this.dataGridView1.Columns.Count)
                    {
                        Regex pass = new Regex(passRegex);
                        Regex fail = new Regex(failRegex);

                        foreach (DataGridViewRow row in this.dataGridView1.Rows)
                        {
                            Match matchPass = pass.Match(row.Cells[colNum].Value.ToString());
                            Match matchFail = fail.Match(row.Cells[colNum].Value.ToString());
                            
                            // Create a new DataGridViewCellStyle object
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.ForeColor = Color.Black;

                            if (matchPass.Success) 
                            {
                                row.Cells[colNum].Style.BackColor = System.Drawing.Color.LightGreen;

                            }
                            else if (matchFail.Success)
                            {
                                row.Cells[colNum].Style.BackColor = System.Drawing.Color.IndianRed;
                            }
                            else
                            {
                                row.Cells[colNum].Style.BackColor = System.Drawing.Color.Orange;
                            } 
                        }
                    }
                }
}