using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.Models
{
    public class Provider : NotifyPropertyChanged
    {
        private string _name;
        private string _description;

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }


        public override bool Equals(object obj)
        {
            if (!(obj is Provider provider))
                return false;

            return _name == provider._name/* && _description == provider._description*/;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397)/* ^ (_description != null ? _description.GetHashCode() : 0)*/;
            }
        }
    }
}
