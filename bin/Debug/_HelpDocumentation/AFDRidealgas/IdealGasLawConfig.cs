// Copyright (C) 2004-2011 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OSIsoft.AF.Asset;

namespace OSIsoft.AF.Asset.DataReference
{	
	internal class AFIdealGasDRConfig : System.Windows.Forms.Form
	{
		private IdealGasLaw dataReference=null;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtMW;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

//'<filename:Constructor>
public AFIdealGasDRConfig(IdealGasLaw dataReference, bool bReadOnly) : base()
{
	// Required for Windows Form Designer support
	InitializeComponent();
    mOriginalFont = this.Font;

	// save for persistence
	this.dataReference = dataReference;

	if (bReadOnly)
	{
		btnOK.Visible = false;
		btnOK.Enabled = false;
		btnCancel.Left = (btnOK.Left + btnCancel.Left) / 2;
		btnCancel.Text = "Close";
		this.AcceptButton = btnCancel;

		// Set Molecular Weight ReadOnly Property
		txtMW.ReadOnly = true;
	}

	// Initialize the text field of the Configuration Dialog
	this.txtMW.Text = dataReference.MolecularWeight.ToString(CultureInfo.CurrentCulture);
}
//'</filename:Constructor>

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AFIdealGasDRConfig));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMW = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtMW
            // 
            resources.ApplyResources(this.txtMW, "txtMW");
            this.txtMW.Name = "txtMW";
            // 
            // AFIdealGasDRConfig
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtMW);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AFIdealGasDRConfig";
            this.ShowInTaskbar = false;
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

//'<filename:GetValuesFromForm>
private bool GetValuesFromForm()
{
	try
	{
		double dMolecularWeight;
			
		try
		{
			dMolecularWeight = Convert.ToDouble(txtMW.Text, CultureInfo.CurrentCulture); // Must be a double.
		}
		catch
		{
			txtMW.Focus();
			MessageBox.Show("The molecular weight must be a double.", "Ideal Gas Law");
			return false;
		}
			
		dataReference.MolecularWeight = dMolecularWeight;
	}
	catch (Exception ex)
	{
		MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "Unable to apply changes: {0}", ex.Message), "Error");
		return false;
	}
	return true;
}
//'</filename:GetValuesFromForm>
}
}
