using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Web;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using ZedGraph;
using OpenTK.Graphics;
using System.Numerics;

namespace PIA_PI
{
    public enum TipoFiltro
    {
        Ninguno,
        Invertido,
        Modificación,
        Aberración,
        Gamma,
        Gris,
        Brillo,
        Contraste,
        Ruido,
        Mosaico,
        OjoDePez,
        Termico,
        Warp,
        Sobel
    }

    public partial class Form2 : Form
    {

        VideoCapture grabber;
        Image<Bgr, Byte> currentFrame;
        double TotalDuration;
        double FrameCount;
        bool Video = false;
        private Timer videoTimer;
        Mat m;

        private TipoFiltro filtroActivo = TipoFiltro.Ninguno;

        private bool isPaused = false;  // Variable para controlar el estado de pausa

        private int[,] conv3x3 = new int[3, 3];
        private int factor;
        private int offset;



        public Form2()
        {
            InitializeComponent();
            videoTimer = new Timer();
            videoTimer.Interval = 33; // Intervalo de 33 ms (~30 FPS)
            videoTimer.Tick += VideoFrameCapture; // Llamará a VideoFrameCapture cada vez que el timer se active
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (grabber != null)
            {
                // Reinicia el video al primer fotograma
                grabber.SetCaptureProperty(CapProp.PosFrames, 0);

                // Lee el primer fotograma
                grabber.Read(m);
                currentFrame = m.ToImage<Bgr, byte>();
                pictureBox1.Image = currentFrame.Bitmap;

                // Si el video no está en pausa, reinicia la reproducción
                if (!isPaused)
                {
                    videoTimer.Start();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                // Reanudar video
                videoTimer.Start();
                isPaused = false;  // Cambiar el estado a "reproducción"
                button2.Text = "Pausar";  // Cambiar el texto del botón a "Pausar"
            }
            else
            {
                // Pausar video
                videoTimer.Stop();
                isPaused = true;   // Cambiar el estado a "pausa"
                button2.Text = "Reanudar";  // Cambiar el texto del botón a "Reanudar"
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                grabber = new VideoCapture(ofd.FileName);
                m = new Mat();

                // Carga el primer fotograma
                grabber.Read(m);
                currentFrame = m.ToImage<Bgr, byte>();
                pictureBox1.Image = currentFrame.Bitmap;

                TotalDuration = grabber.GetCaptureProperty(CapProp.FrameCount);
                FrameCount = grabber.GetCaptureProperty(CapProp.PosFrames);
                Video = true;

                // Reinicia y comienza la reproducción
                videoTimer.Start(); // Inicia el temporizador para la reproducción
                isPaused = false;  // Asegura que la reproducción no comience en pausa
                button2.Text = "Pausar";  // Cambia el texto del botón a "Pausar"
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void fotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 prueba = new Form1();
            this.Hide();
            prueba.ShowDialog();
            this.Close();
        }

        private void cámaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Camara prueba = new Camara();
            this.Hide();
            prueba.ShowDialog();
            this.Close();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Ninguno;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Invertido;
        }

        private void Gamma_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Gamma;
        }

        private void Ruido_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Ruido;
        }

