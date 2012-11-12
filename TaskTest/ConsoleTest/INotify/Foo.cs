using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConsoleTest.INotify
{
    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName, object oldValue = null, object newValue = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedArgs(propertyName, oldValue, newValue));
        }
        void RaisePropertyChanged([CallerMemberName]string propertyName = null, object oldValue = null, object newValue = null)
        {
            PropertyChanged(this, new PropertyChangedArgs(propertyName, oldValue, newValue));
        }

        private string customerName;
        public string CustomerName
        {
            get { return customerName; }
            set
            {
                if (value == customerName)
                {
                    return;
                }
                var oldValue = customerName;
                customerName = value;
                RaisePropertyChanged(oldValue: oldValue, newValue: customerName);
            }
        }
    }

    public class PropertyChangedArgs : PropertyChangedEventArgs
    {
        public readonly object OldValue;
        public readonly object NewValue;

        public PropertyChangedArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
