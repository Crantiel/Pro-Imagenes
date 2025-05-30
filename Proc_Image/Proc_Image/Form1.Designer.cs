namespace Proc_Image
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.Foto_Boton = new System.Windows.Forms.Button();
            this.Video_Boton = new System.Windows.Forms.Button();
            this.Camara_Boton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Gabriola", 28.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Image = global::Proc_Image.Properties.Resources.Brown_and_Orange_Elegant_Simple_Young_Adult_Fantasy_Book_Cover;
            this.label1.Location = new System.Drawing.Point(117, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(301, 87);
            this.label1.TabIndex = 0;
            this.label1.Text = "Menú de Camara";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Foto_Boton
            // 
            this.Foto_Boton.AutoSize = true;
            this.Foto_Boton.BackgroundImage = global::Proc_Image.Properties.Resources._7fa2a2b004ceb661137e13f0defa391c;
            this.Foto_Boton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Foto_Boton.Font = new System.Drawing.Font("MingLiU_HKSCS-ExtB", 22.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Foto_Boton.Location = new System.Drawing.Point(176, 233);
            this.Foto_Boton.Name = "Foto_Boton";
            this.Foto_Boton.Size = new System.Drawing.Size(167, 56);
            this.Foto_Boton.TabIndex = 1;
            this.Foto_Boton.Text = "Foto";
            this.Foto_Boton.UseVisualStyleBackColor = true;
            // 
            // Video_Boton
            // 
            this.Video_Boton.AutoSize = true;
            this.Video_Boton.BackgroundImage = global::Proc_Image.Properties.Resources._7fa2a2b004ceb661137e13f0defa391c;
            this.Video_Boton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Video_Boton.Font = new System.Drawing.Font("MingLiU_HKSCS-ExtB", 22.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Video_Boton.Location = new System.Drawing.Point(176, 308);
            this.Video_Boton.Name = "Video_Boton";
            this.Video_Boton.Size = new System.Drawing.Size(167, 56);
            this.Video_Boton.TabIndex = 2;
            this.Video_Boton.Text = "Video";
            this.Video_Boton.UseVisualStyleBackColor = true;
            // 
            // Camara_Boton
            // 
            this.Camara_Boton.AutoSize = true;
            this.Camara_Boton.BackgroundImage = global::Proc_Image.Properties.Resources._7fa2a2b004ceb661137e13f0defa391c;
            this.Camara_Boton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Camara_Boton.Font = new System.Drawing.Font("MingLiU_HKSCS-ExtB", 22.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Camara_Boton.Location = new System.Drawing.Point(176, 389);
            this.Camara_Boton.Name = "Camara_Boton";
            this.Camara_Boton.Size = new System.Drawing.Size(167, 56);
            this.Camara_Boton.TabIndex = 3;
            this.Camara_Boton.Text = "Camara";
            this.Camara_Boton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.BackgroundImage = global::Proc_Image.Properties.Resources.Brown_and_Orange_Elegant_Simple_Young_Adult_Fantasy_Book_Cover;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(519, 712);
            this.Controls.Add(this.Camara_Boton);
            this.Controls.Add(this.Video_Boton);
            this.Controls.Add(this.Foto_Boton);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.TransparencyKey = System.Drawing.Color.Black;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Foto_Boton;
        private System.Windows.Forms.Button Video_Boton;
        private System.Windows.Forms.Button Camara_Boton;
    }
}

