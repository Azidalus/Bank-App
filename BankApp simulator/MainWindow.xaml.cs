using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Windows.Media.Effects;

namespace Финал
{
    public partial class MainWindow : Window
    {
        ApplicationViewModel avm = new ApplicationViewModel();
        int counter = 1;
        bool selectedType; // Глобальный флажок для определения типа созданного счета
        char[] array = { ' ', '₽' };

        SqlConnection connection = null;
        string connectionString;
        SqlDataAdapter adapter;
        DataTable accountsTable;
        DataTable usersTable;
        int currentUser;
        bool banker = false;
        bool superPass = false;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public class ApplicationViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<Account> Accounts { get; set; }

            public ApplicationViewModel()
            {
                Accounts = new ObservableCollection<Account> { };
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged([CallerMemberName] string prop = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            accountsTable = new DataTable();
            usersTable = new DataTable();
            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand("select * from Accounts", connection);
            adapter = new SqlDataAdapter(command);

            connection.Open();
            adapter.Fill(accountsTable);
            AccountsGrid.ItemsSource = accountsTable.DefaultView;
            SqlCommand command1 = new SqlCommand("select * from Users", connection);
            adapter = new SqlDataAdapter(command1);
            adapter.Fill(usersTable);
            UsersGrid.ItemsSource = usersTable.DefaultView;
            connection.Close();
        }

        // Вход в меню из формы авторизации
        private void GoToMainMenu_Click(object sender, RoutedEventArgs e) 
        {
            bool correctPass = false;
            bool correctLog = false;           
            
            foreach (DataRow row in usersTable.Rows)
            {
                if (login.Text.Equals(row["User_Login"]))
                {
                    correctLog = true;
                    if (password.Password.Equals(row["User_Password"]))
                    {
                        if (Convert.ToInt32(row["SuperPass"]) == 1)
                        {
                            superPass = true;
                            break;
                        }
                        currentUser = Convert.ToInt32(row["User_UID"]);
                        correctPass = true;
                        break;
                    }
                    else
                    {
                        AbovePassword.Text = "Неверный пароль";
                        AbovePassword.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    }
                }
            }
            if (!correctLog)
            {
                AboveLogin.Text = "Неверный логин";
                AboveLogin.Foreground = new SolidColorBrush(Colors.Red);
            }

            if (superPass)
            {
                banker = true;
                foreach (DataRow row in usersTable.Rows)
                {
                    if (Convert.ToInt32(row["SuperPass"]) == 0)
                    {
                        Button b = new Button();
                        b.Margin = new Thickness(14, 11, 3, 11);
                        b.BorderThickness = new Thickness(0);
                        b.Background = new SolidColorBrush(Colors.White);
                        DropShadowEffect dropshadow = new DropShadowEffect();
                        dropshadow.BlurRadius = 10;
                        dropshadow.Direction = 270;
                        dropshadow.Opacity = 0.2;
                        dropshadow.Color = Colors.Gray;
                        b.Effect = dropshadow;

                        ColumnDefinition c1 = new ColumnDefinition();
                        ColumnDefinition c2 = new ColumnDefinition();
                        c1.Width = new GridLength(132);
                        RowDefinition r1 = new RowDefinition();
                        Grid g = new Grid();
                        g.Height = 130;
                        g.Width = 194;
                        g.Margin = new Thickness(0, 4, 0, 4);
                        g.ColumnDefinitions.Add(c1);
                        g.ColumnDefinitions.Add(c2);
                        g.RowDefinitions.Add(r1);

                        // КЛИЕНТ
                        TextBlock client = new TextBlock();
                        client.Text = "КЛИЕНТ ";
                        client.FontSize = 20;
                        client.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                        client.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
                        client.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                        client.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                        g.Children.Add(client);
                        Grid.SetColumn(client, 0);

                        // id
                        TextBlock clientNum = new TextBlock();
                        clientNum.Text = row["User_UID"].ToString();
                        clientNum.FontSize = 20;
                        clientNum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                        clientNum.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
                        clientNum.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                        clientNum.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                        g.Children.Add(clientNum);
                        Grid.SetColumn(clientNum, 1);

                        b.Content = g;
                        BankerTheWrapPanel.Children.Add(b);
                       
                        b.Click += (s, ev) => ClientScores(Convert.ToInt32(clientNum.Text));

                        Authorization_Form.Visibility = Visibility.Collapsed;
                        BankerMainMenu.Visibility = Visibility.Visible;
                    }
                }
            }

            if (correctPass && correctLog)
            {
                foreach (DataRow row in accountsTable.Rows)
                {
                    if (row["User_UID"].Equals(currentUser))
                    {
                        Button b = new Button();
                        b.Margin = new Thickness(14, 11, 3, 11);
                        b.BorderThickness = new Thickness(0);
                        b.Background = new SolidColorBrush(Colors.White);
                        DropShadowEffect dropshadow = new DropShadowEffect();
                        dropshadow.BlurRadius = 10;
                        dropshadow.Direction = 270;
                        dropshadow.Opacity = 0.2;
                        dropshadow.Color = Colors.Gray;
                        b.Effect = dropshadow;                       

                        ColumnDefinition c1 = new ColumnDefinition();
                        ColumnDefinition c2 = new ColumnDefinition();
                        ColumnDefinition c3 = new ColumnDefinition();
                        c1.Width = new GridLength(15);
                        c2.Width = new GridLength(73);
                        RowDefinition r1 = new RowDefinition();
                        RowDefinition r2 = new RowDefinition();
                        RowDefinition r3 = new RowDefinition();
                        r1.Height = new GridLength(34);
                        r2.Height = new GridLength(52);
                        Grid g = new Grid();
                        g.Height = 132.5;
                        g.Width = 189;
                        g.ColumnDefinitions.Add(c1);
                        g.ColumnDefinitions.Add(c2);
                        g.ColumnDefinitions.Add(c3);
                        g.RowDefinitions.Add(r1);
                        g.RowDefinitions.Add(r2);
                        g.RowDefinitions.Add(r3);

                        // Рамочка
                        Border border = new Border();
                        border.BorderBrush = new SolidColorBrush(Colors.Gainsboro);
                        border.BorderThickness = new Thickness(1);
                        g.Children.Add(border);
                        Grid.SetColumnSpan(border, 3);
                        Grid.SetRowSpan(border, 3);

                        // СЧЕТ №
                        TextBlock score = new TextBlock();
                        score.Text = "СЧЕТ № ";
                        score.FontSize = 18;
                        score.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                        score.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                        score.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                        g.Children.Add(score);
                        Grid.SetColumn(score, 1);
                        Grid.SetRow(score, 0);

                        // Тип счета
                        TextBlock accType = new TextBlock();
                        accType.FontSize = 14;
                        accType.Foreground = new SolidColorBrush(Colors.Gray);
                        accType.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                        g.Children.Add(accType);
                        Grid.SetColumn(accType, 1);
                        Grid.SetRow(accType, 1);
                        Grid.SetColumnSpan(accType, 2);

                        // id
                        TextBlock accNum = new TextBlock();
                        accNum.Text = row["Account_UID"].ToString();
                        accNum.FontSize = 18;
                        accNum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                        accNum.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                        accNum.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                        g.Children.Add(accNum);
                        Grid.SetColumn(accNum, 2);
                        Grid.SetRow(accNum, 0);

                        // Сумма на счете
                        TextBlock accSum = new TextBlock();
                        accSum.FontSize = 30;
                        accSum.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                        accSum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                        g.Children.Add(accSum);
                        Grid.SetColumn(accSum, 1);
                        Grid.SetRow(accSum, 2);
                        Grid.SetColumnSpan(accSum, 2);

                        TheWrapPanel.DataContext = avm.Accounts;

                        b.Content = g;
                        TheWrapPanel.Children.Add(b);
                        Account account = new Account();
                        Binding bindSum = new Binding("Sum");
                        bindSum.Source = account;
                        accSum.SetBinding(TextBlock.TextProperty, bindSum);
                        Binding bindType = new Binding("Type");
                        bindType.Source = account;
                        accType.SetBinding(TextBlock.TextProperty, bindType);
                        accType.SetBinding(TextBlock.DataContextProperty, bindType);
                        account.Sum = row["Acc_Sum"].ToString() + " ₽";
                        account.Id = Convert.ToInt32(row["Account_UID"]);

                        avm.Accounts.Add(account);

                        accType.Text = row["Acc_Type"].ToString();
                        account.Type = row["Acc_Type"].ToString();
                        if (accType.Text.Equals("ДО ВОСТРЕБОВАНИЯ"))
                        {
                            b.Click += (s, ev) => NewAccount(accNum, accType, accSum, account.Days_Count);
                        }
                        else
                        {
                            account.Days_Count = 0;
                            b.Click += (s, ev) => NewAccount(accNum, accType, accSum, account.Days_Count);
                        }
                        accType.DataContextChanged += (s, ev) => Delete(b);
                        counter = Convert.ToInt32(row["Account_UID"]) + 1;
                    }
                }

                Binding bindWrapPanel = new Binding("Accounts");
                bindWrapPanel.Source = avm.Accounts;
                TheWrapPanel.SetBinding(WrapPanel.DataContextProperty, bindWrapPanel);

                Authorization_Form.Visibility = Visibility.Collapsed;
                MainMenu.Visibility = Visibility.Visible;
                BackToBankerMainMenu.Visibility = Visibility.Collapsed;
            }
        }

        void ClientScores(int client_id)
        {
            foreach (DataRow row in accountsTable.Rows)
            {
                if (Convert.ToInt32(row["User_UID"]) == client_id)
                {
                    Button b = new Button();
                    b.Margin = new Thickness(14, 11, 3, 11);
                    b.BorderThickness = new Thickness(0);
                    b.Background = new SolidColorBrush(Colors.White);
                    DropShadowEffect dropshadow = new DropShadowEffect();
                    dropshadow.Direction = 270;
                    dropshadow.BlurRadius = 10;
                    dropshadow.Opacity = 0.2;
                    dropshadow.Color = Colors.Gray;
                    b.Effect = dropshadow;

                    ColumnDefinition c1 = new ColumnDefinition();
                    ColumnDefinition c2 = new ColumnDefinition();
                    ColumnDefinition c3 = new ColumnDefinition();
                    c1.Width = new GridLength(15);
                    c2.Width = new GridLength(73);
                    RowDefinition r1 = new RowDefinition();
                    RowDefinition r2 = new RowDefinition();
                    RowDefinition r3 = new RowDefinition();
                    r1.Height = new GridLength(34);
                    r2.Height = new GridLength(52);
                    Grid g = new Grid();
                    g.Height = 132.5;
                    g.Width = 189;
                    g.ColumnDefinitions.Add(c1);
                    g.ColumnDefinitions.Add(c2);
                    g.ColumnDefinitions.Add(c3);
                    g.RowDefinitions.Add(r1);
                    g.RowDefinitions.Add(r2);
                    g.RowDefinitions.Add(r3);

                    // Рамочка
                    Border border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.Gainsboro);
                    border.BorderThickness = new Thickness(1);
                    g.Children.Add(border);
                    Grid.SetColumnSpan(border, 3);
                    Grid.SetRowSpan(border, 3);

                    // СЧЕТ №
                    TextBlock score = new TextBlock();
                    score.Text = "СЧЕТ № ";
                    score.FontSize = 18;
                    score.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                    score.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                    score.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                    g.Children.Add(score);
                    Grid.SetColumn(score, 1);
                    Grid.SetRow(score, 0);

                    // Тип счета
                    TextBlock accType = new TextBlock();
                    accType.FontSize = 14;
                    accType.Text = row["Acc_Type"].ToString();
                    accType.Foreground = new SolidColorBrush(Colors.Gray);
                    accType.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    g.Children.Add(accType);
                    Grid.SetColumn(accType, 1);
                    Grid.SetRow(accType, 1);
                    Grid.SetColumnSpan(accType, 2);

                    // id
                    TextBlock accNum = new TextBlock();
                    accNum.Text = row["Account_UID"].ToString();
                    accNum.FontSize = 18;
                    accNum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                    accNum.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                    accNum.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                    g.Children.Add(accNum);
                    Grid.SetColumn(accNum, 2);
                    Grid.SetRow(accNum, 0);

                    // Сумма на счете
                    TextBlock accSum = new TextBlock();
                    accSum.FontSize = 30;
                    accSum.Text = row["Acc_Sum"].ToString() + " ₽";
                    accSum.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                    accSum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                    g.Children.Add(accSum);
                    Grid.SetColumn(accSum, 1);
                    Grid.SetRow(accSum, 2);
                    Grid.SetColumnSpan(accSum, 2);

                    b.Content = g;
                    TheWrapPanel.Children.Add(b);

                    counter = Convert.ToInt32(row["Account_UID"]) + 1;
                }
                else
                    counter = 1;
            }
            
            currentUser = client_id;

            BankerMainMenu.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
            restart.Visibility = Visibility.Collapsed;
            BackToBankerMainMenu.Visibility = Visibility.Visible;
        }

