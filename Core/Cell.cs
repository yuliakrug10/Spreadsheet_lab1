using System.ComponentModel;

namespace Core
{
    public class Cell : INotifyPropertyChanged
    {
        private string _address = "";
        private string _expression = "";
        private bool _calculatedValue = false;
        private bool _hasError = false;
        private string _errorMessage = "";

        public string Address 
        { 
            get => _address; 
            set 
            { 
                _address = value; 
                UpdateCell(nameof(Address)); 
            } 
        }
    public string Expression 
        { 
            get => _expression; 
            set 
            { 
                _expression = value; 
                UpdateCell(nameof(Expression)); 
            } 
        }
    public bool CalculatedValue 
        { 
            get => _calculatedValue; 
            set 
            { 
                _calculatedValue = value; 
                UpdateCell(nameof(CalculatedValue)); 
            } 
        }
    public bool HasError 
        { 
            get => _hasError; 
            set 
            { 
                _hasError = value; 
                UpdateCell(nameof(HasError)); 
            } 
        }
    public string ErrorMessage 
        { 
            get => _errorMessage; 
            set 
            { 
                _errorMessage = value; 
                UpdateCell(nameof(ErrorMessage)); 
            } 
        }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void UpdateCell(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
   
}