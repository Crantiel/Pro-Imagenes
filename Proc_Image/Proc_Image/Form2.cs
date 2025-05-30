using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing; // Already present
using System.IO;
using System.Drawing.Imaging; // Required for PixelFormat
using AForge.Imaging.Filters;
using AForge; // For IntRange and Range
using AForge.Imaging; // For ImageStatistics
using AForge.Math; // For Histogram
using System.Windows.Forms.DataVisualization.Charting; // For Chart, Series

namespace Proc_Image
{
    public partial class Form2 : Form
    {
        private Bitmap currentBitmap = null;
        public Form2()
        {
            InitializeComponent();
            this.button11.Click += new System.EventHandler(this.button11_Click);
            this.button12.Click += new System.EventHandler(this.button12_Click);
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button8.Click += new System.EventHandler(this.button8_Click);
            this.button9.Click += new System.EventHandler(this.button9_Click);
            this.button10.Click += new System.EventHandler(this.button10_Click);
            this.button6.Click += new System.EventHandler(this.button6_Click);
            this.button5.Click += new System.EventHandler(this.button5_Click);
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            UpdateHistograms(); // Initial call to clear/setup histograms
        }

        private void DisplayHistogram(System.Windows.Forms.DataVisualization.Charting.Chart chartControl, AForge.Math.Histogram histogram, string seriesName, System.Drawing.Color seriesColor)
        {
            chartControl.Series.Clear();
            Series series = new Series(seriesName);
            series.Color = seriesColor;
            series.ChartType = SeriesChartType.Column;
            for (int i = 0; i < histogram.Values.Length; i++)
            {
                series.Points.AddXY(i, histogram.Values[i]);
            }
            chartControl.Series.Add(series);
            chartControl.ChartAreas[0].AxisX.Minimum = 0;
            chartControl.ChartAreas[0].AxisX.Maximum = 255;
            chartControl.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chartControl.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartControl.ChartAreas[0].RecalculateAxesScale();
        }

        public void UpdateHistograms()
        {
            if (currentBitmap == null)
            {
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
                return;
            }

            try
            {
                // Ensure bitmap is in a format supported by ImageStatistics, typically 24bppRgb or 32bppArgb
                Bitmap bitmapForStats = currentBitmap;
                if (currentBitmap.PixelFormat != PixelFormat.Format24bppRgb &&
                    currentBitmap.PixelFormat != PixelFormat.Format32bppArgb &&
                    currentBitmap.PixelFormat != PixelFormat.Format8bppIndexed) // Grayscale is also fine
                {
                    // Attempt to convert to a more common format if necessary
                    bitmapForStats = new Bitmap(currentBitmap.Width, currentBitmap.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bitmapForStats))
                    {
                        g.DrawImage(currentBitmap, 0, 0);
                    }
                }


                AForge.Imaging.ImageStatistics stats = new AForge.Imaging.ImageStatistics(bitmapForStats);

                if (stats.IsGrayscale)
                {
                    DisplayHistogram(chart1, stats.Gray, "Gray", Color.Gray);
                    if (chart2.Series != null) chart2.Series.Clear(); // Clear other charts if grayscale
                    if (chart3.Series != null) chart3.Series.Clear();
                }
                else
                {
                    DisplayHistogram(chart1, stats.Red, "Red", Color.Red);
                    DisplayHistogram(chart2, stats.Green, "Green", Color.Green);
                    DisplayHistogram(chart3, stats.Blue, "Blue", Color.Blue);
                }

                // If bitmapForStats was a temporary conversion, dispose it
                if (bitmapForStats != currentBitmap)
                {
                    bitmapForStats.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception, e.g., if the pixel format is still unsupported
                Console.WriteLine("Error updating histograms: " + ex.Message);
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
            }
        }


