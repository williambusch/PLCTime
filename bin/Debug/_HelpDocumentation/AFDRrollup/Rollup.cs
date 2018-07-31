// Copyright (C) 2009-2011 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.EventFrame;
using OSIsoft.AF.Time;
using OSIsoft.AF.UnitsOfMeasure;
using OSIsoft.AF.Asset.DataReference.Properties;

namespace OSIsoft.AF.Asset.DataReference
{
    [Guid("9BAF575E-F679-2100-8C94-486571D79A59")]
    [Description("Rollup;Rollup calculation")]
	public class Rollup : AFDataReference
	{
		private string sCategoryName;
		private string sCalculation;

		private bool bChecked = false;
        private AFAttributeList paramAttributes = null;
        private DateTime dtLastLoadDBModify = DateTime.MinValue;

        public Rollup()
            : base()
        {
        }

		private void CheckConfig()
		{
			if(Attribute == null)
			{
				UnloadParameters();
                string msg = String.Format(Resources.ERR_AttributeHasNotBeenSet, Name);
				throw new InvalidOperationException(msg);
			}
       
			if(ConfigString == null || ConfigString.Length <= 0)
			{
				UnloadParameters();
				string msg = String.Format(Resources.ERR_DataReferenceNotConfigured, Path);
				throw new ApplicationException(msg);
			}

			bChecked = true;
		}


        #region Configuration Properties
		[Category("Configuration")] 
		[Description("All attributes with the specified category name are operated on by the rollup.")] 
		[DefaultValue("")] 
		public string CategoryName 
		{ 
			get { return sCategoryName; } 
			set  
			{ 
				if (sCategoryName != value) 
				{
					sCategoryName = value;
					if (sCategoryName != null) 
						sCategoryName = sCategoryName.Trim();
					SaveConfigChanges();
					UnloadParameters();
				} 
			} 
		}

		[Category("Configuration")] 
		[Description("The type of the calculation to perform on the child attributes.")] 
		[DefaultValue("")] 
		public string Calculation 
		{ 
			get { return sCalculation; } 
			set  
			{ 
				if (sCalculation != value) 
				{ 
					sCalculation = value;
					if (sCalculation != null) 
						sCalculation = sCalculation.Trim();
					SaveConfigChanges(); 
				} 
			} 
		}
        #endregion


        #region Implementation of AFDataReference
        public override AFDataReferenceContext SupportedContexts
		{
			get
			{
                return (AFDataReferenceContext.All);
			}
		}

		public override AFDataReferenceMethod SupportedMethods
		{
            get
            {
                AFDataReferenceMethod supportedMethods =
                    AFDataReferenceMethod.GetValue | AFDataReferenceMethod.GetValues;
                return supportedMethods;
            }
		}

		public override string ConfigString
		{
			// The ConfigString property is used to store and load the configuration of this data reference.
			// Unless the string format is trivial, you may want to decompose the string
			// into properties of this data reference during the 'set', and build the string from
			// these properties during the 'get', as is done here.
            // Otherwise, you must store the configstring as a field.
            get 
            { 
                StringBuilder configBuilder = new StringBuilder();
				// only build statement if configuration of something exist
				if (!(CategoryName == null || CategoryName.Length <= 0) ||
					!(Calculation == null || Calculation.Length <= 0))
				{
					configBuilder.AppendFormat("CategoryName={0}", CategoryName);
					configBuilder.AppendFormat(";Calculation={0}", Calculation);
				}
                return configBuilder.ToString();
            }
			set 
			{
                if (ConfigString != value)
                {
					// reset to defaults
					CategoryName = "";
					Calculation = "";
                    
					if (value != null)
                    {
						string[] tokens= value.Split(';', '=');
						for(int i= 0; i < tokens.Length; i++)
						{
							string paramName= tokens[i];
							string paramValue= "";
							if(++i < tokens.Length)
								paramValue= tokens[i];

							switch(paramName.ToUpper())
							{
								case "CATEGORYNAME":
									CategoryName = paramValue;
									break;
								case "CALCULATION":
									Calculation = paramValue;
									break;
								default:
                                    throw new ArgumentException(String.Format(
                                        Resources.ERR_UnrecognizedConfigurationSetting, paramName, value));
                            }
						}
                    }

                    // notify clients of change
                    SaveConfigChanges();
					UnloadParameters();
                }
			}
		}

		public override Type EditorType
		{
			// The EditorType property returns the type of the editor to be used to edit this data reference.  
			// The editor must inherit from Windows.Forms and must have a public constructor that accepts 
			// this data reference and a boolean read-only flag.
			get { return typeof(RollupConfig); }
		}

        public override AFAttributeList GetInputs(object context)
        {
            // Return the list of attributes we are interested in.
            // AF SDK will then proved the values for these attributes in the GetValue call.
            // This is much more efficient than asking for them one-by-one in GetValue, because
            // the calls can be batched, and redundant requests for attributes can be eliminated.
            return ParameterAttributes;
        }

