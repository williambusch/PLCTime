using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
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
                return PLCTimeConvert();
            else //selectedOption == "formatRadio"
            {
                List<AFValue> dateVals = new List<AFValue>();
                dateVals.Add(inputValues[0]);
                List<AFValue> timeVals = new List<AFValue>();
                timeVals.Add(inputValues[1]);
                return PLCTimeFormat(dateVals, timeVals)[0];
            }
        }

        //calculate multiple values of attribute
        public override AFValues GetValues(object context, OSIsoft.AF.Time.AFTimeRange timeRange, int numberOfValues, AFAttributeList inputAttributes, AFValues[] inputValues)
        {
            string[] splitConfig = ConfigString.Split(';');
            string selectedOption = splitConfig[2];
            if (selectedOption == "convertRadio")
                return new AFValues();//PLCTimeConvert();
            else //selectedOption == "formatRadio"
            {
                List<AFValue> dateVals = inputValues[0].ToList<AFValue>();
                List<AFValue> timeVals = inputValues[1].ToList<AFValue>();
                return PLCTimeFormat(dateVals, timeVals);
            }
        }

        //this is assuming PLC value is x number of minutes since initial date
        private AFValue PLCTimeConvert()
        {
            //config string is "initial date;name of attribute"
            string[] splitConfig = ConfigString.Split(';');
            string attributeName = splitConfig[1];
            DateTime initDateTime = DateTime.SpecifyKind(Convert.ToDateTime(splitConfig[0]), DateTimeKind.Local);
            DateTime newDateTime;

            //get value of attribute
            AFAttribute attributeObj = GetAttribute(attributeName);
            int attributeVal = Convert.ToInt32(attributeObj.Data.RecordedValue(DateTime.Now, OSIsoft.AF.Data.AFRetrievalMode.Auto, attributeObj.DefaultUOM).Value);

            //check if value is -1, if so leave as -1
            if (attributeVal != -1)
            {
                //convert attribute value to timespan and add to initDateTime to get newDateTime
                TimeSpan timeSinceInit = new TimeSpan(0, attributeVal, 0); //assuming attributeVal is in minutes
                newDateTime = initDateTime.Add(timeSinceInit);
                return new AFValue(newDateTime);
            }
            else
                return new AFValue(-1);
        }

        //this is assuming two PLC values: one is time in HHMM format (e.g. 1456 for 2:56 pm) and the other is date in MMDD format (e.g. 0617 for June 17)
        private AFValues PLCTimeFormat(List<AFValue> dateVals, List<AFValue> timeVals)
        {
            AFValues returnVals = new AFValues();
            
            //config string is "name of date attribute;name of time attribute"
            string[] datetimeAttributes = ConfigString.Split(';');

            AFAttribute dateAttributeObj = GetAttribute(datetimeAttributes[0]);
            AFValue dateAttributeAFVal = dateAttributeObj.Data.RecordedValue(DateTime.Now, OSIsoft.AF.Data.AFRetrievalMode.Auto, dateAttributeObj.DefaultUOM);
            DateTime dateAttributeTimestamp = dateAttributeAFVal.Timestamp; //this will be the queried time (DateTime.Now) if step is turned off
            int dateAttributeVal = Convert.ToInt32(dateAttributeAFVal.Value);

            AFAttribute timeAttributeObj = GetAttribute(datetimeAttributes[1]);
            AFValue timeAttributeAFVal = timeAttributeObj.Data.RecordedValue(DateTime.Now, OSIsoft.AF.Data.AFRetrievalMode.Auto, timeAttributeObj.DefaultUOM);
            DateTime timeAttributeTimestamp = timeAttributeAFVal.Timestamp; //this will be the queried time (DateTime.Now) if step is turned off
            int timeAttributeVal = Convert.ToInt32(timeAttributeAFVal.Value);

            if (dateAttributeVal != 0)
            {
                //date is in format MMDD
                int month = dateAttributeVal / 100;
                int day = dateAttributeVal % 100;
                //time is in format HHMM
                int hour = timeAttributeVal / 100;
                int min = timeAttributeVal % 100;

                //The year is not stored in the PLC so we have to look at the timestamp of the historian archive data for the year for a baseline
                //Then we calculate the the year before and the year after and see which date is closest to the historian archive date

                int compare = DateTime.Compare(dateAttributeTimestamp, timeAttributeTimestamp);
                DateTime compareDateTime;
                if (compare > 0) //use the more recent timestamp
                    compareDateTime = dateAttributeTimestamp;
                else
                    compareDateTime = timeAttributeTimestamp;

                int year = compareDateTime.Year;
                DateTime actualDateTime = new DateTime(year, month, day, hour, min, 0, DateTimeKind.Local);
                DateTime lastYearDateTime = new DateTime(year - 1, month, day, hour, min, 0, DateTimeKind.Local);
                DateTime nextYearDateTime = new DateTime(year + 1, month, day, hour, min, 0, DateTimeKind.Local);

                TimeSpan minTime = actualDateTime.Subtract(compareDateTime).Duration();
                TimeSpan tempTime = lastYearDateTime.Subtract(compareDateTime).Duration();
                if (tempTime < minTime) { minTime = tempTime; actualDateTime = lastYearDateTime; }
                tempTime = nextYearDateTime.Subtract(compareDateTime).Duration();
                if (tempTime < minTime) { minTime = tempTime; actualDateTime = nextYearDateTime; }


                return returnVals; // new AFValue(actualDateTime);
            }
            else
                return returnVals; // new AFValue(0);
        }
    }
}
