using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace DbProviderFactory_HomeTask
{
    public partial class MainWindow : Window
    {
        DbConnection? connection = null;
        DbProviderFactory? providerFactory = null;
        IConfigurationRoot? configuration = null;
        string providerName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            Configuration();
        }

        public void Configuration()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", typeof(System.Data.SqlClient.SqlClientFactory));
            DbProviderFactories.RegisterFactory("System.Data.OleDb", typeof(OleDbFactory));

            configuration = new ConfigurationBuilder().AddJsonFile("appconfig.json").Build();

            DataTable dataTable = DbProviderFactories.GetFactoryClasses();
            providerNameComboBox.Items.Clear();

            foreach (DataRow row in dataTable.Rows)
            {
                providerNameComboBox.Items.Add(row["InvariantName"]).ToString();
            }


        }
        private void providerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (providerNameComboBox.SelectedItem != null)
            {
                providerName = providerNameComboBox.SelectedItem.ToString()!;
                var connectionString = configuration.GetConnectionString(providerName);
                providerFactory = DbProviderFactories.GetFactory(providerName);
                connection = providerFactory.CreateConnection();
                connection!.ConnectionString = connectionString;
            }

        }

        public ListView ExecuteQuery()
        {
            using var command = connection!.CreateCommand();
            command.CommandText = queryTxtBox.Text;

            var adapter = providerFactory!.CreateDataAdapter();
            adapter!.SelectCommand = command;

            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            ListView listView = new ListView();
            GridView gridView = new GridView();

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = dataColumn.ColumnName,
                    DisplayMemberBinding = new Binding(dataColumn.ColumnName)
                });
            }
            listView.View = gridView;
            listView.ItemsSource = dataTable.DefaultView;

            return listView;

        }
        private void execButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(queryTxtBox.Text) || string.IsNullOrWhiteSpace(providerNameComboBox.Text))
            {
                return;
            }
            TabItem tabItem = new TabItem();
            tabItem.Header = "New Tab";
            tabItem.Content = ExecuteQuery();
            tabControl.Items.Add(tabItem);
            tabControl.SelectedItem = tabItem;

        }
    }
}
