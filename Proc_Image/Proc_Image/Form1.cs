using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proc_Image
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Agregar los manejadores de eventos para los botones
            Foto_Boton.Click += new EventHandler(Foto_Boton_Click);
            Video_Boton.Click += new EventHandler(Video_Boton_Click);
            Camara_Boton.Click += new EventHandler(Camara_Boton_Click);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Este método ya estaba en tu código original
        }

        // Método para el botón de Foto
        private void Foto_Boton_Click(object sender, EventArgs e)
        {
            // Crear e instanciar Form2
            Form2 formFoto = new Form2();
            formFoto.Show();
            // Opcional: this.Hide(); // Para ocultar Form1 mientras se muestra Form2
        }

        // Método para el botón de Video
        private void Video_Boton_Click(object sender, EventArgs e)
        {
            // Crear e instanciar Form3
            Form3 formVideo = new Form3();
            formVideo.Show();
            // Opcional: this.Hide(); // Para ocultar Form1 mientras se muestra Form3
        }

        // Método para el botón de Cámara
        private void Camara_Boton_Click(object sender, EventArgs e)
        {
            // Crear e instanciar Form4
            Form4 formCamara = new Form4();
            formCamara.Show();
            // Opcional: this.Hide(); // Para ocultar Form1 mientras se muestra Form4
        }
    }
}