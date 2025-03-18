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
    public enum NumberBase
    {
        Hex,
        Dec,
        Oct,
        Bin
    }

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

        private string _programmerInput = "";

        private bool _isDigitGroupingEnabled;
        public bool IsDigitGroupingEnabled
        {
            get => _isDigitGroupingEnabled;
            set
            {
                _isDigitGroupingEnabled = value;
                OnPropertyChanged(nameof(IsDigitGroupingEnabled));
                Settings.Default.DigitGroupingEnabled = value;
                Settings.Default.Save();
                if (!IsProgrammerMode)
                    Display = FormatNumber(_currentValue);
            }
        }

        private bool _isProgrammerMode;
        public bool IsProgrammerMode
        {
            get => _isProgrammerMode;
            set
            {
                _isProgrammerMode = value;
                OnPropertyChanged(nameof(IsProgrammerMode));
                Settings.Default.IsProgrammerMode = value;
                Settings.Default.Save();
                OnPropertyChanged(nameof(ModeLabel));
                if (IsProgrammerMode)
                    _programmerInput = "";
                else
                    _programmerInput = "";
            }
        }

        public string ModeLabel => IsProgrammerMode ? "Programmer" : "Standard";

        private string _hexDisplay;
        public string HexDisplay
        {
            get => _hexDisplay;
            set
            {
                _hexDisplay = value;
                OnPropertyChanged(nameof(HexDisplay));
            }
        }

        private string _decDisplay;
        public string DecDisplay
        {
            get => _decDisplay;
            set
            {
                _decDisplay = value;
                OnPropertyChanged(nameof(DecDisplay));
            }
        }

        private string _octDisplay;
        public string OctDisplay
        {
            get => _octDisplay;
            set
            {
                _octDisplay = value;
                OnPropertyChanged(nameof(OctDisplay));
            }
        }

        private string _binDisplay;
        public string BinDisplay
        {
            get => _binDisplay;
            set
            {
                _binDisplay = value;
                OnPropertyChanged(nameof(BinDisplay));
            }
        }

        private NumberBase _activeBase = NumberBase.Dec;
        public NumberBase ActiveBase
        {
            get => _activeBase;
            set
            {
                _activeBase = value;
                OnPropertyChanged(nameof(ActiveBase));
                // Salvează setarea ca string
                Settings.Default.ActiveNumberBase = value switch
                {
                    NumberBase.Hex => "Hex",
                    NumberBase.Oct => "Oct",
                    NumberBase.Bin => "Bin",
                    _ => "Dec",
                };
                Settings.Default.Save();
                UpdateProgrammerInputFromCurrentValue();
                UpdateProgrammerDisplays();
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

        #region Comenzi

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

        // -- Comanda pentru setrea bazei active --
        public ICommand SetActiveBaseCommand { get; }

        #endregion

        #endregion

        #region Constructor
        public CalculatorViewModel()
        {
            IsDigitGroupingEnabled = Settings.Default.DigitGroupingEnabled;
            IsProgrammerMode = Settings.Default.IsProgrammerMode;

            string activeBase = Settings.Default.ActiveNumberBase;

            ActiveBase =activeBase.ToLower() switch
            {
                "hex" => NumberBase.Hex,
                "dec" => NumberBase.Dec,
                "oct" => NumberBase.Oct,
                "bin" => NumberBase.Bin,
                _ => NumberBase.Dec
            };

            CutCommand = new RelayCommand(_ =>
            {
                _clipboard = Display;
                Display = "";
                double.TryParse(Display, out _currentValue);
                if(IsProgrammerMode)
                    _programmerInput = "";
            });

            CopyCommand = new RelayCommand(_ =>
            {
                _clipboard = Display;
            });

            PasteCommand = new RelayCommand(_ =>
            {
                Display = _clipboard;
                double.TryParse(Display, out _currentValue);
                if(IsProgrammerMode)
                    _programmerInput = Display;
            });

            DigitGroupingCommand = new RelayCommand(_ =>
            {
                IsDigitGroupingEnabled = !IsDigitGroupingEnabled;
            });

            AboutCommand = new RelayCommand(_ =>
            {
                System.Windows.MessageBox.Show("Năstase Teodor-10LF233", "About",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });

            ProgrammerModeCommand = new RelayCommand(_ =>
            {
                IsProgrammerMode = true;
                ResetDisplay();
            });

            StandardModeCommand = new RelayCommand(_ =>
            {
                IsProgrammerMode = false;
                ResetDisplay();
            });

            SetActiveBaseCommand = new RelayCommand(param =>
            {
                if (param is string baseStr)
                {
                    switch(baseStr.ToLower())
                    {
                        case "hex":
                            ActiveBase = NumberBase.Hex;
                            break;
                        case "dec":
                            ActiveBase = NumberBase.Dec;
                            break;
                        case "oct":
                            ActiveBase = NumberBase.Oct;
                            break;
                        case "bin":
                            ActiveBase = NumberBase.Bin;
                            break;
                    }
                }
            });

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
            if (IsProgrammerMode)
            {

                if (!IsValidDigitForActiveBase(digit))
                    return;

                if (_justPressedOperator || string.IsNullOrEmpty(_programmerInput))
                {
                    _programmerInput = digit;
                    _justPressedOperator = false;
                }
                else
                {
                    _programmerInput += digit;
                }
                try
                {
                    int baseVal = ActiveBase switch
                    {
                        NumberBase.Hex => 16,
                        NumberBase.Oct => 8,
                        NumberBase.Bin => 2,
                        _ => 10,
                    };
                    _currentValue = Convert.ToDouble(Convert.ToInt64(_programmerInput, baseVal));
                }
                catch
                {
                    _currentValue = 0;
                }
                Display = _programmerInput.ToUpper();
                UpdateProgrammerDisplays();
            }
            else
            {
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
                double.TryParse(currentRaw, out _currentValue);
                Display = IsDigitGroupingEnabled ? FormatNumber(_currentValue) : currentRaw;
            }

        }


        private void DecimalExecute(object parameter)
        {
            if (IsProgrammerMode)
                return;

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

            if (IsProgrammerMode)
            {
                _programmerInput = Convert.ToString((long)_lastValue, ActiveBase switch { NumberBase.Hex => 16, NumberBase.Dec => 10, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 }).ToUpper();
                Display = _programmerInput;
                UpdateProgrammerDisplays();
            }
            else
            {
                Display = FormatNumber(_lastValue);
            }
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
            if (IsProgrammerMode)
            {
                long intVal = (long)val;
                _programmerInput = Convert.ToString(intVal, ActiveBase switch { NumberBase.Hex => 16, NumberBase.Dec => 10, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 }).ToUpper();
                Display = _programmerInput;
                _currentValue = intVal;
                UpdateProgrammerDisplays();
            }
            else
            {
                Display = FormatNumber(val);
                double.TryParse(Display, out _currentValue);
            }
        }

        private void EqualsExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(_operation))
            {
                double result = Calculeaza(_lastValue, _currentValue, _operation);
                PreviousExpression = $"{_lastValue} {_operation} {_currentValue} ";
                _lastValue = result;
                if (IsProgrammerMode)
                {
                    int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Dec => 10, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                    long intResult = (long)result;
                    _programmerInput = Convert.ToString(intResult, baseVal).ToUpper();
                    Display = _programmerInput;
                    _currentValue = intResult;
                    UpdateProgrammerDisplays();
                }
                else
                {
                    Display = FormatNumber(result);
                    _currentValue = result;
                }
                _operation = "";
                _justPressedOperator = true;
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
            if (IsProgrammerMode)
            {
                _programmerInput = "";
                UpdateProgrammerDisplays();
            }

        }

        private void ClearEntryExecute(object parameter)
        {
            Display = "0";
            _currentValue = 0;
            if(IsProgrammerMode)
                _programmerInput = "";
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
                if (IsProgrammerMode)
                {
                    long intVal = (long)val;
                    int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Dec => 10, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                    _programmerInput = Convert.ToString(intVal, baseVal).ToUpper();
                    Display = _programmerInput;
                    _currentValue = intVal;
                    UpdateProgrammerDisplays();
                }
                else
                {
                    Display = FormatNumber(val);
                    _currentValue = val;
                }
            }
        }

        private void BackspaceExecute(object parameter)
        {
            if (Display == "Error" || Display == "0" || _justPressedOperator)
            {
                Display = "0";
                _currentValue = 0;
                if(IsProgrammerMode)
                    _programmerInput = "";
                return;
            }
            if (IsProgrammerMode)
            {
                if (!string.IsNullOrEmpty(_programmerInput))
                {
                    _programmerInput = _programmerInput.Substring(0, _programmerInput.Length - 1);
                    if (string.IsNullOrEmpty(_programmerInput))
                    {
                        Display = "0";
                        _currentValue = 0;
                    }
                    else
                    {
                        try
                        {
                            int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                            _currentValue = Convert.ToDouble(Convert.ToInt64(_programmerInput, baseVal));
                        }
                        catch { _currentValue = 0; }
                        Display = _programmerInput.ToUpper();
                    }
                    UpdateProgrammerDisplays();
                }
            }
            else
            {
                if (Display.Length > 1)
                    Display = Display.Substring(0, Display.Length - 1);
                else
                    Display = "0";
                double.TryParse(Display, out _currentValue);
            }
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

            if (IsProgrammerMode)
            {
                int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                long intVal = (long)_currentValue;
                _programmerInput = Convert.ToString(intVal, baseVal).ToUpper();
                Display = _programmerInput;
                UpdateProgrammerDisplays();
            }
            else
            {
                Display = FormatNumber(_currentValue);
            }
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

        private void UpdateProgrammerInputFromCurrentValue()
        {
            int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
            long intVal = (long)_currentValue;
            _programmerInput = Convert.ToString(intVal, baseVal).ToUpper();
            Display = _programmerInput;
        }

        private bool IsValidDigitForActiveBase(string digit)
        {
            if (string.IsNullOrEmpty(digit))
                return false;

            digit = digit.ToUpper();

            switch (ActiveBase)
            {
                case NumberBase.Bin:
                    return digit == "0" || digit == "1";
                case NumberBase.Oct:
                    return "01234567".Contains(digit);
                case NumberBase.Dec:
                    return "0123456789".Contains(digit);
                case NumberBase.Hex:
                    return "0123456789ABCDEF".Contains(digit);
                default:
                    return false;
            }
        }

        private void ResetDisplay()
        {
            Display = "0";
            _currentValue = 0;
            _programmerInput = "";
            PreviousExpression = "";

            HexDisplay = "";
            DecDisplay = "";
            OctDisplay = "";
            BinDisplay = "";
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
                if (IsProgrammerMode)
                {
                    int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                    _programmerInput = Convert.ToString((long)val, baseVal).ToUpper();
                    Display = _programmerInput;
                }
                else
                {
                    Display = FormatNumber(val);
                }
                _currentValue = val;
            }
        }

        // M> – toggling: IsMemoryPanelVisible = !IsMemoryPanelVisible (setat în constructor)

        private void SelectMemoryItemExecute(object parameter)
        {
            if (parameter is double memVal)
            {
                if (IsProgrammerMode)
                {
                    int baseVal = ActiveBase switch { NumberBase.Hex => 16, NumberBase.Oct => 8, NumberBase.Bin => 2, _ => 10 };
                    _programmerInput = Convert.ToString((long)memVal, baseVal).ToUpper();
                    Display = _programmerInput;
                }
                else
                {
                    Display = FormatNumber(memVal);
                }
                _currentValue = memVal;
            }
        }

        #endregion
    }
}