// Copyright (C) 2009-2010 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;

namespace OSIsoft.AF
{
	/// <summary>
	/// This class provides helper methods that can be built into various assemblies
    /// to handle localization issues.
	/// </summary>
    internal static class AFLocalization
    {
        private static Font dialogFont = null;
        /// <summary>
        /// Return the Font used in standard dialogs
        /// </summary>
        /// <returns>
        /// This property returns the font that the standard dialogs should use.
        /// </returns>
        public static Font DialogFont
        {
            get
            {
                if (dialogFont == null)
                {
                    string iso2 = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    if (iso2.Equals("ko") || iso2.Equals("zh") || iso2.Equals("ja"))
                    {
                        if (Environment.OSVersion.Version.Major < 6)
                            dialogFont = new System.Drawing.Font("MS Shell Dlg 2", 8.25F);
                        else
                            dialogFont = SystemFonts.MessageBoxFont;
                    }
                    else
                    {
                        dialogFont = new System.Drawing.Font("MS Shell Dlg", 8.25F);
                    }
                }
                return dialogFont;
            }
        }

        public static void SetDialogFont(Form form)
        {
            // do not do this in design mode.  
            if (!AppDomain.CurrentDomain.FriendlyName.Equals("DefaultDomain"))
                form.Font = DialogFont;
        }

        #region Control Methods
        public static void AdjustLeft(Control toAdjust, int xPosition)
        {
            AdjustLeft(toAdjust, xPosition, true);
        }

        public static void AdjustLeft(Control toAdjust, int xPosition, bool adjustWidth)
        {
            if (toAdjust == null) return;
            //if (toAdjust.Left <= xPosition) return;
            int difference = toAdjust.Left - xPosition;
            toAdjust.Left -= difference;
            if (adjustWidth) toAdjust.Width += difference;
        }

        public static void AdjustLeft(Control[] controls, int xPosition)
        {
            AdjustLeft(controls, xPosition, true);
        }

        public static void AdjustLeft(Control[] controls, int xPosition, bool adjustWidth)
        {
            if (controls == null || controls.Length <= 0) return;
            foreach (Control control in controls)
            {
                if (control == null) continue;
                AdjustLeft(control, xPosition, adjustWidth);
            }
        }

        public static int GetRightMost(Control[] controls)
        {
            if (controls == null || controls.Length <= 0) return 0;
            int right = 0;
            foreach (Control control in controls)
            {
                if (control == null) continue;
                right = Math.Max(right, control.Right);
            }
            return right;
        }
        #endregion
    }
}
