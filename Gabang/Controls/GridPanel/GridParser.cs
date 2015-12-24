﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class GridData {
        public GridData() {
            RowNames = new List<string>();
            ColumnNames = new List<string>();
            Values = new List<List<string>>();
        }

        public List<string> RowNames { get; }

        public List<string> ColumnNames { get; }

        public List<List<string>> Values { get; }
    }

    public class GridParser {
        private const int VectorIndex = 0;
        private readonly static char[] ValueStart = new char[] { 'c', '\"' };
        private const int ClosingIndex = 1;
        private readonly static char[] ValueDelimiter = new char[] { ',', ')' };

        public static GridData Parse(string input) {
            GridData data = new GridData();

            input = CleanEscape(input);

            int current = 0;
            current = input.IndexOf("structure", current);
            current = input.IndexOf('(', current);
            current = input.IndexOf("list", current);
            current = input.IndexOf('(', current);

            current = NamedValue(input, "row.names", data.RowNames, current);
            current = input.IndexOf(',', current);

            current = NamedValue(input, "col.names", data.ColumnNames, current);
            current = input.IndexOf(',', current);

            current = input.IndexOf("data", current);
            current = input.IndexOf('=', current);
            current = input.IndexOf("structure", current);
            current = input.IndexOf('(', current);

            current = input.IndexOf("list", current);
            current = input.IndexOf('(', current);

            foreach (var colname in data.ColumnNames) {
                List<string> columnValues = new List<string>();
                current = NamedValue(input, colname, columnValues, current);
                data.Values.Add(columnValues);

                current = input.IndexOfAny(ValueDelimiter, current);
            }

            return data;
        }

        public static string CleanEscape(string input) {
            StringBuilder sb = new StringBuilder();
            char prev = ' ';
            foreach (char ch in input) {
                if (ch == '\\') {
                    if (prev == '\\') {
                        sb.Append(ch);
                        prev = ' ';
                    }
                } else {
                    sb.Append(ch);
                    prev = ch;
                }
            }
            return sb.ToString();
        }

        private static int NamedValue(string input, string name, List<string> names, int current) {
            current = input.IndexOf(name, current);
            current = input.IndexOf('=', current);

            current = input.IndexOfAny(ValueStart, current);
            if (input[current] == ValueStart[VectorIndex]) {
                // vector
                current = input.IndexOf('(', current);

                while (true) {
                    string value;
                    current = FirstQuotedString(input, current, out value);
                    names.Add(value);

                    current = input.IndexOfAny(ValueDelimiter, current);
                    if (input[current] == ValueDelimiter[ClosingIndex]) {
                        break;
                    }
                }
            } else {
                // constant
                string value;
                current = FirstQuotedString(input, current, out value);
                names.Add(value);
            }

            return current;
        }

        private static int FirstQuotedString(string input, int startIndex, out string value) {
            int start = input.IndexOf('"', startIndex);

            int end = start + 1;
            while (true) {
                end = input.IndexOf('"', end);
                if (input[end - 1] == '\\') {
                    end++;
                    continue;
                }
                break;
            }

            value = input.Substring(start + 1, end - start - 1);
            return end;
        }
    }
}
