using Newtonsoft.Json;

namespace ImageHandle.Scripts
{
    public class ScriptService
    {
        #region 懒加载实现单例

        private static readonly Lazy<ScriptService> _instance = new Lazy<ScriptService>(() => new ScriptService());

        public static ScriptService Instance => _instance.Value;

        private ScriptService()
        {
        }

        #endregion 懒加载实现单例 

        private List<ScriptMethodModel> scriptMethodModels = new();

        public void Register(string methodName, List<ScriptParamModel> paramList, string methodNameCN = "")
        {
            if (scriptMethodModels.Any(x => x.MethodName == methodName || x.MethodNameCN == methodNameCN))
            {
                throw new ArgumentException($"方法已经注册: {methodName},{methodNameCN}");
            }
            scriptMethodModels.Add(new ScriptMethodModel(methodName, paramList, methodNameCN));

            scriptMethodModels = scriptMethodModels.OrderBy(x => x.MethodName).ToList();
        }

        public List<ScriptParamModel> Navigate(string methodName, bool _iscn = false)
        {
            ScriptMethodModel scriptMethod = null;
            if (_iscn == false)
            {
                scriptMethod = scriptMethodModels.FirstOrDefault(x => x.MethodName == methodName);
            }
            else
            {
                scriptMethod = scriptMethodModels.FirstOrDefault(x => x.MethodNameCN == methodName);
            }
            if (scriptMethod == null)
            {
                throw new ArgumentException($"方法未注册: {methodName}");
            }

            List<ScriptParamModel> modelList = scriptMethod.Parameters.Select(param => param.DeepClone()).ToList();
            
            return modelList;
        }

        public List<string> GetRegisteredMethodNames(bool _iscn = false)
        {
            if (_iscn == false)
            {
                return scriptMethodModels.Select(x => x.MethodName).ToList();
            }
            else
            {
                return scriptMethodModels.Select(x => x.MethodNameCN).ToList();
            }
        }

        public string GetMethodName(string methodName, bool _iscn = false)
        {
            if (_iscn == false)
            {
                var scriptMethod = scriptMethodModels.FirstOrDefault(x => x.MethodNameCN == methodName);
                if (scriptMethod == null)
                {
                    throw new ArgumentException($"方法未注册: {methodName}");
                }
                return scriptMethod.MethodName;
            }
            else
            {
                var scriptMethod = scriptMethodModels.FirstOrDefault(x => x.MethodName == methodName);
                if (scriptMethod == null)
                {
                    throw new ArgumentException($"方法未注册: {methodName}");
                }
                return scriptMethod.MethodNameCN;
            }
        }
    }

    public class ScriptMethodModel
    {
        public string MethodName { get; set; }
        public List<ScriptParamModel> Parameters { get; set; }

        public string MethodNameCN { get; set; }

        public ScriptMethodModel(string methodName, List<ScriptParamModel> parameters, string methodNameCN = "")
        {
            MethodName = methodName;
            Parameters = parameters;
            MethodNameCN = methodNameCN == "" ? methodName : methodNameCN;
        }
    }
}