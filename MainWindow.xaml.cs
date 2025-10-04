using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
//построить график, структура программы

using Image = System.Windows.Controls.Image;

namespace CSharp_Image_Manipulator
{
    public partial class MainWindow : Window
    {
        private BitmapImage LastBitmapData; // Последнее обработанное изображение для отображения в интерфейсе
        private int NumberOfThreads = 1; // Количество потоков для многопоточной обработки 
        private Bitmap currentBitmap; // Текущий Bitmap для обработки фильтров, хранит изображение в формате System.Drawing

        // Конструктор главного окна
        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов интерфейса, определённых в XAML
            this.LastBitmapData = new BitmapImage(); // Инициализация пустого BitmapImage для хранения последнего результата

            ThreadsLabel.Content = $"Потоков: {NumberOfThreads}"; // Установка начального значения метки числа потоков
        }

        // Обработчик изменения значения слайдера для выбора количества потоков
        private void ThreadsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            NumberOfThreads = (int)((Slider)sender).Value; // Получение значения слайдера и преобразование в целое число для числа потоков
            if (ThreadsLabel != null) // Проверка, существует ли метка ThreadsLabel
            {
                int numberThreads = 9 - NumberOfThreads; // отображаем число потоков
                ThreadsLabel.Content = $"Потоков: {numberThreads}"; // Обновление метки с числом потоков
            }
        }

        // Обработчик нажатия кнопки "Открыть" для загрузки изображения
        private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog(); // Создание диалогового окна для выбора файла
            dlg.InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // Установка начальной директории 
            dlg.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Все файлы (*.*)|*.*"; // Фильтр для выбора файлов изображений
            dlg.RestoreDirectory = true; // Восстановление последней открытой директории при следующем открытии
            if (dlg.ShowDialog() == true) // Открытие диалога и проверка, выбран ли файл
            {
                string selectedFileName = dlg.FileName; // Получение пути к выбранному файлу
                FilenameLabel.Content = selectedFileName; // Отображение имени файла в метке FilenameLabel
                try
                {
                    currentBitmap?.Dispose(); // Освобождение предыдущего Bitmap, если он существует
                    currentBitmap = new Bitmap(selectedFileName); // Загрузка изображения из файла в объект Bitmap для обработки
                    LastBitmapData = BitmapToImageSource(currentBitmap); // Конвертация Bitmap в BitmapImage для отображения 
                    Image image = new Image(); // Создание нового Image
                    image.Source = LastBitmapData; // Установка источника изображения для контрола
                    ImageDisplay.Source = LastBitmapData; // Отображение изображения в элементе ImageDisplay
                    if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
                    {
                        StatusLabel.Content = "Изображение загружено"; // Обновление статуса в интерфейсе
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); // Вывод сообщения об ошибке
                }
            }
        }

        // Асинхронный метод применения фильтра
        private async void ApplyFilterAsync(Func<Bitmap, int, int, int, int, Bitmap> filterFunc, string operationName)
        {
            if (currentBitmap == null) // Проверка, загружено ли изображение
            {
                MessageBox.Show("Изображение не загружено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); // Сообщение, если изображение не выбрано
                return;
            }
            if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
            {
                StatusLabel.Content = $"Выполняется {operationName}..."; // Установка статуса выполнения операции
            }
            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew(); // Запуск таймера для замера общего времени 
            long filterTime = 0; // Переменная для хранения времени выполнения применения фильтра
            try
            {
                Bitmap resultBitmap = await Task.Run(() =>
                {
                    var filterStopwatch = System.Diagnostics.Stopwatch.StartNew(); // Запуск таймера для замера времени фильтра
                    Bitmap result = ApplyFilterWithTiming(currentBitmap, filterFunc, NumberOfThreads); // Применение фильтра
                    filterStopwatch.Stop(); // Остановка таймера фильтра
                    filterTime = filterStopwatch.ElapsedMilliseconds; // Сохранение времени выполнения фильтра
                    return result; // Возврат результата обработки
                });
                totalStopwatch.Stop(); // Остановка общего таймера
                currentBitmap.Dispose(); // Освобождение текущего Bitmap для предотвращения утечек памяти
                currentBitmap = resultBitmap; // Обновление текущего Bitmap результатом обработки
                BitmapImage resultImage = BitmapToImageSource(resultBitmap); // Конвертация результата в BitmapImage для отображения
                LastBitmapData = resultImage; // Обновление последнего обработанного изображения
                ImageDisplay.Source = resultImage; // Отображение результата в элементе ImageDisplay
                if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
                {
                    StatusLabel.Content = $"{operationName} завершено за {filterTime} мс (общее: {totalStopwatch.ElapsedMilliseconds} мс)"; // Вывод времени фильтра и общего времени
                }
            }
            catch (Exception ex)
            {
                if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
                {
                    StatusLabel.Content = $"Ошибка при {operationName}: {ex.Message}"; // Отображение ошибки в статусе
                }
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка обработки", MessageBoxButton.OK, MessageBoxImage.Error); // Вывод сообщения об ошибке
            }
        }

        // Метод для многопоточной обработки изображения с замерами времени
        private Bitmap ApplyFilterWithTiming(Bitmap source, Func<Bitmap, int, int, int, int, Bitmap> filter, int threadCount)
        {
            int height = source.Height; // Высота исходного изображения
            int width = source.Width; // Ширина исходного изображения
            if (threadCount == 1 || height < 200) // Проверка, нужно ли использовать однопоточный режим
            {
                // Создание копии исходного изображения для безопасной обработки в однопоточном режиме
                using (Bitmap copy = new Bitmap(source))
                {
                    return filter(copy, 0, 0, width, height); // Применение фильтра ко всему изображению
                }
            }
            // Многопоточный режим
            threadCount = Math.Min(threadCount, Environment.ProcessorCount); // Ограничение числа потоков количеством ядер процессора
            threadCount = Math.Min(threadCount, height / 50); // Ограничение числа потоков, чтобы каждая часть была не меньше 50 строк
            if (threadCount <= 1) // Проверка, достаточно ли строк для многопоточности
            {
                // Создание копии для однопоточного режима, если потоков стало 1
                using (Bitmap copy = new Bitmap(source))
                {
                    return filter(copy, 0, 0, width, height); // Применение фильтра ко всему изображению
                }
            }
            int chunkHeight = height / threadCount; // Вычисление высоты части изображения для каждого потока
            var tasks = new Task<Bitmap>[threadCount]; // Массив задач для параллельной обработки
            Bitmap result = new Bitmap(width, height); // Создание результирующего Bitmap
            // Создание частей изображения
            Bitmap[] sourceChunks = new Bitmap[threadCount]; // Массив для хранения частей исходного изображения
            for (int i = 0; i < threadCount; i++)
            {
                int startY = i * chunkHeight; // Начальная строка для текущей части
                int endY = (i == threadCount - 1) ? height : startY + chunkHeight; // Конечная строка (для последнего потока — до конца изображения)
                int chunkH = endY - startY; // Высота текущей части
                sourceChunks[i] = new Bitmap(width, chunkH); // Создание Bitmap для части изображения
                using (Graphics g = Graphics.FromImage(sourceChunks[i])) // Создание объекта Graphics для рисования части
                {
                    // Создание копии исходного изображения для безопасного доступа
                    using (Bitmap sourceCopy = new Bitmap(source))
                    {
                        g.DrawImage(sourceCopy, new Rectangle(0, 0, width, chunkH), new Rectangle(0, startY, width, chunkH), GraphicsUnit.Pixel); // Копирование части изображения
                    }
                }
            }
            // Параллельное применение фильтров
            for (int i = 0; i < threadCount; i++)
            {
                int index = i; // Захват индекса для замыкания в лямбда-выражении
                tasks[index] = Task.Run(() =>
                {
                    return filter(sourceChunks[index], 0, 0, width, sourceChunks[index].Height); // Применение фильтра к части изображения
                });
            }
            Task.WaitAll(tasks); // Ожидание завершения всех задач
            // Сборка результата из обработанных частей
            using (Graphics g = Graphics.FromImage(result)) // Создание объекта Graphics для результирующего изображения
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy; // Установка режима копирования для точной передачи пикселей
                for (int i = 0; i < threadCount; i++)
                {
                    int startY = i * chunkHeight; // Начальная строка для вставки части
                    g.DrawImage(tasks[i].Result, 0, startY); // Вставка обработанной части в результат
                    sourceChunks[i].Dispose(); // Освобождение части исходного изображения
                    tasks[i].Result.Dispose(); // Освобождение обработанной части
                }
            }
            return result; // Возврат результирующего изображения
        }

        // Обработчик кнопки "Оттенки серого"
        private void GrayscaleButton_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyFilterAsync(ImageFilters.ApplyGrayscaleFilter, "Оттенки серого"); // Запуск асинхронного применения фильтра оттенков серого
        }

        // Обработчик кнопки "Инвертировать цвета"
        private void InvertColorButton_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyFilterAsync(ImageFilters.ApplyInvertFilter, "Инвертирование цветов"); // Запуск асинхронного применения фильтра инверсии
        }

        // Обработчик кнопки "Сепия"
        private void SepiaEffectButton_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyFilterAsync(ImageFilters.ApplySepiaFilter, "Эффект сепии"); // Запуск асинхронного применения фильтра сепии
        }

        // Обработчик кнопки "Исходное изображение"
        private void OriginalButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (currentBitmap == null) // Проверка, загружено ли изображение
            {
                MessageBox.Show("Изображение не загружено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); // Сообщение, если изображение не выбрано
                return;
            }
            try
            {
                // Проверка, существует ли исходный файл
                if (FilenameLabel.Content != null && File.Exists(FilenameLabel.Content.ToString()))
                {
                    currentBitmap?.Dispose(); // Освобождение текущего Bitmap
                    currentBitmap = new Bitmap(FilenameLabel.Content.ToString()); // Загрузка оригинального изображения из файла
                    LastBitmapData = BitmapToImageSource(currentBitmap); // Конвертация в BitmapImage для отображения
                    ImageDisplay.Source = LastBitmapData; // Обновление отображения в элементе ImageDisplay
                    if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
                    {
                        StatusLabel.Content = "Исходное изображение загружено"; // Обновление статуса
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оригинала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); // Вывод сообщения об ошибке
            }
        }

        // Обработчик кнопки "Сохранить"
        private void SaveImage_OnClick(object sender, RoutedEventArgs e)
        {
            if (currentBitmap == null) // Проверка, загружено ли изображение
            {
                MessageBox.Show("Изображение не загружено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); // Сообщение, если изображение не выбрано
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog(); // Создание диалогового окна для сохранения файла
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|BMP Image|*.bmp"; // Фильтр для выбора формата сохранения
            if (saveFileDialog.ShowDialog() == true) // Проверка, выбран ли путь для сохранения
            {
                try
                {
                    ImageFormat format = ImageFormat.Jpeg; // Формат по умолчанию — JPEG
                    switch (Path.GetExtension(saveFileDialog.FileName).ToLower()) // Выбор формата на основе расширения файла
                    {
                        case ".png": format = ImageFormat.Png; break; // Формат PNG
                        case ".bmp": format = ImageFormat.Bmp; break; // Формат BMP
                    }
                    currentBitmap.Save(saveFileDialog.FileName, format); // Сохранение изображения в выбранный файл
                    if (StatusLabel != null) // Проверка, существует ли метка StatusLabel
                    {
                        StatusLabel.Content = "Изображение сохранено"; // Обновление статуса
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); // Вывод сообщения об ошибке
                }
            }
        }

        // Конвертация Bitmap в BitmapImage для отображения в WPF
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream()) // Создание потока памяти для сохранения изображения
            {
                bitmap.Save(memory, ImageFormat.Png); // Сохранение Bitmap в поток в формате PNG для сохранения качества
                memory.Position = 0; // Сброс позиции потока в начало
                BitmapImage bitmapimage = new BitmapImage(); // Создание нового BitmapImage
                bitmapimage.BeginInit(); // Начало инициализации BitmapImage
                bitmapimage.StreamSource = memory; // Установка потока как источника данных
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad; // Кэширование изображения при загрузке
                bitmapimage.EndInit(); // Завершение инициализации
                return bitmapimage; // Возврат готового BitmapImage
            }
        }

        // Освобождение ресурсов при закрытии окна
        protected override void OnClosed(EventArgs e)
        {
            currentBitmap?.Dispose(); // Освобождение текущего Bitmap для предотвращения утечек памяти
            base.OnClosed(e); // Вызов базового метода закрытия окна
        }
    }
}