        private Color MapIntensityToThermalColor(byte intensity)
        {
            if (intensity < 51) return Color.FromArgb(0, 0, (int)(intensity * 5));
            if (intensity < 102) return Color.FromArgb(0, (int)((intensity - 51) * 5), 255);
            if (intensity < 153) return Color.FromArgb((int)((intensity - 102) * 5), 255, (int)(255 - (intensity - 102) * 5));
            if (intensity < 204) return Color.FromArgb(255, (int)(255 - (intensity - 153) * 5), 0);
            return Color.FromArgb(255, (int)((intensity - 204) * 5), (int)((intensity - 204) * 5));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null)
            {
                MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap sourceBitmap = currentBitmap.Clone() as Bitmap;
            Bitmap bitmapWithAlpha = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmapWithAlpha))
            {
                g.DrawImage(sourceBitmap, new Rectangle(0, 0, bitmapWithAlpha.Width, bitmapWithAlpha.Height));
            }
            sourceBitmap.Dispose();

            // Usar ColorFiltering en lugar de ReplaceColor
            ColorFiltering colorFilter = new ColorFiltering();

            // Definir rango de verde (ajusta según tu verde específico)
            colorFilter.Red = new IntRange(0, 100);     // Poco rojo
            colorFilter.Green = new IntRange(100, 255); // Mucho verde
            colorFilter.Blue = new IntRange(0, 100);    // Poco azul

