using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMLib
{
    public class ObservableNotifyCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableNotifyCollection()
        {
            PropertyChanged += OpenPropertyChangedHandle;
            CollectionChanged += _orderlist_CollectionChanged;
        }


        #region Property
        private static int _selectedIndex = -1;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value == _selectedIndex) return;
                _selectedIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedIndex)));
            }
        }
        #endregion


        /// <summary>
        /// コレクションの要素自体が変化した際に、要素にイベントを付加するコールバック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _orderlist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= CollectionPropertyChanged;
                foreach (T item in e.NewItems)
                    item.PropertyChanged += CollectionPropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += CollectionPropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= CollectionPropertyChanged;
            }

        }

        /*
        void MyType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //change event hook here
        }*/
        /// <summary>
        /// コレクションのそれぞれに配置される変更イベント
        /// 上のメソッドの代わり
        /// ちゃんと動くか分からん
        /// </summary>
        public event PropertyChangedEventHandler CollectionPropertyChanged;
        
        /// <summary>
        /// PropertyChangedを外部公開する
        /// 保守性もくそもないガイジコード
        /// </summary>
        public event PropertyChangedEventHandler OpenPropertyChanged;
        private void OpenPropertyChangedHandle(object sender, PropertyChangedEventArgs e)
        {
            OpenPropertyChanged?.Invoke(sender, e);
        }
    }
}
