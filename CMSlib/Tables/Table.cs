using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.Tables
{
    public class Table
    {
        internal readonly List<TableSection> sections;
        internal readonly List<TableRow> rows;
        public Table(params TableSection[] sections)
        {
            this.sections = new();
            this.rows = new();
            this.sections.AddRange(sections);
        }
        public Table()
        {
            this.sections = new();
            this.rows = new();
        }
        public void AddSection(TableSection section) => sections.Add(section);
        public void ClearRows() => rows.Clear();
	public TableRow this[int index]{
	    get => rows[index];
	}
	public int RowCount{
	    get => rows.Count;
	}
        public string GetHeader()
        {
            StringBuilder builder = new();
            foreach(TableSection section in sections)
            {
                foreach(TableColumn column in from (TableColumn column, ValueGetter getter) tuple in section.Columns select tuple.column)
                {
                    builder.Append((column.ColumnTitle ?? column.MemberName).TableColumn(column.InnerWidth, column.Adjust, column.Ellipse, column.LeftPipe, column.RightPipe));
                }
            }

            return builder.ToString();
        }

        public IEnumerable<string> GetOutputRows()
        {
            StringBuilder builder = new();
            foreach (var row in rows)
            {
                builder.Clear();
                for (var i = 0; i < sections.Count; i++)
                    foreach (var x in sections[i].Columns)
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

            foreach(TableRow row in rows)
            {
                if ((row.SectionItems?.Length ?? -1) < sections.Count)
                    throw new Exception("Row doesn't have enough items for each section");
                for(int i = 0; i < sections.Count; i++)
                {
                    for(int j = 0; j < sections[i].Columns.Count; j++)
                    {
                        sections[i].Columns[j].column.Deconstruct(out _, out int innerWidth, out _, out bool ellipse, out bool leftPipe, out bool rightPipe, out ExtensionMethods.ColumnAdjust adjust, out CustomStringFormatter formatter);
                        object item = sections[i].Columns[j].getter.Invoke(row.SectionItems[i]);
                        builder.Append((formatter?.Invoke(item) ?? item).TableColumn(innerWidth, adjust, ellipse, leftPipe, rightPipe));
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
            if (sectionItems.Length < sections.Count || sections.Select(x => x.type)
                .Zip(sectionItems.Select(x => x.GetType())).Any(x=>x.First != x.Second && !x.Second.IsSubclassOf(x.First))) return;
            rows.Add(new(sectionItems));
        }
        public record TableRow(params object[] SectionItems);
    }
}
