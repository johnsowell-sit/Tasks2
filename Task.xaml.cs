using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tasks2
{
    /// <summary>
    /// Interaction logic for Task.xaml
    /// </summary>
    /// 
    //  $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $

    public partial class Task : Window
    {
        const int EDIT_TASK = 0;
        const int NEW_TASK  = 1;

        const int CANCELED_TASK = -1;
        const int ASSIGNED_TASK = 0;
        const int COMPLETED_TASK = 1;

        int Action, DueDate, DueTime, ID, Status, WorkerID;
        string Caption, Description, Notes;
        DateTime Present;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public Task(int A, int W, int I)
        {
            int x;

            Present = DateTime.Now;
            InitializeComponent();
            Action = A;
            WorkerID = W; // needed only when inserting a new task into the database
            ID = I;

            lblTaskNumber.Text = "Task #" + ID.ToString();

            cmbMonth.Items.Add("JANUARY");
            cmbMonth.Items.Add("FEBRUARY");
            cmbMonth.Items.Add("MARCH");
            cmbMonth.Items.Add("APRIL");
            cmbMonth.Items.Add("MAY");
            cmbMonth.Items.Add("JUNE");
            cmbMonth.Items.Add("JULY");
            cmbMonth.Items.Add("AUGUST");
            cmbMonth.Items.Add("SEPTEMBER");
            cmbMonth.Items.Add("OCTOBER");
            cmbMonth.Items.Add("NOVEMBER");
            cmbMonth.Items.Add("DECEMBER");

            for (x=1; x<=31; x++)
            {
                cmbDate.Items.Add(x.ToString());
            }

            for (x=2000; x<=Present.Year+30; x++)
            {
                cmbYear.Items.Add(x.ToString());
            }

            for (x=0; x<24; x++)
            {
                cmbHour.Items.Add(x.ToString());
            }   
            
            for (x=0; x<60; x++)
            {
                if (x < 10)
                    cmbMinute.Items.Add("0" + x.ToString());
                else
                    cmbMinute.Items.Add(x.ToString());
            }

            if (Action == NEW_TASK)
            {
                lblTaskNumber.Text += " (New Task)";
                cmbMonth.SelectedIndex = Present.Month - 1;
                cmbDate.SelectedIndex = Present.Day - 1;
                cmbYear.SelectedIndex = Present.Year - 2000;
                cmbHour.SelectedIndex = Present.Hour;
                cmbMinute.SelectedIndex = Present.Minute;
                txtCaption.Text = "";
                txtDescription.Text = "";
                txtNotes.Text = "";
                cmdCancelTask.IsEnabled = false;
                cmdCompleteTask.IsEnabled = false;

                Status = ASSIGNED_TASK;
            }

            if (Action == EDIT_TASK)
            {
                int Len;
                string ConnStr, InitDir, SQL;
                SQLiteConnection Connection;
                SQLiteCommand Command;
                SQLiteDataReader Reader;

                InitDir = System.IO.Path.GetFullPath(".");
                Len = InitDir.Length;
                InitDir = InitDir.Substring(0, Len - 10);

                ConnStr = "DataSource=" + InitDir + "\\Data\\Tasks.db;Version=3";
                Connection = new SQLiteConnection(ConnStr);
                Connection.Open();

                SQL = "SELECT * FROM Task WHERE TaskID = " + ID.ToString();
                Command = new SQLiteCommand(SQL, Connection);
                Reader = Command.ExecuteReader();
                while(Reader.Read() == true)
                {
                    Caption = Reader["Caption"].ToString();
                    Description = Reader["Description"].ToString();
                    Notes = Reader["Notes"].ToString();
                    Status = Convert.ToInt32(Reader["Status"]);
                    DueDate = Convert.ToInt32(Reader["DueDate"]);
                    DueTime = Convert.ToInt32(Reader["DueTime"]);

                    txtCaption.Text = Caption;
                    txtDescription.Text = Description;
                    txtNotes.Text = Notes;
                    cmbYear.SelectedIndex = (DueDate / 10000) - 2000;
                    cmbDate.SelectedIndex = (DueDate % 100) - 1;
                    cmbMonth.SelectedIndex = ((DueDate % 10000) / 100) - 1;
                    cmbHour.SelectedIndex = DueTime / 100;
                    cmbMinute.SelectedIndex = DueTime % 100;
                }

                Reader.Close();
                Connection.Close();
            }
        }

        // ********************************************************************

        private void cmdCancelTask_Click(object sender, RoutedEventArgs e)
        {
            Status = CANCELED_TASK;
        }

        // ********************************************************************

        private void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            int AssignDate, AssignTime, Len;
            string ConnStr, InitDir, SQL;
            SQLiteConnection Connection;
            SQLiteCommand Command;

            InitDir = System.IO.Path.GetFullPath(".");
            Len = InitDir.Length;
            InitDir = InitDir.Substring(0, Len - 10);

            ConnStr = "DataSource=" + InitDir + "\\Data\\Tasks.db;Version=3";
            Connection = new SQLiteConnection(ConnStr);
            Connection.Open();

            Caption = StringCleanUp(txtCaption.Text);
            Description = StringCleanUp(txtDescription.Text);
            Notes = StringCleanUp(txtNotes.Text);
            DueDate = (10000 * (cmbYear.SelectedIndex + 2000)) + (100 * (cmbMonth.SelectedIndex + 1)) + cmbDate.SelectedIndex + 1;
            DueTime = (100 * cmbHour.SelectedIndex) + cmbMinute.SelectedIndex;

            SQL = "";
            if (Action == NEW_TASK)
            {
                Present = DateTime.Now; // to enter assign date and time for a new task
                AssignDate = (10000 * Present.Year) + (100 * Present.Month) + Present.Day;
                AssignTime = (100 * Present.Hour) + Present.Minute;
                SQL = "INSERT INTO Task VALUES(" + ID.ToString() + "," + WorkerID.ToString() + "," + AssignDate.ToString() + "," + AssignTime.ToString() + ",'" + Caption + "','" + Description + "'," + DueDate.ToString() + "," + DueTime.ToString() + "," + Status.ToString() + ",'" + Notes + "')";
            }

            if (Action == EDIT_TASK)
            {
                SQL = "UPDATE Task SET Caption = '" + Caption + "', Description = '" + Description + "', DueDate = " + DueDate.ToString() + ", DueTime = " + DueTime.ToString() + ", Status = " + Status.ToString() + ", Notes = '" + Notes + "' WHERE TaskID = " + ID.ToString();
            }

            Command = new SQLiteCommand(SQL, Connection);
            Command.ExecuteNonQuery();
            Connection.Close();
            if (Action == NEW_TASK)
                MessageBox.Show("New Task Entered!");
            else
                MessageBox.Show("Task Updated!");
            Close();
        }

        // ********************************************************************

        private void cmdCompleteTask_Click(object sender, RoutedEventArgs e)
        {
            Status = COMPLETED_TASK;
        }

        // ####################################################################

        static string StringCleanUp(string S)
        {
            int i;
            string Str = "";

            for (i = 0; i < S.Length; i++)
            {
                if ((S.Substring(i, 1) != "'") && (S.Substring(i, 1) != "&") && (S.Substring(i, 1) != "\""))
                    Str += S.Substring(i, 1);
                else
                {
                    if ((S.Substring(i, 1) == "'") || (S.Substring(i, 1) == "\""))
                        Str += "''";
                    else
                    {
                        if ((i > 0) && (S.Substring(i - 1, 1) != "&"))
                            Str += "&";
                    }
                }
            }
            return Str;
        }

        // ********************************************************************

        private void txtCaption_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdClose.IsEnabled = (txtCaption.Text.Length > 0);
       }
    }
}