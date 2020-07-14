using System.Drawing;
using System.Windows.Forms;

namespace AccountRPD.Extensions
{
    public static class DataGridViewExtensions
    {
        public static object GetSelectedItem(this DataGridView dataGridView)
        {
            var selectedItem = default(object);

            if (!dataGridView.SelectedRows.Count.Equals(0) && dataGridView.SelectedRows[0].DataBoundItem != null)
            {
                selectedItem = dataGridView.SelectedRows[0].DataBoundItem;
            }

            return selectedItem;
        }

        public static int GetSelectedItemIndex(this DataGridView dataGridView)
        {
            var selectedIndex = -1;

            if (!dataGridView.SelectedRows.Count.Equals(0))
            {
                selectedIndex = dataGridView.SelectedRows[0].Index;
            }

            return selectedIndex;
        }

        public static bool IsItemSelected(this DataGridView dataGridView)
        {
            return !dataGridView.GetSelectedItemIndex().Equals(-1);
        }

        public static bool Contains(this DataGridView dataGridView, int index)
        {
            return index >= 0 && index < dataGridView.Rows.Count;
        }

        public static void HighlightHeaderTextInBold(this DataGridView dataGridView)
        {
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
        }
    }
}
