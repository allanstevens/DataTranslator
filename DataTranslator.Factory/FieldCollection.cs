using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;

namespace AllanStevens.DataTranslator.Factory
{

    public enum FieldAlignmentTypes
    {
        Left,
        Right
    }

    //public enum FieldTypes
    //{
    //    FixedWidth,
    //    XMLData
    //}

    class FieldCollection : CollectionBase
    {

        private string _fileType;
        private string _xPathLocation;
        private string _fileNameFormat;
        //private bool _autoGenerate;

        public Field this[int index]
        {
            get
            {
                // if array is empty return null not crash!
                if (index < List.Count)
                {
                    return (Field)List[index];
                }
                else
                {
                    return null;
                }
            }
            set { List[index] = value; }
        }

        public Field this[string fieldName]
        {
            get
            {
                foreach (Field item in List)
                {
                    if (item.Name == fieldName) return (Field)item;
                }
                return null;
            }
        }

        public int Add(Field value)
        {
            return (List.Add(value));
        }
        public void AddRange(Field[] items)
        {
            foreach (Field item in items)
                Add(item);
        }

        public string FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }
        public string XPathLocation
        {
            get { return _xPathLocation; }
            set { _xPathLocation = value; }
        }
        public string FileNameFormat
        {
            get { return _fileNameFormat; }
            set { _fileNameFormat = value; }
        }

        //public bool AutoGenerate
        //{
        //    get { return _autoGenerate; }
        //    set { _autoGenerate = value; }
        //}

        //public bool IncludeFooter
        //{
        //    get { return _includeFooter; }
        //    set { _includeFooter = value; }
        //}

        //public string IncludeHeaderAsString
        //{
        //    get { return _includeHeader.ToString(); }
        //    set { bool.TryParse(value, out _includeHeader); }
        //}


        /// <summary>
        /// Method loads collection from xml file, xml must match strict format in usergudie document
        /// </summary>
        /// <param name="xPathLocation"></param>
        /// <param name="translatorFilename"></param>
        /// <returns></returns>
        public void PopulateFromXML(string xPathLocation, string translatorFilename)
        {

            XmlDocument docTranslator = new XmlDocument();
            docTranslator.Load(translatorFilename);

            // Set the main file information
            XmlNode xmlNodeCurrent = docTranslator.SelectSingleNode(xPathLocation);

            if (xmlNodeCurrent != null)
            {

                for (int i = 0; i < xmlNodeCurrent.Attributes.Count; i++)
                {
                    XmlAttribute attrib = xmlNodeCurrent.Attributes[i];
                    switch (attrib.Name)
                    {
                        case "Type":
                            this.FileType = attrib.Value;
                            break;
                        case "XPathLocation":
                            this.XPathLocation = attrib.Value;
                            break;
                        case "FileNameFormat":
                            this.FileNameFormat = attrib.Value;
                            break;
                        //case "IncludeFooter":
                        //    this.IncludeFooter = (attrib.Value.ToLower() == "true");
                        //    break;
                    }
                }
                // Create the fields
                foreach (XmlNode node in docTranslator.SelectNodes(xPathLocation + "/Fields/Field"))
                {
                    Field field = new Field();
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        XmlAttribute attrib = node.Attributes[i];
                        switch (attrib.Name)
                        {
                            case "Name":
                                field.Name = attrib.Value;
                                break;
                            case "Width":
                                field.Width = int.Parse(attrib.Value);
                                break;
                            case "MappedField":
                                field.MappedField = attrib.Value;
                                break;
                            case "Alignment":
                                field.Alignment = (FieldAlignmentTypes)Enum.Parse(typeof(FieldAlignmentTypes), attrib.Value, true);
                                break;
                            case "DefaultValue":
                                field.DefaultValue = attrib.Value;
                                break;
                            case "DataType":
                                field.DataType = attrib.Value;
                                break;
                            case "Format":
                                field.Format = attrib.Value;
                                break;
                            case "Trim":
                                field.Trim = (attrib.Value.ToLower() == "true");
                                break;
                            case "PaddingText":
                                field.PaddingText = attrib.Value;
                                break;
                            case "XPathLocation":
                                field.XPathLocation = attrib.Value;
                                break;

                        }
                    }
                    this.Add(field);
                }
                docTranslator = null;
            }
        }
    }

    public class Field
    {
        private string _name;
        private int _width;
        private string _mappedField;
        private FieldAlignmentTypes _alignment = FieldAlignmentTypes.Left;
        private string _defaultValue;
        private string _dataType = "System.String";
        private string _format = "{0}";
        private bool _trim;
        private string _paddingtext;
        private string _xPathLocation;

        public Field() { }
        public Field(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        //public string Width
        //{
        //    get { return _width.ToString(); }
        //    set { int.TryParse(value, out _width); }
        //}

        public string MappedField
        {
            get { return _mappedField; }
            set { _mappedField = value; }
        }

        public FieldAlignmentTypes Alignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }

        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        public bool Trim
        {
            get { return _trim; }
            set { _trim = value; }
        }

        public string PaddingText
        {
            get { return _paddingtext; }
            set { _paddingtext = value; }
        }

        public string XPathLocation
        {
            get { return _xPathLocation; }
            set { _xPathLocation = value; }
        }

    }

}
