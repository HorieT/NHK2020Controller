using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MVVMLib
{
    /// <summary>
    /// 某サイトのほぼパクリ
    /// http://yujiro15.net/blog/index.php?id=47
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged のメンバ
        /// <summary>
        /// プロパティ変更時に発生します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged のメンバ


#pragma warning disable CA1030
        /// <summary>
        /// PropertyChanged イベントを発行します。
        /// </summary>
        /// <param name="propertyName">プロパティ名を指定します。</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#pragma warning restore CA1030

        /// <summary>
        /// プロパティ値変更ヘルパです。
        /// </summary>
        /// <typeparam name="T">プロパティの型を表します。</typeparam>
        /// <param name="target">変更するプロパティの実体を指定します。</param>
        /// <param name="value">変更後の値を指定します。</param>
        /// <param name="propertyName">プロパティ名を指定します。</param>
        /// <returns>プロパティ値に変更があった場合に true を返します。</returns>
        protected bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(target, value)) { return false; }
            target = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
