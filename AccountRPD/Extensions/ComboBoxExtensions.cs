using System.Windows.Forms;

namespace AccountRPD.Extensions
{
    public static class ComboBoxExtensions
    {
        public static bool Contains(this ComboBox comboBox, int index)
        {
            return index >= 0 && index < comboBox.Items.Count;
        }
    }
}
