using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MVVMLib
{
    /// <summary>
    /// https://blog.xin9le.net/entry/2013/10/29/222336
    /// 
    /// </summary>
    public static class DependencyObjectExtensions
    {
        #region Descendants
        //--- 子要素を取得
        public static IEnumerable<DependencyObject> Children(this DependencyObject content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var count = VisualTreeHelper.GetChildrenCount(content);
            if (count == 0)
                yield break;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(content, i);
                if (child != null)
                    yield return child;
            }
        }

        //--- 子孫要素を取得
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            foreach (var child in content.Children())
            {
                yield return child;
                foreach (var grandChild in child.Descendants())
                    yield return grandChild;
            }
        }

        //--- 特定の型の子要素を取得
        public static IEnumerable<T> Children<T>(this DependencyObject content)
            where T : DependencyObject
        {
            return content.Children().OfType<T>();
        }

        //--- 特定の型の子孫要素を取得
        public static IEnumerable<T> Descendants<T>(this DependencyObject content)
            where T : DependencyObject
        {
            return content.Descendants().OfType<T>();
        }
        #endregion

        #region Ancestor
        public static T Ancestor<T>(this DependencyObject depObj) where T : class
        {
            var target = depObj;

            try
            {
                do
                {
                    //ビジュアルツリー上の親を探します。
                    //T型のクラスにヒットするまでさかのぼり続けます。
                    target = System.Windows.Media.VisualTreeHelper.GetParent(target);

                } while (target != null && !(target is T));

                return target as T;
            }
            finally
            {
                target = null;
                depObj = null;
            }
        }
        #endregion
    }
}
