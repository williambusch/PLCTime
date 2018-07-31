// Copyright (C) 2008-2010 OSIsoft, LLC. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NONINFRINGEMENT.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using OSIsoft.AF.Asset;
using OSIsoft.AF.EventFrame;
using OSIsoft.AF.UnitsOfMeasure;
using OSIsoft.AF.Time;

namespace OSIsoft.AF.Asset.DataReference
{
    // Implementation of the data reference
    [Serializable]
    [Guid("1B2ED32C-98F7-4f1a-86E2-F9EAB5EF0815")]
    [Description("String Concat;Concatenate strings.")]
    public class StringConcat : AFDataReference
    {
        private string configString = String.Empty;

        public StringConcat()
            : base()
        {
        }

        #region Implementation of AFDataReference
        public override AFDataReferenceContext SupportedContexts
        {
            get
            {
                return (AFDataReferenceContext.All);
            }
        }

        public override string ConfigString
        {
            // The ConfigString property is used to store and load the configuration of this data reference.
            get
            {
                return configString;
            }
            set
            {
                if (ConfigString != value)
                {
                    if (value != null)
                        configString = value.Trim();

                    // notify SDK and clients of change.  Required to have changes saved.
                    SaveConfigChanges();

                    CheckDataType();
                }
            }
        }

        public override AFAttributeList GetInputs(object context)
        {
            // Loop through the config string, looking for attributes
            // The Config string is semicolon separated list of attributes and strings
            // Strings must be enclosed in " "
            // Will also handle standard AF substitions (%ELEMENT%, %TIME%, etc.)
            AFAttributeList paramAttributes = null;
            string[] subStrings = ConfigString.Split(';');
            for (int i = 0; i < subStrings.Length; i++)
            {
                string s = subStrings[i].Trim();
                String subst = SubstituteParameters(s, this, context, null);
                if (!String.IsNullOrEmpty(subst) && !subst.StartsWith("\""))
                {
                    // Get attribute will resolve attribute references 
                    AFAttribute attr = GetAttribute(subst);
                    if (attr == null || attr.IsDeleted)
                    {
                        throw new ApplicationException(String.Format("Unknown attribute '{0}'", s));
                    }
                    if (paramAttributes == null)
                        paramAttributes = new AFAttributeList();
                    paramAttributes.Add(attr);
                }
            }
            return paramAttributes;
        }

        public override AFValue GetValue(object context, object timeContext, AFAttributeList inputAttributes, AFValues inputValues)
        {
            // Evaluate
            AFTime timestamp = AFTime.MinValue;
            if (configString == null) configString = String.Empty;

            StringBuilder sb = new StringBuilder();
            string[] subStrings = configString.Split(';');
            int attributeIndex = 0;
            for (int i=0; i<subStrings.Length; i++)
            {
                String s = subStrings[i].Trim();
                if (s.StartsWith("\""))
                {
                    String subst = SubstituteParameters(s, this, context, timeContext);
                    sb.Append(subst.Substring(1).TrimEnd('"'));
                }
                else if (s != String.Empty)
                {
                    if (inputValues != null && attributeIndex < inputValues.Count)
                    {
                        AFValue v = inputValues[attributeIndex];
                        if (v.Timestamp > timestamp)
                            timestamp = v.Timestamp;

                        if (v.Value == null || !v.IsGood)
                        {
                            AFValue badValue = new AFValue(
                                String.Format("Bad input value in '{0}': {1}",
                                subStrings[i], v.Value), 
                                timestamp, null, AFValueStatus.Bad);
                            return badValue; // just return bad value
                        }

                        string sValue;
                        if (v.Value is DateTime)
                            sValue = ((DateTime)v.Value).ToLocalTime().ToString(CultureInfo.InvariantCulture);
                        else
                            sValue = v.ToString();

                        sb.Append(sValue);
                        attributeIndex++;
                    }
                    else
                    {
                        return new AFValue(
                            "Invalid data sent to GetValue", timestamp, null, AFValueStatus.Bad);
                    }
                }
            }

            // should be returning effective date as absolute minimum
            if (timestamp.IsEmpty && Attribute != null)
            {
                if (Attribute.Element is IAFVersionable)
                    timestamp = ((IAFVersionable)Attribute.Element).Version.EffectiveDate;
                else if (Attribute.Element is AFEventFrame)
                    timestamp = ((AFEventFrame)Attribute.Element).StartTime;
            }
            else if (timestamp.IsEmpty && timeContext is AFTime)
                timestamp = (AFTime)timeContext;

            return new AFValue(sb.ToString(), timestamp);
        }
        #endregion

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

            if (type != typeof(string))
            {
                if (Attribute != null)
                    Attribute.Type = typeof(String);
                else if (Template != null)
                    Template.Type = typeof(String);
            }
        }
    }
}
