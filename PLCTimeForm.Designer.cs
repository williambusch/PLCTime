using System;

namespace PLCTime
{
    partial class PLCTimeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.formatRadio = new System.Windows.Forms.RadioButton();
            this.convertRadio = new System.Windows.Forms.RadioButton();
            this.okBtn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // formatRadio
            // 
            this.formatRadio.AutoSize = true;
            this.formatRadio.Location = new System.Drawing.Point(74, 47);
            this.formatRadio.Name = "formatRadio";
            this.formatRadio.Size = new System.Drawing.Size(305, 17);
            this.formatRadio.TabIndex = 0;
            this.formatRadio.TabStop = true;
            this.formatRadio.Text = "Date as 2 separate values: Time(HHMM) and Date(MMDD)";
            this.formatRadio.UseVisualStyleBackColor = true;
            this.formatRadio.CheckedChanged += new System.EventHandler(this.Radio_CheckedChanged);
            // 
            // convertRadio
            // 
            this.convertRadio.AutoSize = true;
            this.convertRadio.Location = new System.Drawing.Point(74, 70);
            this.convertRadio.Name = "convertRadio";
            this.convertRadio.Size = new System.Drawing.Size(276, 17);
            this.convertRadio.TabIndex = 1;
            this.convertRadio.TabStop = true;
            this.convertRadio.Text = "Date as 1 value: Number of Minutes since Initial Date";
            this.convertRadio.UseVisualStyleBackColor = true;
            this.convertRadio.CheckedChanged += new System.EventHandler(this.Radio_CheckedChanged);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(191, 308);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(189, 52);
            this.okBtn.TabIndex = 2;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(74, 168);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(339, 20);
            this.textBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(74, 239);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(339, 20);
            this.textBox2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(74, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            // 
            // PLCTimeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 381);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.convertRadio);
            this.Controls.Add(this.formatRadio);
            this.Name = "PLCTimeForm";
            this.Text = "PLCTimeForm";
            this.Load += new System.EventHandler(this.PLCTimeForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton formatRadio;
        private System.Windows.Forms.RadioButton convertRadio;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}