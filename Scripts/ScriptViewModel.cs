using CommunityToolkit.Mvvm.ComponentModel;
using ImageHandle.Attributes;
using ImageHandle.Helpers;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ImageHandle.Scripts
{
    public partial class ScriptViewModel : ObservableObject
    {
        public ScriptViewModel()
        {
            RegisterMethodes();
            MethodNames = new ObservableCollection<string>(ScriptService.Instance.GetRegisteredMethodNames(_isCN));
        }

        private List<MethodInfo> GetMethodsWithAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                       .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                       .ToList();
        }

        private void RegisterMethodes()
        {
            List<MethodInfo> methods = GetMethodsWithAttribute<MethodCNNameAttribute>(typeof(ImageOperateMethods));
            foreach (MethodInfo item in methods)
            {
                NoScriptAttribute? attributeFlag = item
                .GetCustomAttributes(typeof(NoScriptAttribute), false)
                .OfType<NoScriptAttribute>()
                .FirstOrDefault();
                if (attributeFlag != null)
                {
                    continue;
                }

                MethodCNNameAttribute? methodCNNameattribute = item
                 .GetCustomAttributes(typeof(MethodCNNameAttribute), false)
                 .OfType<MethodCNNameAttribute>()
                 .FirstOrDefault();
                string nameCN = methodCNNameattribute?.Name ?? "";
                string methodName = item.Name;

                ScriptParamAttribute[]? ScriptParamAttributes = item.GetCustomAttributes(typeof(ScriptParamAttribute), false)
                     .OfType<ScriptParamAttribute>()
                     .ToArray();
                List<ScriptParamModel> list = new List<ScriptParamModel>();
                if (ScriptParamAttributes != null)
                {
                    for (int i = 0; i < ScriptParamAttributes.Length; i++)
                    {
                        Type? enumtype = null;
                        if (ScriptParamAttributes[i].Type == ParamType.Enum)
                        {
                            var paramters = item.GetParameters();
                            enumtype = paramters[i + 1].ParameterType;
                        }
                        string name = ScriptParamAttributes[i].Name;
                        ParamType paramType = ScriptParamAttributes[i].Type;
                        string value = ScriptParamAttributes[i].Value;
                        string paramDisplayName = ScriptParamAttributes[i].ParamDisplayName;
                        string remark = ScriptParamAttributes[i].Remark;
                        ScriptParamModel script = new ScriptParamModel(name, paramType, value, enumtype, paramDisplayName, remark);
                        list.Add(script);
                    }
                }
                ScriptService.Instance.Register(methodName, list, nameCN);
            }
        }

        private ObservableCollection<string> _methodNames = new ObservableCollection<string>();

        public ObservableCollection<string> MethodNames
        {
            get => _methodNames;
            set
            {
                _methodNames = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private string _srcImagePath;

        [ObservableProperty]
        private Mat _dstMat;

        [ObservableProperty]
        private bool _isCN = true;

        partial void OnIsCNChanged(bool value)
        {
            MethodNames = new ObservableCollection<string>(ScriptService.Instance.GetRegisteredMethodNames(_isCN));
        }
    }
}