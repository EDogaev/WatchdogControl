using System.ComponentModel;

namespace WatchdogControl.Enums
{
    public enum EditType
    {
        [Description("Добавить")]
        Add,

        [Description("Изменить")]
        Edit,

        [Description("Удалить")]
        Delete
    }
}
