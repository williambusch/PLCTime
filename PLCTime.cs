using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Time;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PLCTime
{
    //AF needs custom guid for data reference
    [Guid("69686A54-D95F-4885-AE1D-D42373E3F4C0")]
    //This is "name of data reference;description of data reference"
    [Description("PLCTime;This converts PLC values into actual DateTimes")]
    public class PLCTime : AFDataReference
    {
        //override the default configuration string to take our data
        private string configString;
        public override string ConfigString
        {
            get
            {
                return configString;
            }
            set
            {
                configString = AFDataReference.SubstituteParameters(value, this, null, null);
                SaveConfigChanges();
            }
        }

        //this data reference supports the GetValue and GetValues methods
        public override AFDataReferenceMethod SupportedMethods
        {
            get
            {
                AFDataReferenceMethod supportedMethods =
                    AFDataReferenceMethod.GetValue | AFDataReferenceMethod.GetValues;
                return supportedMethods;
            }
        }

        //this data reference supports the following data retrieval methods
        public override AFDataMethods SupportedDataMethods
        {
            get
            {
                return (AFDataMethods.RecordedValue |
                        AFDataMethods.RecordedValues |
                        AFDataMethods.InterpolatedValue |
                        AFDataMethods.InterpolatedValues |
                        AFDataMethods.Summaries |
                        AFDataMethods.Summary);
            }
        }

        public override AFDataReferenceContext SupportedContexts
        {
            get
            {
                return (AFDataReferenceContext.Time |
                        AFDataReferenceContext.TimeRange);
            }
        }


        //our editor will be our custom windows form
        public override Type EditorType
        {
            get
            {
                return typeof(PLCTimeForm);
            }
        }

        //bring in PIPoint attributes
        public override AFAttributeList GetInputs(object context)
        {
            AFAttributeList returnList = new AFAttributeList();
            string[] splitConfig = ConfigString.Split(';');
            string selectedOption = splitConfig[2];
            if (selectedOption == "convertRadio")
            {
                //config string is "initial date;name of attribute"
                AFAttribute minSinceAttributeObj = GetAttribute(splitConfig[1]);
                returnList.Add(minSinceAttributeObj);
            }
            else //selectedOption == "formatRadio"
            {
                //config string is "name of date attribute;name of time attribute"
                AFAttribute dateAttributeObj = GetAttribute(splitConfig[0]);
                AFAttribute timeAttributeObj = GetAttribute(splitConfig[1]);
                returnList.Add(dateAttributeObj);
                returnList.Add(timeAttributeObj);
            }

            return returnList;
        }

        //calculate value of attribute
        public override AFValue GetValue(object context, object timeContext, AFAttributeList inputAttributes, AFValues inputValues)
        {
            string[] splitConfig = ConfigString.Split(';');
            string selectedOption = splitConfig[2];
            if (selectedOption == "convertRadio")
                return PLCTimeConvert(inputValues[0], splitConfig[0]);
            else //selectedOption == "formatRadio"
            {
                AFValue dateVal = inputValues[0];
                AFValue timeVal = inputValues[1];
                return PLCTimeFormatExecution(dateVal, timeVal);
            }
        }
        
        public override AFValues GetValues(object context, AFTimeRange timeRange, int numberOfValues, AFAttributeList inputAttributes, AFValues[] inputValues)
        {
            //retrieve base list of values
            AFValues baseReturnVals = base.GetValues(context, timeRange, numberOfValues, inputAttributes, inputValues);
            return filterValues(baseReturnVals);
        }

        public override AFValues RecordedValues(AFTimeRange timeRange, AFBoundaryType boundaryType, string filterExpression, bool includeFilteredValues, AFAttributeList inputAttributes, AFValues[] inputValues, List<AFTime> inputTimes, int maxCount = 0)
        {
            AFValues baseReturnVals = base.RecordedValues(timeRange, boundaryType, filterExpression, includeFilteredValues, inputAttributes, inputValues, inputTimes, maxCount);
            return filterValues(baseReturnVals);
        }

        private AFValues filterValues (AFValues baseReturnVals)
        {
            AFValues filteredReturnVals = new AFValues();

            if (ConfigString.Split(';')[2] == "convertRadio")
            {
                filteredReturnVals = baseReturnVals;
            }
            else //formatRadio
            {
                //filter out values where the next value is within 1 second
                //this eliminates the problem where the date and time PLC addresses change at slightly different times within 1 second
                //e.g. the date changes to 10/10 right before the time changes to 11:15 from 12:00, then you have an extra filldate of 10/10 12:00 am that is not valid
                for (int i = 0; i < baseReturnVals.Count - 1; i++)
                {
                    if ((baseReturnVals[i + 1].Timestamp - baseReturnVals[i].Timestamp) > new TimeSpan(0, 0, 1))
                        filteredReturnVals.Add(baseReturnVals[i]);
                }
                //always add the last value since there is nothing to compare it to
                filteredReturnVals.Add(baseReturnVals[baseReturnVals.Count - 1]);
            }
            return filteredReturnVals;
        }

        //this is assuming PLC value is x number of minutes since initial date
        private AFValue PLCTimeConvert(AFValue minSinceInit, string initDateString)
        {
            DateTime initDateTime = DateTime.SpecifyKind(Convert.ToDateTime(initDateString), DateTimeKind.Local);
            DateTime newDateTime;
            int minSinceInitVal = Convert.ToInt32(minSinceInit.Value);

            //check if value is less than 0, if so leave alone
            if (minSinceInitVal >= 0)
            {
                //convert attribute value to timespan and add to initDateTime to get newDateTime
                TimeSpan timeSinceInit = new TimeSpan(0, minSinceInitVal, 0); //assuming attributeVal is in minutes
                newDateTime = initDateTime.Add(timeSinceInit);
                return new AFValue(newDateTime, new AFTime(minSinceInit.Timestamp));
            }
            else
                return new AFValue(minSinceInitVal, new AFTime(minSinceInit.Timestamp));
        }

        //this is assuming two PLC values: one is time in HHMM format (e.g. 1456 for 2:56 pm) and the other is date in MMDD format (e.g. 0617 for June 17)
        private AFValue PLCTimeFormatExecution(AFValue dateVal, AFValue timeVal)
        {
            //get Date value
            DateTime dateAttributeTimestamp = dateVal.Timestamp; //this will be the queried time (DateTime.Now) if step is turned off
            int dateAttributeVal = Convert.ToInt32(dateVal.Value);

            //get Time value
            DateTime timeAttributeTimestamp = timeVal.Timestamp; //this will be the queried time (DateTime.Now) if step is turned off
            int timeAttributeVal = Convert.ToInt32(timeVal.Value);

            //The year is not stored in the PLC so we have to look at the timestamp of the historian archive data for the year for a baseline
            //Then we calculate the the year before and the year after and see which date is closest to the historian archive date

            int compare = DateTime.Compare(dateAttributeTimestamp, timeAttributeTimestamp);
            DateTime compareDateTime;
            if (compare > 0) //use the more recent timestamp
                compareDateTime = dateAttributeTimestamp;
            else
                compareDateTime = timeAttributeTimestamp;

            if (dateAttributeVal != 0)
            {    
                //date is in format MMDD
                int month = dateAttributeVal / 100;
                int day = dateAttributeVal % 100;
                //time is in format HHMM
                int hour = timeAttributeVal / 100;
                int min = timeAttributeVal % 100;

                int year = compareDateTime.Year;
                DateTime actualDateTime = new DateTime(year, month, day, hour, min, 0, DateTimeKind.Local);
                DateTime lastYearDateTime = new DateTime(year - 1, month, day, hour, min, 0, DateTimeKind.Local);
                DateTime nextYearDateTime = new DateTime(year + 1, month, day, hour, min, 0, DateTimeKind.Local);

                TimeSpan minTime = actualDateTime.Subtract(compareDateTime).Duration();
                TimeSpan tempTime = lastYearDateTime.Subtract(compareDateTime).Duration();
                if (tempTime < minTime) { minTime = tempTime; actualDateTime = lastYearDateTime; }
                tempTime = nextYearDateTime.Subtract(compareDateTime).Duration();
                if (tempTime < minTime) { minTime = tempTime; actualDateTime = nextYearDateTime; }

                return new AFValue(actualDateTime, new AFTime(compareDateTime));
            }
            else
                return new AFValue(0, new AFTime(compareDateTime));
        }
    }
}
