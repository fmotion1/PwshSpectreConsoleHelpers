namespace PwshSpectreConsole {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a table object for the Spectre console library.
    /// </summary>
    /// <remarks>
    /// This class provides properties to configure the table such as width, wrap, header visibility and color, border style and color, title and title color.
    /// It also provides methods to add data to the table, set column widths by percentage or by header, and add new columns.
    /// </remarks>
    public class SpectreTableObject {

        private SpectreColorValue _borderColor = new SpectreColorValue("#878787");
        private SpectreColorValue _titleColor = new SpectreColorValue("#FFFFFF");
        private SpectreColorValue _headerColor = new SpectreColorValue("#FFFFFF");

        public SpectreColorValue HeaderColor {
            get => _headerColor;
            set => _headerColor = value;
        }

        public SpectreColorValue BorderColor {
            get => _borderColor;
            set => _borderColor = value;
        }

        public SpectreColorValue TitleColor {
            get => _titleColor;
            set => _titleColor = value;
        }

        public int? TableWidth { get; set; }
        public bool TableWrap { get; set; } = false;
        public bool HideHeaders { get; set; } = false;
        public bool AllowMarkup { get; set; } = true;

        private BorderStyle _borderStyle;

        public BorderStyle BorderStyle {
            get => _borderStyle;
            set {
                _borderStyle = value;
            }
        }

        public string TableTitle { get; set; } = "";

        public int NumColumns => _columns.Count;
        public int NumRows => _columns.Max(column => column.NumRows);

        private List<object> _property = new List<object>();
        private List<SpectreTableColumnObject> _columns = new List<SpectreTableColumnObject>();

        /// <summary>
        /// Constructor. Optionally allows to set the title of the table.
        /// </summary>
        /// <param name="tabletitle"></param>
        public SpectreTableObject(string tableTitle="Default Table Title", int? tableWidth=null) {
            TableTitle = tableTitle;
            if (tableWidth != null) {
                TableWidth = tableWidth;
            }
        }

        /// <summary>
        /// Adds data to the SpectreTable. The data can be a list, a single PSObject, or an array of scalars.
        /// </summary>
        /// <param name="data">The data to add. Can be a list, a single PSObject, or an array of scalars.</param>
        /// <remarks>
        /// If the data is a list, the method initializes the columns based on the first PSObject in the list and adds rows from all PSObjects in the list.
        /// If the data is a single PSObject, the method handles it similarly to handling the first PSObject in a list.
        /// If the data is an array of scalars, the method creates a new column and adds each item in the array as a row in the column.
        /// </remarks>
        public void AddData(object data) {
            if (data is IList dataList && dataList.Count > 0) {
            // Determine if the list contains PSObjects
                if (dataList[0] is PSObject) {
                    // Initialize columns based on the first PSObject
                    if (_columns.Count == 0) { // Check if columns are already defined
                        if (dataList[0] is PSObject firstPsObject) {
                            foreach (var prop in firstPsObject.Properties) {
                                _columns.Add(new SpectreTableColumnObject(prop.Name));
                            }
                        }
                    }

                    // Add rows from all PSObjects in the list
                    foreach (PSObject psObject in dataList) {
                        foreach (var prop in psObject.Properties) {
                            var column = _columns.FirstOrDefault(c => c.Header == prop.Name);
                            if (column != null) {
                                string value = prop.Value?.ToString() ?? string.Empty; // Safely handle null
                                column.AddRows(new string[] { value });
                            }
                        }
                    }
                }
            } else if (data is PSObject psObjectSingle) {
                // Handle a single PSObject (similar to handling the first PSObject in a list)
                if (_columns.Count == 0) { // This condition might always be true based on your logic for single PSObjects
                    foreach (var prop in psObjectSingle.Properties) {
                        _columns.Add(new SpectreTableColumnObject(prop.Name));
                        _columns.Last().AddRows(new string[] { prop.Value?.ToString() ?? string.Empty });
                    }
                }
            }

            // Check if data is an array of scalars (e.g., string[], int[], etc.)
            else if (data.GetType().IsArray) {
                var array = data as Array;
                var column = new SpectreTableColumnObject("Value");
                if (array != null) {
                    foreach (var item in array) {
                        column.AddRows(new string[] { item?.ToString() ?? string.Empty });
                    }
                }
                _columns.Add(column);
            }
        }

        /// <summary>
        /// Converts the provided value to a double. If the value is a string ending with a percentage sign, it removes the percentage sign before converting.
        /// </summary>
        /// <param name="value">The value to convert to a double.</param>
        /// <returns>The converted double value, or 0 if the conversion fails.</returns>
        private double ConvertToDouble(object value) {
            if (value is string valueStr && valueStr.EndsWith("%")) {
                valueStr = valueStr.TrimEnd('%');
                return double.TryParse(valueStr, out double result) ? result : 0;
            }
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Sets the widths of the columns in the SpectreTable by percentage.
        /// </summary>
        /// <param name="columnWidths">A Dictionary, Hashtable, or PSObject that maps column headers to their widths as percentages.</param>
        /// <remarks>
        /// If the columnWidths parameter is a Dictionary, Hashtable, or PSObject, the method converts the values to doubles and applies the widths.
        /// If the columnWidths parameter is not a Dictionary, Hashtable, or PSObject, the method throws an ArgumentException.
        /// The method calculates the actual width of each column by multiplying the table width by the percentage for the column.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when the columnWidths parameter is not a Dictionary, Hashtable, or PSObject.</exception>
        public void SetColumnWidthsByPercentage(object columnWidths) {
            if (TableWidth <= 0) return;

            // Handling Dictionary<string, object> to allow mixed value types
            Dictionary<string, double> columnWidthPercentages = new Dictionary<string, double>();
            if (columnWidths is Dictionary<string, object> dict) {
                columnWidthPercentages = dict.ToDictionary(kvp => kvp.Key, kvp => ConvertToDouble(kvp.Value));
            }
            else if (columnWidths is Hashtable widthsHashtable) {
                columnWidthPercentages = widthsHashtable
                    .Cast<DictionaryEntry>()
                    .ToDictionary(kvp => kvp.Key.ToString()!, kvp => ConvertToDouble(kvp.Value ?? 0));
            }
            else if (columnWidths is PSObject psObject) {
                columnWidthPercentages = psObject.Properties
                    .ToDictionary(prop => prop.Name, prop => ConvertToDouble(prop.Value));
            }
            else {
                throw new ArgumentException("Parameter must be a Dictionary, Hashtable, or PSObject");
            }

            // Apply the widths
            foreach (var kvp in columnWidthPercentages) {
                var column = _columns.FirstOrDefault(c => c.Header == kvp.Key);
                if (column != null && TableWidth != null) {
                    int calculatedWidth = (int)(TableWidth * (kvp.Value / 100.0));
                    column.ColumnWidth = calculatedWidth;
                }
            }
        }

        /// <summary>
        /// Configures a columns width identified by its header. Only works if
        /// </summary>
        /// <param name="header"></param>
        /// <param name="width"></param>
        public void SetColumnWidthByHeader(string header, int width) {
            if (TableWidth <= 0){
                var column = _columns.FirstOrDefault(c => c.Header == header);
                if (column != null) {
                    column.ColumnWidth = width;
                }
            }
        }

        /// <summary>
        /// Configures a columns width identified by its index (Order).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        public void SetColumnWidthByIndex(int index, int width) {
            if (TableWidth <= 0){
                if (index >= 0 && index < _columns.Count) {
                    _columns[index].ColumnWidth = width;
                }
            }
        }

        /// <summary>
        /// Adds a new column to the table, with an optional ability to populate values.
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <param name="values"></param>
        public SpectreTableColumnObject addColumn(string columnHeader, string[]? values=null) {
            var newColumn = new SpectreTableColumnObject(columnHeader);
            if (values != null) {
                newColumn.AddRows(values);
            }
            _columns.Add(newColumn);
            return newColumn;
        }

        public SpectreTableColumnObject addColumn(SpectreTableColumnObject columnObject) {
            _columns.Add(columnObject);
            return columnObject;
        }
    }

    /// <summary>
    /// Represents a column in a Spectre table.
    /// </summary>
    public class SpectreTableColumnObject {

        public string Header { get; set; }
        public string AlignContent { get; set; } = "Left";
        public int ColumnWidth { get; set; } = 0;
        public int PadLeft { get; set; } = 2;
        public int PadRight { get; set; } = 2;
        public int NumRows => _rows.Count;
        public bool WrapContent { get; set; } = false;

        private List<string> _rows = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectreTableColumnObject"/> class.
        /// </summary>
        /// <param name="header">The header of the column. Default is "Header".</param>
        public SpectreTableColumnObject(string header = "Header") {
            Header = header;
        }

        /// <summary>
        /// Adds rows to the column.
        /// </summary>
        /// <param name="rowStrings">An array of strings, each representing a row.</param>
        public void AddRows (string[] rowStrings) {
            _rows.AddRange(rowStrings);
        }

        /// <summary>
        /// Sets the padding for the column.
        /// </summary>
        /// <param name="left">The left padding.</param>
        /// <param name="right">The right padding.</param>
        public void setPadding(int left, int right) {
            PadLeft = left;
            PadRight = right;
        }
    }
}
