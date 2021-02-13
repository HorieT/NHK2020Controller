using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;

namespace MVVMLib
{
    /// <summary>
    /// 某サイトのほぼパクリ
    /// https://qiita.com/tricogimmick/items/f07ef53dea817d198475
    /// http://sourcechord.hatenablog.com/entry/2014/03/15/234153
    /// </summary>
    public abstract class ViewModel : NotifyPropertyChanged, IDataErrorInfo
    {
        #region == implemnt of IDataErrorInfo ==

        // IDataErrorInfo用のエラーメッセージを保持する辞書
        private Dictionary<string, string> _ErrorMessages = new Dictionary<string, string>();

        // IDataErrorInfo.Error の実装
        public string Error
        {
            get { return (_ErrorMessages.Count > 0) ? "Has Error" : null; }
        }

        // IDataErrorInfo.Item の実装
        public string this[string columnName]
        {
            get
            {
                if (_ErrorMessages.ContainsKey(columnName))
                    return _ErrorMessages[columnName];
                else
                    return null;
            }
        }

        // エラーメッセージのセット
        protected void SetError(string propertyName, string errorMessage)
        {
            _ErrorMessages[propertyName] = errorMessage;
        }

        // エラーメッセージのクリア
        protected void ClearErrror(string propertyName)
        {
            if (_ErrorMessages.ContainsKey(propertyName))
                _ErrorMessages.Remove(propertyName);
        }

        #endregion

        #region == implemnt of ICommand Helper ==

        #region ** Class : _DelegateCommand
        // ICommand実装用のヘルパークラス
        private class _DelegateCommand : ICommand
        {
            private Action<object> _Command;        // コマンド本体
            private Func<object, bool> _CanExecute;  // 実行可否

            // コンストラクタ
            public _DelegateCommand(Action<object> command, Func<object, bool> canExecute = null)
            {
                _Command = command ?? throw new ArgumentNullException(nameof(command));
                _CanExecute = canExecute;
            }

            // ICommand.Executeの実装
            void ICommand.Execute(object parameter)
            {
                _Command(parameter);
            }

            // ICommand.Executeの実装
            bool ICommand.CanExecute(object parameter)
            {
                if (_CanExecute != null)
                    return _CanExecute(parameter);
                else
                    return true;
            }

            // ICommand.CanExecuteChanged の実装
            event EventHandler ICommand.CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }

        private class _DelegateCommand<T> : ICommand
        {
            private Action<T> _Command;        // コマンド本体
            private Func<T, bool> _CanExecute;  // 実行可否

            // コンストラクタ
            public _DelegateCommand(Action<T> command, Func<T, bool> canExecute = null)
            {
                _Command = command ?? throw new ArgumentNullException(nameof(command));
                _CanExecute = canExecute;
            }

            // ICommand.Executeの実装
            void ICommand.Execute(object parameter)
            {
                _Command((T)parameter);
            }

            // ICommand.Executeの実装
            bool ICommand.CanExecute(object parameter)
            {
                if (_CanExecute != null)
                    return _CanExecute((T)parameter);
                else
                    return true;
            }

            // ICommand.CanExecuteChanged の実装
            event EventHandler ICommand.CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
        #endregion



        // コマンドの生成
        protected static ICommand CreateCommand(Action<object> command, Func<object, bool> canExecute = null)
        {
            return new _DelegateCommand(command, canExecute);
        }

        protected static ICommand CreateCommand<T>(Action<T> command, Func<T, bool> canExecute = null)
        {
            return new _DelegateCommand<T>(command, canExecute);
        }
        #endregion
    }
}