        private void Mosaico_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Mosaico;
        }

        private void Croma_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Aberración;
        }

        private void Modificación_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Modificación;
        }

        private void Homogeneidad_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Termico;
        }

        private void Contraste_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Contraste;
        }

        private void Gris_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Gris;
        }

        private void Brillo_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Brillo;
        }

        private void Gaussiano_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.Sobel;
        }

        private void Suavizado_Click(object sender, EventArgs e)
        {
            filtroActivo = TipoFiltro.OjoDePez;
        }

        private void VideoFrameCapture(object sender, EventArgs e)
        {
            if (grabber != null && grabber.IsOpened)
            {
                // Captura el siguiente fotograma
                m = grabber.QueryFrame();

                if (m != null)
                {
                    currentFrame = m.ToImage<Bgr, byte>(); // Convierte el fotograma en una imagen en formato adecuado

                    //Aplicar los filtros
                    switch (filtroActivo)
                    {
                        case TipoFiltro.Gamma:
                            currentFrame = AplicarFiltroGamma(currentFrame);
                            break;

                        case TipoFiltro.Ruido:
                            currentFrame = AplicarFiltroRuido(currentFrame);
                            break;

                        case TipoFiltro.Mosaico:
                            currentFrame = AplicarFiltroMosaico(currentFrame);
                            break;

                        case TipoFiltro.Modificación:
                            currentFrame = AplicarFiltroModificación(currentFrame);
                            break;

                        case TipoFiltro.Invertido:
                            currentFrame = AplicarFiltroInvertido(currentFrame);
                            break;

                        case TipoFiltro.Aberración:
                            currentFrame = AplicarFiltroAberración(currentFrame);
                            break;


                        case TipoFiltro.Termico:
                            currentFrame = AplicarFiltroTermico(currentFrame);
                            break;
                        case TipoFiltro.OjoDePez:
                            currentFrame = AplicarFiltroOjoDePez(currentFrame);
                            break;
                        case TipoFiltro.Contraste:
                            currentFrame = AplicarFiltroContraste(currentFrame);
                            break;
                        case TipoFiltro.Gris:
                            currentFrame = AplicarFiltroGris(currentFrame);
                            break;
                        case TipoFiltro.Brillo:
                            currentFrame = AplicarFiltroBrillo(currentFrame);
                            break;
                        case TipoFiltro.Warp:
                            currentFrame = AplicarFiltroWarp(currentFrame);
                            break;
                        case TipoFiltro.Sobel:
                            currentFrame = AplicarFiltroSobel(currentFrame);
                            break;

                        case TipoFiltro.Ninguno:
                            // No hacer nada si no hay filtro seleccionado
                            break;
                    }



                    pictureBox1.Image = currentFrame.Bitmap; // Muestra el fotograma en el PictureBox

                    // Calcular el histograma
                    int[] histogramR, histogramG, histogramB;
                    CalcularHistograma(currentFrame.Bitmap, out histogramR, out histogramG, out histogramB);

                    // Mostrar el histograma en los gráficos
                    ActualizarHistogramaEnGrafico(histogramR, histogramG, histogramB);
                }

                // Actualizar el contador de fotogramas
                FrameCount = grabber.GetCaptureProperty(CapProp.PosFrames);

                // Detener la reproducción si llegamos al final del video
                if (FrameCount >= TotalDuration - 1)
                {
                    videoTimer.Stop();
                    MessageBox.Show("El video ha terminado.");
                }
            }
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

        private Image<Bgr, byte> AplicarFiltroModificación(Image<Bgr, byte> imagen)
        {
            int x = 0;
            int y = 0;
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            Color rColor = new Color();
            Color oColor = new Color();

            for (x = 0; x < imgBitmap.Width; x++)
            {
                for (y = 0; y < imgBitmap.Height; y++)
                {
                    //Obtenemos el color del pixel

                    oColor = imgBitmap.GetPixel(x, y);

                    //Procesamos y obtenemos el nuevo color
                    rColor = Color.FromArgb(oColor.R, 0, 0);

                    //Colocamos el color en resultante
                    bmpmodificacion.SetPixel(x, y, rColor);
                }
            }

            return new Image<Bgr, byte>(bmpmodificacion);
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

        private Image<Bgr, byte> AplicarFiltroTermico(Image<Bgr, byte> imagen)
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

        private Image<Bgr, byte> AplicarFiltroOjoDePez(Image<Bgr, byte> imagen)
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
                    double factor = Math.Pow(normalizedDistance,5.0); // Hacer que la distorsión sea más fuerte

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

        private Image<Bgr, byte> AplicarFiltroWarp(Image<Bgr, byte> imagen)
        {
            Bitmap imgBitmap = imagen.ToBitmap();
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);
            Bitmap resultante = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            //punto que vamos a mover
            int xWarp = 100;
            int yWarp = 120;



            int medioX = imgBitmap.Width / 2;
            int medioY = imgBitmap.Height / 2;

            int x1 = 0;
            int x2 = medioX;
            int x3 = xWarp;
            int x4 = 0;
            int y1 = 0;
            int y2 = 0;
            int y3 = yWarp;
            int y4 = medioY;

            int offsetX = 0;
            int offsetY = 0;

            bool bilinear = true;

            if (bilinear)
            {
                bmpmodificacion = warpBilineal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }
            else
            {
                bmpmodificacion = warpNormal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }

            x1 = medioX;
            x2 = imgBitmap.Width - 1;
            x3 = imgBitmap.Width - 1;
            x4 = xWarp;

            y1 = 0;
            y2 = 0;
            y3 = medioY;
            y4 = yWarp;

            offsetX = medioX;
            offsetY = 0;

            if (bilinear)
            {
                bmpmodificacion = warpBilineal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }
            else
            {
                bmpmodificacion = warpNormal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }

            x1 = xWarp;
            x2 = imgBitmap.Width - 1;
            x3 = imgBitmap.Width - 1;
            x4 = medioX;

            y1 = yWarp;
            y2 = medioY;
            y3 = imgBitmap.Height - 1;
            y4 = imgBitmap.Height - 1;

            offsetX = medioX;
            offsetY = medioY;

            if (bilinear)
            {
                bmpmodificacion = warpBilineal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }
            else
            {
                bmpmodificacion = warpNormal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }

            x1 = 0;
            x2 = xWarp;
            x3 = medioX;
            x4 = 0;

            y1 = medioY;
            y2 = yWarp;
            y3 = imgBitmap.Height - 1;
            y4 = imgBitmap.Height - 1;

            offsetX = 0;
            offsetY = medioY;

            if (bilinear)
            {
                bmpmodificacion = warpBilineal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }
            else
            {
                bmpmodificacion = warpNormal(imgBitmap, x1, y1, x2, y2, x3, y3, x4, y4, offsetX, offsetY);
            }

            return new Image<Bgr, byte>(bmpmodificacion);
        }

        private Bitmap warpBilineal(Bitmap Imagen, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int extraX, int extraY)
        {
            Bitmap imgBitmap = Imagen;
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            int alto = Imagen.Height;
            int ancho = Imagen.Width;
            Color miColor;
            double medioX = ancho / 2.0;
            double medioY = alto / 2.0;
            double denominador = medioX * medioY;

            double xa = x2 - x1;
            double xb = x4 - x1;
            double xab = x1 - x2 + x3 - x4;

            double ya = y2 - y1;
            double yb = y4 - y1;
            double yab = y1 - y2 + y3 - y4;

            int y = 0;
            int x = 0;

            double dy = 0;
            double dx = 0;
            double xImagen = 0;
            double yImagen = 0;

            for (y = 0; y < medioY; y++)
            {
                for (x = 0; x < medioX; x++)
                {
                    dy = y;
                    dx = x;

                    xImagen = x1 + (xa * dx) / medioX + (xb * dy) / medioY + (xab * dy * dx) / denominador;
                    yImagen = y1 + (ya * dx) / medioX + (yb * dy) / medioY + (yab * dy * dx) / denominador;

                    miColor = interpolacionBilineal(Imagen, xImagen, yImagen);

                    bmpmodificacion.SetPixel(x + extraX, y + extraY, miColor);
                }
            }
            return bmpmodificacion;
        }

        public Color interpolacionBilineal(Bitmap Imagen, double x, double y)
        {
            Bitmap imgBitmap = Imagen;

            Color resultado = Color.Black;
            Color Color1;
            Color Color2;
            double fraccionX = 0;
            double fraccionY = 0;
            double unoMenosX = 0;
            double unoMenosY = 0;

            int techoX = 0;
            int techoY = 0;
            int pisoX = 0;
            int pisoY = 0;

            int rp1 = 0;
            int rp2 = 0;
            int rp3 = 0;

            int gp1 = 0;
            int gp2 = 0;
            int gp3 = 0;

            int bp1 = 0;
            int bp2 = 0;
            int bp3 = 0;
            int relleno = 128;

            if (x < 0 || x >= imgBitmap.Width - 1 || y < 0 || y >= imgBitmap.Height - 1)
            {
                return Color.FromArgb(relleno, relleno, relleno);
            }

            pisoX = (int)Math.Floor(x);
            pisoY = (int)Math.Floor(y);
            techoX = (int)Math.Ceiling(x);
            techoY = (int)Math.Ceiling(y);

            fraccionX = x - pisoX;
            fraccionY = y - pisoY;

            unoMenosX = 1.0 - fraccionX;
            unoMenosY = 1.0 - fraccionY;

            Color1 = imgBitmap.GetPixel(pisoX, pisoY);
            Color2 = imgBitmap.GetPixel(techoX, techoY);

            rp1 = (int)(unoMenosX * Color1.R + fraccionX * Color2.R);
            gp1 = (int)(unoMenosX * Color1.G + fraccionX * Color2.G);
            bp1 = (int)(unoMenosX * Color1.B + fraccionX * Color2.B);

            Color1 = imgBitmap.GetPixel(pisoX, techoY);
            Color2 = imgBitmap.GetPixel(techoX, techoY);

            rp2 = (int)(unoMenosX * Color1.R + fraccionX * Color2.R);
            gp2 = (int)(unoMenosX * Color1.G + fraccionX * Color2.G);
            bp2 = (int)(unoMenosX * Color1.B + fraccionX * Color2.B);

            rp3 = (int)(unoMenosY * rp1 + fraccionY * rp2);
            gp3 = (int)(unoMenosY * gp1 + fraccionY * gp2);
            bp3 = (int)(unoMenosY * bp1 + fraccionY * bp2);

            resultado = Color.FromArgb(rp3, gp3, bp3);

            return resultado;
        }

        private Bitmap warpNormal(Bitmap Imagen, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int extraX, int extraY)
        {
            Bitmap imgBitmap = Imagen;
            Bitmap bmpmodificacion = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            int alto = Imagen.Height;
            int ancho = Imagen.Width;
            int relleno = 128;
            Color miColor;
            int medioX = ancho / 2;
            int medioY = alto / 2;
            int denominador = medioX * medioY;

            int xa = x2 - x1;
            int xb = x4 - x1;
            int xab = x1 - x2 + x3 - x4;

            int ya = y2 - y1;
            int yb = y4 - y1;
            int yab = y1 - y2 + y3 - y4;

            int y = 0;
            int x = 0;

            int xImagen = 0;
            int yImagen = 0;

            for (y = 0; y < medioY; y++)
            {
                for (x = 0; x < medioX; x++)
                {
                    xImagen = x1 + (xa * x) / medioX + (xb * y) / medioY + (xab * y * x) / denominador;
                    yImagen = y1 + (ya * x) / medioX + (yb * y) / medioY + (yab * y * x) / denominador;

                    if (xImagen < 0 || xImagen >= ancho || yImagen < 0 || yImagen >= alto)
                    {
                        bmpmodificacion.SetPixel(x + extraX, y + extraY, Color.FromArgb(relleno, relleno, relleno));
                    }
                    else
                    {
                        miColor = imgBitmap.GetPixel(xImagen, yImagen);
                        bmpmodificacion.SetPixel(x + extraX, y + extraY, miColor);
                    }
                }
            }
            return bmpmodificacion;
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

    }
}
