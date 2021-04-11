using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.Tables
{
    public class Table
    {
        private readonly List<TableSection> sections;
        private readonly List<TableRow> rows;
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

        public override string ToString()
        {
            StringBuilder builder = new();
            foreach(TableSection section in sections)
            {
                foreach(TableColumn column in from (TableColumn column, ValueGetter getter) tuple in section.Columns select tuple.column)
                {
                    builder.Append((column.ColumnTitle ?? column.MemberName).TableColumn(column.InnerWidth, column.Adjust, column.Ellipse, column.LeftPipe, column.RightPipe));
                }
            }
            builder.Append('\n');
            List<(TableColumn column, ValueGetter getter    )> columns = new();
            foreach(TableSection tableSection in sections)
            {
                columns.AddRange(tableSection.Columns);
            }
            foreach(TableRow row in rows)
            {
                if ((row.SectionItems?.Length ?? -1) < sections.Count)
                    throw new Exception("Row doesn't have enough items for each section");
                for(int i = 0; i < sections.Count; i++)
                {
                    if(row.SectionItems[i].GetType() == sections[i].type)
                    {
                        for(int j = 0; j < sections[i].Columns.Count; j++)
                        {
                            sections[i].Columns[j].column.Deconstruct(out _, out int innerWidth, out _, out bool ellipse, out bool leftPipe, out bool rightPipe, out ExtensionMethods.ColumnAdjust adjust);
                            builder.Append(sections[i].Columns[j].getter.Invoke(row.SectionItems[i]).TableColumn(innerWidth, adjust, ellipse, leftPipe, rightPipe));
                        }
                    }
                }
                builder.Append('\n');
            }
            return builder.ToString();
        }
        public void AddRow(params object[] sectionItems) => rows.Add(new(sectionItems));
        public record TableRow(params object[] SectionItems);
    }
}
