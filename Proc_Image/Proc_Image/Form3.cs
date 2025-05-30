using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video;
using Accord.Video.FFMPEG;
using System.IO;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math;

namespace Proc_Image
{
    public partial class Form3 : Form
    {
        private VideoFileReader videoReader;
        private VideoFileWriter videoWriter;
        private Bitmap currentFrame;
        private List<Bitmap> originalFrames; // To store all frames of the original video
        private List<Bitmap> processedFrames; // To store frames after filter application
        private System.Windows.Forms.Timer playbackTimer;
        private bool isPlaying = false;
        private bool isPaused = false;
        private int currentFrameIndex = 0;
        private double videoFrameRate = 0;
        private int videoWidth = 0;
        private int videoHeight = 0;

        public Form3()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            playbackTimer = new System.Windows.Forms.Timer();
            playbackTimer.Tick += new EventHandler(PlaybackTimer_Tick);
            UpdateHistograms();
            originalFrames = new List<Bitmap>();
            processedFrames = new List<Bitmap>();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form3_FormClosing);
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            playbackTimer?.Stop();
            playbackTimer?.Dispose();
            playbackTimer = null;

            videoReader?.Close();
            videoReader?.Dispose();
            videoReader = null;

            videoWriter?.Close();
            videoWriter?.Dispose();
            videoWriter = null;

            currentFrame?.Dispose();
            currentFrame = null;

            if (originalFrames != null)
            {
                foreach (Bitmap frame in originalFrames)
                {
                    frame?.Dispose();
                }
                originalFrames.Clear();
                originalFrames = null;
            }

