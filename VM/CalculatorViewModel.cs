using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Calculator.VM
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        #region Initializari
        private string _previousExpression = "";
        private string _display = "0";

        private double _lastValue = 0;
        private double _currentValue = 0;
        private string _operation = "";
        private bool _justPressedOperator = false;
        private string _clipboard = "";

        private bool _isDigitGroupingEnabled;
        public bool IsDigitGroupingEnabled
        {
            get => _isDigitGroupingEnabled;
            set
            {
                _isDigitGroupingEnabled = value;
                OnPropertyChanged(nameof(IsDigitGroupingEnabled));
                Display = FormatNumber(_currentValue);
            }
        }

        private string _hexDisplay = "0";
        public string HexDisplay
        {
            get => _hexDisplay;
            set { _hexDisplay = value; OnPropertyChanged(nameof(HexDisplay)); }
        }

        private string _decDisplay = "0";
        public string DecDisplay
        {
            get => _decDisplay;
            set { _decDisplay = value; OnPropertyChanged(nameof(DecDisplay)); }
        }

        private string _octDisplay = "0";
        public string OctDisplay
        {
            get => _octDisplay;
            set { _octDisplay = value; OnPropertyChanged(nameof(OctDisplay)); }
        }

        private string _binDisplay = "0";
        public string BinDisplay
        {
            get => _binDisplay;
            set { _binDisplay = value; OnPropertyChanged(nameof(BinDisplay)); }
        }


        private bool _isProgrammerMode;
        public bool IsProgrammerMode
        {
            get => _isProgrammerMode;
            set
            {
                _isProgrammerMode = value;
                OnPropertyChanged(nameof(IsProgrammerMode));
                // Poți adăuga logică suplimentară aici (ex. resetare display etc.)
            }
        }


        // -- MEMORIE: stivă de valori --
        public ObservableCollection<double> MemoryStack { get; }
            = new ObservableCollection<double>();

        // Aratam/ascundem panoul de memorie    
        private bool _isMemoryPanelVisible;
        public bool IsMemoryPanelVisible
        {
            get => _isMemoryPanelVisible;
            set
            {
                _isMemoryPanelVisible = value;
                OnPropertyChanged(nameof(IsMemoryPanelVisible));
            }
        }

        // Proprietăți de binding
        public string PreviousExpression
        {
            get => _previousExpression;
            set { _previousExpression = value; OnPropertyChanged(nameof(PreviousExpression)); }
        }

        public string Display
        {
            get => _display;
            set { _display = value; OnPropertyChanged(nameof(Display)); }
        }

        // Eveniment standard pentru INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --Comenzi pentru Informatii--

        public ICommand ToggleMenuCommand { get; }
        public ICommand ProgrammerCommand { get; }
        public ICommand DigitGroupingCommand { get; }
        public ICommand AboutCommand { get; }

        // -- Comenzi pentru Calculator --
        public ICommand DigitCommand { get; }
        public ICommand DecimalCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand UnaryCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand SignCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand PercentCommand { get; }

        // -- Comenzi pentru MEMORIE --
        public ICommand MemoryClearCommand { get; }
        public ICommand MemoryAddCommand { get; }
        public ICommand MemoryRemoveCommand { get; }
        public ICommand MemoryStoreCommand { get; }
        public ICommand MemoryRecallCommand { get; }
        public ICommand ToggleMemoryPanelCommand { get; }
        public ICommand SelectMemoryItemCommand { get; }

        // -- Comenzi pentru Clipboard --
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        // -- Comenzi pentru Mode --
        public ICommand ProgrammerModeCommand { get; }
        public ICommand StandardModeCommand { get; }


        #endregion

        #region Constructor
        public CalculatorViewModel()
        {
            CutCommand = new RelayCommand(_ =>
            {
                // Copiază conținutul afișajului în clipboard și apoi șterge afișajul.
                _clipboard = Display;
                Display = "";
                double.TryParse(Display, out _currentValue);
            });

            CopyCommand = new RelayCommand(_ =>
            {
                // Copiază conținutul afișajului în clipboard.
                _clipboard = Display;
            });

            PasteCommand = new RelayCommand(_ =>
            {
                // Setează afișajul cu conținutul din clipboard.
                Display = _clipboard;
                double.TryParse(Display, out _currentValue);
            });

            DigitGroupingCommand = new RelayCommand(_ =>
            {
                // Toggle digit grouping
                IsDigitGroupingEnabled = !IsDigitGroupingEnabled;
            });

            AboutCommand = new RelayCommand(_ =>
            {
                System.Windows.MessageBox.Show("Năstase Teodor-10LF233", "About",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });

            ProgrammerModeCommand = new RelayCommand(_ => IsProgrammerMode = true);
            StandardModeCommand = new RelayCommand(_ => IsProgrammerMode = false);

            IsProgrammerMode = false;
            
            IsMemoryPanelVisible = false;

            // Inițializare comenzi Calcul
            DigitCommand = new RelayCommand(DigitExecute);
            DecimalCommand = new RelayCommand(DecimalExecute);
            OperationCommand = new RelayCommand(OperationExecute);
            UnaryCommand = new RelayCommand(UnaryExecute);
            EqualsCommand = new RelayCommand(EqualsExecute);
            ClearCommand = new RelayCommand(ClearExecute);
            ClearEntryCommand = new RelayCommand(ClearEntryExecute);
            SignCommand = new RelayCommand(SignExecute);
            BackspaceCommand = new RelayCommand(BackspaceExecute);
            PercentCommand = new RelayCommand(PercentExecute);

            //Inițializare comenzi Memorie

            MemoryClearCommand = new RelayCommand(MemoryClearExecute);
            MemoryAddCommand = new RelayCommand(MemoryAddExecute);
            MemoryRemoveCommand = new RelayCommand(MemoryRemoveExecute);
            MemoryStoreCommand = new RelayCommand(MemoryStoreExecute);
            MemoryRecallCommand = new RelayCommand(MemoryRecallExecute);
            ToggleMemoryPanelCommand = new RelayCommand(_ => IsMemoryPanelVisible = !IsMemoryPanelVisible);
            SelectMemoryItemCommand = new RelayCommand(SelectMemoryItemExecute);

        }

        #endregion

        #region Logica de bază a calculatorului

        private void DigitExecute(object parameter)
        {
            string digit = parameter as string;
            // Elimină separatorii de mii din Display pentru a evita concatenări greșite
            string currentRaw = Display.Replace(",", "");

            if (_justPressedOperator || currentRaw == "0")
            {
                currentRaw = digit;
                _justPressedOperator = false;
            }
            else
            {
                currentRaw += digit;
            }

            // Actualizează valoarea curentă
            double.TryParse(currentRaw, out _currentValue);

            // Dacă digit grouping este activ, formatează numărul; altfel, folosește valoarea raw
            Display = IsDigitGroupingEnabled ? FormatNumber(_currentValue) : currentRaw;

            if (IsProgrammerMode)
                UpdateProgrammerDisplays();
        }


        private void DecimalExecute(object parameter)
        {
            if (_justPressedOperator)
            {
                Display = "0.";
                _justPressedOperator = false;
            }
            else if (!Display.Contains("."))
            {
                Display += ".";
            }
            double.TryParse(Display, out _currentValue);
        }

        private void OperationExecute(object parameter)
        {
            string op = parameter as string;

            if (string.IsNullOrEmpty(_operation))
            {
                _lastValue = _currentValue;
                _operation = op;
                PreviousExpression = $"{_lastValue} {op}";
            }
            else
            {
                double result = Calculeaza(_lastValue, _currentValue, _operation);
                _lastValue = result;

                PreviousExpression = $"{PreviousExpression} {_currentValue} =";
                _operation = op;
                PreviousExpression = $"{_lastValue} {op}";
            }

            Display = FormatNumber(_lastValue);
            _justPressedOperator = true;
        }

        private void UnaryExecute(object parameter)
        {
            string op = parameter as string;
            double val = _currentValue;
            switch (op)
            {
                case "1/x":
                    if (Math.Abs(val) < 1e-15)
                    {
                        Display = "Error";
                        return;
                    }
                    val = 1.0 / val;
                    break;
                case "x²":
                    val = val * val;
                    break;
                case "√x":
                    if (val < 0)
                    {
                        Display = "Error";
                        return;
                    }
                    val = Math.Sqrt(val);
                    break;
            }
            Display = FormatNumber(val);
            double.TryParse(Display, out _currentValue);
        }

        private void EqualsExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(_operation))
            {
                double result = Calculeaza(_lastValue, _currentValue, _operation);
                PreviousExpression = $"{_lastValue} {_operation} {_currentValue} ";
                _lastValue = result;
                Display = FormatNumber(result);
                _operation = "";
                _justPressedOperator = true;
                _currentValue = result;

                if (IsProgrammerMode)
                    UpdateProgrammerDisplays();
            }
        }

        private void ClearExecute(object parameter)
        {
            _lastValue = 0;
            _currentValue = 0;
            _operation = "";
            Display = "0";
            PreviousExpression = "";
            _justPressedOperator = false;
        }

        private void ClearEntryExecute(object parameter)
        {
            Display = "0";
            _currentValue = 0;
        }

        private void SignExecute(object parameter)
        {
            if(Display == "Error" || _justPressedOperator)
            {
                return;
            }

            if (double.TryParse(Display, out double val))
            {
                val = -val;
                Display = FormatNumber(val);
                _currentValue = val;
            }
        }

        private void BackspaceExecute(object parameter)
        {
            if (Display == "Error" || Display == "0" || _justPressedOperator)
            {
                Display = "0";
                _currentValue = 0;
                return;
            }
            if (Display.Length > 1)
            {
                Display = Display.Substring(0, Display.Length - 1);
            }
            else
            {
                Display = "0";
            }
            double.TryParse(Display, out _currentValue);
        }

        private void PercentExecute(object parameter)
        {
            if(!string.IsNullOrEmpty(_operation))
            {
                if(_operation == "+" || _operation == "-")
                {
                    _currentValue = _lastValue * _currentValue / 100;
                }
                else if (_operation == "×" || _operation == "÷")
                {
                    _currentValue /= 100;
                }
            }
            else
            {
                _currentValue /= 100;
            }

            Display = FormatNumber(_currentValue);
            _justPressedOperator = true;
        }

        private double Calculeaza(double a, double b, string op)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "×" => a * b,
                "÷" => Math.Abs(b) < 1e-15 ? double.NaN : a / b,
                _ => b
            };
        }

        private string FormatNumber(double number)
        {
            // Formatul "#,##0.########" inserează virgule la fiecare 3 cifre în partea întreagă,
            // iar partea zecimală se afișează doar dacă există.
            return IsDigitGroupingEnabled ? number.ToString("#,##0.########", System.Globalization.CultureInfo.InvariantCulture)
                                          : number.ToString();
        }

        private void UpdateProgrammerDisplays()
        {
            // Pentru modul programmer, vom considera _currentValue ca fiind un număr întreg
            long intVal = (long)_currentValue;
            HexDisplay = intVal.ToString("X");
            DecDisplay = intVal.ToString();
            OctDisplay = Convert.ToString(intVal, 8);
            BinDisplay = Convert.ToString(intVal, 2);
        }




        #endregion

        #region Logica pentru MEMORIE

        //MC - sterge toate valorile din memorie
        private void MemoryClearExecute(object parameter)
        {
            MemoryStack.Clear();
        }

        // M+ – adaugă valoarea curentă la ultima intrare din memorie (dacă nu există, se comportă ca MS)
        private void MemoryAddExecute(object parameter)
        {
            if (MemoryStack.Any())
            {
                int lastIndex = MemoryStack.Count - 1;
                MemoryStack[lastIndex] += _currentValue;
            }
            else
            {
                MemoryStack.Add(_currentValue);
            }
        }

        // M- – scade din ultima intrare din memorie (dacă există)
        private void MemoryRemoveExecute(object parameter)
        {
            if (MemoryStack.Any())
            {
                int lastIndex = MemoryStack.Count - 1;
                MemoryStack[lastIndex] -= _currentValue;
            }
        }

        // MS – stocarea (push) în memorie a valorii de pe display
        private void MemoryStoreExecute(object parameter)
        {
            MemoryStack.Add(_currentValue);
        }

        // MR – afișarea în display a ultimei valori din memorie (dacă există)
        private void MemoryRecallExecute(object parameter)
        {
            if (MemoryStack.Any())
            {
                double val = MemoryStack.Last();
                Display = FormatNumber(val);
                _currentValue = val;
            }
        }

        // M> – toggling: IsMemoryPanelVisible = !IsMemoryPanelVisible (setat în constructor)

        private void SelectMemoryItemExecute(object parameter)
        {
            if (parameter is double memVal)
            {
                Display = FormatNumber(memVal);
                _currentValue = memVal;
            }
        }

        #endregion
    }
}