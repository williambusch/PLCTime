// Copyright (C) 2004-2011 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OSIsoft.AF.Asset;
using OSIsoft.AF.UnitsOfMeasure;
using OSIsoft.AF.Time;
using OSIsoft.AF.Asset.DataReference.Properties;

namespace OSIsoft.AF.Asset.DataReference
{
//'<filename:Declare>
// Implementation of the data reference
[Serializable]
[Guid("D7F3FC3A-D5F1-465E-A03F-3AB6E5BD4258")]
[Description("Ideal Gas Law;Density calculation for ideal gases.")]
public class IdealGasLaw : AFDataReference
{
	private double		    dMolecularWeight = 0;
//'</filename:Declare>

//'<filename:Declare-Parameters>
    private enum AttributeParameter : int
    {
        PressureIndex = 0,
        TemperatureIndex = 1,
        MolecularWeightIndex = 2
    }
//'</filename:Declare-Parameters>

    private AFAttributeList paramAttributes = null;
    private AFAttribute attributePressure;
    private AFAttribute attributeTemperature;
    private AFAttribute attributeMolecularWeight;
	private UOM uomPressure = null;
	private UOM uomTemperature = null;
	private UOM uomMolecularWeight = null;
	private UOM uomDensity = null;

    private bool bChecked = false;
	
public IdealGasLaw() : base()
{
}

private void UnloadParameters()
{
    paramAttributes = null;
    attributePressure = null;
    attributeTemperature = null;
    attributeMolecularWeight = null;
	uomPressure = null;
	uomTemperature = null;
	uomMolecularWeight = null;
	uomDensity = null;
    bChecked = false;
}

// Since base property 'IsInitializing' only exists in AF 2.1 or later, must
//  separate the call into the following two methods because an exception is
//  thrown when 'BaseIsInitializing' is compiled by the CLR.
//  This would only occur when a AF 2.0 client connects to an AFServer 2.1.
private bool CheckIsInitializing()
{
    try
    {
        return BaseIsInitializing();
    }
    catch { }
    return false;
}
private bool BaseIsInitializing()
{
    return IsInitializing;
}

internal void CheckDataType()
{
    if (CheckIsInitializing()) return;
    if (Attribute != null && Attribute.Template != null) return; // can't do anything
    // check to see we are already dirty
    if (Attribute != null && Attribute.Element is IAFTransactable && !((IAFTransactable)Attribute.Element).IsDirty) return;
    if (Template != null && !Template.ElementTemplate.IsDirty) return;


    Type type = null;
    if (Attribute != null)
        type = Attribute.Type;
    else if (Template != null)
        type = Template.Type;

    bool bIsNumeric = type != null && type.IsPrimitive &&
        (type == typeof(Double) || type == typeof(Single));

    if (!bIsNumeric)
    {
        if (Attribute != null)
            Attribute.Type = typeof(Double);
        else if (Template != null)
            Template.Type = typeof(Double);
    }
}

//'<filename:LoadParameters>
private void LoadParameters()
{
    if (Attribute == null)  return; // must be a template
    if (Attribute.Element == null) return; // must be a dynamic attribute
    if (paramAttributes == null)
    {
		paramAttributes = new AFAttributeList();
        attributePressure = Attribute.Element.Attributes["Pressure"];
        if (attributePressure == null || attributePressure.IsDeleted)
            bChecked = false;
        else
            paramAttributes.Add(attributePressure);
        attributeTemperature = Attribute.Element.Attributes["Temperature"];
        if (attributeTemperature == null || attributeTemperature.IsDeleted)
            bChecked = false;
        else
            paramAttributes.Add(attributeTemperature);

		// molecular weight attribute is optional
        attributeMolecularWeight = Attribute.Element.Attributes["Molecular Weight"];
        if (attributeMolecularWeight != null && !attributeMolecularWeight.IsDeleted)
            paramAttributes.Add(attributeMolecularWeight);

		uomPressure = PISystem.UOMDatabase.UOMs["atm"];
		uomTemperature = PISystem.UOMDatabase.UOMs["K"];
		uomMolecularWeight = PISystem.UOMDatabase.UOMs["g/gmol"];
        if (uomDensity == null)
            uomDensity = UOM;
        if (uomPressure == null || uomTemperature == null || uomMolecularWeight == null || uomDensity == null)
            bChecked = false;
    }
}
//'</filename:LoadParameters>

//'<filename:CheckConfig-Begin>
private void CheckConfig()
{
	// This method will check the current configuration of this data reference. 
	if(Attribute == null)
	{
        UnloadParameters();
		throw new InvalidOperationException(
            String.Format(CultureInfo.CurrentCulture, Resources.ERR_AttributeNotSet, Name));
	}
    LoadParameters();
	//'</filename:CheckConfig-Begin>

    //'<filename:VerifyUOMs>
    // verify the UOMs exist
    if (uomPressure == null || uomTemperature == null || uomMolecularWeight == null || uomDensity == null)
    {
        UnloadParameters();
        throw new ApplicationException(Resources.ERR_UOMsMustBeDefined);
    }
    //'</filename:VerifyUOMs>

    //'<filename:VerifyAttributes>
    if (attributePressure == null || attributePressure.IsDeleted)
	{
        UnloadParameters();
        throw new ApplicationException(Resources.ERR_PressureRequired);
	}

	if (attributeTemperature == null || attributeTemperature.IsDeleted)
	{
        UnloadParameters();
        throw new ApplicationException(Resources.ERR_TemperatureRequired);
	}
	//'</filename:VerifyAttributes>

	//'<filename:CheckConfig-End>
	bChecked = true;
}
//'</filename:CheckConfig-End>

//'<filename:ConfigProperties>
#region Configuration Properties
[Category("Configuration")]
[Description("The Molecular Weight")]
[DefaultValue(0)]
public double MolecularWeight
{
	get { return dMolecularWeight; }
	set 
	{
		if (dMolecularWeight != value)
		{
			dMolecularWeight = value;
            SaveConfigChanges();
		}
	}
}
#endregion Configuration Properties
//'</filename:ConfigProperties>

#region Implementation of AFDataReference
//'<filename:SupportedContexts>
public override AFDataReferenceContext SupportedContexts
{
	get
	{
        return(AFDataReferenceContext.All);
	}
}
//'</filename:SupportedContexts>

//'<filename:SupportedMethods>
public override AFDataReferenceMethod SupportedMethods
{
	get
	{
		AFDataReferenceMethod supportedMethods =
            AFDataReferenceMethod.GetValue | AFDataReferenceMethod.GetValues;
        return supportedMethods;
	}
}
//'</filename:SupportedMethods>

//'<filename:ConfigString-Begin>
public override string ConfigString
{
	// The ConfigString property is used to store and load the configuration of this data reference.
	//'</filename:ConfigString-Begin>
	//'<filename:ConfigString-Get>
	get 
	{ 
		StringBuilder configBuilder = new StringBuilder();
		if (MolecularWeight != 0)
			configBuilder.AppendFormat(CultureInfo.InvariantCulture, "MW={0}", MolecularWeight);
		if (configBuilder.Length > 0 && configBuilder[0] == ';')
			configBuilder.Remove(0, 1);
		return configBuilder.ToString();
	}
	//'</filename:ConfigString-Get>
	//'<filename:ConfigString-Set>
	set 
	{
		if (ConfigString != value)
		{
			// reset to defaults
			dMolecularWeight = 0;

			if (!String.IsNullOrEmpty(value))
			{
				string[] tokens= value.Split(';', '=');
				for(int i= 0; i < tokens.Length; i++)
				{
					string paramName= tokens[i];
					string paramValue= "";
					if(++i < tokens.Length)
						paramValue= tokens[i];

					switch(paramName.ToUpper(CultureInfo.InvariantCulture))
					{
						case "MW":
							dMolecularWeight = Convert.ToDouble(paramValue, CultureInfo.InvariantCulture);
							break;

						default:
                            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
								Resources.ERR_UnrecognizedConfigurationSetting, paramName, value));
					}
				}
			}

			// notify clients of change
            SaveConfigChanges();
            CheckDataType();
		}
	}
	//'</filename:ConfigString-Set>
}

