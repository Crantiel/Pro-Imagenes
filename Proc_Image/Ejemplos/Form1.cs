using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace PIA_PI
{
    public partial class Form1 : Form
    {
        Bitmap copia;
        private int[,] conv3x3 = new int[3, 3];
        private int factor;
        private int offset;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Reset_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = copia;
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(copia, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }

        }

        private void CalcularHistograma(Bitmap bmp, out int[] histogramR, out int[] histogramG, out int[] histogramB)
        {
            histogramR = new int[256];
            histogramG = new int[256];
            histogramB = new int[256];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    histogramR[pixelColor.R]++;
                    histogramG[pixelColor.G]++;
                    histogramB[pixelColor.B]++;
                }
            }
        }

        private void ActualizarHistogramaEnGrafico(int[] histogramR, int[] histogramG, int[] histogramB)
        {
            // Limpiar las series anteriores
            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();

            // Crear nuevas series para los canales de color
            chart1.Series.Add("Red");
            chart2.Series.Add("Green");
            chart3.Series.Add("Blue");

            // Agregar los puntos de datos del histograma a las series
            for (int i = 0; i < 256; i++)
            {
                chart1.Series["Red"].Points.AddXY(i, histogramR[i]);
                chart2.Series["Green"].Points.AddXY(i, histogramG[i]);
                chart3.Series["Blue"].Points.AddY(histogramB[i]);
            }
        }




        private void button1_Click(object sender, EventArgs e)
        {
            //Cargar la imagen

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Seleccionar imagen";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Cargar imagen
                    copia = new Bitmap(openFileDialog.FileName);
                    pictureBox1.Image = copia;
                    //Dibujar el histograma
                    // Calcular el histograma
                    int[] histogramR, histogramG, histogramB;
                    CalcularHistograma(copia, out histogramR, out histogramG, out histogramB);

                    // Mostrar el histograma en los gráficos
                    ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Guargar imagen

            if (pictureBox1.Image != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Imagenes PNG|*.png|Imagenes JPG|*.jpg|Imagenes BMP|*.bmp";
                    saveFileDialog.Title = "Guardar imagen como";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Image imagen = pictureBox1.Image;
                        imagen.Save(saveFileDialog.FileName);
                    }
                }
            }
        }

        private void Inversión_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroInvertido(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private void Gamma_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroGamma(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private void Ruido_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroRuido(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroInvertido(Image<Bgr, byte> imagen)
        {
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);
            ImageAttributes Ia = new ImageAttributes();

            ColorMatrix cmPicture = new ColorMatrix(new float[][]
            {
            new float[] {0, 0, -1, 0, 0},
            new float[] {0, -1, 0, 0, 0},
            new float[] {-1, 0, 0, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {1, 1, 1, 0, 1}
            });

            Ia.SetColorMatrix(cmPicture);
            Graphics gr = Graphics.FromImage(bmpmodificacion);
            gr.DrawImage(imgBitmap, new Rectangle(0, 0, imgBitmap.Width, imgBitmap.Height), 0, 0, imgBitmap.Width, imgBitmap.Height, GraphicsUnit.Pixel, Ia);
            gr.Dispose();

            return new Image<Bgr, byte>(bmpmodificacion);
        }


        private Image<Bgr, byte> AplicarFiltroGamma(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            Color rColor = new Color();
            Color oColor = new Color();

            float r = 0;
            float g = 0;
            float b = 0;

            int n = 0;

            // Factor para gamma
            float rg = 0.2f;  // Ajusta este valor según lo necesites
            float gg = 0.5f;  // Ajusta este valor según lo necesites
            float bg = 0.9f;  // Ajusta este valor según lo necesites

            // Creamos las rampas o tablas de cada color
            int[] rGamma = new int[256];
            int[] gGamma = new int[256];
            int[] bGamma = new int[256];

            // Llenamos las tablas de gamma para cada canal
            for (n = 0; n < 256; ++n)
            {
                rGamma[n] = Math.Min(255, (int)((255.0 * Math.Pow(n / 255.0f, 1.0f / rg)) + 0.5f));
                gGamma[n] = Math.Min(255, (int)((255.0 * Math.Pow(n / 255.0f, 1.0f / gg)) + 0.5f));
                bGamma[n] = Math.Min(255, (int)((255.0 * Math.Pow(n / 255.0f, 1.0f / bg)) + 0.5f));
            }

            // Aplicamos el filtro gamma a cada píxel de la imagen
            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    // Obtenemos el color del píxel original
                    oColor = imgBitmap.GetPixel(x, y);

                    // Aplicamos el gamma a cada canal
                    r = rGamma[oColor.R];
                    g = gGamma[oColor.G];
                    b = bGamma[oColor.B];

                    // Creamos el nuevo color con los valores modificados
                    rColor = Color.FromArgb((int)r, (int)g, (int)b);

                    // Asignamos el nuevo color al píxel en la imagen resultante
                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            // Devolvemos la imagen modificada
            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private Image<Bgr, byte> AplicarFiltroRuido(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;
            int porcentaje = 15;

            int rangoMin = 85;
            int rangoMax = 255;
            float pBrillo = 15;

            Random rnd = new Random();

            Color rColor;
            Color oColor;

            int r = 0;
            int g = 0;
            int b = 0;

            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    if (rnd.Next(1, 100) <= porcentaje)
                    {
                        //Procesamos y obtenemos el nuevo color
                        rColor = Color.FromArgb(rnd.Next(rangoMin, rangoMax),
                           rnd.Next(rangoMin, rangoMax),
                           rnd.Next(rangoMin, rangoMax));

                    }
                    else
                    {
                        rColor = imgBitmap.GetPixel(x, y);
                    }

                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private void Mosaico_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroMosaico(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroMosaico(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;
            int mosaico = 36;
            int xm = 0;
            int ym = 0;

            Color rColor;
            Color oColor;

            int rs = 0;
            int gs = 0;
            int bs = 0;

            int r = 0;
            int g = 0;
            int b = 0;

            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            for (x = 0; x < imgBitmap.Width - mosaico; x += mosaico)
            {
                for (y = 0; y < imgBitmap.Height - mosaico; y += mosaico)
                {
                    rs = 0;
                    gs = 0;
                    bs = 0;

                    for (xm = x; xm < (x + mosaico); xm++)
                    {
                        for (ym = y; ym < (y + mosaico); ym++)
                        {
                            oColor = imgBitmap.GetPixel(xm, ym);
                            rs += oColor.R;
                            gs += oColor.G;
                            bs += oColor.B;
                        }

                    }
                    r = rs / (mosaico * mosaico);
                    g = gs / (mosaico * mosaico);
                    b = bs / (mosaico * mosaico);

                    rColor = Color.FromArgb(r, g, b);

                    for (xm = x; xm < (x + mosaico); xm++)
                    {
                        for (ym = y; ym < (y + mosaico); ym++)
                        {
                            bmpmodificacion.SetPixel(xm, ym, rColor);
                        }

                    }


                }


            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private void Croma_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroAberración(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroAberración(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;
            int a = 4; //tamaño de la aberración

            int r = 0;
            int g = 0;
            int b = 0;


            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    //Obtenemos el verde

                    g = imgBitmap.GetPixel(x, y).G;

                    //Obtenemos el rojo
                    if (x + a < imgBitmap.Width)
                    {
                        r = imgBitmap.GetPixel(x + a, y).R;
                    }
                    else
                    {
                        r = 0;
                    }

                    //Obtenemos el azul
                    if (x - a >= 0)
                    {
                        b = imgBitmap.GetPixel(x - a, y).B;
                    }
                    else
                    {
                        b = 0;
                    }

                    //Colocamos el color en resultante
                    bmpmodificacion.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private void Modificación_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroModificación(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroModificación(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;

            // Convertir la imagen de Emgu.CV a Bitmap para su manipulación
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            Color rColor = new Color();
            Color oColor = new Color();

            // Iterar sobre cada píxel de la imagen
            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    // Obtener el color del píxel original
                    oColor = imgBitmap.GetPixel(x, y);

                    // Modificar el color (Ejemplo: dejar solo el canal rojo)
                    rColor = Color.FromArgb(oColor.R, 0, 0);  // Cambia aquí los valores según lo que quieras modificar

                    // Establecer el nuevo color en la imagen modificada
                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            // Convertir la imagen modificada a Image<Bgr, byte> para trabajar con Emgu.CV
            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private void Homogeneidad_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox a Bitmap
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el efecto térmico
                Image<Bgr, byte> imgTermica = AplicarEfectoTermico(imgEmgu);

                // Mostrar la imagen resultante
                pictureBox1.Image = imgTermica.ToBitmap();
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarEfectoTermico(Image<Bgr, byte> imagen)
        {
            // Convertir la imagen a escala de grises
            Image<Gray, byte> imgGris = imagen.Convert<Gray, byte>();

            // Aplicar un mapa de colores térmico
            Image<Bgr, byte> imgTermica = new Image<Bgr, byte>(imagen.Width, imagen.Height);

            for (int y = 0; y < imgGris.Height; y++)
            {
                for (int x = 0; x < imgGris.Width; x++)
                {
                    byte intensidad = imgGris.Data[y, x, 0]; // Intensidad de gris
                    Bgr colorTermico = ObtenerColorTermico(intensidad);
                    imgTermica[y, x] = colorTermico;
                }
            }

            return imgTermica;
        }

        // Mapeo manual de intensidad a colores térmicos
        private Bgr ObtenerColorTermico(byte intensidad)
        {
            if (intensidad < 50) return new Bgr(0, 0, 255);       // Azul frío
            if (intensidad < 100) return new Bgr(0, 128, 255);    // Azul claro
            if (intensidad < 150) return new Bgr(0, 255, 255);    // Cian
            if (intensidad < 200) return new Bgr(0, 255, 0);      // Verde
            if (intensidad < 230) return new Bgr(255, 255, 0);    // Amarillo
            if (intensidad < 255) return new Bgr(255, 0, 0);      // Rojo caliente
            return new Bgr(255, 255, 255);                        // Blanco máximo
        }

        //private Image<Bgr, byte> AplicarFiltroHomogeneidad(Image<Bgr, byte> imagen)
        //{
        //    int x = 0;
        //    int y = 0;

        //    Bitmap imgBitmap = imagen.ToBitmap();
        //    Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

        //    int a = 0;
        //    int b = 0;
        //    int absDif = 0;
        //    int maxDif = 0;
        //    int diferencia = 0;

        //    for (x = 1; x < imgBitmap.Width - 1; x++)
        //    {
        //        for (y = 1; y < imgBitmap.Height - 1; y++)
        //        {
        //            maxDif = 0;

        //            for (a = -1; a <= 1; a++)
        //            {
        //                for (b = -1; b <= 1; b++)
        //                {
        //                    diferencia = imgBitmap.GetPixel(x, y).R - imgBitmap.GetPixel(x + a, y + b).R;
        //                    absDif = Math.Abs(diferencia);

        //                    if (absDif > maxDif)
        //                    {
        //                        maxDif = absDif;
        //                    }
        //                }

        //            }
        //            if (maxDif > 16)
        //            {
        //                maxDif = 255;
        //            }
        //            else
        //            {
        //                maxDif = 0;
        //            }
        //            bmpmodificacion.SetPixel(x, y, Color.FromArgb(maxDif, maxDif, maxDif));

        //        }

        //    }
        //    return new Image<Bgr, byte>(bmpmodificacion);
        //}

        private void Contraste_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroContraste(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroContraste(Image<Bgr, byte> imagen)
        {
            int contraste = 30;

            float c = (100.0f + contraste) / 100.0f;
            c *= c;

            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            for (int x = 0; x < imgBitmap.Width; x++)
            {
                for (int y = 0; y < imgBitmap.Height; y++)
                {
                    Color oColor = imgBitmap.GetPixel(x, y);

                    float r = ((((oColor.R / 255.0f) - 0.5f) * c) + 0.5f) * 255;
                    float g = ((((oColor.G / 255.0f) - 0.5f) * c) + 0.5f) * 255;
                    float b = ((((oColor.B / 255.0f) - 0.5f) * c) + 0.5f) * 255;

                    r = Clamp(r, 0, 255);
                    g = Clamp(g, 0, 255);
                    b = Clamp(b, 0, 255);

                    Color rColor = Color.FromArgb((int)r, (int)g, (int)b);

                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }



        //private void Gris_Click(object sender, EventArgs e)
        //{
        //    if (pictureBox1.Image != null)
        //    {
        //        // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
        //        Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
        //        Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

        //        // Aplicar el filtro de modificación de color
        //        Image<Bgr, byte> imgModificada = AplicarFiltroGris(imgEmgu);

        //        // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
        //        pictureBox1.Image = imgModificada.ToBitmap();

        //        // Calcular el histograma
        //        int[] histogramR, histogramG, histogramB;
        //        CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

        //        // Mostrar el histograma en los gráficos
        //        ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
        //    }
        //    else
        //    {
        //        MessageBox.Show("No se ha cargado ninguna imagen.");
        //    }
        //}

        private Image<Bgr, byte> AplicarFiltroGris(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;

            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpgr = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            Color rColor = new Color();
            Color oColor = new Color();

            float g = 0;

            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    //Obtenemos el color del pixel

                    oColor = imgBitmap.GetPixel(x, y);

                    g = oColor.R * 0.299f + oColor.G * 0.587f + oColor.B * 0.114f;

                    //Procesamos y obtenemos el nuevo color
                    rColor = Color.FromArgb((int)g, (int)g, (int)g);

                    //Colocamos el color en resultante
                    bmpgr.SetPixel(x, y, rColor);
                }
            }

            return new Image<Bgr, byte>(bmpgr);
        }

        private void Brillo_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de modificación de color
                Image<Bgr, byte> imgModificada = AplicarFiltroBrillo(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarFiltroBrillo(Image<Bgr, byte> imagen)
        {
            int brillo = 64;
            float pBrillo = 1.2f;

            int x = 0;
            int y = 0;
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            Color rColor = new Color();
            Color oColor = new Color();

            int r = 0;
            int g = 0;
            int b = 0;

            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    // Obtenemos el color del píxel original
                    oColor = imgBitmap.GetPixel(x, y);

                    // Aplicamos el gamma a cada canal
                    r = oColor.R + brillo;
                    g = oColor.G + brillo;
                    b = oColor.B + brillo;

                    //r = (int)(oColor.R * brillo);
                    //g = (int)(oColor.G * brillo);
                    //b = (int)(oColor.B * brillo);

                    if (r > 255) { r = 255; }
                    else if (r < 0) { r = 0; }

                    if (g > 255) { g = 255; }
                    else if (g < 0) { g = 0; }

                    if (b > 255) { b = 255; }
                    else if (b < 0) { b = 0; }

                    // Creamos el nuevo color con los valores modificados
                    rColor = Color.FromArgb(r, g, b);

                    // Asignamos el nuevo color al píxel en la imagen resultante
                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private void Gaussiano_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el filtro de detección de bordes
                Image<Bgr, byte> imgModificada = AplicarFiltroSobel(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }


        private Image<Bgr, byte> AplicarFiltroSobel(Image<Bgr, byte> imagen)
        {
            // Definir los filtros Sobel en X y Y
            int[,] sobelX = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Aplicar la convolución Sobel en ambas direcciones (X y Y)
            Bitmap bmpmodificacion = AplicarConvolucionSobel(imagen, sobelX, sobelY);

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private Bitmap AplicarConvolucionSobel(Image<Bgr, byte> imagen, int[,] filtroX, int[,] filtroY)
        {
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            int offset = 1;
            int sumX, sumY, sumR, sumG, sumB;

            // Aplicar los filtros Sobel en ambas direcciones a cada píxel (ignorando los bordes)
            for (int x = offset; x < imgBitmap.Width - offset; x++)
            {
                for (int y = offset; y < imgBitmap.Height - offset; y++)
                {
                    sumX = 0;
                    sumY = 0;

                    // Aplicar los filtros de Sobel en X y Y a los píxeles vecinos
                    for (int a = -offset; a <= offset; a++)
                    {
                        for (int b = -offset; b <= offset; b++)
                        {
                            Color pixelVecino = imgBitmap.GetPixel(x + a, y + b);
                            int colorR = pixelVecino.R;
                            int colorG = pixelVecino.G;
                            int colorB = pixelVecino.B;

                            sumX += colorR * filtroX[a + offset, b + offset];
                            sumY += colorR * filtroY[a + offset, b + offset];
                        }
                    }

                    // Calcular la magnitud del gradiente
                    int gradiente = (int)Math.Sqrt(sumX * sumX + sumY * sumY);

                    // Asegurarse de que el valor esté dentro del rango válido [0, 255]
                    gradiente = Math.Min(255, Math.Max(0, gradiente));

                    // Establecer el valor del píxel detectado como borde
                    bmpmodificacion.SetPixel(x, y, Color.FromArgb(gradiente, gradiente, gradiente));
                }
            }

            return bmpmodificacion;
        }

        private void Suavizado_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Convertir la imagen del PictureBox (Bitmap) a Image<Bgr, byte> de Emgu.CV
                Bitmap imgBitmap = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> imgEmgu = new Image<Bgr, byte>(imgBitmap);

                // Aplicar el efecto "Ojo de Pez"
                Image<Bgr, byte> imgModificada = AplicarEfectoOjoDePez(imgEmgu);

                // Convertir de nuevo la imagen modificada a Bitmap y mostrarla en el PictureBox
                pictureBox1.Image = imgModificada.ToBitmap();

                // Calcular el histograma
                int[] histogramR, histogramG, histogramB;
                CalcularHistograma(imgModificada.Bitmap, out histogramR, out histogramG, out histogramB);

                // Mostrar el histograma en los gráficos
                ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
            }
            else
            {
                MessageBox.Show("No se ha cargado ninguna imagen.");
            }
        }

        private Image<Bgr, byte> AplicarEfectoOjoDePez(Image<Bgr, byte> imagen)
        {
            int width = imagen.Width;
            int height = imagen.Height;

            Bitmap imgFinal = new Bitmap(width, height);
            int cx = width / 2; // Centro de la imagen en el eje X
            int cy = height / 2; // Centro de la imagen en el eje Y

            // Aplicar la distorsión de ojo de pez
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calcular la distancia al centro de la imagen
                    int dx = x - cx;
                    int dy = y - cy;
                    double distancia = Math.Sqrt(dx * dx + dy * dy);

                    // Normalizar la distancia al centro
                    double normalizedDistance = distancia / Math.Min(cx, cy);

                    // Aplicar una fórmula de distorsión más pronunciada para el efecto ojo de pez
                    double factor = Math.Pow(normalizedDistance, 1.5); // Hacer que la distorsión sea más fuerte

                    // Ajuste de la curvatura del efecto (hacerla más exagerada)
                    double distortionFactor = (1 - factor) * 0.3; // Aumentar el valor para mayor curvatura

                    // Modificar la distancia para crear el efecto de ojo de pez
                    double newX = cx + dx * (1 - distortionFactor); // Desplazar más los píxeles hacia el centro
                    double newY = cy + dy * (1 - distortionFactor); // Lo mismo para el eje Y

                    // Asegurarse de que las nuevas coordenadas estén dentro de los límites de la imagen
                    newX = Math.Min(Math.Max(newX, 0), width - 1);
                    newY = Math.Min(Math.Max(newY, 0), height - 1);

                    // Obtener el color de la imagen original en las nuevas coordenadas distorsionadas
                    Bgr colorBgr = imagen[(int)newY, (int)newX];

                    // Establecer el color del píxel en las nuevas coordenadas distorsionadas
                    imgFinal.SetPixel(x, y, Color.FromArgb((int)colorBgr.Red, (int)colorBgr.Green, (int)colorBgr.Blue));
                }
            }

            return new Image<Bgr, byte>(imgFinal);
        }

        private void fotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void videoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Video prueba = new Video();
            Form2 form2 = new Form2();
            this.Hide();
            form2.ShowDialog();
            this.Close();

        }

        private void cámaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Camara prueba = new Camara();
            this.Hide();
            prueba.ShowDialog();
            this.Close();
        }
    }
}
