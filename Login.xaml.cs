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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tasks2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //  $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $ $

    public partial class MainWindow : Window
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public MainWindow()
        {
            InitializeComponent();
            txtUserName.Focus();
        }

        // ********************************************************************

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ********************************************************************

        private void cmdEnter_Click(object sender, RoutedEventArgs e)
        {
            bool Found;
            int ID, Len;
            string ConnStr, InitDir, Name, Password, UserName;
            SQLiteCommand Command;
            SQLiteConnection Connection;
            SQLiteDataReader Reader;
            Main dlg;

            InitDir = System.IO.Path.GetFullPath(".");
            Len = InitDir.Length;
            InitDir = InitDir.Substring(0, Len - 10);

            ID = 0;
            Name = "";


            ConnStr = "DataSource=" + InitDir + "\\Data\\Tasks.db;Version=3";
            Connection = new SQLiteConnection(ConnStr);
            Connection.Open();
            Command = new SQLiteCommand("SELECT * FROM Worker", Connection);
            Reader = Command.ExecuteReader();
            Found = false;
            while (Reader.Read())
            {
                ID = Convert.ToInt32(Reader[0]);
                Name = Reader[1].ToString();
                UserName = Reader[2].ToString();
                Password = Reader[3].ToString();

                if ((txtUserName.Text == UserName) && (txtPassword.Password == Password))
                {
                    Found = true;
                    break;
                }
            }

            Reader.Close();
            Connection.Close();

            if (Found == true)
            {
                dlg = new Main(ID, Name);
                dlg.ShowDialog();
                Close();
            }
            else
                lblError.Visibility = Visibility.Visible;
        }

        // ********************************************************************

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdEnter.IsEnabled = (txtUserName.Text.Length > 0);
        }
    }
}