//'<filename:UOM>
public override UOM UOM
{
	get 
	{
		if (uomDensity == null)
			uomDensity = PISystem.UOMDatabase.UOMs["g/L"];
		return uomDensity;
	}
	set
	{
		throw new InvalidOperationException(Resources.ERR_UOMNotConfigurable);
	}
}
//'</filename:UOM>

public override Type EditorType
{
	// The EditorType property returns the type of the editor to be used to edit this data reference.  
	// The editor must inherit from Windows.Forms and must have a public constructor that accepts 
	// this data reference and a boolean read-only flag.
	get { return typeof(AFIdealGasDRConfig); }
}

//'<filename:GetInputs>
public override AFAttributeList GetInputs(object context)
{
	LoadParameters();
	return paramAttributes;
}
//'</filename:GetInputs>

//'<filename:GetValue>
public override AFValue GetValue(object context, object timeContext, AFAttributeList inputAttributes, AFValues inputValues)
{
	if (!bChecked)
		CheckConfig();

	// Validate number of input values.
	if (inputValues == null || inputValues.Count < 2 || inputValues.Count > 3)
		throw new ArgumentException(Resources.ERR_ValuesDontMatch, "inputValues");
	
	// Evaluate
    try
    { 
        // Perform the equation calculations.
		AFValue objValue;
		double MW = 0;			 // g/gmol
		double pressure;		 // atm
		double temperature;		 // K
		const double R = 0.082057; //(atm l)/(gmol K)	
        AFTime timestamp = AFTime.MinValue;

		// If 'Molecular Weight' attribute is not defined or is zero, then default
		// to the value of the 'MolecularWeight' property.
		if (inputValues.Count == 3)
		{
			objValue = inputValues[(int)AttributeParameter.MolecularWeightIndex];
            if (objValue.Value == null || !objValue.IsGood || !objValue.Value.GetType().IsValueType)
            {
                UnloadParameters();
                return new AFValue(Resources.ERR_InvalidMW, timestamp, null, AFValueStatus.Bad);
            }
			MW = uomMolecularWeight.Convert(objValue.Value, objValue.UOM);
            timestamp = objValue.Timestamp;
		}

		if (MW == 0)
			MW = dMolecularWeight;

        if (MW == 0)
        {
            UnloadParameters();
            return new AFValue(Resources.ERR_ConfigureMW,
                timestamp, null, AFValueStatus.Bad);
        }

		objValue = inputValues[(int)AttributeParameter.PressureIndex];
        if (objValue == null || objValue.Value == null || !objValue.IsGood || !objValue.Value.GetType().IsValueType)
        {
            UnloadParameters();
            return new AFValue(Resources.ERR_InvalidPressure, timestamp, null, AFValueStatus.Bad);
        }
		pressure = uomPressure.Convert(objValue.Value, objValue.UOM);
        if (objValue.Timestamp > timestamp) timestamp = objValue.Timestamp;

		objValue = inputValues[(int)AttributeParameter.TemperatureIndex];
        if (objValue == null || objValue.Value == null || !objValue.IsGood || !objValue.Value.GetType().IsValueType)
        {
            UnloadParameters();
            return new AFValue(Resources.ERR_InvalidTemperature, timestamp, null, AFValueStatus.Bad);
        }
		temperature = uomTemperature.Convert(objValue.Value, objValue.UOM);
        if (objValue.Timestamp > timestamp) timestamp = objValue.Timestamp;

		if (temperature != 0.0)
			return new AFValue((MW * pressure) / (temperature * R), timestamp, UOM);
		else
			return new AFValue(Resources.ERR_InvalidZeroTemperature,
                timestamp, null, AFValueStatus.Bad);
    }
    catch
    {
        // For any exception, unload parameters and set flag so parameters
        //  are rechecked on the next call.
        UnloadParameters();
        throw;
    }
}
//'</filename:GetValue>

//'<filename:GetValues>
public override AFValues GetValues(object context, AFTimeRange timeContext, int numberOfValues, AFAttributeList inputAttributes, AFValues[] inputValues)
{
    if (!bChecked)
        CheckConfig();

    // Evaluate
    try
    {
		// base implementation is sufficient for all calculation data references.
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
//'</filename:GetValues>
#endregion
}
}
