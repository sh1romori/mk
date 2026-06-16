using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;

namespace Api232
{
    public partial class MainWindow : Window
    {
        private string[] chek = { "+", "!" };
        private int count = 0;
        private string[] statusResponse = new string[2]; // Инициализируем массив для хранения результатов

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RequestApi_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://127.0.0.1:4444/TransferSimulator/fullName";
            try
            {
                var httpClient = new HttpClient();
                var jsonResponse = await httpClient.GetStringAsync(url);
                var jsonObject = JObject.Parse(jsonResponse);

                TBApi.Text = (string?)jsonObject["value"];
                BtnResult.Content = $"Проверить вхождение символа {chek[count]}";
                TBAnswer.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}");
            }
        }

        private void EditWord()
        {
            string filePath = @"C:\Users\sen\Desktop\ТестКейс.docx";
            try
            {
                var app = new Word.Application();
                var doc = app.Documents.Open(filePath);

                // Получаем первую таблицу в документе
                var tbl = doc.Tables[1];

                // Добавляем данные в таблицу
                tbl.Rows[3].Cells[3].Range.Text = statusResponse[0];
                tbl.Rows[4].Cells[3].Range.Text = statusResponse[1];

                // Сохраняем и закрываем документ
                doc.Save();
                doc.Close();
                app.Quit();

                // Освобождаем COM-ресурсы
                Marshal.ReleaseComObject(doc);
                Marshal.ReleaseComObject(app);

                MessageBox.Show("ТестКейс.docx успешно заполнен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnResult_Click(object sender, RoutedEventArgs e)
        {
            string currentCheck = chek[count];
            TBAnswer.Text = TBApi.Text.Contains(currentCheck)
                 ? $"ФИО содержит запрещенный символ {currentCheck}"
                 : $"ФИО не содержит запрещенный символ {currentCheck}";

            statusResponse[count] = TBApi.Text.Contains(currentCheck) ? "Не успешно" : "Успешно";

            if (count == 1) // Последний шаг
            {
                EditWord();
                this.Close();
            }
            count++; // Увеличиваем счётчик только после обработки
        }
    }
}