            if (processedFrames != null)
            {
                foreach (Bitmap frame in processedFrames)
                {
                    frame?.Dispose();
                }
                processedFrames.Clear();
                processedFrames = null;
            }
        }

        private void ApplyFilterToAllFrames(IFilter filter)
        {
            if (originalFrames == null || originalFrames.Count == 0)
            {
                MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool wasPlaying = isPlaying;
            if (isPlaying)
            {
                playbackTimer.Stop();
                isPlaying = false;
                // BTN_Continuar.Text = "Continuar"; // Optionally update button text
            }

            // Clear previous processed frames and dispose them
            processedFrames.ForEach(f => f.Dispose());
            processedFrames.Clear();

            Cursor = Cursors.WaitCursor; // Show busy cursor

            try
            {
                for (int i = 0; i < originalFrames.Count; i++)
                {
                    Bitmap originalFrame = originalFrames[i];
                    // Apply filter. Assuming IFilter creates a new image. 
                    // If it's an IInPlaceFilter, originalFrame itself would be modified if not cloned.
                    Bitmap processedFrame = filter.Apply(originalFrame); 
                    processedFrames.Add(processedFrame);
                }

                // Update currentFrame to reflect the change at the currentFrameIndex
                if (currentFrameIndex < processedFrames.Count)
                {
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else if (processedFrames.Count > 0) // If index was out of bounds, reset to first frame
                {
                    currentFrameIndex = 0;
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[0].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error aplicando el filtro: {ex.Message}", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Restore processedFrames from originalFrames if filter failed mid-way
                processedFrames.ForEach(f => f.Dispose());
                processedFrames.Clear();
                foreach (Bitmap originalFrame in originalFrames)
                {
                    processedFrames.Add((Bitmap)originalFrame.Clone());
                }
                // Restore currentFrame display
                if (currentFrameIndex < processedFrames.Count)
                {
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else if (processedFrames.Count > 0) // If index became invalid
                {
                    currentFrameIndex = 0;
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[0].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            finally
            {
                Cursor = Cursors.Default; // Restore cursor
            }

            if (wasPlaying)
            {
                playbackTimer.Start();
                isPlaying = true;
                // BTN_Continuar.Text = "Pausar"; // Optionally update button text
            }
        }

        private void DisplayHistogram(System.Windows.Forms.DataVisualization.Charting.Chart chartControl, AForge.Math.Histogram histogram, string seriesName, System.Drawing.Color seriesColor)
        {
            chartControl.Series.Clear();
            System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series(seriesName);
            series.Color = seriesColor;
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            for (int i = 0; i < histogram.Values.Length; i++)
            {
                series.Points.AddXY(i, histogram.Values[i]);
            }
            chartControl.Series.Add(series);
            chartControl.ChartAreas[0].AxisX.Minimum = 0;
            chartControl.ChartAreas[0].AxisX.Maximum = 255;
            chartControl.ChartAreas[0].AxisY.LabelStyle.Enabled = false; // Keep Y labels off for cleaner look
            chartControl.ChartAreas[0].AxisX.LabelStyle.Enabled = false; // Keep X labels off
            chartControl.ChartAreas[0].RecalculateAxesScale();
        }

        private void UpdateHistograms()
        {
            // Ensure chart controls are named chart1, chart2, chart3 as in Form2 and Form3.Designer.cs
            if (currentFrame == null || pictureBox1.Image == null) // Check pictureBox1.Image as well, as currentFrame might be set before UI update
            {
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
                return;
            }

            Bitmap bitmapForStats = null;
            try
            {
                // It's crucial that currentFrame is not disposed while stats are being calculated.
                // Clone it if there's any doubt, or ensure lifecycle is managed.
                // For video frames that are frequently updated, using the direct currentFrame should be fine
                // as long as it's not disposed by another thread during this operation.
                // Let's proceed with a clone to be safe, given the video context.
                bitmapForStats = (Bitmap)currentFrame.Clone();

                // Ensure bitmap is in a format supported by ImageStatistics
                if (bitmapForStats.PixelFormat != PixelFormat.Format24bppRgb &&
                    bitmapForStats.PixelFormat != PixelFormat.Format32bppArgb &&
                    bitmapForStats.PixelFormat != PixelFormat.Format8bppIndexed) // Grayscale
                {
                    Bitmap tempBitmap = new Bitmap(bitmapForStats.Width, bitmapForStats.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(tempBitmap))
                    {
                        g.DrawImage(bitmapForStats, 0, 0);
                    }
                    bitmapForStats.Dispose(); // Dispose the previous clone
                    bitmapForStats = tempBitmap; // Assign the new 24bppRgb bitmap
                }

                AForge.Imaging.ImageStatistics stats = new AForge.Imaging.ImageStatistics(bitmapForStats);

                if (stats.IsGrayscale)
                {
                    DisplayHistogram(chart1, stats.Gray, "Gray", Color.Gray);
                    if (chart2.Series != null) chart2.Series.Clear();
                    if (chart3.Series != null) chart3.Series.Clear();
                }
                else
                {
                    DisplayHistogram(chart1, stats.Red, "Red", Color.Red);
                    DisplayHistogram(chart2, stats.Green, "Green", Color.Green);
                    DisplayHistogram(chart3, stats.Blue, "Blue", Color.Blue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating histograms for video frame: " + ex.Message);
                // Optionally, display a message to the user or log more formally
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
            }
            finally
            {
                // Dispose the bitmapForStats if it was created/cloned
                bitmapForStats?.Dispose();
            }
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (!isPlaying || processedFrames == null || processedFrames.Count == 0)
            {
                return;
            }

            currentFrameIndex++;

            if (currentFrameIndex >= processedFrames.Count) // End of video
            {
                currentFrameIndex = 0; // Loop back to the beginning
                playbackTimer.Stop();
                isPlaying = false;
                isPaused = false; 
                BTN_Continuar.Text = "Continuar";
                
                currentFrame?.Dispose();
                currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
            }
            else
            {
                currentFrame?.Dispose();
                currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
            }

            pictureBox1.Image = currentFrame;
            UpdateHistograms();
        }

        private void BTN_Continuar_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) return;

            if (isPlaying) // Video is playing, so pause it
            {
                playbackTimer.Stop();
                isPlaying = false;
                isPaused = true;
                BTN_Continuar.Text = "Continuar";
            }
            else // Video is paused or stopped, so play it
            {
                playbackTimer.Start();
                isPlaying = true;
                isPaused = false;
                BTN_Continuar.Text = "Pausar";
            }
        }

        private void BTN_Parar_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) return;

            playbackTimer.Stop();
            isPlaying = false;
            isPaused = false;
            currentFrameIndex = 0;

            if (processedFrames.Count > 0)
            {
                currentFrame?.Dispose();
                currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                pictureBox1.Image = currentFrame;
                UpdateHistograms();
            }
            BTN_Continuar.Text = "Continuar";
        }

        private void BTN_Retroceder_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) return;

            playbackTimer.Stop();
            isPlaying = false;
            isPaused = false;
            currentFrameIndex = 0;

            if (processedFrames.Count > 0)
            {
                currentFrame?.Dispose();
                currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                pictureBox1.Image = currentFrame;
                UpdateHistograms();
            }
            BTN_Continuar.Text = "Continuar";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.avi;*.mp4;*.mov;*.wmv|All Files|*.*",
                Title = "Seleccionar Video"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Dispose existing resources
                    if (videoReader != null)
                    {
                        videoReader.Close();
                        videoReader.Dispose();
                        videoReader = null;
                    }
                    if (currentFrame != null)
                    {
                        currentFrame.Dispose();
                        currentFrame = null;
                    }
                    originalFrames.ForEach(f => f.Dispose());
                    originalFrames.Clear();
                    processedFrames.ForEach(f => f.Dispose());
                    processedFrames.Clear();

                    isPlaying = false;
                    isPaused = false;
                    currentFrameIndex = 0;
                    BTN_Continuar.Text = "Continuar"; // Assuming BTN_Continuar is the play/pause button

                    if (playbackTimer.Enabled)
                    {
                        playbackTimer.Stop();
                    }

                    videoReader = new VideoFileReader();
                    videoReader.Open(openFileDialog.FileName);

                    videoFrameRate = videoReader.FrameRate.ToDouble();
                    videoWidth = videoReader.Width;
                    videoHeight = videoReader.Height;

                    if (videoFrameRate > 0)
                    {
                        playbackTimer.Interval = (int)(1000 / videoFrameRate);
                    }
                    else
                    {
                        // Default interval if frame rate is invalid
                        playbackTimer.Interval = 100; 
                    }
                    
                    for (int i = 0; i < videoReader.FrameCount; i++)
                    {
                        Bitmap frame = videoReader.ReadVideoFrame();
                        if (frame != null)
                        {
                            originalFrames.Add(frame);
                            processedFrames.Add((Bitmap)frame.Clone());
                        }
                        else
                        {
                            // Optional: Log or handle null frame if necessary
                            break; 
                        }
                    }

                    if (originalFrames.Count > 0)
                    {
                        currentFrame = (Bitmap)originalFrames[0].Clone();
                        pictureBox1.Image = currentFrame;
                        // TODO: Enable relevant buttons (e.g., Play, Stop, filter buttons)
                    }
                    else
                    {
                        MessageBox.Show("No se pudieron leer los fotogramas del video.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    UpdateHistograms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al abrir o leer el video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Ensure cleanup even on error
                    if (videoReader != null)
                    {
                        videoReader.Close();
                        videoReader.Dispose();
                        videoReader = null;
                    }
                    originalFrames.ForEach(f => f.Dispose());
                    originalFrames.Clear();
                    processedFrames.ForEach(f => f.Dispose());
                    processedFrames.Clear();
                    if (currentFrame != null)
                    {
                        currentFrame.Dispose();
                        currentFrame = null;
                    }
                    pictureBox1.Image = null;
                    UpdateHistograms();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WaterWave waterWaveFilter = new WaterWave();
            waterWaveFilter.HorizontalWavesCount = 10;
            waterWaveFilter.HorizontalWavesAmplitude = 5;
            waterWaveFilter.VerticalWavesCount = 3;
            waterWaveFilter.VerticalWavesAmplitude = 15;
            ApplyFilterToAllFrames(waterWaveFilter);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) { MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            bool wasPlaying = isPlaying; if (isPlaying) { playbackTimer.Stop(); isPlaying = false; }
            processedFrames.ForEach(f => f.Dispose()); processedFrames.Clear();
            Cursor = Cursors.WaitCursor;
            try
            {
                SaltAndPepperNoise noiseFilter = new SaltAndPepperNoise();
                noiseFilter.NoiseAmount = 0.05;
                foreach (Bitmap originalFrame in originalFrames)
                {
                    Bitmap clonedFrame = (Bitmap)originalFrame.Clone(); // Clone for inplace filter
                    noiseFilter.ApplyInPlace(clonedFrame);
                    processedFrames.Add(clonedFrame);
                }
                if (currentFrameIndex < processedFrames.Count) { currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
                else if (processedFrames.Count > 0) { currentFrameIndex = 0; currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[0].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Error aplicando el filtro: {ex.Message}", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Restore processedFrames from originalFrames
                processedFrames.ForEach(f => f.Dispose());
                processedFrames.Clear();
                if (originalFrames != null)
                {
                    foreach (Bitmap originalBitmap in originalFrames)
                    {
                        processedFrames.Add((Bitmap)originalBitmap.Clone());
                    }
                }
                // Restore currentFrame display from the (now restored) processedFrames
                if (processedFrames.Count > 0) // Check if processedFrames has content after potential restoration
                {
                    if (currentFrameIndex >= processedFrames.Count) // Adjust index if it's now out of bounds
                    {
                        currentFrameIndex = 0;
                    }
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else // If originalFrames was also empty or null, there's nothing to show
                {
                     pictureBox1.Image = null; // Or a placeholder image
                }
            }
            finally { Cursor = Cursors.Default; }
            if (wasPlaying) { playbackTimer.Start(); isPlaying = true; }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) { MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            bool wasPlaying = isPlaying; if (isPlaying) { playbackTimer.Stop(); isPlaying = false; }
            processedFrames.ForEach(f => f.Dispose()); processedFrames.Clear();
            Cursor = Cursors.WaitCursor;
            try
            {
                GaussianBlur blurFilter = new GaussianBlur();
                blurFilter.Sigma = 2;
                blurFilter.Size = 5;
                foreach (Bitmap originalFrame in originalFrames)
                {
                    Bitmap clonedFrame = (Bitmap)originalFrame.Clone();
                    blurFilter.ApplyInPlace(clonedFrame);
                    processedFrames.Add(clonedFrame);
                }
                if (currentFrameIndex < processedFrames.Count) { currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
                else if (processedFrames.Count > 0) { currentFrameIndex = 0; currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[0].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Error aplicando el filtro: {ex.Message}", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Restore processedFrames from originalFrames
                processedFrames.ForEach(f => f.Dispose());
                processedFrames.Clear();
                if (originalFrames != null)
                {
                    foreach (Bitmap originalBitmap in originalFrames)
                    {
                        processedFrames.Add((Bitmap)originalBitmap.Clone());
                    }
                }
                // Restore currentFrame display from the (now restored) processedFrames
                if (processedFrames.Count > 0)
                {
                    if (currentFrameIndex >= processedFrames.Count)
                    {
                        currentFrameIndex = 0;
                    }
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else
                {
                     pictureBox1.Image = null;
                }
            }
            finally { Cursor = Cursors.Default; }
            if (wasPlaying) { playbackTimer.Start(); isPlaying = true; }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (originalFrames == null || originalFrames.Count == 0) { MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            bool wasPlaying = isPlaying; if (isPlaying) { playbackTimer.Stop(); isPlaying = false; }
            processedFrames.ForEach(f => f.Dispose()); processedFrames.Clear();
            Cursor = Cursors.WaitCursor;
            try
            {
                Invert invertFilter = new Invert();
                foreach (Bitmap originalFrame in originalFrames)
                {
                    Bitmap clonedFrame = (Bitmap)originalFrame.Clone();
                    invertFilter.ApplyInPlace(clonedFrame);
                    processedFrames.Add(clonedFrame);
                }
                if (currentFrameIndex < processedFrames.Count) { currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
                else if (processedFrames.Count > 0) { currentFrameIndex = 0; currentFrame?.Dispose(); currentFrame = (Bitmap)processedFrames[0].Clone(); pictureBox1.Image = currentFrame; UpdateHistograms(); }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Error aplicando el filtro: {ex.Message}", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Restore processedFrames from originalFrames
                processedFrames.ForEach(f => f.Dispose());
                processedFrames.Clear();
                if (originalFrames != null)
                {
                    foreach (Bitmap originalBitmap in originalFrames)
                    {
                        processedFrames.Add((Bitmap)originalBitmap.Clone());
                    }
                }
                // Restore currentFrame display from the (now restored) processedFrames
                if (processedFrames.Count > 0)
                {
                    if (currentFrameIndex >= processedFrames.Count)
                    {
                        currentFrameIndex = 0;
                    }
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else
                {
                     pictureBox1.Image = null;
                }
            }
            finally { Cursor = Cursors.Default; }
            if (wasPlaying) { playbackTimer.Start(); isPlaying = true; }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (processedFrames == null || processedFrames.Count == 0)
            {
                MessageBox.Show("No hay video para guardar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "AVI Video|*.avi|MP4 Video|*.mp4|All Files|*.*",
                Title = "Guardar Video Como",
                FileName = "processed_video.avi"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                videoWriter = new VideoFileWriter();
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Ensure video properties are valid before opening the writer
                    if (videoWidth <= 0 || videoHeight <= 0 || videoFrameRate <= 0)
                    {
                        MessageBox.Show("Las propiedades del video (ancho, alto, FPS) no son válidas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    videoWriter.Open(filePath, videoWidth, videoHeight, (int)Math.Round(videoFrameRate), Accord.Video.FFMPEG.VideoCodec.MPEG4, 1000000);

                    foreach (Bitmap frame in processedFrames)
                    {
                        videoWriter.WriteVideoFrame(frame);
                    }
                    MessageBox.Show("Video guardado exitosamente!", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar el video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (videoWriter != null)
                    {
                        videoWriter.Close();
                        videoWriter.Dispose();
                        videoWriter = null;
                    }
                }
            }
        }
    }
}