        public override AFValue GetValue(object context, object timeContext, AFAttributeList inputAttributes, AFValues inputValues)
        {
			if (!bChecked)
				CheckConfig();

			// Evaluate
			try
			{
				// Perform the equation calculations.
				return Calculate(inputValues);
			}
			catch
			{
				// For any exception, unload parameters and set flag so parameters
				//  are rechecked on the next call.
				UnloadParameters();
				bChecked= false;
				throw;
			}
		}

        public override AFValues GetValues(object context, AFTimeRange timeContext, int numberOfValues, AFAttributeList inputAttributes, AFValues[] inputValues)
        {
            if (!bChecked)
                CheckConfig();

            // Evaluate
            try
            {
                // base implementation is sufficient for all calculation data references except when no inputs
                if (numberOfValues > 0 && (inputValues == null || inputValues.Length == 0))
                {
                    // when no inputs on a plot values call, just return the value at the start time
                    AFValues values = new AFValues();
                    values.Add(GetValue(context, timeContext.StartTime, inputAttributes, null));
                    return values;
                }
                return base.GetValues(context, timeContext, numberOfValues, inputAttributes, inputValues);
            }
            catch
            {
                // For any exception, unload parameters and set flag so parameters
                //  are rechecked on the next call.
                UnloadParameters();
                throw;
            }
		}
		#endregion


        #region Internal methods for performing Calcuation
        private void UnloadParameters()
        {
            paramAttributes = null;
            bChecked = false;
        }

        private void LoadParameters()
        {
            if (Attribute == null) return; // must be a template
            if (Attribute.Element == null) return; // must be a dynamic attribute
            if (String.IsNullOrEmpty(CategoryName)) return; // nothing specified.
            if (paramAttributes == null)
            {
                paramAttributes = new AFAttributeList();

                AFCategory cat = Database.AttributeCategories[CategoryName];
                if (cat != null)
                {
                    // Look for attributes matching the category in all child elements.
                    AFElement element = Attribute.Element as AFElement;
                    if (element == null) return;
                    if (dtLastLoadDBModify <= DateTime.MinValue)
                        AFElement.LoadElements(element.Elements);
                    foreach (AFElement e in element.Elements)
                    {
                        foreach (AFAttribute attr in e.Attributes)
                        {
                            if (attr.Categories.Contains(cat))
                                paramAttributes.Add(attr);
                        }
                    }
                }
                dtLastLoadDBModify = Database.ModifyDate.UtcTime;
            }
        }

        private AFAttributeList ParameterAttributes
        {
            get
            {
                // Reload the attributes if more than 10 seconds has passed since the last calculation
                // This handles changes in heirarchy.  Do this less frequently if possible.
                if (dtLastLoadDBModify != Database.ModifyDate.UtcTime || paramAttributes == null)
                {
                    paramAttributes = null;
                    LoadParameters();
                }
                return paramAttributes;
            }
        }

        private AFValue Calculate(AFValues inputValues)
		{
            // This method performs our calculation.
			double dCalcVal = 0;
			int  iCount = 0;
			bool bCalcSet = false;
            AFTime timestamp = AFTime.MinValue;

            if (inputValues != null && inputValues.Count > 0)
            {
                foreach (AFValue inVal in inputValues)
                {
                    if (inVal.IsGood && inVal.Value != null)
                    {
                        double dCurVal;
                        if (inVal.Timestamp > timestamp)
                            timestamp = inVal.Timestamp;

                        // make sure we do all operations in same UOM, if applicable
                        if (inVal.UOM != null && Attribute.DefaultUOM != null && inVal.UOM != Attribute.DefaultUOM)
                            dCurVal = Attribute.DefaultUOM.Convert(inVal.Value, inVal.UOM);
                        else
                            dCurVal = Convert.ToDouble(inVal.Value);

                        switch (Calculation.ToUpper())
                        {
                            case "AVG":
                                dCalcVal += dCurVal;
                                iCount++;
                                break;
                            case "SUM":
                                dCalcVal += dCurVal;
                                break;
                            case "MIN":
                                if (!bCalcSet)
                                {
                                    dCalcVal = dCurVal;
                                    bCalcSet = true;
                                }
                                dCalcVal = Math.Min(dCalcVal, dCurVal);
                                break;
                            case "MAX":
                                if (!bCalcSet)
                                {
                                    dCalcVal = dCurVal;
                                    bCalcSet = true;
                                }
                                dCalcVal = Math.Max(dCalcVal, dCurVal);
                                break;

                        }
                    }
                }

                if (String.Equals(Calculation, "AVG", StringComparison.InvariantCultureIgnoreCase))
                {
                    dCalcVal = dCalcVal / iCount;
                }
            }

            if (timestamp.IsEmpty && Attribute != null)
            {
                if (Attribute.Element is IAFVersionable)
                    timestamp = ((IAFVersionable)Attribute.Element).Version.EffectiveDate;
                else if (Attribute.Element is AFEventFrame)
                    timestamp = ((AFEventFrame)Attribute.Element).StartTime;
            }

			return new AFValue(dCalcVal, timestamp, Attribute.DefaultUOM);
		}
        #endregion
	}
}
