using System.Drawing;
using System.Drawing.Imaging;

namespace CSharp_Image_Manipulator
{
    public static class ImageFilters
    {
        // Фильтр оттенков серого
        public static Bitmap ApplyGrayscaleFilter(Bitmap bitmap, int startX, int startY, int width, int height)
        {
            Bitmap result = new Bitmap(width, height); // Создание нового Bitmap для результата
            using (Graphics g = Graphics.FromImage(result)) // Создание объекта Graphics для рисования результата
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {.3f, .3f, .3f, 0, 0}, // Коэффициенты для красного канала
                        new float[] {.59f, .59f, .59f, 0, 0}, // Коэффициенты для зелёного канала
                        new float[] {.11f, .11f, .11f, 0, 0}, // Коэффициенты для синего канала
                        new float[] {0, 0, 0, 1, 0}, // Альфа-канал (без изменений)
                        new float[] {0, 0, 0, 0, 1} // Смещение
                    });
                ImageAttributes attributes = new ImageAttributes(); // Создание атрибутов для применения матрицы
                attributes.SetColorMatrix(colorMatrix); // Установка матрицы преобразования цветов
                g.DrawImage(bitmap, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attributes); // Применение фильтра
            }
            return result; // Возврат обработанного изображения
        }//выделить модули для фильтров

        // Фильтр инверсии цветов
        public static Bitmap ApplyInvertFilter(Bitmap bitmap, int startX, int startY, int width, int height)
        {
            Bitmap result = new Bitmap(width, height); // Создание нового Bitmap для результата
            using (Graphics g = Graphics.FromImage(result)) // Создание объекта Graphics для рисования результата
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {-1, 0, 0, 0, 0}, // Инверсия красного канала
                        new float[] {0, -1, 0, 0, 0}, // Инверсия зелёного канала
                        new float[] {0, 0, -1, 0, 0}, // Инверсия синего канала
                        new float[] {0, 0, 0, 1, 0}, // Альфа-канал 
                        new float[] {1, 1, 1, 0, 1} // Смещение для инверсии
                    });
                ImageAttributes attributes = new ImageAttributes(); // Создание атрибутов для применения матрицы
                attributes.SetColorMatrix(colorMatrix); // Установка матрицы преобразования цветов
                g.DrawImage(bitmap, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attributes); // Применение фильтра
            }
            return result; // Возврат обработанного изображения
        }
        // Фильтр сепии
        public static Bitmap ApplySepiaFilter(Bitmap bitmap, int startX, int startY, int width, int height)
        {
            Bitmap result = new Bitmap(width, height); // Создание нового Bitmap для результата
            using (Graphics g = Graphics.FromImage(result)) // Создание объекта Graphics для рисования результата
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {.393f, .349f, .272f, 0, 0}, // Коэффициенты для красного канала (сепия)
                        new float[] {.769f, .686f, .534f, 0, 0}, // Коэффициенты для зелёного канала (сепия)
                        new float[] {.189f, .168f, .131f, 0, 0}, // Коэффициенты для синего канала (сепия)
                        new float[] {0, 0, 0, 1, 0}, // Альфа-канал (без изменений)
                        new float[] {0, 0, 0, 0, 1} // Смещение
                    });
                ImageAttributes attributes = new ImageAttributes(); // Создание атрибутов для применения матрицы
                attributes.SetColorMatrix(colorMatrix); // Установка матрицы преобразования цветов
                g.DrawImage(bitmap, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attributes); // Применение фильтра
            }
            return result; // Возврат обработанного изображения
        }
    }
}