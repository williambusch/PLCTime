// Copyright (C) 2009-2011 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;
using OSIsoft.AF.UnitsOfMeasure;
using OSIsoft.AF.Asset.DataReference.Properties;

namespace OSIsoft.AF.Asset.DataReference
{	
	internal class RollupConfig : System.Windows.Forms.Form
	{
		private Rollup dataReference=null;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ComboBox cboCalculations;
        private System.Windows.Forms.Label lblCalculation;
		private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategories;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RollupConfig(Rollup dataReference, bool bReadOnly) : base()
		{
			// Required for Windows Form Designer support
			InitializeComponent();
            mOriginalFont = this.Font;

			// save for persitance
			this.dataReference = dataReference;

			if (bReadOnly)
			{
				cboCategories.Enabled = false;
				cboCalculations.Enabled = false;
				btnOK.Visible = false;
				btnOK.Enabled = false;
				btnCancel.Left = (btnOK.Left + btnCancel.Left) / 2;
				btnCancel.Text = "Close";
				this.AcceptButton = btnCancel;
			}

			AFCategories categories = null;
			if (dataReference.Attribute != null)
			{
				categories = dataReference.Attribute.Database.AttributeCategories;
			}
			else if (dataReference.Template != null)
			{
				categories = dataReference.Template.Database.AttributeCategories;
			}
			foreach(AFCategory cat in categories)
			{
				cboCategories.Items.Add(cat.Name);
			}
			if (cboCategories.Items.Count > 0) cboCategories.SelectedIndex = 0;

			cboCalculations.Items.Add("Avg");
			cboCalculations.Items.Add("Max");
			cboCalculations.Items.Add("Min");
			cboCalculations.Items.Add("Sum");
			if (cboCalculations.Items.Count > 0) cboCalculations.SelectedIndex = 0;

			if (dataReference.CategoryName != null &&
				cboCategories.Items.Contains(dataReference.CategoryName))
			{
				cboCategories.Text = dataReference.CategoryName;
			}
			if (dataReference.Calculation != null &&
				cboCalculations.Items.Contains(dataReference.Calculation))
			{
				cboCalculations.Text = dataReference.Calculation;
			}
		}

        private Font mOriginalFont;
        protected void SetDialogFont()
        {
            if (!DesignMode)
            {
                // Don't set the dialog font if the user has set a different font.
                if (Font.Equals(mOriginalFont, this.Font)) AFLocalization.SetDialogFont(this);
                PerformAutoScale();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            SetDialogFont();
            base.OnLoad(e);
        }

        /// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategories = new System.Windows.Forms.ComboBox();
            this.cboCalculations = new System.Windows.Forms.ComboBox();
            this.lblCalculation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(158, 100);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(239, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(0, 8);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "Category:";
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCategories
            // 
            this.cboCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategories.Location = new System.Drawing.Point(102, 8);
            this.cboCategories.Name = "cboCategories";
            this.cboCategories.Size = new System.Drawing.Size(216, 21);
            this.cboCategories.Sorted = true;
            this.cboCategories.TabIndex = 1;
            // 
            // cboCalculations
            // 
            this.cboCalculations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCalculations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCalculations.Location = new System.Drawing.Point(102, 40);
            this.cboCalculations.Name = "cboCalculations";
            this.cboCalculations.Size = new System.Drawing.Size(216, 21);
            this.cboCalculations.Sorted = true;
            this.cboCalculations.TabIndex = 3;
            // 
            // lblCalculation
            // 
            this.lblCalculation.AutoSize = true;
            this.lblCalculation.Location = new System.Drawing.Point(0, 40);
            this.lblCalculation.Name = "lblCalculation";
            this.lblCalculation.Size = new System.Drawing.Size(62, 13);
            this.lblCalculation.TabIndex = 2;
            this.lblCalculation.Text = "Calculation:";
            this.lblCalculation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // RollupConfig
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(326, 135);
            this.Controls.Add(this.cboCalculations);
            this.Controls.Add(this.lblCalculation);
            this.Controls.Add(this.cboCategories);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RollupConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rollup Data Reference";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			// save the settings
			if (!GetValuesFromForm())
			{
				// don't close if validation error
				DialogResult = DialogResult.None;
			}
		}

		private bool GetValuesFromForm()
		{
			try
			{
				this.dataReference.CategoryName = this.cboCategories.Text;
				this.dataReference.Calculation = this.cboCalculations.Text;
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Resources.ERR_UnableToApplyChanges, ex.Message), Resources.TITLE_Error);
				return false;
			}
			return true;
		}
	}
}