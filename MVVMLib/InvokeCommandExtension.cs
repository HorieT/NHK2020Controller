using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;

namespace MVVMLib
{

    /// <summary>
    /// 
    /// http://sourcechord.hatenablog.com/entry/2014/12/08/030947
    /// </summary>
    [MarkupExtensionReturnType(typeof(EventHandler))]
    public sealed class InvokeCommandExtension : MarkupExtension
    {
        /// <summary>
        /// イベント発生時に呼び出すコマンドのパスを取得または設定します。
        /// </summary>
        public PropertyPath Path { get; set; }
        /// <summary>
        /// イベントが固有のハンドラを持たずに生でEvent<T>実装されてた時の引数型指定
        /// </summary>
        public Type Arg { get; set; }

        private TargetObject _targetObject;

        public InvokeCommandExtension(PropertyPath bindingCommandPath)
        {
            this.Path = bindingCommandPath;
        }

        public InvokeCommandExtension(PropertyPath bindingCommandPath, Type arg)
        {
            this.Path = bindingCommandPath;
            this.Arg = arg;
        }

#pragma warning disable CA1062
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (pvt != null)
            {
                var ei = pvt.TargetProperty as EventInfo;
                var mi = pvt.TargetProperty as MethodInfo;
                var type = ei?.EventHandlerType ?? mi?.GetParameters()[1].ParameterType;

                var element = pvt.TargetObject as FrameworkElement;
                var contentElement = pvt.TargetObject as FrameworkContentElement;
                if (element != null)
                {
                    _targetObject = new TargetObject();
                    this.SetBinding(element.DataContext);
                    element.DataContextChanged += (s, e) => { this.SetBinding(e.NewValue); };
                }
                else if (contentElement != null)
                {
                    _targetObject = new TargetObject();
                    this.SetBinding(contentElement.DataContext);
                    contentElement.DataContextChanged += (s, e) => { this.SetBinding(e.NewValue); };
                }
                else
                {
                    return null;
                }

                // ここで、イベントハンドラを作成し、マークアップ拡張の結果として返す
                var nonGenericMethod = GetType().GetMethod("PrivateHandlerGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
                var argType = type?.GetMethod("Invoke").GetParameters()[1].ParameterType ?? Arg;
                var genericMethod = nonGenericMethod.MakeGenericMethod(argType);

                if(type != null)
                {
                    return Delegate.CreateDelegate(type, this, genericMethod);
                }
                else if(argType != null)
                { 
                    var genericType = typeof(EventHandler<>).MakeGenericType(argType);
                    return Delegate.CreateDelegate(genericType, this, genericMethod);
                }
            }

            return null;
        }
#pragma warning restore CA1062

        private void SetBinding(object dataContext)
        {
            var binding = new Binding()
            {
                Source = dataContext,
                Path = this.Path,
            };
            BindingOperations.SetBinding(_targetObject, TargetObject.TargetValueProperty, binding);
        }

#pragma warning disable CA1801
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private void PrivateHandlerGeneric<T>(object sender, T e)
        {
            // コマンドの取得
            var command = _targetObject.TargetValue as ICommand;

            // コマンドを呼び出す
            if (command != null && command.CanExecute(e))
            {
                command.Execute(e);
            }
        }
    }
#pragma warning restore CA1801

    /// <summary>
    /// 実行対象のコマンドをバインディングターゲットとして保持するために使用するクラス
    /// InvokeCommandExtensionクラス内で使用します
    /// </summary>
    internal class TargetObject : DependencyObject
    {
        public object TargetValue
        {
            get { return (object)GetValue(TargetValueProperty); }
            set { SetValue(TargetValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetValueProperty =
            DependencyProperty.Register("TargetValue", typeof(object), typeof(TargetObject), new PropertyMetadata(null));
    }
}
