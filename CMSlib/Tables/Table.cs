using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSlib.Tables
{
    public class Table
    {
        internal readonly List<TableSection> Sections;
        internal readonly List<TableRow> Rows;

        public Table(params TableSection[] sections)
        {
            this.Sections = new();
            this.Rows = new();
            this.Sections.AddRange(sections);
        }

        public Table()
        {
            this.Sections = new();
            this.Rows = new();
        }

        public void AddSection(TableSection section) => Sections.Add(section);
        public void ClearRows() => Rows.Clear();

        public TableRow this[int index]
        {
            get => Rows[index];
        }

        public int RowCount
        {
            get => Rows.Count;
        }

        public string GetHeader()
        {
            StringBuilder builder = new();
            foreach (TableSection section in Sections)
            {
                foreach (TableColumn column in from (TableColumn column, ValueGetter getter) tuple in section.Columns
                    select tuple.column)
                {
                    builder.Append((column.ColumnTitle ?? column.MemberName).TableColumn(column.InnerWidth,
                        column.Adjust, column.Ellipse, column.LeftPipe, column.RightPipe));
                }
            }

            return builder.ToString();
        }

        public IEnumerable<string> GetOutputRows()
        {
            StringBuilder builder = new();
            foreach (var row in Rows)
            {
                builder.Clear();
                for (var i = 0; i < Sections.Count; i++)
                    foreach (var x in Sections[i].Columns)
                    {
                        object item = x.getter.Invoke(row.SectionItems[i]);
                        builder.Append((x.column.CustomFormatter?.Invoke(item) ?? item).TableColumn(x.column.InnerWidth,
                            x.column.Adjust, x.column.Ellipse, x.column.LeftPipe, x.column.RightPipe));
                    }

                yield return builder.ToString();
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append(GetHeader());
            builder.Append('\n');

            foreach (TableRow row in Rows)
            {
                if ((row.SectionItems?.Length ?? -1) < Sections.Count)
                    throw new Exception("Row doesn't have enough items for each section");
                for (int i = 0; i < Sections.Count; i++)
                {
                    for (int j = 0; j < Sections[i].Columns.Count; j++)
                    {
                        Sections[i].Columns[j].column.Deconstruct(out _, out int innerWidth, out _, out bool ellipse,
                            out bool leftPipe, out bool rightPipe, out ExtensionMethods.ColumnAdjust adjust,
                            out CustomStringFormatter formatter);
                        object item = Sections[i].Columns[j].getter.Invoke(row.SectionItems?[i]);
                        builder.Append((formatter?.Invoke(item) ?? item).TableColumn(innerWidth, adjust, ellipse,
                            leftPipe, rightPipe));
                    }
                }

                builder.Append('\n');
            }

            if (builder.Length > 0 && builder[^1] is '\n')
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        public void AddRow(params object[] sectionItems)
        {
            if (sectionItems.Length < Sections.Count || Sections.Select(x => x.Type)
                .Zip(sectionItems.Select(x => x.GetType()))
                .Any(x => x.First != x.Second && !x.Second.IsSubclassOf(x.First))) return;
            Rows.Add(new(sectionItems));
        }

        public record TableRow(params object[] SectionItems);
    }
}