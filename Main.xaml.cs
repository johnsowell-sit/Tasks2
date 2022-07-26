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
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        const int UPCOMING_TASKS  = 0;
        const int OVERDUE_TASKS   = 1;
        const int COMPLETED_TASKS = 2;
        const int ALL_TASKS       = 3;

        const int CANCELED_TASK   = -1;
        const int ASSIGNED_TASK   = 0;
        const int COMPLETED_TASK  = 1;

        const int EDIT_TASK       = 0;
        const int NEW_TASK        = 1;

        List<int> IDList;

        int GrandTotalTasks, WorkerID;
        string WorkerName;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public Main(int ID, string Name)
        {
            IDList = new List<int>();

            InitializeComponent();
            WorkerID = ID;
            WorkerName = Name;
            lblTaskName.Text = "Tasks For " + WorkerName;

            cmbTaskType.Items.Add("UPCOMING TASKS");
            cmbTaskType.Items.Add("OVERDUE TASKS");
            cmbTaskType.Items.Add("COMPLETED TASKS");
            cmbTaskType.Items.Add("ALL TASKS");
            cmbTaskType.SelectedIndex = 3;
        }

        /// <summary>
        //
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        // *******************************************************************

        private void cmbTaskType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTaskCounts();
            ListTasks(cmbTaskType.SelectedIndex);
        }

        // *******************************************************************

        private void cmdEditTask_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedIndex > -1)
            {
                Task dlg = new Task(EDIT_TASK, WorkerID, IDList[lstTasks.SelectedIndex]);

                dlg.ShowDialog();
                ListTasks(ALL_TASKS);
                UpdateTaskCounts();
            }
        }

        // *******************************************************************

        private void cmdNewTask_Click(object sender, RoutedEventArgs e)
        {
            Task dlg = new Task(NEW_TASK, WorkerID, GrandTotalTasks + 1);

            dlg.ShowDialog();
            ListTasks(ALL_TASKS);
            UpdateTaskCounts();
        }

        // *******************************************************************

        private void ListTasks(int TaskType)
        {
            int DueDate, DueTime, ID, Items, Len, StarDate, StarTime, Status;
            string Caption, ConnStr, InitDir, SQL, TaskStr;
            DateTime Present = DateTime.Now;
            SQLiteConnection Connection;
            SQLiteCommand Command;
            SQLiteDataReader Reader;

            StarDate = (10000 * Present.Year) + (100 * Present.Month) + Present.Day;
            StarTime = (100 * Present.Hour) + Present.Minute;

            InitDir = System.IO.Path.GetFullPath(".");
            Len = InitDir.Length;
            InitDir = InitDir.Substring(0, Len - 10);

            ConnStr = "DataSource=" + InitDir + "\\Data\\Tasks.db;Version=3";
            Connection = new SQLiteConnection(ConnStr);
            Connection.Open();

            SQL = "SELECT * FROM task WHERE WorkerID = " + WorkerID.ToString();

            if (TaskType == COMPLETED_TASKS)
                SQL += " AND Status = " + COMPLETED_TASK.ToString();
            if (TaskType == UPCOMING_TASKS)
                SQL += " AND Status = " + ASSIGNED_TASK.ToString() + " AND (DueDate > " + StarDate.ToString() + " OR (DueDate = " + StarDate.ToString() + " AND DueTime >= " + StarTime.ToString() + "))";
            if (TaskType == OVERDUE_TASKS)
                SQL += " AND Status = " + ASSIGNED_TASK.ToString() + " AND (DueDate < " + StarDate.ToString() + " OR (DueDate = " + StarDate.ToString() + " AND DueTime < " + StarTime.ToString() + "))";

            Command = new SQLiteCommand(SQL, Connection);
            Reader = Command.ExecuteReader();
            lstTasks.Items.Clear();
            cmdEditTask.IsEnabled = false;
            Items = 0;
            IDList.Clear();
            while (Reader.Read() == true)
            {
                ID = Convert.ToInt32(Reader["TaskID"]);
                Caption = Reader["Caption"].ToString();
                Status = Convert.ToInt32(Reader["Status"]);
                DueDate = Convert.ToInt32(Reader["DueDate"]);
                DueTime = Convert.ToInt32(Reader["DueTime"]);

                IDList.Add(ID);
                Items++;

                TaskStr = "TASK #" + ID.ToString() + ": " + Caption;
                if (Status == CANCELED_TASK)
                    TaskStr += " (CANCELED)";
                if (Status == COMPLETED_TASK)
                    TaskStr += " (COMPLETED)";
                if (Status == ASSIGNED_TASK)
                {
                    if ((DueDate < StarDate) || ((DueDate == StarDate) && (DueTime < StarTime)))
                        TaskStr += " (OVERDUE)";
                    if ((DueDate > StarDate) || ((DueDate == StarDate) && (DueTime >= StarTime)))
                        TaskStr += " (UPCOMING)";
                }

                lstTasks.Items.Add(TaskStr);
                cmdEditTask.IsEnabled = true;
            }
            Reader.Close();
            Connection.Close();
            if (Items > 0)
                lstTasks.SelectedIndex = Items - 1;
        }

        // *******************************************************************

        private void UpdateTaskCounts()
        {
            int Count, DueDate, DueTime, ID, Len, StarDate, StarTime, Status;
            string Caption, ConnStr, InitDir, SQL, TaskStr;
            DateTime Present = DateTime.Now;
            SQLiteConnection Connection;
            SQLiteCommand Command, Command2, Command3, Command4, Command5;
            SQLiteDataReader Reader, Reader2, Reader3, Reader4, Reader5;

            StarDate = (10000 * Present.Year) + (100 * Present.Month) + Present.Day;
            StarTime = (100 * Present.Hour) + Present.Minute;

            InitDir = System.IO.Path.GetFullPath(".");
            Len = InitDir.Length;
            InitDir = InitDir.Substring(0, Len - 10);

            ConnStr = "DataSource=" + InitDir + "\\Data\\Tasks.db;Version=3";
            Connection = new SQLiteConnection(ConnStr);
            Connection.Open();

            SQL = "SELECT COUNT(*) FROM task";
            Command = new SQLiteCommand(SQL, Connection);
            Reader = Command.ExecuteReader();
            GrandTotalTasks = 0;
            while (Reader.Read() == true)
            {
                GrandTotalTasks = Convert.ToInt32(Reader[0]);
            }
            Reader.Close();
            Connection.Close();

            Connection.Open();
            SQL = "SELECT COUNT(*) FROM task WHERE WorkerID = " + WorkerID.ToString();
            Command2 = new SQLiteCommand(SQL, Connection);
            Reader2 = Command2.ExecuteReader();
            Count = 0;
            while (Reader2.Read() == true)
            {
                Count = Convert.ToInt32(Reader2[0]);
            }
            Reader2.Close();
            Connection.Close();
            lblAllTasks.Text = Count.ToString();

            Connection.Open();
            SQL = "SELECT COUNT(*) FROM task WHERE WorkerID = " + WorkerID.ToString() + " AND Status = " + COMPLETED_TASK.ToString();
            Command3 = new SQLiteCommand(SQL, Connection);
            Reader3 = Command3.ExecuteReader();
            Count = 0;
            while (Reader3.Read() == true)
            {
                Count = Convert.ToInt32(Reader3[0]);
            }
            Reader3.Close();
            Connection.Close();
            lblCompleted.Text = Count.ToString();

            Connection.Open();
            SQL = "SELECT COUNT(*) FROM Task WHERE WorkerID = " + WorkerID.ToString() + " AND Status = " + ASSIGNED_TASK.ToString() + " AND (DueDate > " + StarDate.ToString() + " OR (DueDate = " + StarDate.ToString() + " AND DueTime >= " + StarTime.ToString() + "))";
            Command4 = new SQLiteCommand(SQL, Connection);
            Reader4 = Command4.ExecuteReader();
            Count = 0;
            while (Reader4.Read() == true)
            {
                Count = Convert.ToInt32(Reader4[0]);
            }
            Reader4.Close();
            Connection.Close();
            lblUpcoming.Text = Count.ToString();

            Connection.Open();
            SQL = "SELECT COUNT(*) FROM Task WHERE WorkerID = " + WorkerID.ToString() + " AND Status = " + ASSIGNED_TASK.ToString() + " AND (DueDate < " + StarDate.ToString() + " OR (DueDate = " + StarDate.ToString() + " AND DueTime < " + StarTime.ToString() + "))";
            Command5 = new SQLiteCommand(SQL, Connection);
            Reader5 = Command5.ExecuteReader();
            Count = 0;
            while (Reader5.Read() == true)
            {
                Count = Convert.ToInt32(Reader5[0]);
            }
            Reader5.Close();
            Connection.Close();
            lblOverdue.Text = Count.ToString();
        }
    }
}