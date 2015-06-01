using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace AllanStevens.DataTranslator.Factory
{
    public class Translator : ITranslator, IDisposable
    {
        /// <summary>
        /// Private Variables
        /// </summary>

        private string sourceFilename;
        private string targetFilename;
        private string translatorFilename;
        private FieldCollection sourceFields;
        private FieldCollection targetFields;
        private FieldCollection headerFields;
        private FieldCollection footerFields;

        /// <summary>
        /// Constructor(s)
        /// </summary>

        public Translator()
        {
            sourceFields = new FieldCollection();
            targetFields = new FieldCollection();
            headerFields = new FieldCollection();
            footerFields = new FieldCollection();
        }

        /// <summary>
        /// Class Properties
        /// </summary>

        public string TranslatorFilename { set { translatorFilename = value; } }
        public string SourceFilename { set { sourceFilename = value; } }
        public string TargetFilename { set { targetFilename = value; } }

        /// <summary>
        /// Public Methods
        /// </summary>

        public void Initialize()
        {

            // Do some validation
            if (!File.Exists(translatorFilename))
            {
                throw new Exception("Translator XML file missing.");
            }
            else if (!File.Exists(sourceFilename))
            {
                throw new Exception("Source File is missing.");
            }
            else
            {
                // populate the source and target FieldCollections from translator XML.
                sourceFields.PopulateFromXML("/Translator/Source", translatorFilename);
                targetFields.PopulateFromXML("/Translator/Target", translatorFilename);
                headerFields.PopulateFromXML("/Translator/Header", translatorFilename);
                footerFields.PopulateFromXML("/Translator/Footer", translatorFilename);
            }

            // update target filename if format is specified in xml
            if (targetFields.FileNameFormat != null && targetFields.FileNameFormat != "")
            {
                string newTargetFilename = string.Format(targetFields.FileNameFormat, DateTime.Now);

                if (targetFilename.Contains("\\"))
                {
                    targetFilename =
                        targetFilename.Substring(0, targetFilename.LastIndexOf("\\")) + "\\" +
                        newTargetFilename.Replace("%%SourceFileName%%", targetFilename.Substring(targetFilename.LastIndexOf("\\") + 1));
                }
                else
                {
                    targetFilename = newTargetFilename.Replace("%%SourceFileName%%", targetFilename);
                }
            }

            // validate to check target filename does NOT exist
            if (File.Exists(targetFilename))
            {
                throw new Exception("Target already exists.");
            }
        }

        public void RunTranslator()
        {

            // If Initialize has not run, run now.
            if (sourceFields == null && targetFields == null)
            {
                Initialize();
            }

            try
            {
                // Start convert

                // If target file is Fixed Text then (other senarios will be added once requried)
                if (targetFields.FileType == "FixedWidth")
                {

                    // Create text file
                    using (StreamWriter text = new StreamWriter(targetFilename, true))
                    {
                        int lineNumber = 1;

                        // If source file is XML then (other senarios will be added once requried)
                        if (sourceFields.FileType == "XMLData")
                        {
                            XmlDocument docToConvert = new XmlDocument();
                            docToConvert.Load(sourceFilename);

                            // Create header if required
                            if (!(headerFields.FileType == null && headerFields.Count == 0))
                            {
                                if (headerFields.FileType == "AutoGenerate")
                                {
                                    foreach (Field column in targetFields)
                                    {
                                        text.Write(column.Name.Padding(column.Width));
                                    }
                                    text.WriteLine();
                                    lineNumber++;
                                }
                                else
                                {

                                    foreach (Field column in headerFields)
                                    {
                                        object value = BuildRow(column, ref lineNumber, docToConvert);

                                        // Add row to text file
                                        text.Write(value.ToString().Padding(
                                            column.Width,
                                            (PaddingDirectionTypes)Enum.Parse(typeof(PaddingDirectionTypes), column.Alignment.ToString(), true),
                                            column.PaddingText,
                                            column.Trim));
                                    }
                                    text.WriteLine();
                                    lineNumber++;
                                }
                            }

                            // Step though each row/node in source xml
                            foreach (XmlNode row in docToConvert.SelectNodes(sourceFields.XPathLocation))
                            {

                                // Build output col by col using targetfields as template
                                foreach (Field targetColumn in targetFields)
                                {
                                    //string stringValue;
                                    XmlNode newRow;

                                    // If DataLocation attribute is present then look somewhere else for the CurrentRow
                                    if (targetColumn.MappedField != null &&
                                        sourceFields[targetColumn.MappedField] != null &&
                                        sourceFields[targetColumn.MappedField].XPathLocation != null &&
                                        sourceFields[targetColumn.MappedField].XPathLocation != "")
                                    {
                                        newRow = row.SelectSingleNode(sourceFields[targetColumn.MappedField].XPathLocation);
                                    }
                                    else
                                    {
                                        newRow = row;
                                    }

                                    object value = BuildRow(newRow, targetColumn, ref lineNumber, docToConvert);

                                    // Add row to text file
                                    text.Write(value.ToString().Padding(
                                        targetColumn.Width,
                                        (PaddingDirectionTypes)Enum.Parse(typeof(PaddingDirectionTypes), targetColumn.Alignment.ToString(), true),
                                        targetColumn.PaddingText,
                                        targetColumn.Trim));

                                }
                                // New Line
                                text.WriteLine();
                                lineNumber++;
                            }

                            // Create footer if required
                            if (!(footerFields.FileType == null && footerFields.Count == 0))
                            {
                                if (footerFields.FileType == "AutoGenerate")
                                {
                                    text.Write(DateTime.Now.ToString());
                                    text.WriteLine();
                                    lineNumber++;
                                }
                                else
                                {
                                    foreach (Field column in footerFields)
                                    {
                                        object value = BuildRow(column, ref lineNumber, docToConvert);

                                        // Add row to text file
                                        text.Write(value.ToString().Padding(
                                            column.Width,
                                            (PaddingDirectionTypes)Enum.Parse(typeof(PaddingDirectionTypes), column.Alignment.ToString(), true),
                                            column.PaddingText,
                                            column.Trim));
                                    }
                                    text.WriteLine();
                                    lineNumber++;
                                }
                            }


                            // File created
                            docToConvert = null;
                        }
                        else if (sourceFields.FileType == "FixedWidth")
                        {
                            throw (new Exception("Unable to create from source fixed width.  Feature is not implemented."));
                            //TODO: Code to be implemented for fixed with sources
                        }
                    }
                }
                else if (targetFields.FileType == "XMLData")
                {
                    throw (new Exception("Unable to create from target xml data.  Feature is not implemented."));
                    //TODO: Code to be implemented for xml targets
                }
            }
            catch
            {
                // There has been an issue, remove target file, the rethrow error
                if (File.Exists(targetFilename)) File.Delete(targetFilename);
                throw;
            }
        }

        private object BuildRow(Field targetColumn, ref int lineNumber)
        {
            XmlNode emptyRow = null;
            XmlDocument emptyDoc = null;
            return BuildRow(emptyRow, targetColumn, ref lineNumber, emptyDoc);
        }

        private object BuildRow(Field targetColumn, ref int lineNumber, XmlDocument xmlDoc)
        {
            XmlNode emptyRow = null;
            return BuildRow(emptyRow, targetColumn, ref lineNumber, xmlDoc);
        }

        private object BuildRow(XmlNode newRow, Field targetColumn, ref int lineNumber, XmlDocument xmlDoc)
        {

            object value;

            // If MappedField is blank or does not match docToConvert.CurrentRow then return DefaultValue (if present)
            if (newRow == null || (newRow.Attributes != null && newRow.Attributes[targetColumn.MappedField] == null))
            {

                // If default value is a calculated (%%) then intercept before and dont assign the default value as static
                switch (targetColumn.DefaultValue)
                {
                    // If default value is Date Time Now
                    case "%%DateTimeNow%%":
                        value = new DateTime();
                        value = DateTime.Now;
                        break;
                    // If default value is Line Number
                    case "%%LineNumber%%":
                        value = lineNumber;
                        break;
                    // If default value is NEWLINE
                    case "%%NewLine%%":
                        value = "\r\n";
                        lineNumber++;
                        break;
                    default:
                        // If default value is null, set as so
                        if (targetColumn.DefaultValue == null)
                        {
                            value = null;
                        }
                        // If default value is an xpath query do the following
                        else if (targetColumn.DefaultValue.StartsWith("%%XPath%%"))
                        {
                            //throw new Exception("Code to be added");
                            XPathNavigator nav = xmlDoc.CreateNavigator();
                            value = nav.Evaluate(targetColumn.DefaultValue.Replace("%%XPath%%", ""));
                        }
                        // Else just display default value
                        else
                        {
                            value = targetColumn.DefaultValue;
                        }
                        break;
                }
            }            
            // Mapped filed is valid so pull from docToConvert.CurrentRow
            else if (newRow.Attributes != null && newRow.Attributes[targetColumn.MappedField] != null)
            {
                // parse value to correct datatype and set value
                value =
                    TypeDescriptor.GetConverter(
                    Type.GetType(sourceFields[targetColumn.MappedField].DataType)).ConvertFrom(
                            newRow.Attributes[targetColumn.MappedField].Value);
            }
            // all else fails look in the innerXml
            else
            {
                value =
                    TypeDescriptor.GetConverter(
                    Type.GetType(sourceFields[targetColumn.MappedField].DataType)).ConvertFrom(
                            newRow.InnerText);
            }

            // Perform formatting if set
            if (targetColumn.Format != null || targetColumn.Format != "")
            {
                // If Format start with $RegExReplace the preform a regular expression replace
                if (targetColumn.Format.StartsWith("%%RegExReplace%%"))
                {
                    value = Regex.Replace(value.ToString(), targetColumn.Format.Substring(16), string.Empty);
                }
                // Else use string.format
                else
                {
                    string stringValue = String.Format(targetColumn.Format, value);
                    // If Julian Date was passed in format replace JJJ
                    if (value != null && value.GetType() == typeof(System.DateTime) && stringValue.Contains("JJJ"))
                    {
                        stringValue = stringValue.ToString().Replace("JJJ", ((DateTime)value).DayOfYear.ToString("000"));
                    }
                    value = stringValue;

                }
            }

            //strip new lines if present
            if (value is string)
            {
                value = ((string)value).Replace("\r\n", "¬").Replace("\r", "¬").Replace("\n", "¬");
            }
            return value;
        }

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
             sourceFilename = null;
             targetFilename = null;
             translatorFilename = null;
             sourceFields = null;
             targetFields = null;
             headerFields = null;
             footerFields = null;
        }

        #endregion
    }
}