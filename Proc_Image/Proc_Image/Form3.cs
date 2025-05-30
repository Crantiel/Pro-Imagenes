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
        private List<Bitmap> originalFrames;
        private List<Bitmap> processedFrames;
        private System.Windows.Forms.Timer playbackTimer;
        private bool isPlaying = false;
        private bool isPaused = false;
        private int currentFrameIndex = 0;
        private double videoFrameRate = 0;
        private int videoWidth = 0;
        private int videoHeight = 0;

        // Real-time preview members
        private bool realTimePreviewEnabled = false;
        private IFilter activeRealTimeFilter = null;
        private CheckBox chkRealTimePreview;

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

            // Programmatically create and add the CheckBox
            this.chkRealTimePreview = new System.Windows.Forms.CheckBox();
            this.chkRealTimePreview.AutoSize = true;
            this.chkRealTimePreview.Location = new System.Drawing.Point(903, 450); // Adjusted position
            this.chkRealTimePreview.Name = "chkRealTimePreview";
            this.chkRealTimePreview.Size = new System.Drawing.Size(180, 21); // Approx size, AutoSize should adjust
            this.chkRealTimePreview.TabIndex = 28; // After Cargar (27) and Guardar (26) - wait, Guardar is 26, Cargar 27. So this should be 28.
            this.chkRealTimePreview.Text = "Vista Previa en Tiempo Real";
            this.chkRealTimePreview.UseVisualStyleBackColor = true;
            this.chkRealTimePreview.CheckedChanged += new System.EventHandler(this.chkRealTimePreview_CheckedChanged);
            this.Controls.Add(this.chkRealTimePreview);
        }

        private void chkRealTimePreview_CheckedChanged(object sender, EventArgs e)
        {
            this.realTimePreviewEnabled = this.chkRealTimePreview.Checked;
            if (!this.realTimePreviewEnabled)
            {
                this.activeRealTimeFilter = null; // Clear active preview filter when disabling
                // If video was playing, it will now use processedFrames.
                // If it was paused, it remains paused.
                // We might want to refresh the current frame to show the non-realtime version.
                if (processedFrames != null && processedFrames.Count > 0 && currentFrameIndex < processedFrames.Count)
                {
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
                else if (originalFrames != null && originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    // Fallback if processedFrames is empty but original is not (e.g. before any filter applied)
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)originalFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            else
            {
                // When enabling, if a filter was last applied, it's not automatically set as activeRealTimeFilter.
                // User needs to click a filter button again to make it active for real-time.
                // Refresh current frame to show original if real-time is on but no filter active yet.
                if (this.activeRealTimeFilter == null && originalFrames != null && originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)originalFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
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

            // When a full filter application is done, disable real-time preview for that filter.
            this.activeRealTimeFilter = null;
            if (this.chkRealTimePreview != null) { this.chkRealTimePreview.Checked = false; }
            this.realTimePreviewEnabled = false;


            bool wasPlaying = isPlaying;
            if (isPlaying)
            {
                playbackTimer.Stop();
                isPlaying = false;
                BTN_Continuar.Text = "Continuar";
            }

            processedFrames.ForEach(f => f.Dispose());
            processedFrames.Clear();

            Cursor = Cursors.WaitCursor;

            try
            {
                for (int i = 0; i < originalFrames.Count; i++)
                {
                    Bitmap originalBitmap = originalFrames[i];
                    Bitmap processedBitmap;

                    if (filter is IInPlaceFilter inPlaceFilter)
                    {
                        Bitmap clonedFrame = (Bitmap)originalBitmap.Clone();
                        inPlaceFilter.ApplyInPlace(clonedFrame);
                        processedBitmap = clonedFrame;
                    }
                    else
                    {
                        processedBitmap = filter.Apply(originalBitmap);
                    }
                    processedFrames.Add(processedBitmap);
                }

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
                    currentFrame?.Dispose();
                    currentFrame = null;
                    pictureBox1.Image = null;
                    UpdateHistograms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error aplicando el filtro: {ex.Message}", "Error de Filtro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                processedFrames.ForEach(f => f.Dispose());
                processedFrames.Clear();
                foreach (Bitmap originalBitmap in originalFrames)
                {
                    processedFrames.Add((Bitmap)originalBitmap.Clone());
                }

                if (processedFrames.Count > 0)
                {
                    if (currentFrameIndex >= processedFrames.Count) currentFrameIndex = 0;
                    currentFrame?.Dispose();
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                    pictureBox1.Image = currentFrame;
                }
                else
                {
                    pictureBox1.Image = null;
                }
                UpdateHistograms();
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (wasPlaying)
            {
                playbackTimer.Start();
                isPlaying = true;
                BTN_Continuar.Text = "Pausar";
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
            chartControl.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chartControl.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartControl.ChartAreas[0].RecalculateAxesScale();
        }

        private void UpdateHistograms()
        {
            if (currentFrame == null || pictureBox1.Image == null)
            {
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
                return;
            }

            Bitmap bitmapForStats = null;
            try
            {
                bitmapForStats = (Bitmap)currentFrame.Clone();
                if (bitmapForStats.PixelFormat != PixelFormat.Format24bppRgb &&
                    bitmapForStats.PixelFormat != PixelFormat.Format32bppArgb &&
                    bitmapForStats.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    Bitmap tempBitmap = new Bitmap(bitmapForStats.Width, bitmapForStats.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(tempBitmap))
                    {
                        g.DrawImage(bitmapForStats, 0, 0);
                    }
                    bitmapForStats.Dispose();
                    bitmapForStats = tempBitmap;
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
                if (chart1.Series != null) chart1.Series.Clear();
                if (chart2.Series != null) chart2.Series.Clear();
                if (chart3.Series != null) chart3.Series.Clear();
            }
            finally
            {
                bitmapForStats?.Dispose();
            }
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (!isPlaying) return;

            List<Bitmap> sourceListForTick = (realTimePreviewEnabled && activeRealTimeFilter != null) ? originalFrames : (realTimePreviewEnabled ? originalFrames : processedFrames);

            if (sourceListForTick == null || sourceListForTick.Count == 0)
            {
                // This case might happen if realTimePreviewEnabled is true, but originalFrames is empty.
                // Or if not realTimePreviewEnabled, but processedFrames is empty.
                playbackTimer.Stop();
                isPlaying = false;
                isPaused = false;
                BTN_Continuar.Text = "Continuar";
                return;
            }

            currentFrameIndex++;

            if (currentFrameIndex >= sourceListForTick.Count)
            {
                currentFrameIndex = 0;
                // Stop if not in real-time preview with an active filter, or if simply looping processed frames.
                // If realTimePreviewEnabled AND activeRealTimeFilter IS NOT NULL, it means we want to loop the preview.
                if (!realTimePreviewEnabled || activeRealTimeFilter == null)
                {
                    playbackTimer.Stop();
                    isPlaying = false;
                    isPaused = false;
                    BTN_Continuar.Text = "Continuar";
                }
                // If it stops, display the first frame. If it loops for real-time preview, it will also display the first frame (of the preview).
            }

            currentFrame?.Dispose(); // Dispose the old currentFrame

            if (realTimePreviewEnabled && activeRealTimeFilter != null)
            {
                if (currentFrameIndex < originalFrames.Count) // Ensure index is valid for originalFrames
                {
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    if (activeRealTimeFilter is IInPlaceFilter inPlaceFilter)
                    {
                        inPlaceFilter.ApplyInPlace(frameToProcess);
                        currentFrame = frameToProcess;
                    }
                    else // IFilter
                    {
                        currentFrame = activeRealTimeFilter.Apply(frameToProcess);
                        frameToProcess.Dispose(); // Dispose the clone as Apply returns a new bitmap
                    }
                }
                else if (originalFrames.Count > 0) // Fallback if index became invalid somehow
                {
                    currentFrameIndex = 0; // Reset to be safe
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    if (activeRealTimeFilter is IInPlaceFilter inPlaceFilter) { inPlaceFilter.ApplyInPlace(frameToProcess); currentFrame = frameToProcess; }
                    else { currentFrame = activeRealTimeFilter.Apply(frameToProcess); frameToProcess.Dispose(); }

                }
                else
                { // No original frames to preview
                    playbackTimer.Stop(); isPlaying = false; isPaused = false; BTN_Continuar.Text = "Continuar"; return;
                }
            }
            else if (realTimePreviewEnabled) // Real-time preview enabled, but no active filter (show original)
            {
                if (currentFrameIndex < originalFrames.Count)
                {
                    currentFrame = (Bitmap)originalFrames[currentFrameIndex].Clone();
                }
                else if (originalFrames.Count > 0)
                {
                    currentFrameIndex = 0; // Reset to be safe
                    currentFrame = (Bitmap)originalFrames[currentFrameIndex].Clone();
                }
                else
                { // No original frames
                    playbackTimer.Stop(); isPlaying = false; isPaused = false; BTN_Continuar.Text = "Continuar"; return;
                }
            }
            else // Not in real-time preview (show processed frames)
            {
                if (currentFrameIndex < processedFrames.Count)
                {
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                }
                else if (processedFrames.Count > 0)
                {
                    currentFrameIndex = 0; // Reset to be safe
                    currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                }
                else
                { // No processed frames
                    playbackTimer.Stop(); isPlaying = false; isPaused = false; BTN_Continuar.Text = "Continuar"; return;
                }
            }

            if (currentFrame != null)
            {
                pictureBox1.Image = currentFrame;
                UpdateHistograms();
            }
            else
            {
                // Handle case where currentFrame could not be set (e.g. sourceListForTick was empty after all checks)
                playbackTimer.Stop();
                isPlaying = false;
                isPaused = false;
                BTN_Continuar.Text = "Continuar";
                pictureBox1.Image = null;
                UpdateHistograms();
            }
        }


        private void BTN_Continuar_Click(object sender, EventArgs e)
        {
            List<Bitmap> relevantFrames = realTimePreviewEnabled ? originalFrames : processedFrames;
            if (relevantFrames == null || relevantFrames.Count == 0)
            {
                MessageBox.Show("No hay video cargado para reproducir.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isPlaying)
            {
                playbackTimer.Stop();
                isPlaying = false;
                isPaused = true;
                BTN_Continuar.Text = "Continuar";
            }
            else
            {
                if (currentFrameIndex == relevantFrames.Count - 1 && !isPaused)
                {
                    currentFrameIndex = -1;
                }
                playbackTimer.Start();
                isPlaying = true;
                isPaused = false;
                BTN_Continuar.Text = "Pausar";
            }
        }

        private void BTN_Parar_Click(object sender, EventArgs e)
        {
            List<Bitmap> sourceList = realTimePreviewEnabled ? originalFrames : processedFrames;
            if (sourceList == null || sourceList.Count == 0) return;

            playbackTimer.Stop();
            isPlaying = false;
            isPaused = false;
            currentFrameIndex = 0;

            currentFrame?.Dispose();
            if (realTimePreviewEnabled && activeRealTimeFilter != null)
            {
                Bitmap originalTickFrame = sourceList[currentFrameIndex];
                Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                if (activeRealTimeFilter is IInPlaceFilter inPlaceFilter) { inPlaceFilter.ApplyInPlace(frameToProcess); currentFrame = frameToProcess; }
                else { currentFrame = activeRealTimeFilter.Apply(frameToProcess); frameToProcess.Dispose(); }
            }
            else
            {
                currentFrame = (Bitmap)sourceList[currentFrameIndex].Clone();
            }
            pictureBox1.Image = currentFrame;
            UpdateHistograms();
            BTN_Continuar.Text = "Continuar";
        }

        private void BTN_Retroceder_Click(object sender, EventArgs e)
        {
            List<Bitmap> sourceList = realTimePreviewEnabled ? originalFrames : processedFrames;
            if (sourceList == null || sourceList.Count == 0) return;

            playbackTimer.Stop(); // Stop playback
            isPlaying = false;
            isPaused = true; // Effectively paused at the beginning
            currentFrameIndex = 0;

            currentFrame?.Dispose();
            if (realTimePreviewEnabled && activeRealTimeFilter != null)
            {
                Bitmap originalTickFrame = sourceList[currentFrameIndex];
                Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                if (activeRealTimeFilter is IInPlaceFilter inPlaceFilter) { inPlaceFilter.ApplyInPlace(frameToProcess); currentFrame = frameToProcess; }
                else { currentFrame = activeRealTimeFilter.Apply(frameToProcess); frameToProcess.Dispose(); }
            }
            else
            {
                currentFrame = (Bitmap)sourceList[currentFrameIndex].Clone();
            }
            pictureBox1.Image = currentFrame;
            UpdateHistograms();
            BTN_Continuar.Text = "Continuar"; // Ready to play from start
        }

        private void button11_Click(object sender, EventArgs e) // Load Video
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
                    if (videoReader != null) { videoReader.Close(); videoReader.Dispose(); videoReader = null; }
                    currentFrame?.Dispose(); currentFrame = null;
                    originalFrames.ForEach(f => f.Dispose()); originalFrames.Clear();
                    processedFrames.ForEach(f => f.Dispose()); processedFrames.Clear();

                    isPlaying = false; isPaused = false; currentFrameIndex = 0;
                    BTN_Continuar.Text = "Continuar";
                    if (playbackTimer.Enabled) { playbackTimer.Stop(); }

                    // Reset real-time preview state
                    this.activeRealTimeFilter = null;
                    if (this.chkRealTimePreview != null) { this.chkRealTimePreview.Checked = false; }
                    this.realTimePreviewEnabled = false;

                    videoReader = new VideoFileReader();
                    videoReader.Open(openFileDialog.FileName);
                    videoFrameRate = videoReader.FrameRate.ToDouble();
                    videoWidth = videoReader.Width; videoHeight = videoReader.Height;

                    if (videoFrameRate > 0 && !double.IsInfinity(videoFrameRate) && !double.IsNaN(videoFrameRate))
                    { playbackTimer.Interval = (int)(1000 / videoFrameRate); }
                    else { playbackTimer.Interval = 33; }

                    for (int i = 0; i < videoReader.FrameCount; i++)
                    {
                        Bitmap frame = videoReader.ReadVideoFrame();
                        if (frame != null)
                        {
                            originalFrames.Add(frame);
                            processedFrames.Add((Bitmap)frame.Clone());
                        }
                        else { break; }
                    }

                    if (processedFrames.Count > 0)
                    {
                        currentFrameIndex = 0;
                        currentFrame?.Dispose();
                        currentFrame = (Bitmap)processedFrames[currentFrameIndex].Clone();
                        pictureBox1.Image = currentFrame;
                    }
                    else { MessageBox.Show("No se pudieron leer los fotogramas del video.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    UpdateHistograms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al abrir o leer el video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (videoReader != null) { videoReader.Close(); videoReader.Dispose(); videoReader = null; }
                    originalFrames.ForEach(f => f.Dispose()); originalFrames.Clear();
                    processedFrames.ForEach(f => f.Dispose()); processedFrames.Clear();
                    currentFrame?.Dispose(); currentFrame = null;
                    pictureBox1.Image = null; UpdateHistograms();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) // WaterWave (IFilter)
        {
            if (originalFrames == null || originalFrames.Count == 0)
            {
                MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            WaterWave filter = new WaterWave();
            filter.HorizontalWavesCount = 10; filter.HorizontalWavesAmplitude = 5;
            filter.VerticalWavesCount = 3; filter.VerticalWavesAmplitude = 15;

            if (this.realTimePreviewEnabled)
            {
                this.activeRealTimeFilter = filter;
                // Refresh current frame with preview
                if (isPlaying && originalFrames.Count > 0) { /* PlaybackTimer_Tick will handle */ }
                else if (originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    currentFrame?.Dispose();
                    currentFrame = activeRealTimeFilter.Apply(frameToProcess); // WaterWave is IFilter
                    frameToProcess.Dispose();
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            else
            {
                this.activeRealTimeFilter = null;
                ApplyFilterToAllFrames(filter);
            }
        }

        private void button2_Click(object sender, EventArgs e) // SaltAndPepper (IInPlaceFilter)
        {
            if (originalFrames == null || originalFrames.Count == 0)
            {
                MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            SaltAndPepperNoise filter = new SaltAndPepperNoise();
            filter.NoiseAmount = 0.05;

            if (this.realTimePreviewEnabled)
            {
                this.activeRealTimeFilter = filter;
                if (isPlaying && originalFrames.Count > 0) { /* PlaybackTimer_Tick will handle */ }
                else if (originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    currentFrame?.Dispose();
                    ((IInPlaceFilter)activeRealTimeFilter).ApplyInPlace(frameToProcess); // SaltAndPepper is IInPlaceFilter
                    currentFrame = frameToProcess;
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            else
            {
                this.activeRealTimeFilter = null;
                ApplyFilterToAllFrames(filter);
            }
        }

        private void button8_Click(object sender, EventArgs e) // GaussianBlur (IInPlaceFilter)
        {
            if (originalFrames == null || originalFrames.Count == 0)
            {
                MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            GaussianBlur filter = new GaussianBlur();
            filter.Sigma = 2; filter.Size = 5;

            if (this.realTimePreviewEnabled)
            {
                this.activeRealTimeFilter = filter;
                if (isPlaying && originalFrames.Count > 0) { /* PlaybackTimer_Tick will handle */ }
                else if (originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    currentFrame?.Dispose();
                    ((IInPlaceFilter)activeRealTimeFilter).ApplyInPlace(frameToProcess);
                    currentFrame = frameToProcess;
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            else
            {
                this.activeRealTimeFilter = null;
                ApplyFilterToAllFrames(filter);
            }
        }

        private void button7_Click(object sender, EventArgs e) // Invert (IInPlaceFilter)
        {
            if (originalFrames == null || originalFrames.Count == 0)
            {
                MessageBox.Show("Por favor, cargue un video primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            Invert filter = new Invert();

            if (this.realTimePreviewEnabled)
            {
                this.activeRealTimeFilter = filter;
                if (isPlaying && originalFrames.Count > 0) { /* PlaybackTimer_Tick will handle */ }
                else if (originalFrames.Count > 0 && currentFrameIndex < originalFrames.Count)
                {
                    Bitmap originalTickFrame = originalFrames[currentFrameIndex];
                    Bitmap frameToProcess = (Bitmap)originalTickFrame.Clone();
                    currentFrame?.Dispose();
                    ((IInPlaceFilter)activeRealTimeFilter).ApplyInPlace(frameToProcess);
                    currentFrame = frameToProcess;
                    pictureBox1.Image = currentFrame;
                    UpdateHistograms();
                }
            }
            else
            {
                this.activeRealTimeFilter = null;
                ApplyFilterToAllFrames(filter);
            }
        }

        private void button12_Click(object sender, EventArgs e) // Save Video
        {
            if (processedFrames == null || processedFrames.Count == 0)
            {
                MessageBox.Show("No hay video procesado para guardar. Aplique un filtro primero si desea guardar los cambios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (realTimePreviewEnabled && activeRealTimeFilter != null)
            {
                MessageBox.Show("Desactive la 'Vista Previa en Tiempo Real' y aplique el filtro de forma permanente antes de guardar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                { MessageBox.Show("Error al guardar el video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally
                {
                    if (videoWriter != null) { videoWriter.Close(); videoWriter.Dispose(); videoWriter = null; }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
