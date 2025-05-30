using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging; // Necesario para BitmapData e ImageLockMode
using System.Runtime.InteropServices; // Necesario para Marshal
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Proc_Image
{
    public partial class Form4 : Form
    {
        private FilterInfoCollection dispositivosCamara;
        private VideoCaptureDevice fuenteVideo;
        private Color colorSeleccionado = Color.Black;
        private bool isColorFilterActive = false;

        public Form4()
        {
            InitializeComponent();
            CargarDispositivos();

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Agregar manejadores de eventos
            ActivarCam_Boton.Click += new EventHandler(ActivarCam_Boton_Click);
            ActivarColor_Boton.Click += new EventHandler(ActivarColor_Boton_Click);
            DetenerColor_Boton.Click += new EventHandler(DetenerColor_Boton_Click);

            pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_MouseClick);
            this.FormClosing += new FormClosingEventHandler(Form4_FormClosing);

            // Inicialmente el botón de detener color debe estar deshabilitado
            DetenerColor_Boton.Enabled = false;
        }

        private void CargarDispositivos()
        {
            // Obtener la lista de dispositivos de cámara disponibles
            dispositivosCamara = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Limpiar el ComboBox
            comboBox_Cam.Items.Clear();

            // Si hay dispositivos disponibles, agregarlos al ComboBox
            if (dispositivosCamara.Count > 0)
            {
                foreach (FilterInfo dispositivo in dispositivosCamara)
                {
                    comboBox_Cam.Items.Add(dispositivo.Name);
                }
                comboBox_Cam.SelectedIndex = 0; // Seleccionar el primer dispositivo por defecto
            }
            else
            {
                MessageBox.Show("No se encontraron dispositivos de cámara", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivarCam_Boton_Click(object sender, EventArgs e)
        {
            if (comboBox_Cam.SelectedIndex >= 0)
            {
                // Detener la cámara si ya está en funcionamiento
                if (fuenteVideo != null && fuenteVideo.IsRunning)
                {
                    DetenerCamara();
                    ActivarCam_Boton.Text = "Activar Cámara";
                }
                else
                {
                    // Iniciar la cámara seleccionada
                    int indice = comboBox_Cam.SelectedIndex;
                    string moniker = dispositivosCamara[indice].MonikerString;
                    fuenteVideo = new VideoCaptureDevice(moniker);
                    fuenteVideo.NewFrame += FuenteVideo_NewFrame;
                    fuenteVideo.Start();
                    ActivarCam_Boton.Text = "Detener Cámara";
                }
            }
        }

        private void FuenteVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();

            // Si el filtro de color está activo, aplicamos la detección de color
            if (isColorFilterActive)
            {
                // Detección de color
                Bitmap processedImage = DetectColor(bmp, colorSeleccionado);

                // Mostrar la imagen procesada con detección de color
                pictureBox1.Image = processedImage;
            }
            else
            {
                // Si no, mostramos la imagen sin procesar
                pictureBox1.Image = bmp;
            }
        }

        private void ActivarColor_Boton_Click(object sender, EventArgs e)
        {
            // Abrir un diálogo para seleccionar color
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorSeleccionado = colorDialog.Color;
                isColorFilterActive = true; // Activar el filtro de color

                // Actualizar estados de los botones
                ActivarColor_Boton.Enabled = false;
                DetenerColor_Boton.Enabled = true;
            }
        }

        private void DetenerColor_Boton_Click(object sender, EventArgs e)
        {
            isColorFilterActive = false; // Desactivar el filtro de color

            // Actualizar estados de los botones
            ActivarColor_Boton.Enabled = true;
            DetenerColor_Boton.Enabled = false;
        }

        private Bitmap DetectColor(Bitmap image, Color targetColor)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    // Tolerancia para detectar colores similares
                    if (Math.Abs(pixelColor.R - targetColor.R) < 80 &&
                        Math.Abs(pixelColor.G - targetColor.G) < 80 &&
                        Math.Abs(pixelColor.B - targetColor.B) < 80)
                    {
                        result.SetPixel(x, y, pixelColor); // Muestra el pixel detectado en su color original
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.Transparent); // Fondo transparente
                    }
                }
            }

            return result;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // Obtener el color del pixel donde se hizo clic
            if (pictureBox1.Image != null)
            {
                try
                {
                    Bitmap bmp = (Bitmap)pictureBox1.Image;
                    // Calcular las coordenadas relativas al tamaño real de la imagen
                    int x = e.X * bmp.Width / pictureBox1.Width;
                    int y = e.Y * bmp.Height / pictureBox1.Height;

                    if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                    {
                        colorSeleccionado = bmp.GetPixel(x, y);
                        MostrarColorInfo(colorSeleccionado);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al obtener el color: " + ex.Message);
                }
            }
        }

        private void MostrarColorInfo(Color color)
        {
            // Dependiendo de qué botón está activo, mostrar información RGB o Hexadecimal
            if (ActivarColor_Boton.Enabled)
            {
                MessageBox.Show($"Color RGB: R={color.R}, G={color.G}, B={color.B}");
            }
            else
            {
                string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                MessageBox.Show($"Color Hexadecimal: {hex}");
            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Detener la cámara al cerrar el formulario
            DetenerCamara();
        }

        private void DetenerCamara()
        {
            if (fuenteVideo != null && fuenteVideo.IsRunning)
            {
                fuenteVideo.SignalToStop();
                fuenteVideo = null;
            }
        }
    }
}