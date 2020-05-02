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
using System.Threading;

namespace Дз10
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TelegramClient Client { get; set; } // экземпляр обработчика
        public MainWindow()
        {
            InitializeComponent();
            Client = new TelegramClient(this);
            UserNames.ItemsSource = Client.Clients;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(IdBox.Text, out int ID); // парсит чат айди выбранного юзера из невидимого IdBox

            if (!string.IsNullOrEmpty(txtbox.Text)) // если строка не пуста, то происходит отправка сообщения и добавление в Messages
            {
                Client.bot.SendTextMessageAsync(IdBox.Text, txtbox.Text);
                Client.MessageAdd(null, ID, txtbox.Text, false, null, Telegram.Bot.Types.Enums.MessageType.Text);
                txtbox.Text = string.Empty; // если сообщение отправилось, то строка становится пустой
                Client.JsonUpload("Default", Client.Clients);
            }
        }
        private void UserNames_SelectionChanged(object sender, SelectionChangedEventArgs e) //если мы выбираем юзера в UserNames, то убирается
        {                                                                        //приветственное окно и можно писать в поле для текста
            HelloBox.Visibility = Visibility.Collapsed;
            txtbox.IsReadOnly = false;
        }
        private void InputSearch_GotFocus(object sender, RoutedEventArgs e) // GotFocus и LostFocus - используется для задания placeholder
        {
            if (InputSearch.Text == "Поиск")
            {
                InputSearch.Text = string.Empty;
            }
        }
        private void InputSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputSearch.Text))
                InputSearch.Text = "Поиск";
        }
        private void DownloadMessagesButton_Click(object sender, RoutedEventArgs e) // Кнопка для загрузки файла из сообщения(не текста)
        {
            if (MessageCollectionList.SelectedIndex != -1) // Если сообщение выбрано
            {
                if (Client.Clients[UserNames.SelectedIndex].Messages[MessageCollectionList.SelectedIndex].Downloadable)// проверка на скачиваемость
                {
                    try
                    {
                        Client.Download(Client.Clients[UserNames.SelectedIndex].Messages[MessageCollectionList.SelectedIndex].FileName,
                            Client.Clients[UserNames.SelectedIndex].Messages[MessageCollectionList.SelectedIndex].FileId,
                            Client.Clients[UserNames.SelectedIndex].Messages[MessageCollectionList.SelectedIndex].Type);
                        MessageBox.Show("Файл скачан и находится на рабочем столе!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Вы скачиваете не файл"); 
                }
                
            }
            else
            {
                MessageBox.Show("Скачивать нечего");
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Options.Visibility == Visibility.Collapsed)
            {
                Options.Visibility = Visibility.Visible;
                UserNames.Visibility = Visibility.Collapsed;
            }
            else if (UserNames.Visibility == Visibility.Collapsed)
            {
                UserNames.Visibility = Visibility.Visible;
                Options.Visibility = Visibility.Collapsed;
                DownloadTab.Visibility = Visibility.Collapsed;
                UploadTab.Visibility = Visibility.Collapsed;
            }
        }
        private void DownloadMessages_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadTab.Visibility == Visibility.Collapsed)
            {
                UploadTab.Visibility = Visibility.Collapsed;
                DownloadTab.Visibility = Visibility.Visible;
            }
            else
            {
                DownloadTab.Visibility = Visibility.Collapsed;

            }
        }
        private void UploadMessages_Click(object sender, RoutedEventArgs e)
        {
            if (UploadTab.Visibility == Visibility.Collapsed)
            {
                DownloadTab.Visibility = Visibility.Collapsed;
                UploadTab.Visibility = Visibility.Visible;
            }
            else
            {
                UploadTab.Visibility = Visibility.Collapsed;
            }
        }
        private void UploadTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UploadTextField.Text))
            {
                if (!Client.FileExists($"{UploadTextField.Text}.json"))
                {
                    Client.JsonUpload(UploadTextField.Text, Client.Clients);
                    MessageBox.Show($"Информация выгружена на рабочем столе! Имя файла - {UploadTextField.Text}.json", "Файл скачан", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Такой файл уже существует","Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Поле ввода пусто","Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DownloadTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DownloadTextField.Text))
            {
                if (Client.FileExists($"{DownloadTextField.Text}.json"))
                {
                    Client.JsonDownload(DownloadTextField.Text);
                    UserNames.ItemsSource = Client.Clients;
                    MessageBox.Show("Файл скачан!", "Файл скачан", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Такого файла не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Поле ввода пусто", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void InputSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputSearch.Text))
            {
                var temp = Client.Clients;
                UserNames.ItemsSource = temp.Where(x => x.FirstName.ToLower().Contains(InputSearch.Text.ToLower()));
            }
            else UserNames.ItemsSource = Client.Clients;
        }
    }
} 