        // Добавление нового счета в главное меню 
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!NewSum.Text.Equals(""))
            {
                Button b = new Button();
                b.Margin = new Thickness(14, 11, 3, 11);
                b.BorderThickness = new Thickness(0);
                b.Background = new SolidColorBrush(Colors.White);
                DropShadowEffect dropshadow = new DropShadowEffect();
                dropshadow.Direction = 270;
                dropshadow.BlurRadius = 10;
                dropshadow.Opacity = 0.2;
                dropshadow.Color = Colors.Gray;
                b.Effect = dropshadow;

                ColumnDefinition c1 = new ColumnDefinition();
                ColumnDefinition c2 = new ColumnDefinition();
                ColumnDefinition c3 = new ColumnDefinition();
                c1.Width = new GridLength(15);
                c2.Width = new GridLength(73);
                RowDefinition r1 = new RowDefinition();
                RowDefinition r2 = new RowDefinition();
                RowDefinition r3 = new RowDefinition();
                r1.Height = new GridLength(34);
                r2.Height = new GridLength(52);
                Grid g = new Grid();
                g.Height = 132.5;
                g.Width = 189;
                g.ColumnDefinitions.Add(c1);
                g.ColumnDefinitions.Add(c2);
                g.ColumnDefinitions.Add(c3);
                g.RowDefinitions.Add(r1);
                g.RowDefinitions.Add(r2);
                g.RowDefinitions.Add(r3);

                // Рамочка
                Border border = new Border();
                border.BorderBrush = new SolidColorBrush(Colors.Gainsboro);
                border.BorderThickness = new Thickness(1);
                g.Children.Add(border);
                Grid.SetColumnSpan(border, 3);
                Grid.SetRowSpan(border, 3);

                // СЧЕТ №
                TextBlock score = new TextBlock();
                score.Text = "СЧЕТ № ";
                score.FontSize = 18;
                score.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                score.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                score.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                g.Children.Add(score);
                Grid.SetColumn(score, 1);
                Grid.SetRow(score, 0);

                // Тип счета
                TextBlock accType = new TextBlock();
                accType.FontSize = 14;
                accType.Foreground = new SolidColorBrush(Colors.Gray);
                accType.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                g.Children.Add(accType);
                Grid.SetColumn(accType, 1);
                Grid.SetRow(accType, 1);
                Grid.SetColumnSpan(accType, 2);

                // id
                TextBlock accNum = new TextBlock();
                accNum.Text = counter.ToString();
                accNum.FontSize = 18;
                accNum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                accNum.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
                accNum.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                g.Children.Add(accNum);
                Grid.SetColumn(accNum, 2);
                Grid.SetRow(accNum, 0);

                // Сумма на счете
                TextBlock accSum = new TextBlock();
                accSum.FontSize = 30;
                accSum.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                accSum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E3E"));
                g.Children.Add(accSum);
                Grid.SetColumn(accSum, 1);
                Grid.SetRow(accSum, 2);
                Grid.SetColumnSpan(accSum, 2);

                TheWrapPanel.DataContext = avm.Accounts;

                b.Content = g;
                TheWrapPanel.Children.Add(b);
                Account account = new Account();
                Binding bindSum = new Binding("Sum");
                bindSum.Source = account;
                accSum.SetBinding(TextBlock.TextProperty, bindSum);
                Binding bindType = new Binding("Type");
                bindType.Source = account;
                accType.SetBinding(TextBlock.TextProperty, bindType);
                accType.SetBinding(TextBlock.DataContextProperty, bindType);
                account.Sum = NewSum.Text + " ₽";
                account.Id = counter;

                avm.Accounts.Add(account);
                counter++;

                if (!selectedType)
                {
                    account.Type = "ДО ВОСТРЕБОВАНИЯ";
                    b.Click += (s, ev) => NewAccount(accNum, accType, accSum, account.Days_Count);
                }
                else
                {
                    account.Type = "ДЕПОЗИТ";
                    account.Days_Count = 0;
                    b.Click += (s, ev) => NewAccount(accNum, accType, accSum, account.Days_Count);
                }
                accType.DataContextChanged += (s, ev) => Delete(b);

                foreach (Account a in avm.Accounts)
                {
                    a.Days_Count++;
                    Percentage(a);
                }

                // Добавление счета в бд
                SqlCommand insert = new SqlCommand();
                insert.CommandType = CommandType.Text;
                string sum_for_db = account.Sum.Trim(array);
                insert.CommandText = "INSERT INTO Accounts (User_UID, Account_UID, Acc_Type, Acc_Sum)" +
                    "VALUES (" + currentUser + ", " + account.Id + ", '" + account.Type + "', " + sum_for_db.Replace(',', '.') + ")";
                connection = new SqlConnection(connectionString);
                insert.Connection = connection;
                connection.Open();
                insert.ExecuteNonQuery();
                connection.Close();

                //DataRow dataRow = accountsTable.NewRow();
                //dataRow["User_UID"] = currentUser;
                //dataRow["Account_UID"] = account.Id;
                //dataRow["Acc_Type"] = account.Type;
                //dataRow["Acc_Sum"] = account.Sum.Trim(array);
                //accountsTable.Rows.Add(dataRow);
            }

