using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot;
using System.IO;
using Newtonsoft.Json;

namespace Дз10
{
    public class TelegramClient
    {
        /// <summary>
        /// Ссылка на основное окно
        /// </summary>
        private MainWindow window;
        /// <summary>
        /// Телеграм бот
        /// </summary>
        public TelegramBotClient bot;
        /// <summary>
        /// Список клиентов
        /// </summary>
        public ObservableCollection<Client> Clients { get; set; }
        /// <summary>
        /// Конструктор обработчика входящих сообщений
        /// </summary>
        /// <param name="window">Экземпляр основного окна</param>
        /// <param name="token">Токен</param>
        public TelegramClient(MainWindow window, string token = "1086634674:AAGoyEE9i28XJDBow_lU4JMgamknvGpVbNc")
        {
            this.window = window; // присваиваем локальному полю TelegramClient.window ссылку на экземпляр класса MainWindow
            bot = new TelegramBotClient(token); // Инициализация бота
            bot.OnMessage += MessageListener; // Когда приходит сообщение, выполняется обработчик сообщения
            bot.StartReceiving(); // Старт работы бота
            Clients = new ObservableCollection<Client>(); // Инициализация списка клиентов
            JsonDownload("Default");
        }
        /// <summary>
        /// Обработчик сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            window.Dispatcher.Invoke(() =>
            {
                bool flag = false; 

                if (Clients.Count == 0)
                    Clients.Add(new Client(e.Message.From.FirstName, e.Message.From.Id)); // если клиентов нет, добавляем нового

                foreach (var item in Clients)
                {
                    if (e.Message.From.Id == item.ChatId) // для каждого клиента делаем проверку, есть ли он в списке клиентов.
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag) // если такого клиента в списке нет, то добавляем его
                {
                    Clients.Add(new Client(e.Message.From.FirstName, e.Message.From.Id));
                }

                // Обработка сообщений
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text) // если тип сообщение = текст
                {
                    MessageAdd(null, (int)e.Message.Chat.Id, e.Message.Text, false, null, Telegram.Bot.Types.Enums.MessageType.Text);
                }
                else
                {
                    switch (e.Message.Type) // если тип сообщения != текст
                    {
                        case Telegram.Bot.Types.Enums.MessageType.Photo:
                            MessageAdd(e.Message.Photo[e.Message.Photo.Length - 1].FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили фото (Вы можете это скачать)", true, null, Telegram.Bot.Types.Enums.MessageType.Photo);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Video:
                            MessageAdd(e.Message.Video.FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили видео (Вы можете это скачать)", true, null, Telegram.Bot.Types.Enums.MessageType.Video);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Voice:
                            MessageAdd(e.Message.Voice.FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили голос. соообщение (Вы можете это скачать)", true, null, Telegram.Bot.Types.Enums.MessageType.Voice);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Document:
                            MessageAdd(e.Message.Document.FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили документ (Вы можете это скачать)", true, e.Message.Document.FileName, Telegram.Bot.Types.Enums.MessageType.Document);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Sticker:
                            MessageAdd(e.Message.Sticker.FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили стикер (Вы можете это скачать)", true, null, Telegram.Bot.Types.Enums.MessageType.Sticker);
                            break;

                        default:
                            MessageAdd(e.Message.Sticker.FileId, (int)e.Message.Chat.Id,
                                $"Вам отправили неизвестный файл (Невозможно скачать)", false, null, Telegram.Bot.Types.Enums.MessageType.Sticker);
                            break;
                    }
                }
                JsonUpload("Default", Clients);
                
            });
        }
        /// <summary>
        /// Добавление сообщений в коллекцию Messages пользователя, который отправил сообщение
        /// </summary>
        /// <param name="fileid">Уникальный номер файла</param>
        /// <param name="chatId">Номер чата</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="IsDownloadable">Возможно ли скачать</param>
        /// <param name="filename">Имя файла(если это документ) у остальных null</param>
        /// <param name="type">Тип сообщения</param>
        public void MessageAdd(string fileid, int chatId, string message, bool IsDownloadable, string filename, Telegram.Bot.Types.Enums.MessageType type)
        {
            foreach (var item in Clients) // ищем клиента с указанным ChatId
            {
                if (item.ChatId == chatId) // если находим добавляем в коллекцию клиента сообщение
                {
                    item.Messages.Add(new Message(type, fileid, chatId, message, DateTime.Now.ToLongTimeString(), IsDownloadable, filename));
                }
            }
        }
        /// <summary>
        /// Проверка на наличие файла на рабочем столе
        /// </summary>
        /// <param name="name">путь файла</param>
        /// <returns></returns>
        public bool FileExists(string name)
        {
            bool result = false; // результат проверки наличия файлов true - файл существует; false - файла не существует

            // берем информацию о каталоге логического рабочего стола
            DirectoryInfo info = new DirectoryInfo($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\");
             
            var files = info.GetFiles(); // присваиваем files информацию о файлах на рабочем столе
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name == name) // если такой файл существует то возвращает true
                {
                    result = true;
                }
            }
            if (!result) // если файла не существует, то false
                return false;
            else
                return true;

        }
        /// <summary>
        /// Скачивание файла
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="fileid">Уник. номер файла</param>
        /// <param name="type">Тип файла</param>
        public async void Download(string filename, string fileid, Telegram.Bot.Types.Enums.MessageType type)
        {
            // вспомогательные переменные, чтобы сделать названия файлов уникальными
            int photo = 0;
            int video = 0;
            int voice = 0;
            int sticker = 0;
            var file = await bot.GetFileAsync(fileid); // получаем информацию о файле в file 
            switch (type) // в зависимости от типа сообщения, сохраняем на рабочий стол
            {
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    while (FileExists($"photo{photo}.png"))
                        photo++;
                    FileStream photoStream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\photo{photo}.png",
                        FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, photoStream);
                    photoStream.Close();
                    photoStream.Dispose();
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Video:
                    while (FileExists($"photo{video}.png"))
                        video++;
                    FileStream videoStream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\video{video}.mp4",
                        FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, videoStream);
                    videoStream.Close();
                    videoStream.Dispose();
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Voice:
                    while (FileExists($"voice{voice}.mp3"))
                        voice++;
                    FileStream voiceStream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\voice{voice}.mp3",
                        FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, voiceStream);
                    voiceStream.Close();
                    voiceStream.Dispose();
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Document:
                    FileStream documentStream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\1_{filename}",
                        FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, documentStream);
                    documentStream.Close();
                    documentStream.Dispose();
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Sticker:
                    while (FileExists($"sticker{sticker}.gif"))
                        sticker++;
                    FileStream stickerStream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\sticker{sticker}.gif",
                        FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, stickerStream);
                    stickerStream.Close();
                    stickerStream.Dispose();
                    break;
                
                default:
                    break;
            }
        }
        /// <summary>
        /// Выгрузка информации о клиентах в .json файл
        /// </summary>
        /// <param name="filename">Имя файла(путем является логический рабочий стол)</param>
        /// <param name="clients">Список клиентов для сериализации</param>
        public void JsonUpload(string filename, ObservableCollection<Client> clients)
        {
            string json = JsonConvert.SerializeObject(clients);
            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{filename}.json", json);
        }
        /// <summary>
        /// Загрузка информации о клиентах из .json файла
        /// </summary>
        /// <param name="filename">Имя файла без расширения(путем является логический рабочий стол)</param>
        public void JsonDownload(string filename)
        {
            try
            {
                string json = File.ReadAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{filename}.json");
                Clients = JsonConvert.DeserializeObject<ObservableCollection<Client>>(json);
            }
            catch { }
        }
        /// <summary>
        /// Определение расширения файла
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="extension">Расширение</param>
        public void ExtensionCheck(string filename, out string extension, out string extensionType)
        {
            var temp = filename.Split('.');
            extension = temp[temp.Length - 1];
            switch (extension)
            {
                case "jpg":
                    extensionType = "photo";
                    break;
                case "png":
                    extensionType = "photo";
                    break;
                case "jpeg":
                    extensionType = "photo";
                    break;
                case "gif":
                    extensionType = "photo";
                    break;
                case "mp3":
                    extensionType = "audio";
                    break;
                case "mp4":
                    extensionType = "video";
                    break;
                default:
                    extensionType = "doc";
                    break;
            }
        }
    }
}