            // Color de reemplazo (transparente)
            colorFilter.FillColor = new RGB(Color.Transparent);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmapWithAlpha);

            if (currentBitmap != null && currentBitmap != bitmapWithAlpha)
            {
                currentBitmap.Dispose();
            }
            currentBitmap = bitmapWithAlpha;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            WaterWave waterWaveFilter = new WaterWave();
            waterWaveFilter.HorizontalWavesCount = 10; waterWaveFilter.HorizontalWavesAmplitude = 5;
            waterWaveFilter.VerticalWavesCount = 3; waterWaveFilter.VerticalWavesAmplitude = 15;
            Bitmap resultBitmap = waterWaveFilter.Apply(processedBitmap);
            if (this.currentBitmap != null && this.currentBitmap != resultBitmap && this.currentBitmap != processedBitmap) { this.currentBitmap.Dispose(); }
            if (processedBitmap != resultBitmap) processedBitmap.Dispose();
            currentBitmap = resultBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            SaltAndPepperNoise noiseFilter = new SaltAndPepperNoise();
            noiseFilter.NoiseAmount = 0.05;
            noiseFilter.ApplyInPlace(processedBitmap);
            if (currentBitmap != processedBitmap) currentBitmap.Dispose(); // Dispose old if it's not the same as the new one
            currentBitmap = processedBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            GaussianBlur blurFilter = new GaussianBlur();
            blurFilter.Sigma = 2; blurFilter.Size = 5;
            blurFilter.ApplyInPlace(processedBitmap);
            if (currentBitmap != processedBitmap) currentBitmap.Dispose();
            currentBitmap = processedBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap initialClone = currentBitmap.Clone() as Bitmap;
            Bitmap grayscaleBitmap = Grayscale.CommonAlgorithms.BT709.Apply(initialClone);
            initialClone.Dispose();
            Threshold thresholdFilter = new Threshold();
            thresholdFilter.ThresholdValue = 128;
            thresholdFilter.ApplyInPlace(grayscaleBitmap);
            if (currentBitmap != null && currentBitmap != grayscaleBitmap) currentBitmap.Dispose();
            currentBitmap = grayscaleBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            Pixellate pixellateFilter = new Pixellate();
            pixellateFilter.PixelSize = 8;
            pixellateFilter.ApplyInPlace(processedBitmap);
            if (currentBitmap != processedBitmap) currentBitmap.Dispose();
            currentBitmap = processedBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap initialClone = currentBitmap.Clone() as Bitmap;
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayBitmap = grayscaleFilter.Apply(initialClone);
            initialClone.Dispose();
            Bitmap thermalBitmap = new Bitmap(grayBitmap.Width, grayBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < grayBitmap.Width; x++) { for (int y = 0; y < grayBitmap.Height; y++) { byte intensity = grayBitmap.GetPixel(x, y).R; thermalBitmap.SetPixel(x, y, MapIntensityToThermalColor(intensity)); } }
            grayBitmap.Dispose();
            if (currentBitmap != null && currentBitmap != thermalBitmap) currentBitmap.Dispose();
            currentBitmap = thermalBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap originalColors = currentBitmap.Clone() as Bitmap;
            if (originalColors.PixelFormat != PixelFormat.Format32bppArgb) { Bitmap temp = new Bitmap(originalColors.Width, originalColors.Height, PixelFormat.Format32bppArgb); using (Graphics g = Graphics.FromImage(temp)) { g.DrawImage(originalColors, 0, 0); } originalColors.Dispose(); originalColors = temp; }
            Bitmap grayClone = currentBitmap.Clone() as Bitmap;
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayBitmap = grayscaleFilter.Apply(grayClone);
            grayClone.Dispose();
            DifferenceEdgeDetector edgeDetector = new DifferenceEdgeDetector();
            Bitmap edgesBitmap = edgeDetector.Apply(grayBitmap);
            Invert invertFilter = new Invert();
            invertFilter.ApplyInPlace(edgesBitmap);
            Threshold thresholdFilter = new Threshold(100);
            thresholdFilter.ApplyInPlace(edgesBitmap);
            Bitmap resultBitmap = originalColors;
            for (int x = 0; x < edgesBitmap.Width; x++) { for (int y = 0; y < edgesBitmap.Height; y++) { if (edgesBitmap.GetPixel(x, y).R == 0) { resultBitmap.SetPixel(x, y, Color.Black); } } }
            grayBitmap.Dispose(); edgesBitmap.Dispose();
            if (currentBitmap != null && currentBitmap != resultBitmap) currentBitmap.Dispose();
            currentBitmap = resultBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            Invert invertFilter = new Invert();
            invertFilter.ApplyInPlace(processedBitmap);
            if (currentBitmap != processedBitmap) currentBitmap.Dispose();
            currentBitmap = processedBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentBitmap == null) { MessageBox.Show("Por favor, cargue una imagen primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Bitmap processedBitmap = currentBitmap.Clone() as Bitmap;
            BrightnessCorrection brightnessFilter = new BrightnessCorrection();
            brightnessFilter.AdjustValue = 50;
            brightnessFilter.ApplyInPlace(processedBitmap);
            if (currentBitmap != processedBitmap) currentBitmap.Dispose();
            currentBitmap = processedBitmap;
            pictureBox1.Image = currentBitmap;
            UpdateHistograms();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            UpdateHistograms(); // Also call when form loads, in case an image is pre-loaded (though not in this app)
        }

        private void button11_Click(object sender, EventArgs e) // Cargar button
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";
            openFileDialog.Title = "Seleccionar Imagen";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (currentBitmap != null) currentBitmap.Dispose();
                currentBitmap = new Bitmap(openFileDialog.FileName);
                pictureBox1.Image = currentBitmap;
                UpdateHistograms();
            }
        }

        private void button12_Click(object sender, EventArgs e) // Guardar button
        {
            if (currentBitmap == null) { MessageBox.Show("No hay imagen para guardar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveFileDialog.Title = "Guardar Imagen Como";
            saveFileDialog.FileName = "processed_image.png";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                ImageFormat format = ImageFormat.Png;
                string ext = Path.GetExtension(filePath).ToLower();
                switch (ext)
                {
                    case ".jpg": case ".jpeg": format = ImageFormat.Jpeg; break;
                    case ".bmp": format = ImageFormat.Bmp; break;
                }
                currentBitmap.Save(filePath, format);
            }
        }
    }
}