            NewSum.Text = "";
            AddNewAcc_Form.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
            if(banker)
                BackToBankerMainMenu.Visibility = Visibility.Visible;
        }

        void Delete(Button button)
        {
            button.Visibility = Visibility.Collapsed;
            TheWrapPanel.Children.Remove(button);
        }

        // Заполнение формы действий со счетом для конкретного счета
        void NewAccount(TextBlock aN, TextBlock aT, TextBlock aS, int days)
        {
            if (!superPass)
            {
                AccForm_AccNumber.Text = aN.Text;
                AccForm_AccSum.Text = aS.Text;
                AccForm_AccType.Text = aT.Text;
                if (aT.Text.Equals("ДЕПОЗИТ") && days < 30)
                {
                    WithdrawMoney.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDBDBD"));
                    PutMoney.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDBDBD"));
                    WithdrawMoney.IsEnabled = false;
                    PutMoney.IsEnabled = false;
                    WithdrawMoney.Effect = null;
                    PutMoney.Effect = null;
                }
                else
                {
                    WithdrawMoney.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF757575"));
                    PutMoney.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF757575"));
                    WithdrawMoney.IsEnabled = true;
                    PutMoney.IsEnabled = true;
                    DropShadowEffect dropshadow = new DropShadowEffect();
                    dropshadow.BlurRadius = 10;
                    dropshadow.Direction = 270;
                    dropshadow.Opacity = 0.35;
                    dropshadow.Color = Colors.Gray;
                    WithdrawMoney.Effect = dropshadow;
                    PutMoney.Effect = dropshadow;
                }
                MainMenu.Visibility = Visibility.Collapsed;
                BackToBankerMainMenu.Visibility = Visibility.Collapsed;
                NewAccount_Form.Visibility = Visibility.Visible;
                BackToMenu.Visibility = Visibility.Visible;
            }
        }

        private void AddNewAcc_Click(object sender, EventArgs e) // Вход в форму создания нового счета
        {
            MainMenu.Visibility = Visibility.Collapsed;
            BackToBankerMainMenu.Visibility = Visibility.Collapsed;
            AddNewAcc_Form.Visibility = Visibility.Visible;
        }

        private void Deposit_Click(object sender, RoutedEventArgs e)    //
        {                                                               //
            selectedType = true;                                        //
        }                                                               //
                                                                        // Выбор типа счета 
        private void DoVostreb_Click(object sender, RoutedEventArgs e)  //
        {                                                               //
            selectedType = false;                                       //
        }                                                               //

        private void BackToMenu_Click(object sender, RoutedEventArgs e) // Выход в меню из формы действий со счетом
        {
            AccForm_AccSum.Text = "";
            NewAccount_Form.Visibility = Visibility.Collapsed;
            BackToMenu.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
            if(banker)
                BackToBankerMainMenu.Visibility = Visibility.Visible;
        }

        private void WithdrawMoney_Click(object sender, RoutedEventArgs e) // Снять деньги со счета
        {
            secret_id.Text = AccForm_AccNumber.Text;
            NewAccount_Form.Visibility = Visibility.Collapsed;
            BackToMenu.Visibility = Visibility.Collapsed;
            WithdrawingMoney_Form.Visibility = Visibility.Visible;
        }

        private void PutMoney_Click(object sender, RoutedEventArgs e) // Положить деньги на счет
        {
            secret_id.Text = AccForm_AccNumber.Text;
            NewAccount_Form.Visibility = Visibility.Collapsed;
            BackToMenu.Visibility = Visibility.Collapsed;
            PuttingMoney_Form.Visibility = Visibility.Visible;
        }

        private void DeleteAcc_Click(object sender, RoutedEventArgs e) // Удалить счет
        {
            foreach (Account a in avm.Accounts)
            {
                a.Days_Count++;
                if (a.Id.ToString().Equals(AccForm_AccNumber.Text))
                {
                    SqlCommand delete = new SqlCommand();
                    delete.CommandType = CommandType.Text;
                    string sum_for_db0 = a.Sum.Trim(array);
                    delete.CommandText = "DELETE FROM Accounts WHERE Account_UID = " + a.Id + "AND User_UID = " + currentUser;
                    connection = new SqlConnection(connectionString);
                    delete.Connection = connection;
                    connection.Open();
                    delete.ExecuteNonQuery();
                    connection.Close();

                    a.Id = -1;
                    a.Type = "";
                }
                Percentage(a);
            }
            NewAccount_Form.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }

        private void BackToAccForm_Click(object sender, RoutedEventArgs e) // Выход из формы пополнения счета
        {
            enterSum.Clear();
            PuttingMoney_Form.Visibility = Visibility.Collapsed;
            NewAccount_Form.Visibility = Visibility.Visible;
            BackToMenu.Visibility = Visibility.Visible;
        }

        private void BackToAccForm2_Click(object sender, RoutedEventArgs e) // Выход из формы снятия со счета
        {
            enterSum2.Clear();
            WithdrawingMoney_Form.Visibility = Visibility.Collapsed;
            NewAccount_Form.Visibility = Visibility.Visible;
            BackToMenu.Visibility = Visibility.Visible;
        }

        private void Withdraw_and_Back_Click(object sender, RoutedEventArgs e) // Снять деньги и выйти из формы снятия со счета 
        {
            foreach (Account a in avm.Accounts)
            {
                a.Days_Count++;
                Percentage(a);
                if (a.Id.ToString().Equals(secret_id.Text))
                {
                    a.Sum = Math.Round(Convert.ToDecimal(a.Sum.Trim(array)) - Convert.ToDecimal(enterSum2.Text), 3).ToString() + " ₽";
                    AccForm_AccSum.Text = a.Sum.ToString();

                    SqlCommand update = new SqlCommand();
                    update.CommandType = CommandType.Text;
                    string sum_for_db0 = a.Sum.Trim(array);
                    update.CommandText = "UPDATE Accounts SET Acc_Sum = " + sum_for_db0.Replace(',', '.') +
                        "WHERE Account_UID = " + a.Id + "AND User_UID = " + currentUser;
                    connection = new SqlConnection(connectionString);
                    update.Connection = connection;
                    connection.Open();
                    update.ExecuteNonQuery();
                    connection.Close();
                }              
            }
            enterSum2.Clear();
            WithdrawingMoney_Form.Visibility = Visibility.Collapsed;
            NewAccount_Form.Visibility = Visibility.Visible;
            BackToMenu.Visibility = Visibility.Visible;
        }

        private void Put_and_Back_Click(object sender, RoutedEventArgs e) // Положить деньги и выйти из формы пополнения счета 
        {
            foreach (Account a in avm.Accounts)
            {
                a.Days_Count++;
                Percentage(a);
                if (a.Id.ToString().Equals(secret_id.Text))
                {
                    a.Sum = Math.Round(Convert.ToDecimal(enterSum.Text) + Convert.ToDecimal(a.Sum.Trim(array)), 3).ToString() + " ₽";
                    AccForm_AccSum.Text = a.Sum.ToString();

                    SqlCommand update = new SqlCommand();
                    update.CommandType = CommandType.Text;
                    string sum_for_db0 = a.Sum.Trim(array);
                    update.CommandText = "UPDATE Accounts SET Acc_Sum = " + sum_for_db0.Replace(',', '.') +
                        "WHERE Account_UID = " + a.Id + "AND User_UID = " + currentUser;
                    connection = new SqlConnection(connectionString);
                    update.Connection = connection;
                    connection.Open();
                    update.ExecuteNonQuery();
                    connection.Close();
                }               
            }
            enterSum.Clear();
            PuttingMoney_Form.Visibility = Visibility.Collapsed;
            NewAccount_Form.Visibility = Visibility.Visible;
            BackToMenu.Visibility = Visibility.Visible;
        }

        private void restart_Click(object sender, RoutedEventArgs e) // Обновление дней
        {
            foreach (Account a in avm.Accounts)
            {
                a.Days_Count++;
                Percentage(a);
            }
        }

        void Percentage(Account a)
        {
            if (a.Type.Equals("ДО ВОСТРЕБОВАНИЯ"))
            {
                a.Sum = Math.Round(1.01m * Convert.ToDecimal(a.Sum.Trim(array)), 3).ToString() + " ₽";

                SqlCommand update = new SqlCommand();
                update.CommandType = CommandType.Text;
                string sum_for_db0 = a.Sum.Trim(array);
                update.CommandText = "UPDATE Accounts SET Acc_Sum = " + sum_for_db0.Replace(',', '.') +
                    "WHERE Account_UID = " + a.Id + "AND Acc_Type = 'ДО ВОСТРЕБОВАНИЯ'" + "AND User_UID = " + currentUser;
                connection = new SqlConnection(connectionString);
                update.Connection = connection;
                connection.Open();
                update.ExecuteNonQuery();
                connection.Close();
            }
            if (a.Type.Equals("ДЕПОЗИТ") && a.Days_Count % 30 == 0)
            {
                a.Sum = Math.Round(1.4m * Convert.ToDecimal(a.Sum.Trim(array)), 3).ToString() + " ₽";

                SqlCommand update = new SqlCommand();
                update.CommandType = CommandType.Text;
                string sum_for_db0 = a.Sum.Trim(array);
                update.CommandText = "UPDATE Accounts SET Acc_Sum = " + sum_for_db0.Replace(',', '.') +
                    "WHERE Account_UID = " + a.Id + "AND Acc_Type = 'ДЕПОЗИТ'" + "AND User_UID = " + currentUser;
                connection = new SqlConnection(connectionString);
                update.Connection = connection;
                connection.Open();
                update.ExecuteNonQuery();
                connection.Close();
            }          
        }

        private void BackToBankerMainMenu_Click(object sender, RoutedEventArgs e)
        {
            var children = TheWrapPanel.Children.OfType<UIElement>().ToList();
            foreach (UIElement child in children)
            {               
                if(child != AddNewAcc)
                    TheWrapPanel.Children.Remove(child);
            }
            //TheWrapPanel.Children.Clear();
            MainMenu.Visibility = Visibility.Collapsed;
            BackToBankerMainMenu.Visibility = Visibility.Collapsed;
            BankerMainMenu.Visibility = Visibility.Visible;
        }
    }
}
